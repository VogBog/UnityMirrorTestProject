using System;
using System.Collections.Generic;
using Game.Scripts.GameSystems.Factories.Data;
using Game.Scripts.GameSystems.GameStarting;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.GameSystems.Factories
{
    public class ZenjectMirrorFactory : NetworkBehaviour, INetworkObjectFactory
    {
        [SerializeField] private PrefabsListScriptable _prefabs;
        [SerializeField] private Transform _objectsParent;
        
        private DiContainer _container;

        private readonly Dictionary<GameObject, uint> _prefabToId = new();
        private readonly Dictionary<uint, GameObject> _idToPrefab = new();
        private readonly Dictionary<uint, List<NetworkIdentity>> _spawned = new();

        public event Action<ObjectSpawnedEvent> Spawned;
        public event Action<ObjectDespawnedEvent> Despawned; 

        #region Initialization

        [Inject]
        private void Construct(
            DiContainer container,
            IGameStarter gameStarter)
        {
            _container = container;
            
            if (NetworkClient.active)
            {
                Initialize(assetId => 
                    NetworkClient.RegisterSpawnHandler(assetId, OnSpawnClient, OnDespawnClient));
            }
            else
            {
                Initialize(null);
            }
        }

        private void Initialize(Action<uint> forEachAsset)
        {
            var prefabs = _prefabs.Prefabs;
            for (int i = 0; i < prefabs.Length; i++)
            {
                uint assetId = prefabs[i].assetId;
                _prefabToId.Add(prefabs[i].gameObject, assetId);
                _idToPrefab.Add(assetId, prefabs[i].gameObject);
                forEachAsset?.Invoke(assetId);
            }
        }

        private void OnDestroy()
        {
            if (!isClient)
                return;
            
            foreach (var id in _prefabToId.Values)
            {
                NetworkClient.UnregisterSpawnHandler(id);
            }
        }
        #endregion
        
        #region General

        public uint GetPrefabId(GameObject go)
        {
            if (_prefabToId.TryGetValue(go, out var id))
                return id;
            return 0;
        }

        public GameObject GetPrefabById(uint prefabId)
        {
            return _idToPrefab.GetValueOrDefault(prefabId);
        }

        public NetworkIdentity[] GetRegisteredPrefabs() => _prefabs.Prefabs;
        
        public List<NetworkIdentity> GetSpawned(uint prefabId) => _spawned.GetValueOrDefault(prefabId);

        private void AddSpawned(uint assetId, NetworkIdentity identity)
        {
            if (!_spawned.TryGetValue(assetId, out var list))
            {
                list = new List<NetworkIdentity>();
                _spawned.Add(assetId, list);
            }
            
            list.Add(identity);
        }

        private void RemoveSpawned(NetworkIdentity identity)
        {
            if (!_spawned.TryGetValue(identity.assetId, out var list))
                return;

            list.Remove(identity);
            if (list.Count == 0)
                _spawned.Remove(identity.assetId);
        }
        #endregion
        
        #region Server
        [Server]
        public NetworkIdentity InstantiateAndSpawnServer(InstantiateAndSpawnCommand command)
        {
            if (!_prefabToId.TryGetValue(command.Prefab, out var prefabId))
                throw new KeyNotFoundException($"Prefab {command.Prefab.name} not found");
            
            var instance = _container.InstantiatePrefab(
                command.Prefab, command.Position, command.Rotation, _objectsParent);
            
            if (!instance.TryGetComponent(out NetworkIdentity networkIdentity))
                throw new InvalidOperationException($"Cannot spawn object without NetworkIdentity {instance}");
            
            NetworkServer.Spawn(instance, command.OwnerPlayer);
            AddSpawned(networkIdentity.assetId, networkIdentity);

            var spawnEvent = new ObjectSpawnedEvent(networkIdentity, new SpawnMessage
            {
                assetId = prefabId,
                isLocalPlayer = networkIdentity.isLocalPlayer,
                isOwner = networkIdentity.isOwned,
                position = command.Position,
                rotation = command.Rotation,
                scale = Vector3.one,
                netId = networkIdentity.netId
            });
            
            Spawned?.Invoke(spawnEvent);
            return networkIdentity;
        }

        [Server]
        public void DespawnAndDestroyServer(GameObject instance)
        {
            if (!instance.TryGetComponent(out NetworkIdentity networkIdentity))
                throw new InvalidOperationException($"Cannot despawn object without NetworkIdentity {instance}");
            
            var despawnEvent = new ObjectDespawnedEvent(networkIdentity);
            Despawned?.Invoke(despawnEvent);
            NetworkServer.Destroy(instance);
            RemoveSpawned(networkIdentity);
        }
        #endregion
        
        #region Client
        private GameObject OnSpawnClient(SpawnMessage msg)
        {
            if (!_idToPrefab.TryGetValue(msg.assetId, out var prefab))
                throw new KeyNotFoundException($"Prefab {msg.assetId} not found");

            var instance = _container.InstantiatePrefab(prefab, msg.position, msg.rotation, _objectsParent);
            
            if (!instance.TryGetComponent(out NetworkIdentity networkIdentity))
                throw new InvalidOperationException($"Cannot spawn prefab without NetworkIdentity {instance}");
            
            AddSpawned(msg.assetId, networkIdentity);

            var spawnEvent = new ObjectSpawnedEvent(networkIdentity, msg);
            Spawned?.Invoke(spawnEvent);
            
            return instance;
        }

        private void OnDespawnClient(GameObject obj)
        {
            var ev = new ObjectDespawnedEvent(obj.GetComponent<NetworkIdentity>());
            Despawned?.Invoke(ev);
            
            RemoveSpawned(obj.GetComponent<NetworkIdentity>());
            Destroy(obj);
        }
        #endregion
    }
}
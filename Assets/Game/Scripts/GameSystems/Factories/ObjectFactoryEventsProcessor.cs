using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.GameSystems.Factories.Data;
using Game.Scripts.Network.EventBus;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.GameSystems.Factories
{
    public class ObjectFactoryEventsProcessor : NetworkBehaviour
    {
        [Inject] private INetworkObjectFactory _factory;
        [Inject] private INetworkServerPublishing _serverPublish;
        
        private readonly List<(uint, Action)> _waitForSpawn = new();
        private readonly Dictionary<uint, (int, Coroutine, Action)> _waitForDestroy = new();
        private readonly Dictionary<Type, IFactoryMessage> _createdEvents = new();
        private readonly Dictionary<Type, object> _afterSpawnEvents = new();
        private readonly Dictionary<Type, object> _beforeDestroyEvents = new();

        public const float MaxWaitingTime = 5f;

        private void Awake()
        {
            _factory.Spawned += OnObjectSpawned;
        }

        private void OnDestroy()
        {
            _factory.Spawned -= OnObjectSpawned;
        }

        public void RegisterHandler<T>(Action<T> handler, bool afterSpawn)
            where T : struct, IFactoryMessage
        {
            var type = typeof(T);
            var dict = afterSpawn ? _afterSpawnEvents : _beforeDestroyEvents;
            if (!dict.TryAdd(type, handler))
                throw new InvalidOperationException($"Cannot register handlers of type {type.Name}. " +
                                                    "You can register only one handler for one event type");

            if (afterSpawn)
            {
                if (NetworkServer.active)
                    NetworkServer.RegisterHandler<T>((_, msg) => InvokeAfterSpawnClientRpc(msg), false);
                else 
                    NetworkClient.RegisterHandler<T>(InvokeAfterSpawnClientRpc, false);
            }
            else
            {
                if (NetworkServer.active)
                    NetworkServer.RegisterHandler<T>((_, msg) => InvokeBeforeDestroyClientRpc(msg), false);
                else 
                    NetworkClient.RegisterHandler<T>(InvokeBeforeDestroyClientRpc, false);
            }
        }

        public void UnregisterHandler<T>(bool afterSpawn) where T : struct, IFactoryMessage
        {
            var dict = afterSpawn ? _afterSpawnEvents : _beforeDestroyEvents;
            if (!dict.Remove(typeof(T)))
                throw new InvalidOperationException($"Cannot unregister handler of type {typeof(T)}. " +
                                                    "Handler has not been registered");
            
            NetworkServer.UnregisterHandler<T>();
        }

        public void SendAfterSpawn<T>(T message, uint objectId)
            where T : struct, IFactoryMessage
        {
            message.SetFactoryItemId(objectId);
            Invoke(message, true);
            _serverPublish.SendToPlayersExcludeServer(message); //-> InvokeAfterSpawnClientRpc
        }

        public void SendBeforeDestroy<T>(T message, NetworkIdentity obj, Action destroy)
            where T : struct, IFactoryMessage
        {
            message.SetFactoryItemId(obj.netId);
            Invoke(message, false);

            if (NetworkServer.connections.Count == 1 && isClient ||
                NetworkServer.connections.Count == 0)
            {
                destroy.Invoke();
                return;
            }
            
            obj.gameObject.SetActive(false);
            _waitForDestroy.Add(obj.netId, (0, StartCoroutine(WaitForDestroyRoutine(obj.netId)), destroy));
            _serverPublish.SendToPlayersExcludeServer(message); //-> InvokeBeforeDestroyClientRpc
        }

        private void Invoke<T>(T message, bool isAfterSpawn)
            where T : struct, IFactoryMessage
        {
            var dict = isAfterSpawn ? _afterSpawnEvents : _beforeDestroyEvents;
            
            var type = typeof(T);
            if (!dict.TryGetValue(type, out var handlerObj))
            {
                Debug.LogError($"Cannot invoke factory event {type}. Handler has not been registered.");
                return;
            }

            if (handlerObj is not Action<T> handler)
            {
                Debug.LogError($"Cannot invoke factory event {type}. Handler has incorrect type");
                return;
            }
            
            if (!_createdEvents.TryGetValue(type, out var ev))
            {
                ev = (IFactoryMessage)Activator.CreateInstance(type);
                _createdEvents.Add(type, ev);
            }

            handler.Invoke(message);
        }

        private void OnObjectSpawned(ObjectSpawnedEvent ev)
        {
            foreach (var (id, handle) in _waitForSpawn)
            {
                if (ev.NetworkIdentity.netId == id)
                {
                    _waitForSpawn.Remove((id, handle));
                    handle.Invoke();
                    return;
                }
            }
        }

        private void DespawnObject(uint objectId)
        {
            var (_, coroutine, handle) = _waitForDestroy[objectId];
            _waitForDestroy.Remove(objectId);
            
            if (coroutine != null)
                StopCoroutine(coroutine);
            
            handle?.Invoke();
        }
        
        private void InvokeAfterSpawnClientRpc<T>(T message)
            where T : struct, IFactoryMessage
        {
            if (!NetworkClient.spawned.ContainsKey(message.GetFactoryItemId()))
            {
                _waitForSpawn.Add((message.GetFactoryItemId(), () => Invoke(message, true)));
                return;
            }
            
            Invoke(message, true);
        }

        private void InvokeBeforeDestroyClientRpc<T>(T message)
            where T : struct, IFactoryMessage
        {
            if (!NetworkClient.spawned.TryGetValue(message.GetFactoryItemId(), out var obj))
            {
                throw new NullReferenceException($"Cannot find object with id {message.GetFactoryItemId()}. " +
                                                 "Cannot invoke BeforeDestroy command.");
            }
            
            Invoke(message, false);
            obj.gameObject.SetActive(false);
            
            CmdEventBeforeDestroyConfirmed(message.GetFactoryItemId());
        }

        [Command(requiresAuthority = false)]
        private void CmdEventBeforeDestroyConfirmed(uint objectId)
        {
            if (_waitForDestroy.TryGetValue(objectId, out var handle))
            {
                int count = handle.Item1 + 1;
                _waitForDestroy[objectId] = (count, handle.Item2, handle.Item3);

                int maxCount = NetworkServer.connections.Count;
                if (isClient)
                    maxCount--;
                
                if (count == maxCount)
                    DespawnObject(objectId);
            }
        }

        private IEnumerator WaitForDestroyRoutine(uint objectId)
        {
            yield return new WaitForSeconds(MaxWaitingTime);
            
            DespawnObject(objectId);
        }
    }
}
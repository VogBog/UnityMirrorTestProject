using System;
using System.Collections;
using System.IO;
using System.Linq;
using Game.Scripts.GameSystems.Factories;
using Game.Scripts.GameSystems.Factories.Data;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.GameSystems.SavingWorld.Scene
{
    public class SceneSaver : MonoBehaviour
    {
        [SerializeField] private NetworkIdentity _playerPrefab;
        
        private INetworkObjectFactory _factory;

        [Inject]
        private void Construct(INetworkObjectFactory factory)
        {
            _factory = factory;
        }
        
        public IEnumerator SaveSceneRoutine(BinaryWriter writer)
        {
            if (!NetworkServer.active)
            {
                yield break;
            }

            uint playerAssetId = _playerPrefab.assetId;
            var spawned = NetworkServer.spawned.Values.ToArray();
            
            yield return null;

            int i = 0;
            foreach (var identity in spawned)
            {
                if (_factory.GetPrefabById(identity.assetId) == null || identity.assetId == playerAssetId)
                    continue;

                if (!identity.TryGetComponent(out ISceneSavingComponent savingComponent) || savingComponent.DoNotSave)
                    continue;
                
                ++i;
                if (i % 10 == 0)
                    yield return null;

                var pos = identity.transform.position;
                var rot = identity.transform.rotation;
                
                writer.Write(true);
                writer.Write(identity.assetId);
                
                writer.Write(pos.x);
                writer.Write(pos.y);
                writer.Write(pos.z);
                
                writer.Write(rot.x);
                writer.Write(rot.y);
                writer.Write(rot.z);
                writer.Write(rot.w);
                
                savingComponent.SaveData(writer);
            }
            
            writer.Write(false);
        }

        public IEnumerator LoadSceneRoutine(BinaryReader reader)
        {
            if (!NetworkServer.active)
            {
                yield break;
            }
            
            yield return null;

            int i = 0;
            var spawned = NetworkServer.spawned.Values.ToArray();
            foreach (var identity in spawned)
            {
                if (_factory.GetPrefabById(identity.assetId) == null || identity.assetId == _playerPrefab.assetId)
                    continue;
                
                _factory.DespawnAndDestroyServer(identity.gameObject);
                ++i;
                
                if (i % 20 == 0)
                    yield return null;
            }

            yield return null;

            while (reader.ReadBoolean())
            {
                uint assetId = reader.ReadUInt32();
                
                float posX = reader.ReadSingle();
                float posY = reader.ReadSingle();
                float posZ = reader.ReadSingle();
                
                float rotX = reader.ReadSingle();
                float rotY = reader.ReadSingle();
                float rotZ = reader.ReadSingle();
                float rotW = reader.ReadSingle();
                
                var pos = new Vector3(posX, posY, posZ);
                var rot = new Quaternion(rotX, rotY, rotZ, rotW);

                var prefab = _factory.GetPrefabById(assetId);
                if (prefab == null)
                    throw new NullReferenceException($"Cannot find prefab with assetId {assetId}");

                var instance = _factory.InstantiateAndSpawnServer(new InstantiateAndSpawnCommand(
                    prefab,
                    pos,
                    rot));

                if (!instance.TryGetComponent(out ISceneSavingComponent savingComponent))
                    throw new InvalidOperationException($"Somehow not ISceneSavingComponent is loaded. NetId: {instance.netId}");
                
                savingComponent.LoadData(reader);
            }
        }
    }
}
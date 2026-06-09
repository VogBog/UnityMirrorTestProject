using System;
using System.Collections.Generic;
using Game.Scripts.GameSystems.Factories.Data;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameSystems.Factories
{
    public interface INetworkObjectFactory
    {
        event Action<ObjectSpawnedEvent> Spawned;
        event Action<ObjectDespawnedEvent> Despawned;
        
        NetworkIdentity InstantiateAndSpawnServer(InstantiateAndSpawnCommand command);

        void DespawnAndDestroyServer(GameObject instance);

        uint GetPrefabId(GameObject go);
        GameObject GetPrefabById(uint prefabId);
        List<NetworkIdentity> GetSpawned(uint prefabId);
        NetworkIdentity[] GetRegisteredPrefabs();
    }
}
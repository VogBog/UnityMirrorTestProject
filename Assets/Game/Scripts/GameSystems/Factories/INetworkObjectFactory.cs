using System;
using System.Collections.Generic;
using Game.Scripts.GameSystems.Factories.Data;
using Game.Scripts.Network.PayloadTransfer;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.GameSystems.Factories
{
    public interface INetworkObjectFactory
    {
        event Action<ObjectSpawnedEvent> Spawned;
        event Action<ObjectDespawnedEvent> Despawned;
        
        NetworkIdentity InstantiateAndSpawnServer(InstantiateAndSpawnCommand command);

        NetworkIdentity InstantiateAndSpawnServer<T>(InstantiateAndSpawnCommand command, T afterSpawnCommand)
            where T : struct, IFactoryMessage;

        void DespawnAndDestroyServer(GameObject instance);

        void DespawnAndDestroyServerWithEvent<T>(GameObject instance, T beforeDestroyEvent)
            where T : struct, IFactoryMessage;

        uint GetPrefabId(GameObject go);
        GameObject GetPrefabById(uint prefabId);
        List<NetworkIdentity> GetSpawned(uint prefabId);
        NetworkIdentity[] GetRegisteredPrefabs();

        void RegisterAfterSpawnHandler<T>(Action<T> handler) where T : struct, IFactoryMessage;
        void RegisterBeforeDestroyHandler<T>(Action<T> handler) where T : struct, IFactoryMessage;
        void UnregisterAfterSpawnHandler<T>() where T : struct, IFactoryMessage;
        void UnregisterBeforeDestroyHandler<T>() where T : struct, IFactoryMessage;
    }
}
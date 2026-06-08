using JetBrains.Annotations;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameSystems.Factories.Data
{
    public struct InstantiateAndSpawnCommand
    {
        public GameObject Prefab;
        [CanBeNull] public NetworkConnectionToClient OwnerPlayer;
        public Vector3 Position;
        public Quaternion Rotation;

        public InstantiateAndSpawnCommand(
            GameObject prefab, 
            Vector3 position,
            Quaternion rotation,
            NetworkConnectionToClient ownerPlayer = null)
        {
            Prefab = prefab;
            Position = position;
            Rotation = rotation;
            OwnerPlayer = ownerPlayer;
        }
    }
}
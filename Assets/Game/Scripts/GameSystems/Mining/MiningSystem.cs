using Game.Scripts.GameSystems.Factories;
using Game.Scripts.GameSystems.Factories.Data;
using Game.Scripts.Player.Inventory.Events;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.GameSystems.Mining
{
    public class MiningSystem : MonoBehaviour, IMiningSystem
    {
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _maxDistance;

        private INetworkObjectFactory _factory;

        [Inject]
        private void Construct(INetworkObjectFactory factory)
        {
            _factory = factory;
        }

        public bool TryMine(Vector3 eyes, Vector3 lookDirection, out IOre ore)
        {
            ore = null;
            var ray = new Ray(eyes, lookDirection);
            if (!Physics.Raycast(ray, out var hit, _maxDistance, _layerMask) ||
                !hit.collider.TryGetComponent(out ore))
                return false;

            return true;
        }

        public void MineOre(uint playerId, IOre ore)
        {
            if (!NetworkServer.active)
                return;

            var command = new InstantiateAndSpawnCommand(
                ore.ExtractableItemPrefab.gameObject,
                ore.SpawnPosition,
                Quaternion.identity);
            
            _factory.InstantiateAndSpawnServer(command, new AddItemAfterSpawnCommand(playerId));
        }
    }
}
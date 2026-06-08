using Game.Scripts.Player.Inventory.Items;
using UnityEngine;

namespace Game.Scripts.GameSystems.Mining
{
    public class OreObject : MonoBehaviour, IOre
    {
        [field: SerializeField] public BaseItem ExtractableItemPrefab { get; private set; }
        [SerializeField] private Transform _spawnPosition;
        
        public Vector3 SpawnPosition => _spawnPosition.position;
    }
}
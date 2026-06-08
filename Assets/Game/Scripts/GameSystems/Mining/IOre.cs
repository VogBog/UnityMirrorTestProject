using Game.Scripts.Player.Inventory.Items;
using UnityEngine;

namespace Game.Scripts.GameSystems.Mining
{
    public interface IOre
    {
        public BaseItem ExtractableItemPrefab { get; }
        public Vector3 SpawnPosition { get; }
    }
}
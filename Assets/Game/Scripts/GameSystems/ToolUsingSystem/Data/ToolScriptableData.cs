using Game.Scripts.Player.Inventory.Items.ScriptableData;
using UnityEngine;

namespace Game.Scripts.GameSystems.ToolUsingSystem.Data
{
    [CreateAssetMenu(menuName = "Data/Items/Tool")]
    public class ToolScriptableData : BaseItemData
    {
        [field: SerializeField] public int Durability { get; private set; }
        [field: SerializeField] public float UsingTime { get; private set; }
        [field: SerializeField] public float UseStamina { get; private set; }
    }
}
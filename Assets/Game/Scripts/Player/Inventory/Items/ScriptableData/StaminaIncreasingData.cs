using Game.Scripts.GameSystems.ToolUsingSystem.Data;
using UnityEngine;

namespace Game.Scripts.Player.Inventory.Items.ScriptableData
{
    [CreateAssetMenu(menuName = "Data/Items/Stamina Adder")]
    public class StaminaIncreasingData : ToolScriptableData
    {
        [field: SerializeField] public float AddStamina { get; private set; }
    }
}
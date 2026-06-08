using Game.Scripts.GameSystems.ToolUsingSystem.Data;
using Game.Scripts.Player.Inventory.Items.ScriptableData;
using UnityEngine;

namespace Game.Scripts.Player.Inventory.Items
{
    public class SimpleMiningTool : BaseMiningTool
    {
        [SerializeField] private ToolScriptableData _data;

        public override BaseItemData BaseItemData => _data;
        public override ToolScriptableData ToolInitData => _data;
    }
}
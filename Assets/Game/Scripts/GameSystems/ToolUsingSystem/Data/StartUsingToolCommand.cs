using Game.Scripts.Player;
using Game.Scripts.Player.Inventory.Items;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Scripts.GameSystems.ToolUsingSystem.Data
{
    public struct StartUsingToolCommand
    {
        public PlayerMainDataComponents Player;
        public BaseItem Item;
        public ITool Tool;
        public Vector3 LookDirection;
        public UsingToolCommandType Type;
        [CanBeNull] public IToolUsingSystem ToolUsingSystem;

        public StartUsingToolCommand(
            PlayerMainDataComponents player,
            BaseItem item,
            ITool tool,
            Vector3 lookDirection,
            UsingToolCommandType type = UsingToolCommandType.ToServer,
            IToolUsingSystem toolUsingSystem = null)
        {
            Player = player;
            Item = item;
            Tool = tool;
            LookDirection = lookDirection;
            Type = type;
            ToolUsingSystem = toolUsingSystem;
        }
    }
}
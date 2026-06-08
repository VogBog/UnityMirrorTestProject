using Game.Scripts.Player;
using UnityEngine;

namespace Game.Scripts.GameSystems.ToolUsingSystem.Data
{
    public struct UseToolCommand
    {
        public PlayerMainDataComponents Player;
        public Vector3 LookDirection;
        public IToolUsingSystem UsingSystem;
        public UsingToolCommandType Type;

        public UseToolCommand(
            PlayerMainDataComponents player,
            Vector3 lookDirection,
            IToolUsingSystem usingSystem,
            UsingToolCommandType type)
        {
            Player = player;
            LookDirection = lookDirection;
            UsingSystem = usingSystem;
            Type = type;
        }
    }
}
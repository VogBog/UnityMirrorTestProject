using Mirror;
using UnityEngine;

namespace Game.Scripts.GameSystems.ToolUsingSystem.Data
{
    public struct UsingToolNetworkCommand : NetworkMessage
    {
        public uint PlayerId;
        public uint ItemId;
        public Vector3 LookDirection;
        public bool IsUsing;
        public int Type;

        public UsingToolNetworkCommand(
            uint playerId,
            uint itemId,
            Vector3 lookDirection,
            bool isUsing,
            int type)
        {
            PlayerId = playerId;
            ItemId = itemId;
            LookDirection = lookDirection;
            IsUsing = isUsing;
            Type = type;
        }
    }
}
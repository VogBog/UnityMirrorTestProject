using Mirror;
using UnityEngine;

namespace Game.Scripts.Player.Control.Data
{
    public struct PlayerInteractionCommand : NetworkMessage
    {
        public uint PlayerId;
        public uint ItemId;
        public PlayerInteractionCommandType Type;
        public Vector3 LookDirection;
        public Vector3 Position;
        public Quaternion Rotation;

        public PlayerInteractionCommand(
            uint playerId, uint itemId, Vector3 lookDirection, PlayerInteractionCommandType type)
        {
            PlayerId = playerId;
            ItemId = itemId;
            Type = type;
            LookDirection = lookDirection;
            
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
        }

        public PlayerInteractionCommand(
            uint playerId, uint itemId, Vector3 lookDirection, Vector3 cancelPosition, Quaternion cancelRotation)
        {
            PlayerId = playerId;
            ItemId = itemId;
            Type = PlayerInteractionCommandType.Cancellation;
            LookDirection = lookDirection;
            
            Position = cancelPosition;
            Rotation = cancelRotation;
        }
    }
}
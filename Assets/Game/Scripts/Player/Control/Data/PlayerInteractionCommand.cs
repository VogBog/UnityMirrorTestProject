using Game.Scripts.Network.PayloadTransfer;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Player.Control.Data
{
    public struct PlayerInteractionCommand : NetworkMessage, INetworkSerializable
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

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteUInt(PlayerId);
            writer.WriteUInt(ItemId);
            writer.WriteInt((int)Type);
            writer.WriteVector3(LookDirection);
        }

        public void Deserialize(NetworkReader reader)
        {
            PlayerId = reader.ReadUInt();
            ItemId = reader.ReadUInt();
            Type = (PlayerInteractionCommandType)reader.ReadInt();
            LookDirection = reader.ReadVector3();
        }
    }
}
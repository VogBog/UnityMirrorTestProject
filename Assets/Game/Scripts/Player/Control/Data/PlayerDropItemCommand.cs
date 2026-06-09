using Game.Scripts.Network.PayloadTransfer;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Player.Control.Data
{
    public struct PlayerDropItemCommand : NetworkMessage, INetworkSerializable
    {
        public uint PlayerId;
        public uint ItemId;
        public Vector3 LookDirection;

        public PlayerDropItemCommand(
            uint playerId,
            uint itemId,
            Vector3 lookDirection)
        {
            PlayerId = playerId;
            ItemId = itemId;
            LookDirection = lookDirection;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteUInt(PlayerId);
            writer.WriteUInt(ItemId);
            writer.WriteVector3(LookDirection);
        }

        public void Deserialize(NetworkReader reader)
        {
            PlayerId = reader.ReadUInt();
            ItemId = reader.ReadUInt();
            LookDirection = reader.ReadVector3();
        }
    }
}
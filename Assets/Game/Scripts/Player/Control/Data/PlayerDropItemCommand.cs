using Game.Scripts.Network.PayloadTransfer;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Player.Control.Data
{
    public struct PlayerDropItemCommand : NetworkMessage, INetworkSerializable
    {
        public uint PlayerId;
        public uint ItemId;
        public int Index;
        public Vector3 LookDirection;

        public bool CommandToServerForConfirm;

        public PlayerDropItemCommand(
            uint playerId,
            uint itemId,
            int index,
            Vector3 lookDirection,
            bool commandToServerForConfirm)
        {
            PlayerId = playerId;
            ItemId = itemId;
            Index = index;
            LookDirection = lookDirection;
            
            CommandToServerForConfirm = commandToServerForConfirm;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteUInt(PlayerId);
            writer.WriteUInt(ItemId);
            writer.WriteInt(Index);
            writer.WriteVector3(LookDirection);
            
            writer.WriteBool(CommandToServerForConfirm);
        }

        public void Deserialize(NetworkReader reader)
        {
            PlayerId = reader.ReadUInt();
            ItemId = reader.ReadUInt();
            Index = reader.ReadInt();
            LookDirection = reader.ReadVector3();
            
            CommandToServerForConfirm = reader.ReadBool();
        }
    }
}
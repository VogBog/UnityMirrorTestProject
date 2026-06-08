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
        public NetworkPayload ItemData;

        public bool CommandToServerForConfirm;

        public PlayerDropItemCommand(
            uint playerId,
            uint itemId,
            int index,
            Vector3 lookDirection,
            NetworkPayload itemData,
            bool commandToServerForConfirm)
        {
            PlayerId = playerId;
            ItemId = itemId;
            Index = index;
            LookDirection = lookDirection;
            ItemData = itemData;
            
            CommandToServerForConfirm = commandToServerForConfirm;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteUInt(PlayerId);
            writer.WriteUInt(ItemId);
            writer.WriteInt(Index);
            writer.WriteVector3(LookDirection);
            
            writer.WriteBool(ItemData.Data != null);
            if (ItemData.Data != null)
                writer.WriteBytesAndSize(ItemData.Data);
            
            writer.WriteBool(CommandToServerForConfirm);
        }

        public void Deserialize(NetworkReader reader)
        {
            PlayerId = reader.ReadUInt();
            ItemId = reader.ReadUInt();
            Index = reader.ReadInt();
            LookDirection = reader.ReadVector3();
            
            bool hasItemData = reader.ReadBool();
            if (hasItemData)
                ItemData.Data = reader.ReadBytesAndSize();
            
            CommandToServerForConfirm = reader.ReadBool();
        }
    }
}
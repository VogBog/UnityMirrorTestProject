using Game.Scripts.Network.PayloadTransfer;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameSystems.ToolUsingSystem.Data
{
    public struct UsingToolNetworkCommand : NetworkMessage, INetworkSerializable
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

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteUInt(PlayerId);
            writer.WriteUInt(ItemId);
            writer.WriteVector3(LookDirection);
            writer.WriteBool(IsUsing);
            writer.WriteInt(Type);
        }

        public void Deserialize(NetworkReader reader)
        {
            PlayerId = reader.ReadUInt();
            ItemId = reader.ReadUInt();
            LookDirection = reader.ReadVector3();
            IsUsing = reader.ReadBool();
            Type = reader.ReadInt();
        }
    }
}
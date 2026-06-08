using Game.Scripts.Network.PayloadTransfer;
using Mirror;

namespace Game.Scripts.Player.Control.Data
{
    public struct PlayerChooseItemCommand : NetworkMessage, INetworkSerializable
    {
        public uint PlayerId;
        public uint ItemId;
        public int Index;

        public bool IsServerCommandForConfirm;

        public PlayerChooseItemCommand(uint playerId, uint itemId, int index, bool isServerCommandForConfirm)
        {
            PlayerId = playerId;
            ItemId = itemId;
            Index = index;
            IsServerCommandForConfirm = isServerCommandForConfirm;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteUInt(PlayerId);
            writer.WriteUInt(ItemId);
            writer.WriteInt(Index);
            writer.WriteBool(IsServerCommandForConfirm);
        }

        public void Deserialize(NetworkReader reader)
        {
            PlayerId = reader.ReadUInt();
            ItemId = reader.ReadUInt();
            Index = reader.ReadInt();
            IsServerCommandForConfirm = reader.ReadBool();
        }
    }
}
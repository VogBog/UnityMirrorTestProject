using Game.Scripts.Network.PayloadTransfer;
using Mirror;

namespace Game.Scripts.Player.Control.Data
{
    public struct RemoveItemFromInventoryCommand : NetworkMessage, INetworkSerializable
    {
        public uint ItemId;

        public RemoveItemFromInventoryCommand(uint itemId)
        {
            ItemId = itemId;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteUInt(ItemId);
        }

        public void Deserialize(NetworkReader reader)
        {
            ItemId = reader.ReadUInt();
        }
    }
}
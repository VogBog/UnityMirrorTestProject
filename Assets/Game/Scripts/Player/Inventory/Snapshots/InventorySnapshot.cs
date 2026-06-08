using Game.Scripts.Network.PayloadTransfer;
using Mirror;

namespace Game.Scripts.Player.Inventory.Snapshots
{
    public struct InventorySnapshot : NetworkMessage, INetworkSerializable
    {
        public ItemStackSnapshot[] Items;

        public InventorySnapshot(ItemStackSnapshot[] items)
        {
            Items = items;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteInt(Items.Length);
            for (int i = 0; i < Items.Length; i++)
            {
                var itemStack = Items[i];
                itemStack.Serialize(writer);
            }
        }

        public void Deserialize(NetworkReader reader)
        {
            int count = reader.ReadInt();
            Items = new ItemStackSnapshot[count];

            for (int i = 0; i < count; i++)
            {
                var itemStack = new ItemStackSnapshot();
                itemStack.Deserialize(reader);
                Items[i] = itemStack;
            }
        }
    }
}
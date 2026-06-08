using Mirror;

namespace Game.Scripts.Player.Inventory.Snapshots
{
    public struct ItemStackSnapshot
    {
        public int MaxCount;
        public uint[] Items;

        public ItemStackSnapshot(int maxCount, uint[] items)
        {
            MaxCount = maxCount;
            Items = items;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteInt(MaxCount);
            writer.WriteInt(Items?.Length ?? 0);

            if (Items != null)
            {
                for (int i = 0; i < Items.Length; i++)
                {
                    writer.WriteUInt(Items[i]);
                }
            }
        }

        public void Deserialize(NetworkReader reader)
        {
            MaxCount = reader.ReadInt();
            int count = reader.ReadInt();

            if (count == 0)
                return;
            
            Items = new uint[count];
            for (int i = 0; i < count; i++)
            {
                Items[i] = reader.ReadUInt();
            }
        }
    }
}
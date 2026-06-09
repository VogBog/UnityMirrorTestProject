using Mirror;

namespace Game.Scripts.Player.Inventory.Snapshots
{
    public struct ApplyInventoryDataCommand : NetworkMessage
    {
        public byte[] Payload;

        public ApplyInventoryDataCommand(InventorySnapshot snapshot)
        {
            var writer = new NetworkWriter();
            snapshot.Serialize(writer);
            Payload = writer.ToArray();
        }

        public InventorySnapshot GetSnapshot()
        {
            var result = new InventorySnapshot();
            var reader = new NetworkReader(Payload);
            result.Deserialize(reader);
            return result;
        }
    }
}
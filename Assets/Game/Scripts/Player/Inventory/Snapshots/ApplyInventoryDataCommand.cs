using Game.Scripts.Network.PayloadTransfer;
using Mirror;

namespace Game.Scripts.Player.Inventory.Snapshots
{
    public struct ApplyInventoryDataCommand : NetworkMessage, INetworkSerializable
    {
        public uint PlayerId;
        public bool IsCancel;
        public bool IsConfirm;
        public NetworkPayload Payload;

        public ApplyInventoryDataCommand(uint playerId, InventorySnapshot snapshot, bool isCancel, bool isConfirm)
        {
            PlayerId = playerId;
            
            Payload = new NetworkPayload();
            Payload.Serialize(snapshot);
            
            IsCancel = isCancel;
            IsConfirm = isConfirm;
        }

        public InventorySnapshot GetSnapshot()
        {
            var result = new InventorySnapshot();
            return Payload.Deserialize(result);
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteUInt(PlayerId);
            writer.WriteBool(IsCancel);
            writer.WriteBool(IsConfirm);
            writer.WriteBytesAndSize(Payload.Data);
        }

        public void Deserialize(NetworkReader reader)
        {
            PlayerId = reader.ReadUInt();
            IsCancel = reader.ReadBool();
            IsConfirm = reader.ReadBool();
            Payload.Data = reader.ReadBytesAndSize();
        }
    }
}
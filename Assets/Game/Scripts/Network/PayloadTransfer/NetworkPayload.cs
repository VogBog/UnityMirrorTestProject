using Mirror;

namespace Game.Scripts.Network.PayloadTransfer
{
    public struct NetworkPayload : NetworkMessage
    {
        public byte[] Data;

        public void Serialize<T>(T serializable)
            where T : struct, INetworkSerializable
        {
            var writer = new NetworkWriter();
            serializable.Serialize(writer);
            Data = writer.ToArray();
        }

        public T Deserialize<T>(T serializable)
            where T : struct, INetworkSerializable
        {
            if (Data == null)
                return serializable;
            
            var reader = new NetworkReader(Data);
            serializable.Deserialize(reader);
            return serializable;
        }

        public void SerializeBoxed(INetworkSerializable serializable)
        {
            var writer = new NetworkWriter();
            serializable.Serialize(writer);
            Data = writer.ToArray();
        }

        public void DeserializeBoxed(INetworkSerializable serializable)
        {
            if (Data == null)
                return;
            
            var reader = new NetworkReader(Data);
            serializable.Deserialize(reader);
        }
    }
}
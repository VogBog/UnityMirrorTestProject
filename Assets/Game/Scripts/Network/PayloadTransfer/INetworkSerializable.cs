using Mirror;

namespace Game.Scripts.Network.PayloadTransfer
{
    public interface INetworkSerializable
    {
        void Serialize(NetworkWriter writer);
        void Deserialize(NetworkReader reader);
    }
}
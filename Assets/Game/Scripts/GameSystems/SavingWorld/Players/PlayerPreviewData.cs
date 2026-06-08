using Game.Scripts.Network.PayloadTransfer;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameSystems.SavingWorld.Players
{
    public struct PlayerPreviewData : INetworkSerializable
    {
        public string Name;
        public Vector3 Position;
        public Quaternion Rotation;

        public PlayerPreviewData(string name, Vector3 position, Quaternion rotation)
        {
            Name = name;
            Position = position;
            Rotation = rotation;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteString(Name);
            writer.WriteVector3(Position);
            writer.WriteQuaternion(Rotation);
        }

        public void Deserialize(NetworkReader reader)
        {
            Name = reader.ReadString();
            Position = reader.ReadVector3();
            Rotation = reader.ReadQuaternion();
        }
    }
}
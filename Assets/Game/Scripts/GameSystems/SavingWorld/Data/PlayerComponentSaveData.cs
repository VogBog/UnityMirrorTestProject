using System.IO;

namespace Game.Scripts.GameSystems.SavingWorld.Data
{
    public class PlayerComponentSaveData
    {
        public string TypeName;
        public byte[] Data;

        public PlayerComponentSaveData(string typeName, byte[] data)
        {
            TypeName = typeName;
            Data = data;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(TypeName);
            writer.Write(Data.Length);
            writer.Write(Data);
        }

        public void Deserialize(BinaryReader reader)
        {
            TypeName = reader.ReadString();
            int length = reader.ReadInt32();
            Data = reader.ReadBytes(length);
        }
    }
}
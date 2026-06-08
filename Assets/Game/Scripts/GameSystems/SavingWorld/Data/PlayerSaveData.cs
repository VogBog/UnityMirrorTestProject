using System.IO;
using UnityEngine;

namespace Game.Scripts.GameSystems.SavingWorld.Data
{
    public class PlayerSaveData
    {
        public string Name;
        public Vector3 Position;
        public Quaternion Rotation;
        public PlayerComponentSaveData[] Components;

        public PlayerSaveData(
            string name,
            Vector3 position,
            Quaternion rotation,
            PlayerComponentSaveData[] components)
        {
            Name = name;
            Position = position;
            Rotation = rotation;
            Components = components;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Name);
            
            writer.Write(Position.x);
            writer.Write(Position.y);
            writer.Write(Position.z);
            
            writer.Write(Rotation.x);
            writer.Write(Rotation.y);
            writer.Write(Rotation.z);
            writer.Write(Rotation.w);
            
            writer.Write(Components.Length);
            foreach (var component in Components)
            {
                component.Serialize(writer);
            }
        }

        public void Deserialize(BinaryReader reader)
        {
            Name = reader.ReadString();

            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            Position = new Vector3(x, y, z);
            
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
            float w = reader.ReadSingle();
            Rotation = new Quaternion(x, y, z, w);
            
            int length = reader.ReadInt32();
            Components = new PlayerComponentSaveData[length];
            for (int i = 0; i < length; i++)
            {
                Components[i] = new PlayerComponentSaveData(string.Empty, null);
                Components[i].Deserialize(reader);
            }
        }
    }
}
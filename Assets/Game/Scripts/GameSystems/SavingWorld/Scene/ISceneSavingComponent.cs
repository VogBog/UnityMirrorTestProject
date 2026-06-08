using System.IO;

namespace Game.Scripts.GameSystems.SavingWorld.Scene
{
    public interface ISceneSavingComponent
    {
        bool DoNotSave { get; }
        
        void SaveData(BinaryWriter writer);
        void LoadData(BinaryReader reader);
    }
}
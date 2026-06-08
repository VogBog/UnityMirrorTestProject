using System.IO;

namespace Game.Scripts.GameSystems.SavingWorld.Players
{
    public interface IPlayerSavingComponent
    {
        void SaveData(BinaryWriter writer);
        void LoadData(BinaryReader reader);
    }
}
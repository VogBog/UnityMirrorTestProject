using System.Collections;

namespace Game.Scripts.GameSystems.SavingWorld.World
{
    public interface IWorldSaver
    {
        bool HasSavedData(string saveName);
        
        IEnumerator SaveWorldRoutine(string saveName);
        IEnumerator LoadWorldRoutine(string saveName);
    }
}
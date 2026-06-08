using System.Collections;
using Game.Scripts.GameSystems.SavingWorld.Data;
using Game.Scripts.Player;

namespace Game.Scripts.GameSystems.SavingWorld.Players
{
    public interface IPlayersSaver
    {
        int MaxSavedPlayersCount { get; }

        PlayerPreviewData GetPreview(string playerName);
        PlayerPreviewData[] GetAllPreviews();
        PlayerSaveData[] GetFullData();
        void ApplyPlayerData(PlayerMainDataComponents playerObject, string playerName);
        void SavePlayer(PlayerMainDataComponents playerObject);
        void SetData(PlayerSaveData[] players);
        IEnumerator SaveAllPlayersRoutine();
    }
}
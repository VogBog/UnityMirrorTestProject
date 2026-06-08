using System.Collections;
using Game.Scripts.GameSystems.SavingWorld.World;
using Game.Scripts.SceneManagement;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Game.Scripts.GameSystems.GameFinishing
{
    public class GameFinisher : MonoBehaviour, IGameFinisher
    {
        [Inject] private IWorldSaver _worldSaver;
        
        private void OnEnable()
        {
            if (!NetworkServer.active)
                NetworkClient.OnDisconnectedEvent += OnDisconnected;
        }

        private void OnDisable()
        {
            if (!NetworkServer.active)
                NetworkClient.OnDisconnectedEvent -= OnDisconnected;
        }
        
        public void QuitGame()
        {
            StartCoroutine(QuitGameRoutine());
        }

        private void OnDisconnected()
        {
            StaticData.DisconnectedFromServer = true;
            SceneManager.LoadScene(nameof(Scenes.MenuOffline));
        }

        private IEnumerator QuitGameRoutine()
        {
            if (!string.IsNullOrEmpty(StaticData.SaveFileName))
                yield return _worldSaver.SaveWorldRoutine(StaticData.SaveFileName);
            
            StaticData.DisconnectedByHimself = true;
            
            if (NetworkServer.active)
                NetworkManager.singleton.StopHost();
            else 
                NetworkManager.singleton.StopClient();

            SceneManager.LoadScene(nameof(Scenes.MenuOffline));
        }
    }
}
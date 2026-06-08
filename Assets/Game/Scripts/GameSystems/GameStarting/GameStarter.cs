using System.Collections;
using System.Collections.Generic;
using Game.Scripts.GameSystems.PlayerSpawning;
using Game.Scripts.GameSystems.SavingWorld.PlayerChoosing;
using Game.Scripts.GameSystems.SavingWorld.World;
using Game.Scripts.Ui.MenusSystem;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.GameSystems.GameStarting
{
    public class GameStarter : MonoBehaviour, IGameStarter, IMenuPage
    {
        [SerializeField] private GameObject _blackScreen;
        [SerializeField] private NetworkPlayerSpawner _playerSpawner;
        [SerializeField] private WorldSaver _worldSaver;
        [SerializeField] private PlayerChooser _playerChooser;

        private IMenusSystem _menusSystem;

        private readonly List<IInitializationWaiter> _initializationQueue = new();

        [Inject]
        private void Construct(IMenusSystem menusSystem)
        {
            _menusSystem = menusSystem;
        }

        public IEnumerable<IGameStartPipelineStep> CreatePipeline()
            => new IGameStartPipelineStep[]
            {
                _worldSaver,
                _playerChooser,
                _playerSpawner
            };
        
        public void AddToInitializationQueue(IInitializationWaiter waiter) => _initializationQueue.Add(waiter);
        
        private IEnumerator Start()
        {
            _blackScreen.SetActive(true);
            _menusSystem.TryOpen(this);
            
            if (NetworkClient.active)
            {
                if (!NetworkClient.ready)
                    NetworkClient.Ready();

                yield return null;
            
                NetworkClient.AddPlayer();

                while (NetworkClient.localPlayer == null || !NetworkClient.localPlayer.isClient)
                    yield return null;
            }

            yield return null;
            
            var initializationQueue = _initializationQueue.ToArray();
            foreach (var waiter in initializationQueue)
            {
                while (!waiter.Initialized)
                    yield return null;
            }
            
            var pipeline = CreatePipeline();
            foreach (var step in pipeline)
            {
                step.StartStep();

                while (!step.Completed)
                    yield return null;
            }
            
            _blackScreen.SetActive(false);
            _menusSystem.Close();
        }

        public bool Open()
        {
            return true;
        }

        public void Close()
        {
            
        }
    }
}
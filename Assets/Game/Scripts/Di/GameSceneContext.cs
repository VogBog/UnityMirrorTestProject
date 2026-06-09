using Game.Scripts.GameSystems.Factories;
using Game.Scripts.GameSystems.GameFinishing;
using Game.Scripts.GameSystems.GameStarting;
using Game.Scripts.GameSystems.Mining;
using Game.Scripts.GameSystems.Repositories;
using Game.Scripts.GameSystems.SavingWorld.Players;
using Game.Scripts.GameSystems.SavingWorld.World;
using Game.Scripts.GameSystems.ToolUsingSystem;
using Game.Scripts.Network.EventBus;
using Game.Scripts.Player.Control;
using Game.Scripts.Ui.MenusSystem;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Di
{
    public class GameSceneContext : MonoInstaller
    {
        [SerializeField] private PlayerControls _playerControls;
        [SerializeField] private ZenjectMirrorFactory _objectFactory;
        [SerializeField] private PlayerNetworkRepository _playerRepository;
        [SerializeField] private EventBus _eventBus;
        [SerializeField] private MiningSystem _miningSystem;
        [SerializeField] private NetworkToolUsingSystem _usingSystem;
        [SerializeField] private GameStarter _gameStarter;
        [SerializeField] private GameFinisher _gameFinisher;
        
        [SerializeField] private PlayersSaver _playersSaver;
        [SerializeField] private WorldSaver _worldSaver;
        
        [SerializeField] private MenusSystem _menusSystem;
        
        public override void InstallBindings()
        {
            Container.Bind<IPlayerControls>().FromInstance(_playerControls).AsSingle();
            Container.Bind<INetworkObjectFactory>().FromInstance(_objectFactory).AsSingle();
            Container.Bind<IPlayerRepository>().FromInstance(_playerRepository).AsSingle();
            Container.Bind<IEventBus>().FromInstance(_eventBus).AsSingle();
            Container.Bind<IMiningSystem>().FromInstance(_miningSystem).AsSingle();
            Container.Bind<IToolUsingSystem>().FromInstance(_usingSystem).AsSingle();
            Container.Bind<IGameStarter>().FromInstance(_gameStarter).AsSingle();
            Container.Bind<IGameFinisher>().FromInstance(_gameFinisher).AsSingle();
            
            Container.Bind<IPlayersSaver>().FromInstance(_playersSaver).AsSingle();
            Container.Bind<IWorldSaver>().FromInstance(_worldSaver).AsSingle();
            
            Container.Bind<IMenusSystem>().FromInstance(_menusSystem).AsSingle();
        }
    }
}
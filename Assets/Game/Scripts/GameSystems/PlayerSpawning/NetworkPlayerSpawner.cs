using System.Collections;
using Game.Scripts.GameSystems.Factories;
using Game.Scripts.GameSystems.Factories.Data;
using Game.Scripts.GameSystems.GameStarting;
using Game.Scripts.GameSystems.Repositories;
using Game.Scripts.GameSystems.SavingWorld.Players;
using Game.Scripts.Player;
using Game.Scripts.Player.Network;
using Game.Scripts.SceneManagement;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.GameSystems.PlayerSpawning
{
    public class NetworkPlayerSpawner : NetworkBehaviour, IGameStartPipelineStep
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Transform[] _spawnPoints;

        private IPlayerRepository _playerRepository;
        private INetworkObjectFactory _networkObjectFactory;
        private IPlayersSaver _playersSaver;
        
        public bool Completed { get; private set; }
        
        [Inject]
        private void Construct(
            IPlayerRepository playerRepository,
            INetworkObjectFactory factory,
            IPlayersSaver playersSaver)
        {
            _playerRepository = playerRepository;
            _networkObjectFactory = factory;
            _playersSaver = playersSaver;
        }

        private void Awake()
        {
            _networkObjectFactory.Spawned += OnObjectSpawn;
            _networkObjectFactory.Despawned += OnObjectDespawn;
            
            if (NetworkServer.active)
                NetworkServer.RegisterHandler<SpawnPlayerCommand>(OnPlayerSpawnCommandReceived);
        }

        private void OnDestroy()
        {
            _networkObjectFactory.Spawned -= OnObjectSpawn;
            _networkObjectFactory.Despawned -= OnObjectDespawn;
            
            if (NetworkServer.active)
                NetworkServer.UnregisterHandler<SpawnPlayerCommand>();
        }

        public void StartStep()
        {
            if (NetworkServer.active)
                StartGameServer(NetworkServer.localConnection, StaticData.PlayerName);
            else 
                StartGameClient();
        }

        public void StartGameServer(NetworkConnectionToClient connection, string playerName)
        {
            var preview = _playersSaver.GetPreview(playerName);
            var position = _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;
            var rotation = Quaternion.identity;

            if (!string.IsNullOrEmpty(preview.Name))
            {
                position = preview.Position;
                rotation = preview.Rotation;
            }

            var instance = _networkObjectFactory.InstantiateAndSpawnServer(new InstantiateAndSpawnCommand(
                    _playerPrefab,
                    position,
                    rotation,
                    connection));

            if (instance.TryGetComponent(out PlayerMainDataComponents player))
            {
                player.Name = playerName;
                _playersSaver.ApplyPlayerData(player, playerName);
            }
        }

        public void StartGameClient()
        {
            Debug.Log("Start game client");
            StartCoroutine(WaitForMyPlayerSpawn());
            NetworkClient.connection.Send(new SpawnPlayerCommand(StaticData.PlayerName));
        }

        private void OnObjectSpawn(ObjectSpawnedEvent ev)
        {
            var playerPrefabId = _networkObjectFactory.GetPrefabId(_playerPrefab);
            if (ev.SpawnMessage.assetId != playerPrefabId)
                return;
            
            _playerRepository.RegisterPlayer(ev.NetworkIdentity);

            if (ev.NetworkIdentity.isOwned)
                Completed = true;
        }

        private void OnObjectDespawn(ObjectDespawnedEvent ev)
        {
            if (_playerRepository.IsPlayer(ev.NetworkIdentity))
                _playerRepository.UnRegisterPlayer(ev.NetworkIdentity);
        }

        private void OnPlayerSpawnCommandReceived(NetworkConnectionToClient connection, SpawnPlayerCommand command)
        {
            StartGameServer(connection, command.Name);
        }

        private IEnumerator WaitForMyPlayerSpawn()
        {
            while (_playerRepository.MyPlayer == null)
                yield return null;
            Completed = true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.GameSystems.GameStarting;
using Game.Scripts.GameSystems.Repositories;
using Game.Scripts.GameSystems.SavingWorld.Players;
using Game.Scripts.SceneManagement;
using Game.Scripts.Ui.PlayerChooser;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.GameSystems.SavingWorld.PlayerChoosing
{
    public class PlayerChooser : NetworkBehaviour, IGameStartPipelineStep, IInitializationWaiter
    {
        [SerializeField] private PlayerChooserView _view;
        
        private IPlayersSaver _playersSaver;
        private IPlayerRepository _playerRepository;
        
        private readonly List<string> _usingNames = new();

        public bool Initialized => _view.Initialized;
        public bool Completed { get; private set; }

        [Inject]
        private void Construct(IPlayersSaver playersSaver, IGameStarter starter, IPlayerRepository repository)
        {
            _playersSaver = playersSaver;
            _playerRepository = repository;
            
            starter.AddToInitializationQueue(this);
        }

        private void Awake()
        {
            _view.PlayerChosen += OnPlayerChosen;
            _view.NewPlayerCreated += OnPlayerChosen;
            _view.Initialize(_playersSaver.MaxSavedPlayersCount);

            if (NetworkServer.active)
            {
                NetworkServer.RegisterHandler<ChoosePlayerNameCommand>(OnChoosePlayerNameCommandReceivedServer);
                NetworkServer.OnDisconnectedEvent += OnClientDisconnected;
            }
            else
            {
                NetworkClient.RegisterHandler<ChoosePlayerNameCommand>(OnChoosePlayerNameCommandReceivedClient);
            }
        }

        private void OnDestroy()
        {
            if (NetworkServer.active)
            {
                NetworkServer.UnregisterHandler<ChoosePlayerNameCommand>();
                NetworkServer.OnDisconnectedEvent -= OnClientDisconnected;
            }
            else
            {
                NetworkClient.UnregisterHandler<ChoosePlayerNameCommand>();
            }
        }

        public void StartStep()
        {
            if (NetworkServer.active)
            {
                var players = _playersSaver.GetAllPreviews();
                _view.ShowPlayerPreviews(players);
            }
            else
            {
                CmdGetPlayersPreviews();
            }
        }

        private void OnPlayerChosen(string playerName)
        {
            if (NetworkServer.active)
            {
                _usingNames.Add(playerName);
                StaticData.PlayerName = playerName;
                Completed = true;
                _view.Close();
                
                if (NetworkServer.connections.Count > 1)
                    UpdatePlayersPreviews();

                return;
            }
            
            _view.Close();
            NetworkClient.Send(new ChoosePlayerNameCommand(playerName));
        }

        [Command(requiresAuthority = false)]
        private void CmdGetPlayersPreviews()
        {
            UpdatePlayersPreviews();
        }

        private void UpdatePlayersPreviews()
        {
            var players = _playersSaver
                .GetAllPreviews()
                .Where(x => !_usingNames.Contains(x.Name))
                .ToArray();
            
            var writer = new NetworkWriter();
            writer.WriteInt(players.Length);
            foreach (var player in players)
            {
                player.Serialize(writer);
            }
            
            GetPlayerPreviewsClientRpc(writer.ToArray());
        }

        [ClientRpc]
        private void GetPlayerPreviewsClientRpc(byte[] payload)
        {
            if (Completed)
                return;
            
            var reader = new NetworkReader(payload);
            int length = reader.ReadInt();
            if (length == 0)
            {
                _view.ShowPlayerPreviews(Array.Empty<PlayerPreviewData>());
            }
            else
            {
                var previews = new PlayerPreviewData[length];
                for (int i = 0; i < length; i++)
                {
                    var data = new PlayerPreviewData();
                    data.Deserialize(reader);
                    previews[i] = data;
                }
                
                _view.ShowPlayerPreviews(previews);
            }
        }

        private void OnChoosePlayerNameCommandReceivedServer(
            NetworkConnectionToClient connection, ChoosePlayerNameCommand command)
        {
            if (!_usingNames.Contains(command.Name))
            {
                _usingNames.Add(command.Name);
                connection.Send(command);
                return;
            }
            
            UpdatePlayersPreviews();
        }

        private void OnChoosePlayerNameCommandReceivedClient(ChoosePlayerNameCommand command)
        {
            StaticData.PlayerName = command.Name;
            Completed = true;
            _view.Close();
        }

        private void OnClientDisconnected(NetworkConnectionToClient connection)
        {
            var player = _playerRepository.GetPlayerObjectByConnection(connection);
            if (player == null)
                return;

            _usingNames.RemoveAll(x => player.Name.Equals(x));
        }
    }
}
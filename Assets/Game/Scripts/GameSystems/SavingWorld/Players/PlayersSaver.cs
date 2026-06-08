using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Scripts.GameSystems.Repositories;
using Game.Scripts.GameSystems.SavingWorld.Data;
using Game.Scripts.Player;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.GameSystems.SavingWorld.Players
{
    public class PlayersSaver : MonoBehaviour, IPlayersSaver
    {
        [Inject] private IPlayerRepository _playerRepository;
        
        private readonly List<PlayerSaveData> _savedPlayers = new();

        public int MaxSavedPlayersCount => 10;
        
        private void Awake()
        {
            if (NetworkServer.active)
                NetworkServer.OnDisconnectedEvent += OnPlayerDisconnected;
        }

        private void OnDestroy()
        {
            if (NetworkServer.active)
                NetworkServer.OnDisconnectedEvent -= OnPlayerDisconnected;
        }

        public PlayerPreviewData GetPreview(string playerName)
        {
            var data = _savedPlayers.FirstOrDefault(x => x.Name == playerName);
            if (data == null)
                return default;

            return new PlayerPreviewData(data.Name, data.Position, data.Rotation);
        }

        public PlayerPreviewData[] GetAllPreviews()
        {
            if (_savedPlayers.Count == 0)
                return Array.Empty<PlayerPreviewData>();

            var result = new PlayerPreviewData[_savedPlayers.Count];
            for (int i = 0; i < _savedPlayers.Count; i++)
            {
                result[i] = new PlayerPreviewData(
                    _savedPlayers[i].Name, _savedPlayers[i].Position, _savedPlayers[i].Rotation);
            }

            return result;
        }
        
        public PlayerSaveData[] GetFullData() => _savedPlayers.ToArray();

        public void ApplyPlayerData(PlayerMainDataComponents playerObject, string playerName)
        {
            var savedData = _savedPlayers.Find(x => x.Name == playerName);
            if (savedData == null)
                return;
            
            playerObject.Name = playerName;
            var components = playerObject.GetComponents<IPlayerSavingComponent>();
            if (components == null || components.Length == 0)
                return;

            foreach (var component in components)
            {
                var name = component.GetType().Name;
                foreach (var savedComponent in savedData.Components)
                {
                    if (name.Equals(savedComponent.TypeName))
                    {
                        var stream = new MemoryStream(savedComponent.Data);
                        var reader = new BinaryReader(stream);
                        component.LoadData(reader);
                        stream.Dispose();
                        reader.Dispose();
                    }
                }
            }
        }

        public void SavePlayer(PlayerMainDataComponents playerObject)
        {
            var componentsToSave = playerObject.GetComponents<IPlayerSavingComponent>();
            var savedComponents = new PlayerComponentSaveData[componentsToSave.Length];

            for (int i = 0; i < componentsToSave.Length; i++)
            {
                var stream = new MemoryStream();
                var writer = new BinaryWriter(stream);
                componentsToSave[i].SaveData(writer);
                var className = componentsToSave[i].GetType().Name;
                savedComponents[i] = new PlayerComponentSaveData(className, stream.ToArray());
                
                stream.Dispose();
                writer.Dispose();
            }
            
            var playerName = playerObject.Name;
            var savedPlayer = new PlayerSaveData(
                playerName,
                playerObject.transform.position,
                playerObject.transform.rotation,
                savedComponents);

            _savedPlayers.RemoveAll(x => x.Name == playerName);
            _savedPlayers.Add(savedPlayer);
        }

        public IEnumerator SaveAllPlayersRoutine()
        {
            var players = new List<PlayerMainDataComponents>();
            _playerRepository.ForEach(player => players.Add(player));

            foreach (var player in players)
            {
                SavePlayer(player);
                yield return null;
            }
        }

        public void SetData(PlayerSaveData[] players)
        {
            _savedPlayers.Clear();
            _savedPlayers.AddRange(players);
        }

        private void OnPlayerDisconnected(NetworkConnectionToClient connection)
        {
            var playerObject = _playerRepository.GetPlayerObjectByConnection(connection);
            if (playerObject == null)
                return;
            
            SavePlayer(playerObject);
            StartCoroutine(RemovePlayerDelayed(connection));
        }

        private IEnumerator RemovePlayerDelayed(NetworkConnectionToClient connection)
        {
            yield return null;
            yield return null;
            
            NetworkServer.DestroyPlayerForConnection(connection);
        }
    }
}
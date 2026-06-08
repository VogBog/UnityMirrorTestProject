using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Game.Scripts.GameSystems.GameStarting;
using Game.Scripts.GameSystems.SavingWorld.Data;
using Game.Scripts.GameSystems.SavingWorld.Players;
using Game.Scripts.GameSystems.SavingWorld.Scene;
using Game.Scripts.SceneManagement;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.GameSystems.SavingWorld.World
{
    [RequireComponent(typeof(SceneSaver))]
    public class WorldSaver : MonoBehaviour, IWorldSaver, IGameStartPipelineStep
    {
        [Inject] private IPlayersSaver _playersSaver;
        
        private SceneSaver _sceneSaver;
        private readonly List<IDisposable> _toDispose = new();
        
        public bool Completed { get; private set; }

        private void Awake()
        {
            _sceneSaver = GetComponent<SceneSaver>();
        }

        private void OnDestroy()
        {
            foreach (var toDispose in _toDispose)
                toDispose.Dispose();
            _toDispose.Clear();
        }

        public bool HasSavedData(string saveName) => File.Exists(GetSavePath(saveName));
        
        public IEnumerator SaveWorldRoutine(string saveName)
        {
            if (!Directory.Exists(GetSaveFolderPath()))
                Directory.CreateDirectory(GetSaveFolderPath());
            
            if (!File.Exists(GetSavePath(saveName)))
                File.Create(GetSavePath(saveName)).Close();
            
            yield return _playersSaver.SaveAllPlayersRoutine();

            var players = _playersSaver.GetFullData();

            var fs = new FileStream(GetSavePath(saveName), FileMode.Create);
            var writer = new BinaryWriter(fs);
            _toDispose.Add(writer);
            _toDispose.Add(fs);
            
            writer.Write(players?.Length ?? 0);
            if (players != null)
            {
                foreach (var player in players)
                    player.Serialize(writer);
            }
            
            yield return _sceneSaver.SaveSceneRoutine(writer);
            
            writer.Dispose();
            fs.Dispose();
            _toDispose.Remove(writer);
            _toDispose.Remove(fs);
        }

        public IEnumerator LoadWorldRoutine(string saveName)
        {
            if (!File.Exists(GetSavePath(saveName)))
            {
                Completed = true;
                yield break;
            }
            
            var fs = new FileStream(GetSavePath(saveName), FileMode.Open);
            var reader = new BinaryReader(fs);
            _toDispose.Add(reader);
            _toDispose.Add(fs);

            yield return null;

            int playersCount = reader.ReadInt32();
            if (playersCount > 0)
            {
                var players = new PlayerSaveData[playersCount];
                for (int i = 0; i < playersCount; i++)
                {
                    var player = new PlayerSaveData(string.Empty, Vector3.zero, Quaternion.identity, null);
                    player.Deserialize(reader);
                    players[i] = player;
                    yield return null;
                }
                
                _playersSaver.SetData(players);
            }

            yield return _sceneSaver.LoadSceneRoutine(reader);
            
            reader.Dispose();
            fs.Dispose();
            _toDispose.Remove(reader);
            _toDispose.Remove(fs);

            Completed = true;
        }
        
        public void StartStep()
        {
            if (!NetworkServer.active || string.IsNullOrEmpty(StaticData.SaveFileName))
            {
                Completed = true;
                return;
            }

            StartCoroutine(LoadWorldRoutine(StaticData.SaveFileName));
        }

        private string GetSavePath(string saveName) => Path.Combine(Application.persistentDataPath, "saves", saveName + ".dat");
        
        private string GetSaveFolderPath() => Path.Combine(Application.persistentDataPath, "saves");
    }
}
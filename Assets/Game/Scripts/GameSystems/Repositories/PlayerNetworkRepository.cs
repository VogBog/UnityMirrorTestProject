using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Player;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameSystems.Repositories
{
    public class PlayerNetworkRepository : NetworkBehaviour, IPlayerRepository
    {
        private readonly List<PlayerMainDataComponents> _players = new();

        public int Count => _players.Count;
        public PlayerMainDataComponents MyPlayer { get; private set; }

        private void OnEnable()
        {
            if (MyPlayer == null)
                StartCoroutine(WaitForMyPlayer());
        }
        
        public void RegisterPlayer(NetworkIdentity playerObject)
        {
            var player = playerObject.GetComponent<PlayerMainDataComponents>();
            
            if (_players.Contains(player))
                throw new InvalidOperationException($"Cannot add same player twice: {playerObject}");
            
            _players.Add(player);
        }

        public void UnRegisterPlayer(NetworkIdentity playerObject)
        {
            _players.Remove(playerObject.GetComponent<PlayerMainDataComponents>());
        }

        public PlayerMainDataComponents GetPlayerObject(uint netId)
        {
            foreach (var player in _players)
            {
                if (player.Identity.netId == netId)
                    return player;
            }

            return null;
        }

        public PlayerMainDataComponents GetPlayerObjectByConnection(NetworkConnectionToClient connection)
        {
            if (!NetworkServer.active)
                throw new InvalidOperationException("GetPlayerObjectByConnection can be called only on server.");
            
            foreach (var player in _players)
            {
                if (player.Identity.connectionToClient == connection)
                    return player;
            }

            return null;
        }

        public bool IsPlayer(NetworkIdentity obj)
        {
            return _players.FindIndex(p => p.Identity == obj) != -1;
        }

        public bool IsMyPlayer(NetworkIdentity obj)
        {
            return obj?.isOwned ?? false;
        }

        public void ForEach(Action<PlayerMainDataComponents> forEachAction)
        {
            foreach (var player in _players)
            {
                if (player != null)
                    forEachAction.Invoke(player);
            }
        }

        public override void OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (!initialState)
                return;
            
            writer.WriteInt(_players.Count);
            foreach (var player in _players)
                writer.WriteUInt(player.Identity.netId);
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (!initialState)
                return;
            
            int count = reader.ReadInt();
            _players.Clear();

            for (int i = 0; i < count; i++)
            {
                uint playerId = reader.ReadUInt();
                if (NetworkClient.active && NetworkClient.spawned.TryGetValue(playerId, out var playerObject))
                    RegisterPlayer(playerObject);
                else 
                    Debug.LogError($"Something went wrong when trying to receive player {playerId}");
            }
        }

        private IEnumerator WaitForMyPlayer()
        {
            while (MyPlayer == null)
            {
                foreach (var player in _players)
                {
                    if (player.Identity == null)
                        continue;
                    if (player.Identity.isOwned)
                    {
                        MyPlayer = player;
                        break;
                    }
                }

                yield return null;
            }
        }
    }
}
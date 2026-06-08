using System;
using Game.Scripts.GameSystems.Repositories;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Network.EventBus
{
    public class NetworkServerPublishing : MonoBehaviour, INetworkServerPublishing
    {
        [Inject] private IPlayerRepository _repository;
        
        public void SendToPlayers<T>(T message) where T : struct, NetworkMessage
        {
            NetworkServer.SendToReady(message);
        }

        public void SendToPlayersExcludeServer<T>(T message) where T : struct, NetworkMessage
        {
            foreach (var connection in NetworkServer.connections.Values)
            {
                if (connection != NetworkServer.localConnection)
                    connection.Send(message);
            }
        }

        public void SendToPlayersExcludeOne<T>(T message, uint excludePlayerId, bool excludeServer = false)
            where T : struct, NetworkMessage
        {
            var excludePlayer = _repository.GetPlayerObject(excludePlayerId);
            if (excludePlayer == null)
                throw new NullReferenceException($"Cannot find player with id {excludePlayerId}");

            var excludeConnection = excludePlayer.Identity.connectionToClient;
            
            foreach (var connection in NetworkServer.connections.Values)
            {
                if (connection != excludeConnection &&
                    (connection != NetworkServer.localConnection || !excludeServer))
                {
                    connection.Send(message);
                }
            }
        }

        public void SendToTargetPlayer<T>(T message, uint playerId) where T : struct, NetworkMessage
        {
            var player = _repository.GetPlayerObject(playerId);
            if (player == null)
                throw new NullReferenceException($"Cannot find player with id {playerId}");
            
            player.Identity.connectionToClient.Send(message);
        }
    }
}
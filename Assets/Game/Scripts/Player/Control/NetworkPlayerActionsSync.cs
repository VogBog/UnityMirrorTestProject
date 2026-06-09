using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Network.PayloadTransfer;
using Game.Scripts.Player.Control.Data;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Player.Control
{
    public class NetworkPlayerActionsSync : MonoBehaviour, IPlayerActionCommandHandler
    {
        private readonly Dictionary<string, Action<PlayerActionCommand>> _handlers = new();

        public const float Timeout = 5f;

        public void RegisterHandler<T>(Action<T> handler, Func<T> factory)
            where T : struct, INetworkSerializable
        {
            var key = typeof(T).Name;
            if (!_handlers.TryAdd(key, command => handler.Invoke(command.GetCommand(factory.Invoke()))))
            {
                throw new InvalidOperationException($"Cannot register handler for {key}. You can register only " +
                                                    "one handler for one command");
            }
        }

        public void UnregisterHandler<T>()
            where T : struct, INetworkSerializable
        {
            if (!_handlers.Remove(typeof(T).Name))
            {
                throw new NullReferenceException($"Cannot unregister handler for {typeof(T).Name}. " +
                                                 "Handler has not been registered.");
            }
        }

        public void RegisterHandlerForPlayer<T>(uint playerId, Action<T> handler, Func<T> factory)
            where T : struct, INetworkSerializable
        {
            var key = typeof(T).Name + "___" + playerId;
            if (!_handlers.TryAdd(key, command => handler.Invoke(command.GetCommand(factory.Invoke()))))
            {
                throw new InvalidOperationException($"Cannot register handler for {typeof(T).Name} of player {playerId}. " +
                                                    "You can register only one handler for one player command");
            }
        }

        public void UnregisterHandlerForPlayer<T>(uint playerId)
            where T : struct, INetworkSerializable
        {
            if (playerId == 0)
                return;
            
            var key = typeof(T).Name + "___" + playerId;
            if (!_handlers.Remove(key))
            {
                throw new NullReferenceException($"Cannot unregister handler for {typeof(T).Name} for player {playerId}. " +
                                                 "Handler has not been registered.");
            }
        }

        public void RegisterHandlerForPlayer<T>(NetworkIdentity playerIdentity, Action<T> handler, Func<T> factory)
            where T : struct, INetworkSerializable
        {
            if (playerIdentity.netId != 0)
            {
                RegisterHandlerForPlayer(playerIdentity.netId, handler, factory);
            }
            else
            {
                StartCoroutine(RegisterWhenInitialized(playerIdentity, id =>
                    RegisterHandlerForPlayer(id, handler, factory)));
            }
        }

        public PlayerActionCommand CreateCommand<T>(T command)
            where T : struct, INetworkSerializable
        {
            return PlayerActionCommand.Create(command);
        }

        public PlayerActionCommand CreateCommandForPlayer<T>(uint playerId, T command)
            where T : struct, INetworkSerializable
        {
            return PlayerActionCommand.CreateForPlayer(playerId, command);
        }
        
        private void Awake()
        {
            if (NetworkServer.active)
            {
                NetworkServer.RegisterHandler<PlayerActionCommand>(
                    (_, msg) => OnPlayerActionCommandReceived(msg));
            }
            else
            {
                NetworkClient.RegisterHandler<PlayerActionCommand>(OnPlayerActionCommandReceived);
            }
        }

        private void OnDestroy()
        {
            if (NetworkServer.active)
            {
                NetworkServer.UnregisterHandler<PlayerActionCommand>();
            }
            else
            {
                NetworkClient.UnregisterHandler<PlayerActionCommand>();
            }
        }

        private void OnPlayerActionCommandReceived(PlayerActionCommand command)
        {
            var key = command.Type;
            if (command.ForPlayer)
                key += "___" + command.PlayerId;
            
            if (!_handlers.TryGetValue(key, out var handler))
            {
                Debug.LogWarning($"Cannot handle message for type {key}. Handler has not been registered. Waiting...");
                StartCoroutine(InvokeWhenRegistered(key, command));
                return;
            }
            
            handler.Invoke(command);
        }

        private IEnumerator RegisterWhenInitialized(NetworkIdentity identity, Action<uint> onInitialized)
        {
            while (identity.netId == 0)
                yield return null;
            onInitialized.Invoke(identity.netId);
        }

        private IEnumerator InvokeWhenRegistered(string key, PlayerActionCommand command)
        {
            yield return null;
            float timeout = Timeout - Time.deltaTime;
            while (timeout >= 0f && !_handlers.ContainsKey(key))
            {
                yield return null;
                timeout -= Time.deltaTime;
            }

            if (!_handlers.TryGetValue(key, out var handle))
            {
                Debug.LogError($"Cannot handle message for type {key}. Handler has not been registered.");
                yield break;
            }
            
            Debug.Log($"Handle for type {key} have been find. Invoking...");
            handle.Invoke(command);
        }
    }
}
using System;
using Game.Scripts.Network.PayloadTransfer;
using Game.Scripts.Player.Control.Data;
using Mirror;

namespace Game.Scripts.Player.Control
{
    public interface IPlayerActionCommandHandler
    {
        public void RegisterHandler<T>(Action<T> handler, Func<T> factory)
            where T : struct, INetworkSerializable;

        public void UnregisterHandler<T>()
            where T : struct, INetworkSerializable;

        void RegisterHandlerForPlayer<T>(uint playerId, Action<T> handler, Func<T> factory)
            where T : struct, INetworkSerializable;

        void RegisterHandlerForPlayer<T>(NetworkIdentity playerIdentity, Action<T> handler, Func<T> factory)
            where T : struct, INetworkSerializable;

        void UnregisterHandlerForPlayer<T>(uint playerId)
            where T : struct, INetworkSerializable;

        public PlayerActionCommand CreateCommand<T>(T command)
            where T : struct, INetworkSerializable;

        PlayerActionCommand CreateCommandForPlayer<T>(uint playerId, T command)
            where T : struct, INetworkSerializable;
    }
}
using System;
using JetBrains.Annotations;
using Mirror;

namespace Game.Scripts.Player.Network
{
    public static class NetworkObjectResolver
    {
        [CanBeNull]
        public static T Resolve<T>(uint netId) where T : class
        {
            NetworkIdentity networkIdentity = null;
            
            if (NetworkServer.active)
                NetworkServer.spawned.TryGetValue(netId, out networkIdentity);
            else if (NetworkClient.active)
                NetworkClient.spawned.TryGetValue(netId, out networkIdentity);

            if (networkIdentity == null)
                return null;

            if (networkIdentity is T tRes)
                return tRes;

            return networkIdentity.GetComponent<T>();
        }

        public static T ResolveOrException<T>(uint netId)
            where T : class
        {
            var result = Resolve<T>(netId);
            if (result == null)
                throw new NullReferenceException(
                    $"Cannot resolve object with id {netId}");

            return result;
        }
    }
}
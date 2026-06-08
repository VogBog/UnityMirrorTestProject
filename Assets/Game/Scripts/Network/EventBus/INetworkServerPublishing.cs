using Mirror;

namespace Game.Scripts.Network.EventBus
{
    public interface INetworkServerPublishing
    {
        public void SendToPlayers<T>(T message) where T : struct, NetworkMessage;

        public void SendToPlayersExcludeServer<T>(T message) where T : struct, NetworkMessage;

        public void SendToPlayersExcludeOne<T>(T message, uint excludePlayerId, bool excludeServer = false)
            where T : struct, NetworkMessage;

        public void SendToTargetPlayer<T>(T message, uint playerId) where T : struct, NetworkMessage;
    }
}
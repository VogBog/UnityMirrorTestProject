using Mirror;

namespace Game.Scripts.Network.PayloadTransfer
{
    public static class NetworkServerCommands
    {
        public static void SendToOtherClients<T>(T message)
            where T : struct, NetworkMessage
        {
            foreach (var connection in NetworkServer.connections.Values)
            {
                if (connection == NetworkServer.localConnection)
                    continue;
                connection.Send(message);
            }
        }

        public static void SendToOtherClients<T>(T message, NetworkConnectionToClient excludeConnection)
            where T : struct, NetworkMessage
        {
            foreach (var connection in NetworkServer.connections.Values)
            {
                if (connection == excludeConnection || connection == NetworkServer.localConnection)
                    continue;
                connection.Send(message);
            }
        }
    }
}
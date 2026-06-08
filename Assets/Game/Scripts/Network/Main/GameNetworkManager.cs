using Mirror;

namespace Game.Scripts.Network.Main
{
    public class GameNetworkManager : NetworkManager
    {
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            //Do nothing
        }
    }
}
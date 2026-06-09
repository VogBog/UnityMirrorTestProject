using Game.Scripts.Player.Inventory;
using Game.Scripts.Player.Network;
using Mirror;

namespace Game.Scripts.Player
{
    public class PlayerMainDataComponents : NetworkBehaviour
    {
        [field: SyncVar]
        public string Name { get; set; }
        public NetworkIdentity Identity { get; private set; }
        public NetworkInventoryActionsSync Inventory { get; private set; }
        public NetworkInventorySync NetworkInventorySync { get; private set; }
        public PlayerCharacteristics Characteristics { get; private set; }
        public PlayerEyes Eyes { get; private set; }

        private void Awake()
        {
            Identity = GetComponent<NetworkIdentity>();
            Inventory = GetComponent<NetworkInventoryActionsSync>();
            NetworkInventorySync = GetComponent<NetworkInventorySync>();
            Characteristics = GetComponent<PlayerCharacteristics>();
            Eyes = GetComponentInChildren<PlayerEyes>();
        }
    }
}
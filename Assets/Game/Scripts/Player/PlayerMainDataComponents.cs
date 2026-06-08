using Game.Scripts.Player.Inventory;
using Game.Scripts.Player.Network;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Player
{
    public class PlayerMainDataComponents : MonoBehaviour
    {
        public string Name { get; set; }
        public NetworkIdentity Identity { get; private set; }
        public IInventory Inventory { get; private set; }
        public NetworkInventorySync NetworkInventorySync { get; private set; }
        public PlayerCharacteristics Characteristics { get; private set; }
        public PlayerEyes Eyes { get; private set; }

        private void Awake()
        {
            Identity = GetComponent<NetworkIdentity>();
            Inventory = GetComponent<IInventory>();
            NetworkInventorySync = GetComponent<NetworkInventorySync>();
            Characteristics = GetComponent<PlayerCharacteristics>();
            Eyes = GetComponentInChildren<PlayerEyes>();
        }
    }
}
using System.IO;
using Game.Scripts.GameSystems.SavingWorld.Scene;
using Game.Scripts.Network.Physics;
using Game.Scripts.Player.Interactions;
using Game.Scripts.Player.Inventory.Items.ScriptableData;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Player.Inventory.Items
{
    [RequireComponent(
        typeof(NetworkRigidbodyPredictable),
        typeof(Rigidbody))]
    public abstract class BaseItem : NetworkBehaviour, IInteractable, ISceneSavingComponent
    {
        [SerializeField] private GameObject _object;
        
        private NetworkRigidbodyPredictable _networkRigidbody;
        private Rigidbody _rigidbody;
        private Collider[] _colliders;

        public NetworkIdentity NetworkIdentity => netIdentity;
        public NetworkRigidbodyPredictable NetworkRigidbody => _networkRigidbody;
        public PlayerMainDataComponents OwnerPlayer { get; private set; }
        public bool CanInteract { get; private set; } = true;
        
        public abstract BaseItemData BaseItemData { get; }
        public virtual bool DoNotSave => OwnerPlayer != null;

        public const float ThrowForce = 4f;

        protected virtual void Awake()
        {
            _networkRigidbody = GetComponent<NetworkRigidbodyPredictable>();
            _rigidbody = GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>();
        }

        public bool ServerInteract(PlayerMainDataComponents player) => TryInteract(player);

        public bool ClientInteractPrediction(PlayerMainDataComponents player) => TryInteract(player);

        public void CancelInteractPrediction(PlayerMainDataComponents player)
        {
            if (OwnerPlayer != player)
                return;
            
            var inventory = player.Inventory;
            inventory?.TryRemove(this);
            NetworkRigidbody.SyncForceCommand();
        }

        public void ClientConfirmInteraction(PlayerMainDataComponents player, bool isOwner)
        {
            if (OwnerPlayer == player)
                return;

            if (OwnerPlayer != null)
                OwnerPlayer.Inventory.TryRemove(this);
            
            player.Inventory.TryAdd(this);
        }

        public virtual void OnAddedToInventory(PlayerMainDataComponents player)
        {
            foreach (var collider in _colliders)
                collider.enabled = false;
            
            _rigidbody.isKinematic = true;
            _networkRigidbody.enabled = false;
            CanInteract = false;
            OwnerPlayer = player;
            _object.SetActive(false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public virtual void OnRemovedFromInventory(PlayerMainDataComponents player)
        {
            foreach (var collider in _colliders)
                collider.enabled = true;
            
            _object.SetActive(true);
            _rigidbody.isKinematic = false;
            _networkRigidbody.enabled = true;
            _networkRigidbody.SetVelocity(player.transform.forward * ThrowForce, Vector3.zero);
            
            CanInteract = true;
            OwnerPlayer = null;
        }

        public virtual void OnChoose(PlayerMainDataComponents player)
        {
            _object.SetActive(true);
        }

        public virtual void OnNotChoose(PlayerMainDataComponents player)
        {
            _object.SetActive(false);
        }

        private bool TryInteract(PlayerMainDataComponents player)
        {
            if (!CanInteract)
                return false;

            if (OwnerPlayer != null)
                return false;

            if (!player.Inventory.TryAdd(this))
                return false;

            return true;
        }

        public abstract void SaveData(BinaryWriter writer);

        public abstract void LoadData(BinaryReader reader);
    }
}
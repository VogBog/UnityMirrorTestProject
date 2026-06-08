using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Game.Scripts.Network.EventBus;
using Game.Scripts.Player.Control.Data;
using Game.Scripts.Player.Inventory;
using Game.Scripts.Player.Inventory.Items;
using Game.Scripts.Player.Network;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Player.Control
{
    [RequireComponent(
        typeof(Inventory.Inventory),
        typeof(NetworkIdentity))]
    public class NetworkInventoryController : MonoBehaviour
    {
        private IInventory _inventory;
        private InventoryController _inventoryController;
        private NetworkIdentity _networkIdentity;
        private INetworkServerPublishing _serverPublishing;
        private IPlayerActionCommandHandler _playerActions;

        private Coroutine _itemChosenCoroutine;
        private bool _initialized;

        public const float ItemChoseSendEventFreeze = 0.2f;

        [Inject]
        private void Construct(
            INetworkServerPublishing serverPublishing,
            IPlayerActionCommandHandler playerActions)
        {
            _inventory = GetComponent<IInventory>();
            _inventoryController = GetComponent<InventoryController>();
            _networkIdentity = GetComponent<NetworkIdentity>();
            
            _serverPublishing = serverPublishing;
            _playerActions = playerActions;
        }

        private void OnEnable()
        {
            if (_inventoryController != null)
            {
                _inventoryController.Dropped += OnItemDropped;
                _inventoryController.Chosen += OnItemChosen;
            }
        }

        private void OnDisable()
        {
            if (_inventoryController != null)
            {
                _inventoryController.Dropped -= OnItemDropped;
                _inventoryController.Chosen -= OnItemChosen;
            }
        }

        private void Awake()
        {
            _playerActions.RegisterHandlerForPlayer(_networkIdentity, DropItemCommand, () => new PlayerDropItemCommand());
            _playerActions.RegisterHandlerForPlayer(_networkIdentity, ChooseItemCommand, () => new PlayerChooseItemCommand());
        }

        private void OnDestroy()
        {
            _playerActions.UnregisterHandlerForPlayer<PlayerDropItemCommand>(_networkIdentity.netId);
            _playerActions.UnregisterHandlerForPlayer<PlayerChooseItemCommand>(_networkIdentity.netId);
        }

        private void OnItemChosen(int index, BaseItem item)
        {
            if (_itemChosenCoroutine != null)
                StopCoroutine(_itemChosenCoroutine);
            _itemChosenCoroutine = StartCoroutine(ItemChosenRoutine(index, item?.NetworkIdentity.netId ?? 0));
        }

        private void OnItemDropped(int index, BaseItem removed)
        {
            if (!NetworkServer.active)
            {
                NetworkClient.Send(PlayerActionCommand.CreateForPlayer(
                    _networkIdentity.netId,
                    new PlayerDropItemCommand(
                        _networkIdentity.netId,
                        removed.NetworkIdentity.netId,
                        index,
                        _networkIdentity.transform.forward,
                        true)));
            }
            else
            {
                _serverPublishing.SendToPlayersExcludeServer(PlayerActionCommand.CreateForPlayer(
                    _networkIdentity.netId,
                    new PlayerDropItemCommand(
                        _networkIdentity.netId,
                        removed.NetworkIdentity.netId,
                        index,
                        _networkIdentity.transform.forward,
                        false)));
            }
        }

        private void DropItemCommand(PlayerDropItemCommand command)
        {
            if (command.PlayerId != _networkIdentity.netId)
                return;
            
            var item = NetworkObjectResolver.Resolve<BaseItem>(command.ItemId);
            if (item == null)
            {
                Debug.LogWarning($"Cannot drop item. Cannot find item with id {command.ItemId}");
                return;
            }

            if (NetworkServer.active && command.CommandToServerForConfirm)
                DropItemServerCommand(command, item);
            else
                DropItemClientCommand(command, item);
        }
        
        private void DropItemServerCommand(PlayerDropItemCommand command, [NotNull] BaseItem item)
        {
            int serverIndex = _inventory.GetIndex(item);
            _inventory.TryRemove(item);
            item.NetworkRigidbody.SetLinearVelocity(command.LookDirection * BaseItem.ThrowForce);
            
            if (serverIndex != command.Index)
                UpdateInventorySnapshot();
                
            _serverPublishing.SendToPlayersExcludeOne(PlayerActionCommand.CreateForPlayer(
                command.PlayerId,
                new PlayerDropItemCommand(
                    command.PlayerId,
                    command.ItemId,
                    command.Index,
                    command.LookDirection,
                    false)),
                command.PlayerId,
                true);
        }
        
        private void DropItemClientCommand(PlayerDropItemCommand command, [NotNull] BaseItem item)
        {
            _inventory.TryRemove(item);
            item.NetworkRigidbody.SyncForceCommand();
        }

        private void ChooseItemCommand(PlayerChooseItemCommand command)
        {
            if (command.PlayerId != _networkIdentity.netId)
                return;
            
            if (NetworkServer.active && command.IsServerCommandForConfirm)
                ChooseItemServerCommand(command);
            else 
                ChooseItemClientCommand(command);
        }

        private void ChooseItemServerCommand(PlayerChooseItemCommand command)
        {
            if (_inventory.ChosenIndex == command.Index &&
                _inventory.ChosenObject?.NetworkIdentity.netId == command.ItemId)
                return;
            
            _inventory.ChooseAt(command.Index);
            uint serverId = 0;
            if (_inventory.ChosenObject != null)
                serverId = _inventory.ChosenObject.NetworkIdentity.netId;

            if (serverId != command.ItemId)
            {
                UpdateInventorySnapshot();
            }
            else
            {
                _serverPublishing.SendToPlayersExcludeOne(PlayerActionCommand.CreateForPlayer(
                    command.PlayerId,
                    new PlayerChooseItemCommand(
                        command.PlayerId,
                        command.ItemId,
                        command.Index,
                        false)),
                    command.PlayerId,
                    true);
            }
        }

        private void ChooseItemClientCommand(PlayerChooseItemCommand command)
        {
            if (_inventory.ChosenIndex == command.Index &&
                _inventory.ChosenObject?.NetworkIdentity.netId == command.ItemId)
                return;
            
            if (command.ItemId == 0)
            {
                _inventory.ChooseAt(command.Index);
                return;
            }
            
            var item = NetworkObjectResolver.Resolve<BaseItem>(command.ItemId);
            if (item != null)
                _inventory.ChooseAt(_inventory.GetIndex(item));
        }

        private IEnumerator ItemChosenRoutine(int index, uint itemId)
        {
            yield return new WaitForSeconds(ItemChoseSendEventFreeze);

            if (!NetworkServer.active)
            {
                NetworkClient.Send(PlayerActionCommand.CreateForPlayer(
                    _networkIdentity.netId,
                    new PlayerChooseItemCommand(
                        _networkIdentity.netId,
                        itemId,
                        index,
                        true)));
            }
            else
            {
                _serverPublishing.SendToPlayersExcludeServer(PlayerActionCommand.CreateForPlayer(
                    _networkIdentity.netId,
                    new PlayerChooseItemCommand(
                        _networkIdentity.netId,
                        itemId,
                        index,
                        false)));
            }
        }

        private void UpdateInventorySnapshot()
        {
            Debug.LogWarning("Find differences in inventory queue. Need to add inventory snapshoting");
        }
    }
}
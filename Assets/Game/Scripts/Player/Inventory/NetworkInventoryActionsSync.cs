using System;
using System.Collections;
using Game.Scripts.Network.EventBus;
using Game.Scripts.Player.Control;
using Game.Scripts.Player.Control.Data;
using Game.Scripts.Player.Inventory.Items;
using Game.Scripts.Player.Network;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Player.Inventory
{
    [RequireComponent(typeof(Inventory), typeof(NetworkIdentity))]
    public class NetworkInventoryActionsSync : MonoBehaviour, IInventory
    {
        private IInventory _inventory;
        private IPlayerActionCommandHandler _playerActions;
        private INetworkServerPublishing _serverPublishing;
        private NetworkIdentity _networkIdentity;
        
        private Coroutine _chooseItemCoroutine;
        
        public int ChosenIndex => _inventory.ChosenIndex;
        public BaseItem ChosenObject => _inventory.ChosenObject;
        public int Count => _inventory.Count;

        public const float Timeout = 5f;
        public const float ChooseItemFreezeTime = 0.2f;
        public const float DropForce = 4f;

        [Inject]
        private void Construct(IPlayerActionCommandHandler playerActions, INetworkServerPublishing serverPublishing)
        {
            _inventory = GetComponent<Inventory>();
            _networkIdentity = GetComponent<NetworkIdentity>();
            _playerActions = playerActions;
            _serverPublishing = serverPublishing;
        }

        private void OnEnable()
        {
            _playerActions.RegisterHandlerForPlayer(
                _networkIdentity, OnAddItemToInventory, () => new AddItemToInventoryCommand());
            _playerActions.RegisterHandlerForPlayer(
                _networkIdentity, OnItemRemove, () => new RemoveItemFromInventoryCommand());
            _playerActions.RegisterHandlerForPlayer(
                _networkIdentity, OnChooseItem, () => new PlayerChooseItemCommand());
            _playerActions.RegisterHandlerForPlayer(
                _networkIdentity, OnDropItem, () => new PlayerDropItemCommand());
        }

        private void OnDisable()
        {
            _playerActions.UnregisterHandlerForPlayer<AddItemToInventoryCommand>(_networkIdentity.netId);
            _playerActions.UnregisterHandlerForPlayer<RemoveItemFromInventoryCommand>(_networkIdentity.netId);
            _playerActions.UnregisterHandlerForPlayer<PlayerChooseItemCommand>(_networkIdentity.netId);
            _playerActions.UnregisterHandlerForPlayer<PlayerDropItemCommand>(_networkIdentity.netId);
        }

        public bool TryAdd(BaseItem item)
        {
            if (_inventory.TryAdd(item))
            {
                if (NetworkServer.active)
                {
                    _serverPublishing.SendToPlayersExcludeServer(PlayerActionCommand.CreateForPlayer(
                        _networkIdentity.netId,
                        new AddItemToInventoryCommand(item.netId)));
                }
                
                return true;
            }

            return false;
        }

        public bool TryRemove(BaseItem item)
        {
            if (_inventory.TryRemove(item))
            {
                if (NetworkServer.active)
                {
                    _serverPublishing.SendToPlayersExcludeServer(PlayerActionCommand.CreateForPlayer(
                        _networkIdentity.netId,
                        new RemoveItemFromInventoryCommand(item.netId)));
                }
                
                return true;
            }

            return false;
        }

        public bool TryRemoveAt(int index)
        {
            var itemStack = _inventory.GetAt(index);
            if (itemStack.Items == null || itemStack.Items.Count == 0)
                return false;
            
            return TryRemove(itemStack.Items[0]);
        }

        public void ChooseAt(int index)
        {
            _inventory.ChooseAt(index);
            
            if (_chooseItemCoroutine != null)
                StopCoroutine(_chooseItemCoroutine);
            _chooseItemCoroutine = StartCoroutine(ChooseItemRoutine(index));
        }
        
        public void ChooseAtLocal(int index) => ChooseAt(index);

        public ItemStack GetAt(int index) => _inventory.GetAt(index);

        public int GetIndex(BaseItem item) => _inventory.GetIndex(item);

        public ItemStack[] GetAllCopy() => _inventory.GetAllCopy();

        public void ForEach(Action<ItemStack> action) => _inventory.ForEach(action);

        public bool Contains(BaseItem item) => _inventory.Contains(item);

        public void ApplyData(ItemStack[] data) => _inventory.ApplyData(data);

        public void DropItem(BaseItem item)
        {
            if (!_networkIdentity.isOwned || item == null)
                return;

            if (!_inventory.TryRemove(item))
                return;

            var command = new PlayerDropItemCommand(
                _networkIdentity.netId,
                item.netId,
                _networkIdentity.transform.forward);
            
            item.NetworkRigidbody.SetLinearVelocity(command.LookDirection * DropForce);

            if (NetworkServer.active)
            {
                _serverPublishing.SendToPlayersExcludeServer(PlayerActionCommand.CreateForPlayer(
                    _networkIdentity.netId,
                    command));
            }
            else
            {
                NetworkClient.Send(PlayerActionCommand.CreateForPlayer(_networkIdentity.netId, command));
            }
        }

        private void OnAddItemToInventory(AddItemToInventoryCommand command)
        {
            StartCoroutine(AddItemRoutine(command.ItemId));
        }

        private void OnItemRemove(RemoveItemFromInventoryCommand command)
        {
            var item = NetworkObjectResolver.Resolve<BaseItem>(command.ItemId);
            _inventory.TryRemove(item); //If item is null, it will remove null data in inventory
        }

        private void OnDropItem(PlayerDropItemCommand command)
        {
            var item = NetworkObjectResolver.Resolve<BaseItem>(command.ItemId);
            _inventory.TryRemove(item); //If item is null, it will remove null data in inventory

            if (item != null)
            {
                item.NetworkRigidbody.SetLinearVelocity(command.LookDirection * DropForce);
            }

            if (NetworkServer.active)
            {
                _serverPublishing.SendToPlayersExcludeOne(
                    PlayerActionCommand.CreateForPlayer(_networkIdentity.netId, command),
                    _networkIdentity.netId,
                    true);
            }
        }

        private void OnChooseItem(PlayerChooseItemCommand command)
        {
            if (_inventory.ChosenIndex == command.Index &&
                _inventory.ChosenObject?.NetworkIdentity.netId == command.ItemId)
                return;
            
            _inventory.ChooseAt(command.Index);

            if (command.ItemId != 0 && (_inventory.ChosenObject?.netId ?? 0) != command.ItemId)
            {
                var item = NetworkObjectResolver.Resolve<BaseItem>(command.ItemId);
                if (item != null && _inventory.Contains(item))
                    _inventory.ChooseAt(_inventory.GetIndex(item));
            }

            if (NetworkServer.active && command.IsServerCommandForConfirm)
            {
                command.IsServerCommandForConfirm = false;
                _serverPublishing.SendToPlayersExcludeOne(command, command.PlayerId, true);
            }
        }

        private IEnumerator AddItemRoutine(uint itemId)
        {
            var item = NetworkObjectResolver.Resolve<BaseItem>(itemId);
            float timeout = Timeout;
            while (item == null && timeout > 0f)
            {
                yield return null;
                timeout -= Time.deltaTime;
                item = NetworkObjectResolver.Resolve<BaseItem>(itemId);
            }

            if (item == null)
            {
                Debug.LogError($"Cannot add item with id {itemId} to inventory. Item has not been spawned");
                yield break;
            }
            
            if (item.OwnerPlayer != null && item.OwnerPlayer.Identity == _networkIdentity)
                yield break;

            if (item.OwnerPlayer != null && item.OwnerPlayer.Identity != _networkIdentity)
            {
                item.OwnerPlayer.Inventory.TryRemove(item);
            }

            _inventory.TryAdd(item);
        }

        private IEnumerator ChooseItemRoutine(int index)
        {
            yield return new WaitForSeconds(ChooseItemFreezeTime);

            if (NetworkServer.active)
            {
                _serverPublishing.SendToPlayersExcludeServer(PlayerActionCommand.CreateForPlayer(
                    _networkIdentity.netId,
                    new PlayerChooseItemCommand(
                        _networkIdentity.netId,
                        _inventory.ChosenObject?.netId ?? 0,
                        index,
                        false
                        )));
            }
            else
            {
                NetworkClient.Send(PlayerActionCommand.CreateForPlayer(_networkIdentity.netId,
                    new PlayerChooseItemCommand(
                        _networkIdentity.netId,
                        _inventory.ChosenObject?.netId ?? 0,
                        index,
                        true)));
            }
        }
    }
}
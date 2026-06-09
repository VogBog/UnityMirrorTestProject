using System;
using System.Collections;
using Game.Scripts.Player.Inventory.Items;
using Game.Scripts.Player.Network;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Player.Inventory
{
    [RequireComponent(typeof(Inventory))]
    public class NetworkInventoryActionsSync : NetworkBehaviour, IInventory
    {
        private IInventory _inventory;
        
        private Coroutine _chooseItemCoroutine;
        
        public int ChosenIndex => _inventory.ChosenIndex;
        public BaseItem ChosenObject => _inventory.ChosenObject;
        public int Count => _inventory.Count;

        public const float Timeout = 5f;
        public const float ChooseItemFreezeTime = 0.2f;
        public const float DropForce = 4f;

        private void Awake()
        {
            _inventory = GetComponent<Inventory>();
        }

        public bool TryAdd(BaseItem item)
        {
            if (_inventory.TryAdd(item))
            {
                if (NetworkServer.active)
                {
                    AddItemToInventoryClientRpc(item.netId);
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
                    RemoveItemClientRpc(item.netIdentity);
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
            if (!isOwned || item == null)
                return;

            if (!_inventory.TryRemove(item))
                return;

            var lookDirection = transform.forward;
            item.NetworkRigidbody.SetLinearVelocity(lookDirection * DropForce);

            if (isServer)
            {
                DropItemClientRpc(item.netIdentity, lookDirection);
            }
            else
            {
                CmdDropItem(item.netIdentity, lookDirection);
            }
        }

        [ClientRpc]
        private void AddItemToInventoryClientRpc(uint itemId)
        {
            StartCoroutine(AddItemRoutine(itemId));
        }

        [ClientRpc]
        private void RemoveItemClientRpc(NetworkIdentity itemIdentity)
        {
            var item = itemIdentity.GetComponent<BaseItem>();
            _inventory.TryRemove(item); //If item is null, it will remove null data in inventory
        }

        [Command(requiresAuthority = false)]
        private void CmdDropItem(NetworkIdentity itemIdentity, Vector3 lookDirection)
        {
            var item = itemIdentity.GetComponent<BaseItem>();
            _inventory.TryRemove(item); //If item is null, it will remove null data in inventory

            if (item != null)
            {
                item.NetworkRigidbody.SetLinearVelocity(lookDirection * DropForce);
            }
            
            DropItemClientRpc(itemIdentity, lookDirection);
        }

        [ClientRpc]
        private void DropItemClientRpc(NetworkIdentity itemIdentity, Vector3 lookDirection)
        {
            var item = itemIdentity.GetComponent<BaseItem>();
            bool removed = _inventory.TryRemove(item); //If item is null, it will remove null data in inventory

            if (item != null && removed)
            {
                item.NetworkRigidbody.SetLinearVelocity(lookDirection * DropForce);
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdChooseItem(uint itemId, int index)
        {
            OnChooseItem(itemId, index);
        }

        [ClientRpc(includeOwner = false)]
        private void ChooseItemClientRpc(uint itemId, int index)
        {
            if (isServer)
                return;
            
            OnChooseItem(itemId, index);
        }

        private void OnChooseItem(uint itemId, int index)
        {
            if (_inventory.ChosenIndex == index &&
                _inventory.ChosenObject?.NetworkIdentity.netId == itemId)
                return;
            
            _inventory.ChooseAt(index);

            if (itemId != 0 && (_inventory.ChosenObject?.netId ?? 0) != itemId)
            {
                var item = NetworkObjectResolver.Resolve<BaseItem>(itemId);
                if (item != null && _inventory.Contains(item))
                    _inventory.ChooseAt(_inventory.GetIndex(item));
            }

            if (NetworkServer.active)
            {
                ChooseItemClientRpc(itemId, index);
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
            
            if (item.OwnerPlayer != null && item.OwnerPlayer.Identity == netIdentity)
                yield break;

            if (item.OwnerPlayer != null && item.OwnerPlayer.Identity != netIdentity)
            {
                item.OwnerPlayer.Inventory.TryRemove(item);
            }

            _inventory.TryAdd(item);
        }

        private IEnumerator ChooseItemRoutine(int index)
        {
            yield return new WaitForSeconds(ChooseItemFreezeTime);

            if (isServer)
            {
                ChooseItemClientRpc(_inventory.ChosenObject?.netId ?? 0, index);
            }
            else
            {
                CmdChooseItem(_inventory.ChosenObject?.netId ?? 0, index);
            }
        }
    }
}
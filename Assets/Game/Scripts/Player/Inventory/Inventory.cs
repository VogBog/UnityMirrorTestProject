using System;
using System.Collections.Generic;
using Game.Scripts.Network.EventBus;
using Game.Scripts.Network.PayloadTransfer;
using Game.Scripts.Player.Inventory.Events;
using Game.Scripts.Player.Inventory.Items;
using Game.Scripts.Player.Network;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Player.Inventory
{
    public class Inventory : MonoBehaviour, IInventory, INetworkSerializable
    {
        [SerializeField] private Transform _armTransform;
        
        private IEventBus _eventBus;
        private PlayerMainDataComponents _player;
        
        private readonly ItemStack[] _items = new ItemStack[Capacity];

        public const int Capacity = 9;

        public int ChosenIndex { get; private set; } = 0;
        public BaseItem ChosenObject => _items[ChosenIndex].Items?[0];
        public int Count { get; private set; } = 0;

        [Inject]
        private void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _player = GetComponent<PlayerMainDataComponents>();
        }

        public bool TryAdd(BaseItem item)
        {
            int index = TryAddStack(item);

            if (index == -1)
            {
                index = TryAddNew(item);

                if (index == -1)
                    return false;
            }
            
            OnItemAdded(item, index);

            return true;
        }

        public bool TryInsert(int index, BaseItem item)
        {
            if (index < 0 || index >= Capacity)
                return false;
            
            if (_items[index].Items == null)
            {
                _items[index] = new ItemStack(item);
                OnItemAdded(item, index);
                return true;
            }

            var stack = _items[index];
            if (!item.GetType().IsEquivalentTo(stack.Items[0].GetType()))
                return false;

            if (stack.Items.Count >= stack.MaxStack)
                return false;
            
            stack.Items.Add(item);
            OnItemAdded(item, index);

            return true;
        }

        public bool TryRemove(BaseItem item)
        {
            for (int i = 0; i < _items.Length; i++)
            {
                var stack = _items[i];
                if (stack.Items == null)
                    continue;
                
                if (stack.Items.Contains(item))
                {
                    stack.Items.Remove(item);
                    if (stack.Items.Count == 0)
                    {
                        _items[i] = default;
                        Count--;
                    }
                    
                    OnItemRemoved(item, i);
                    ChooseAt(i);
                    return true;
                }
            }

            return false;
        }

        public bool TryRemoveAt(int index)
        {
            if (_items[index].Items == null)
                return false;

            return TryRemove(_items[index].Items[0]);
        }

        public void ChooseAt(int index)
        {
            if (index < 0)
                index += Capacity;
            else if (index >= Capacity)
                index %= Capacity;

            if (ChosenObject != null)
            {
                ChosenObject.OnNotChoose(_player);
            }
            
            ChosenIndex = index;
            if (ChosenObject != null)
            {
                ChosenObject.OnChoose(_player);
            }
            
            var ev = new ItemChosenEvent(
                _player,
                ChosenObject, 
                index);
            
            _eventBus.Publish(ev);
        }

        public ItemStack GetAt(int index)
        {
            return _items[index];
        }

        public int GetIndex(BaseItem item)
        {
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i].Items != null && _items[i].Items.Contains(item))
                    return i;
            }

            return -1;
        }

        public ItemStack[] GetAllCopy()
        {
            var result = new ItemStack[Capacity];
            Array.Copy(_items, result, Capacity);
            return result;
        }

        public void ForEach(Action<ItemStack> action)
        {
            foreach (var itemStack in _items)
            {
                action.Invoke(itemStack);
            }
        }

        public bool Contains(BaseItem item) => GetIndex(item) != -1;

        public void ApplyData(ItemStack[] data)
        {
            if (data.Length != IInventory.Capacity)
                throw new Exception($"Data length must be equal to {IInventory.Capacity}, not {data.Length}");

            var prevChosenObject = ChosenObject;
            
            data.CopyTo(_items, 0);
            _eventBus.Publish(new InventoryChangedEvent(_player));
            
            if (ChosenObject != prevChosenObject)
            {
                if (prevChosenObject != null)
                {
                    prevChosenObject.OnNotChoose(_player);
                }
                    
                ChooseAt(ChosenIndex);
            }
        }

        private int TryAddStack(BaseItem item)
        {
            var type = item.GetType();
            for (int i = 0; i < Capacity; i++)
            {
                if (_items[i].Items == null || !_items[i].Items[0].GetType().IsEquivalentTo(type))
                    continue;

                var stack = _items[i];
                if (stack.Items.Count >= stack.MaxStack)
                    continue;
                
                stack.Items.Add(item);
                return i;
            }

            return -1;
        }

        private int TryAddNew(BaseItem item)
        {
            if (Count >= Capacity)
                return -1;

            for (int i = 0; i < Capacity; i++)
            {
                if (_items[i].Items == null)
                {
                    _items[i] = new ItemStack(item);
                    Count++;
                    
                    return i;
                }
            }

            throw new Exception("Unknown exception in Inventory.TryAdd");
        }

        private void OnItemAdded(BaseItem item, int index)
        {
            item.transform.SetParent(_armTransform);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            item.OnAddedToInventory(_player);
            
            if (ChosenObject == item)
                item.OnChoose(_player);
            
            _eventBus.Publish(
                new ItemAddedEvent(_player, item, index));
            _eventBus.Publish(new InventoryChangedEvent(_player));
        }

        private void OnItemRemoved(BaseItem item, int index)
        {
            item.transform.SetParent(null);
            if (ChosenObject == item)
                item.OnNotChoose(_player);
            
            item.OnRemovedFromInventory(_player);
            
            _eventBus.Publish(new ItemRemovedEvent(
                _player, item, index));
            _eventBus.Publish(new InventoryChangedEvent(_player));
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteInt(ChosenIndex);
            foreach (var itemStack in _items)
            {
                if (itemStack.Items == null)
                {
                    writer.WriteBool(false);
                }
                else
                {
                    writer.WriteBool(true);
                    writer.WriteInt(itemStack.MaxStack);
                    writer.WriteInt(itemStack.Items.Count);
                    foreach (var item in itemStack.Items)
                    {
                        writer.WriteUInt(item.NetworkIdentity.netId);
                    }
                }
            }
        }

        public void Deserialize(NetworkReader reader)
        {
            int chooseIndex = reader.ReadInt();
            for (int i = 0; i < _items.Length; i++)
            {
                bool hasItem = reader.ReadBool();
                if (!hasItem)
                {
                    _items[i] = default;
                    continue;
                }
                
                int maxStack = reader.ReadInt();
                int count = reader.ReadInt();
                var list = new List<BaseItem>();
                for (int j = 0; j < count; j++)
                {
                    uint netId = reader.ReadUInt();
                    var item = NetworkObjectResolver.ResolveOrException<BaseItem>(netId);
                    list.Add(item);
                    OnItemAdded(item, i);
                }

                _items[i] = new ItemStack
                {
                    Items = list,
                    MaxStack = maxStack
                };
            }
            
            ChooseAt(chooseIndex);
        }
    }
}
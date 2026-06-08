using System.Collections;
using System.Collections.Generic;
using System.IO;
using Game.Scripts.GameSystems.Factories;
using Game.Scripts.GameSystems.Factories.Data;
using Game.Scripts.GameSystems.SavingWorld.Players;
using Game.Scripts.GameSystems.ToolUsingSystem;
using Game.Scripts.GameSystems.ToolUsingSystem.Data;
using Game.Scripts.Network.EventBus;
using Game.Scripts.Player;
using Game.Scripts.Player.Inventory;
using Game.Scripts.Player.Inventory.Events;
using Game.Scripts.Player.Inventory.Items;
using Game.Scripts.Player.Network;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.GameSystems.SavingWorld.Inventory
{
    //TODO: Refactor this later
    [RequireComponent(typeof(PlayerMainDataComponents))]
    public class InventorySaver : NetworkBehaviour, IPlayerSavingComponent
    {
        [Inject] private INetworkObjectFactory _factory;
        [Inject] private IEventBus _eventBus;

        private PlayerMainDataComponents _player;
        
        private ItemStackSavedData[] _data;

        private void Awake()
        {
            _player = GetComponent<PlayerMainDataComponents>();
        }

        public void SaveData(BinaryWriter writer)
        {
            if (!NetworkServer.active)
                return;
            
            var itemStacks = _player.Inventory.GetAllCopy();
            writer.Write(itemStacks.Length);
            foreach (var stack in itemStacks)
            {
                writer.Write(stack.MaxStack);
                if (stack.Items == null || stack.Items.Count == 0)
                {
                    writer.Write(0);
                    continue;
                }
                
                writer.Write(stack.Items.Count);
                foreach (var item in stack.Items)
                {
                    writer.Write(item.NetworkIdentity.assetId);
                    item.SaveData(writer);
                }
            }
        }

        public void LoadData(BinaryReader reader)
        {
            if (!NetworkServer.active)
                return;
            
            int length = reader.ReadInt32();
            if (length == 0)
                return;

            var itemStacks = new ItemStackSavedData[length];
            for (int i = 0; i < length; i++)
            {
                int maxStack = reader.ReadInt32();
                int stackCount = reader.ReadInt32();
                if (stackCount == 0)
                {
                    itemStacks[i] = new ItemStackSavedData(maxStack, null);
                    continue;
                }

                var itemsData = new uint[stackCount];
                for (int j = 0; j < stackCount; j++)
                {
                    uint assetId = reader.ReadUInt32();
                    var instanceIdentity = _factory.InstantiateAndSpawnServer(
                        new InstantiateAndSpawnCommand(
                            _factory.GetPrefabById(assetId),
                            transform.position,
                            Quaternion.identity),
                        new AddItemAfterSpawnCommand(_player.Identity.netId));
                    if (instanceIdentity.TryGetComponent(out BaseItem item))
                        item.LoadData(reader);

                    itemsData[j] = instanceIdentity.netId;
                }

                itemStacks[i] = new ItemStackSavedData(maxStack, itemsData);
            }
            
            _data = itemStacks;
            
            var networkWriter = new NetworkWriter();
            networkWriter.WriteInt(_data.Length);
            foreach (var itemStack in _data)
            {
                networkWriter.WriteInt(itemStack.MaxStack);
                networkWriter.WriteInt(itemStack.Items?.Length ?? 0);
                if (itemStack.Items != null && itemStack.Items.Length > 0)
                {
                    foreach (var itemId in itemStack.Items)
                    {
                        networkWriter.WriteUInt(itemId);
                    }
                        
                }
            }
            ApplyDataClientRpc(networkWriter.ToArray());
        }

        [ClientRpc]
        private void ApplyDataClientRpc(byte[] payload)
        {
            if (isServer)
                return;
            
            var networkReader = new NetworkReader(payload);
            int length = networkReader.ReadInt();
            _data = new ItemStackSavedData[length];
            for (int i = 0; i < length; i++)
            {
                int maxStack = networkReader.ReadInt();
                int stackCount = networkReader.ReadInt();
                if (stackCount == 0)
                {
                    _data[i] = new ItemStackSavedData(maxStack, null);
                    continue;
                }

                var items = new uint[stackCount];
                for (int j = 0; j < stackCount; j++)
                {
                    items[j] = networkReader.ReadUInt();
                }
                _data[i] = new ItemStackSavedData(maxStack, items);
            }

            StartCoroutine(ApplyDataRoutine());
        }

        private IEnumerator ApplyDataRoutine()
        {
            yield return null;
            
            var itemStacks = new ItemStack[_data.Length];
            for (int i = 0; i < itemStacks.Length; i++)
            {
                if (_data[i].Items == null || _data[i].Items.Length == 0)
                {
                    itemStacks[i] = new ItemStack();
                    continue;
                }

                var items = new List<BaseItem>(_data[i].Items.Length);
                var savedItems = _data[i].Items;
                for (int j = 0; j < savedItems.Length; j++)
                {
                    var item = NetworkObjectResolver.Resolve<BaseItem>(savedItems[j]);
                    while (item == null)
                    {
                        yield return null;
                        item = NetworkObjectResolver.Resolve<BaseItem>(savedItems[j]);
                    }
                    
                    items.Add(item);
                }

                itemStacks[i] = new ItemStack
                {
                    MaxStack = _data[i].MaxStack,
                    Items = items
                };

                yield return null;
            }
            
            _player.NetworkInventorySync.ApplyData(itemStacks);
            if (isOwned)
                CmdSyncItems();
        }

        [Command(requiresAuthority = false)]
        private void CmdSyncItems()
        {
            foreach (var itemStack in _data)
            {
                if (itemStack.Items == null || itemStack.Items.Length == 0)
                    continue;
                foreach (var itemId in itemStack.Items)
                {
                    var item = NetworkObjectResolver.Resolve<BaseItem>(itemId);
                    if (item == null)
                        continue;
                    var writer = new NetworkWriter();
                    item.OnSerialize(writer, true);
                    SyncItemDataClientRpc(itemId, writer.ToArray());
                }
            }
        }

        [ClientRpc]
        private void SyncItemDataClientRpc(uint itemId, byte[] payload)
        {
            var item = NetworkObjectResolver.Resolve<BaseItem>(itemId);
            if (item == null)
                return;

            var networkReader = new NetworkReader(payload);
            item.OnDeserialize(networkReader, true);

            //TODO: this is bad solution, rewrite later
            if (item is ITool tool)
            {
                var ev = new ToolDataChangedEvent(_player, item, tool);
                _eventBus.Publish(ev);
            }
        }
    }
}
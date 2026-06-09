using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Network.EventBus;
using Game.Scripts.Player.Control;
using Game.Scripts.Player.Control.Data;
using Game.Scripts.Player.Inventory.Items;
using Game.Scripts.Player.Inventory.Snapshots;
using Game.Scripts.Player.Network;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Player.Inventory
{
    [RequireComponent(typeof(PlayerMainDataComponents))]
    public class NetworkInventorySync : NetworkBehaviour
    {
        private PlayerMainDataComponents _player;

        private int _processing = 0;
        private ItemStack[] _lastCommited;

        public const float Timeout = 4f;

        private void Awake()
        {
            _player = GetComponent<PlayerMainDataComponents>();
        }

        public void ApplyData(ItemStack[] items)
        {
            if (_processing == 0)
                _lastCommited = _player.Inventory.GetAllCopy();
            
            _player.Inventory.ApplyData(items);

            var command = new ApplyInventoryDataCommand(CreateSnapshot(items));
            
            if (!NetworkServer.active)
            {
                _processing++;
                CmdApplyData(command);
            }
            else
            {
                ConfirmApplyingDataClientRpc(command);
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdApplyData(ApplyInventoryDataCommand command)
        {
            var snapshot = command.GetSnapshot();
            TryDeserializeSnapshotShuffling(snapshot, (success, data) =>
            {
                if (!success)
                {
                    CancelApplyingTargetRpc();
                    return;
                }
                
                _player.Inventory.ApplyData(data);
            
                ConfirmApplyingDataClientRpc(command);
            });
        }

        [TargetRpc]
        private void CancelApplyingTargetRpc()
        {
            _processing--;
            _player.Inventory.ApplyData(_lastCommited);
        }

        [ClientRpc]
        private void ConfirmApplyingDataClientRpc(ApplyInventoryDataCommand command)
        {
            if (_player.Identity.isOwned)
            {
                _processing--;
                return;
            }
            
            var snapshot = command.GetSnapshot();
            TryDeserializeSnapshotShuffling(snapshot, (success, data) =>
            {
                if (success)
                    _player.Inventory.ApplyData(data);
            });
        }

        private InventorySnapshot CreateSnapshot(ItemStack[] data)
        {
            var stacks = new ItemStackSnapshot[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Items == null)
                {
                    stacks[i] = new ItemStackSnapshot();
                    continue;
                }

                var ids = new uint[data[i].Items.Count];
                for (int j = 0; j < ids.Length; j++)
                {
                    ids[j] = data[i].Items[j].NetworkIdentity.netId;
                }

                stacks[i] = new ItemStackSnapshot(data[i].MaxStack, ids);
            }
            
            return new InventorySnapshot(stacks);
        }

        private void TryDeserializeSnapshotShuffling(
            InventorySnapshot snapshot,
            Action<bool, ItemStack[]> result)
        {
            StartCoroutine(TryDeserializeSnapshotRoutine(
                snapshot,
                result,
                item => _player.Inventory.Contains(item),
                0f));
        }

        private IEnumerator TryDeserializeSnapshotRoutine(
            InventorySnapshot snapshot,
            Action<bool, ItemStack[]> onFinish, 
            Predicate<BaseItem> validation,
            float waitItemSpawnTime)
        {
            var result = new ItemStack[snapshot.Items.Length];
            for (int i = 0; i < result.Length; i++)
            {
                var itemsSnapshot = snapshot.Items[i];
                if (itemsSnapshot.Items == null || itemsSnapshot.Items.Length == 0)
                {
                    result[i] = new ItemStack();
                    continue;
                }

                var items = new List<BaseItem>();
                foreach (var id in itemsSnapshot.Items)
                {
                    var item = NetworkObjectResolver.Resolve<BaseItem>(id);
                    if (item == null)
                    {
                        if (waitItemSpawnTime > 0f)
                        {
                            float time = waitItemSpawnTime;
                            while (time > 0f && item == null)
                            {
                                yield return null;
                                time -= Time.deltaTime;
                                item = NetworkObjectResolver.Resolve<BaseItem>(id);
                            }
                        }

                        if (item == null)
                        {
                            onFinish.Invoke(false, result);
                            yield break;
                        }
                    }

                    if (!validation.Invoke(item))
                    {
                        onFinish.Invoke(false, result);
                        yield break;
                    }
                    
                    items.Add(item);
                }

                result[i] = new ItemStack
                {
                    MaxStack = itemsSnapshot.MaxCount,
                    Items = items
                };
            }

            onFinish.Invoke(true, result);
        }

        public override void OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (!initialState)
                return;

            var snapshot = CreateSnapshot(_player.Inventory.GetAllCopy());
            snapshot.Serialize(writer);
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (!initialState)
                return;

            var snapshot = new InventorySnapshot();
            snapshot.Deserialize(reader);
            if (snapshot.Items == null || snapshot.Items.Length == 0)
                return;

            StartCoroutine(TryDeserializeSnapshotRoutine(
                snapshot,
                (success, data) =>
                {
                    if (!success)
                        return;
                    
                    foreach (var itemStack in data)
                    {
                        if (itemStack.Items != null && itemStack.Items.Count > 0)
                        {
                            foreach (var item in itemStack.Items)
                            {
                                if (item.OwnerPlayer == null)
                                    _player.Inventory.TryAdd(item);
                            }
                        }
                    }
                    
                    _player.Inventory.ApplyData(data);
                },
                _ => true,
                Timeout));
        }
    }
}
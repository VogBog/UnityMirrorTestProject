using Game.Scripts.GameSystems.Factories;
using Game.Scripts.GameSystems.Repositories;
using Game.Scripts.Player.Inventory.Events;
using Game.Scripts.Player.Inventory.Items;
using Game.Scripts.Player.Network;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Player.Inventory
{
    public class InventoryCommandsExecutor : MonoBehaviour
    {
        [Inject] private INetworkObjectFactory _factory;
        [Inject] private IPlayerRepository _playerRepository;

        private void OnEnable()
        {
            _factory.RegisterAfterSpawnHandler<AddItemAfterSpawnCommand>(OnAddCommandReceived);
            _factory.RegisterBeforeDestroyHandler<RemoveItemBeforeDestroyCommand>(OnRemoveCommandReceived);
        }

        private void OnDisable()
        {
            _factory.UnregisterAfterSpawnHandler<AddItemAfterSpawnCommand>();
            _factory.UnregisterBeforeDestroyHandler<RemoveItemBeforeDestroyCommand>();
        }

        private void OnRemoveCommandReceived(RemoveItemBeforeDestroyCommand command)
        {
            var inventory = _playerRepository.GetPlayerObject(command.PlayerId)?.Inventory;
            var item = NetworkObjectResolver.Resolve<BaseItem>(command.FactoryItemId);

            if (inventory != null && item != null)
                inventory.TryRemove(item);
        }

        private void OnAddCommandReceived(AddItemAfterSpawnCommand command)
        {
            var inventory = _playerRepository.GetPlayerObject(command.PlayerId)?.Inventory;
            var item = NetworkObjectResolver.Resolve<BaseItem>(command.FactoryItemId);
            
            if (inventory != null && item != null)
                inventory.TryAdd(item);
        }
    }
}
using System.Collections.Generic;
using Game.Scripts.GameSystems.GameStarting;
using Game.Scripts.GameSystems.Repositories;
using Game.Scripts.GameSystems.ToolUsingSystem;
using Game.Scripts.GameSystems.ToolUsingSystem.Data;
using Game.Scripts.Network.EventBus;
using Game.Scripts.Player;
using Game.Scripts.Player.Inventory;
using Game.Scripts.Player.Inventory.Events;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Ui.Inventory
{
    public class InventoryPresenter : MonoBehaviour, IInitializationWaiter
    {
        [SerializeField] private InventoryView _view;

        private IPlayerRepository _playerRepository;
        private IEventBus _eventBus;

        private bool _firstTime = true;

        public bool Initialized => _view.Initialized;

        [Inject]
        private void Construct(IPlayerRepository playerRepository, IEventBus eventBus, IGameStarter starter)
        {
            _playerRepository = playerRepository;
            _eventBus = eventBus;
            
            starter.AddToInitializationQueue(this);
        }

        private void OnEnable()
        {
            _eventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            _eventBus.Subscribe<ItemChosenEvent>(OnItemChosen);
            _eventBus.Subscribe<ToolDataChangedEvent>(OnToolDataChanged);
        }

        private void OnDisable()
        {
            _eventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            _eventBus.Unsubscribe<ItemChosenEvent>(OnItemChosen);
            _eventBus.Unsubscribe<ToolDataChangedEvent>(OnToolDataChanged);
        }
        
        private void OnInventoryChanged(InventoryChangedEvent ev) => OnInventoryChanged(ev.Player);

        private void OnItemChosen(ItemChosenEvent ev)
        {
            if (_playerRepository.IsMyPlayer(ev.Player.Identity))
                _view.SetChosen(ev.Index);
        }

        private void OnToolDataChanged(ToolDataChangedEvent ev)
        {
            if (!_playerRepository.IsMyPlayer(ev.PlayerComponents.Identity))
                return;
            
            if (ev.PlayerComponents.Inventory.ChosenObject != ev.Item)
                return;
            
            _view.SetSlider(
                ev.PlayerComponents.Inventory.ChosenIndex,
                ev.Tool.Durability,
                ev.Tool.MaxDurability);
        }

        private void OnInventoryChanged(PlayerMainDataComponents player)
        {
            if (!_playerRepository.IsMyPlayer(player.Identity))
                return;
            
            OnInventoryChanged(player.Inventory);

            if (_firstTime)
            {
                _firstTime = false;
                _view.SetChosen(player.Inventory.ChosenIndex);
            }
        }

        private void OnInventoryChanged(IInventory inventory)
        {
            var list = new List<InventorySlotData>();
            inventory.ForEach(item =>
            {
                list.Add(ProcessItem(item));
            });
            
            _view.SetItems(list);
        }

        private InventorySlotData ProcessItem(ItemStack itemStack)
        {
            if (itemStack.Items == null)
                return new InventorySlotData(null);
            
            var slotData = new InventorySlotData(itemStack.Items[0].BaseItemData.Icon);
            
            if (itemStack.Items.Count > 1)
                slotData.SetStackCount(itemStack.Items.Count);

            if (itemStack.Items[0] is ITool tool)
            {
                Debug.Log($"Set tool {tool.Durability}/{tool.MaxDurability}");
                slotData.SetSliderValue(
                    tool.Durability,
                    tool.MaxDurability);
            }

            return slotData;
        }
    }
}
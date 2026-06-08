using System.Collections.Generic;
using Game.Scripts.GameSystems.GameStarting;
using Game.Scripts.GameSystems.Repositories;
using Game.Scripts.GameSystems.ToolUsingSystem;
using Game.Scripts.Network.EventBus;
using Game.Scripts.Player.Inventory;
using Game.Scripts.Player.Inventory.Events;
using Game.Scripts.Ui.Inventory;
using Game.Scripts.Ui.MenusSystem;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Ui.InventoryWindow
{
    public class InventoryWindowPresenter : MonoBehaviour, IMenuPage, IInitializationWaiter
    {
        [SerializeField] private InventoryWindowView _view;
        
        private IPlayerRepository _playerRepository;
        private IEventBus _eventBus;

        private ItemStack[] _items;
        private bool _opened;

        public bool Initialized => _view.Initialized;

        [Inject]
        private void Construct(IPlayerRepository playerRepository, IEventBus eventBus, IGameStarter starter)
        {
            _playerRepository = playerRepository;
            _eventBus = eventBus;
            
            starter.AddToInitializationQueue(this);
        }

        private void Awake()
        {
            _view.Show();
        }

        private void OnEnable()
        {
            _view.Dropped += OnDropped;
        }

        private void OnDisable()
        {
            _view.Dropped -= OnDropped;
        }

        public bool Open()
        {
            var player = _playerRepository.MyPlayer;
            if (player == null)
                return false;

            _eventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            _items = player.Inventory.GetAllCopy();
            Repaint();
            
            return true;
        }

        public void Close()
        {
            _view.Close();
            _eventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            
            var player = _playerRepository.MyPlayer;
            if (player == null)
                return;
            
            player.NetworkInventorySync.ApplyData(_items);
        }

        private void Repaint()
        {
            var slots = new List<InventorySlotData>(IInventory.Capacity);
            foreach (var stack in _items)
            {
                slots.Add(ProcessStack(stack));
            }
            
            _view.Show(slots);
        }

        private void OnDropped(InventoryDropSlot drop, int dropIndex, InventoryDragSlot drag, int dragIndex)
        {
            if (dropIndex == dragIndex)
                return;

            var dropStack = _items[dropIndex];
            var dragStack = _items[dragIndex];

            _items[dropIndex] = dragStack;
            _items[dragIndex] = dropStack;
            
            _view.Set(dropIndex, ProcessStack(dragStack));
            _view.Set(dragIndex, ProcessStack(dropStack));
        }

        private InventorySlotData ProcessStack(ItemStack stack)
        {
            if (stack.Items == null || stack.Items.Count == 0)
            {
                return new InventorySlotData();
            }

            var slot = new InventorySlotData(stack.Items[0].BaseItemData.Icon);
            if (stack.Items.Count > 1)
                slot.SetStackCount(stack.Items.Count);
            if (stack.Items[0] is ITool tool)
                slot.SetSliderValue(tool.Durability, tool.MaxDurability);

            return slot;
        }

        private void OnInventoryChanged(InventoryChangedEvent ev)
        {
            _items = ev.Player.Inventory.GetAllCopy();
            Repaint();
        }
    }
}
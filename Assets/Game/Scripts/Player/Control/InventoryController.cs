using System;
using Game.Scripts.Player.Inventory;
using Game.Scripts.Player.Inventory.Items;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Player.Control
{
    [RequireComponent(typeof(Inventory.Inventory))]
    public class InventoryController : MonoBehaviour
    {
        private IPlayerControls _playerControls;
        private IInventory _inventory;

        public event Action<int, BaseItem> Dropped;
        public event Action<int, BaseItem> Chosen;

        [Inject]
        private void Construct(IPlayerControls playerControls)
        {
            _playerControls = playerControls;
            
            _inventory = GetComponent<Inventory.Inventory>();
        }

        private void OnEnable()
        {
            _playerControls.Dropped += OnDrop;
            _playerControls.NumPressed += OnNumPressed;
            _playerControls.Scrolled += OnScrolled;
        }

        private void OnDisable()
        {
            _playerControls.Dropped -= OnDrop;
            _playerControls.NumPressed -= OnNumPressed;
            _playerControls.Scrolled -= OnScrolled;
        }

        private void OnDrop()
        {
            var item = _inventory.ChosenObject;
            if (_inventory.TryRemoveAt(_inventory.ChosenIndex))
                Dropped?.Invoke(_inventory.ChosenIndex, item);
        }

        private void OnNumPressed(int value)
        {
            _inventory.ChooseAt(value - 1);
            Chosen?.Invoke(value - 1, _inventory.ChosenObject);
        }

        private void OnScrolled(int scroll)
        {
            int index = _inventory.ChosenIndex - scroll;
            _inventory.ChooseAt(index);
            Chosen?.Invoke(index, _inventory.ChosenObject);
        }
    }
}
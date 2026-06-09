using Game.Scripts.Player.Inventory;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Player.Control
{
    [RequireComponent(typeof(NetworkInventoryActionsSync))]
    public class InventoryController : MonoBehaviour
    {
        private IPlayerControls _playerControls;
        private NetworkInventoryActionsSync _inventory;

        [Inject]
        private void Construct(IPlayerControls playerControls)
        {
            _playerControls = playerControls;
            
            _inventory = GetComponent<NetworkInventoryActionsSync>();
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
            if (item != null)
                _inventory.DropItem(item);
        }

        private void OnNumPressed(int value)
        {
            _inventory.ChooseAt(value);
        }

        private void OnScrolled(int scroll)
        {
            int index = _inventory.ChosenIndex - scroll;
            _inventory.ChooseAt(index);
        }
    }
}
using Game.Scripts.Ui.InventoryWindow;
using Game.Scripts.Ui.MenusSystem;
using Game.Scripts.Ui.PauseMenu;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Player.Control
{
    public class MenusController : MonoBehaviour
    {
        [Inject] private IMenusSystem _menus;
        [Inject] private IPlayerControls _controls;
        
        [SerializeField] private InventoryWindowPresenter _inventoryWindow;
        [SerializeField] private PauseMenu _pauseMenu;

        private void OnEnable()
        {
            _controls.OpenedInventory += OnOpenInventoryPressed;
            _controls.PausePressed += OnPausePressed;
        }

        private void OnDisable()
        {
            _controls.OpenedInventory -= OnOpenInventoryPressed;
            _controls.PausePressed -= OnPausePressed;
        }

        private void OnOpenInventoryPressed()
        {
            if (ReferenceEquals(_menus.ActivePage, _inventoryWindow))
                _menus.Close();
            else
                _menus.TryOpen(_inventoryWindow);
        }

        private void OnPausePressed()
        {
            if (_menus.ActivePage != null)
                _menus.Close();
            else
                _menus.TryOpen(_pauseMenu);
        }
    }
}
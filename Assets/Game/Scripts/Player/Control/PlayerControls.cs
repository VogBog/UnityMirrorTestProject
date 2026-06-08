using System;
using Game.Scripts.Ui.MenusSystem;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Player.Control
{
    public class PlayerControls : MonoBehaviour, IPlayerControls
    {
        [Inject] private IMenusSystem _menus;
        
        private InputSystemActions _actions;

        private bool _jumped = false;

        public Vector2 MoveVector => _actions.Player.Move.ReadValue<Vector2>();

        public event Action JumpedFixedUpdate;
        public event Action<Vector2> Looking;
        public event Action Dropped;
        public event Action Interacted;
        public event Action StartUsingItem;
        public event Action EndUsingItem;

        public event Action<int> NumPressed;
        public event Action<int> Scrolled; 
        public event Action OpenedInventory;
        public event Action PausePressed;

        private void Awake()
        {
            _actions = new InputSystemActions();
            _actions.Player.Jump.started += _ => _jumped = true;
            _actions.Player.Look.started += ctx => Looking?.Invoke(ctx.ReadValue<Vector2>());
            _actions.Player.Drop.started += _ => Dropped?.Invoke();
            _actions.Player.Interact.started += _ => Interacted?.Invoke();
            _actions.Player.Attack.started += _ => StartUsingItem?.Invoke();
            _actions.Player.Attack.canceled += _ => EndUsingItem?.Invoke();
            
            //Wtf is going on :O
            _actions.UI.Num1.started += _ => NumPressed?.Invoke(1);
            _actions.UI.Num2.started += _ => NumPressed?.Invoke(2);
            _actions.UI.Num3.started += _ => NumPressed?.Invoke(3);
            _actions.UI.Num4.started += _ => NumPressed?.Invoke(4);
            _actions.UI.Num5.started += _ => NumPressed?.Invoke(5);
            _actions.UI.Num6.started += _ => NumPressed?.Invoke(6);
            _actions.UI.Num7.started += _ => NumPressed?.Invoke(7);
            _actions.UI.Num8.started += _ => NumPressed?.Invoke(8);
            _actions.UI.Num9.started += _ => NumPressed?.Invoke(9);

            _actions.UI.WheelScroll.performed += ctx => Scrolled?.Invoke((int)ctx.ReadValue<Vector2>().y);
            _actions.UI.OpenInventory.started += _ => OpenedInventory?.Invoke();
            _actions.UI.OpenPause.started += _ => PausePressed?.Invoke();
        }

        private void OnEnable()
        {
            _actions.Enable();
            _menus.Opened += OnMenuOpened;
            _menus.Closed += OnMenuClosed;
        }

        private void OnDisable()
        {
            _actions.Disable();
            _menus.Opened -= OnMenuOpened;
            _menus.Closed -= OnMenuClosed;
        }

        private void OnDestroy()
        {
            _actions.Dispose();
        }

        private void FixedUpdate()
        {
            if (_jumped)
            {
                _jumped = false;
                JumpedFixedUpdate?.Invoke();
            }
        }

        private void OnMenuOpened()
        {
            _actions.Player.Disable();
        }

        private void OnMenuClosed()
        {
            _actions.Player.Enable();
        }
    }
}
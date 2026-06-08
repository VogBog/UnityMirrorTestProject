using Game.Scripts.GameSystems.ToolUsingSystem;
using Game.Scripts.GameSystems.ToolUsingSystem.Data;
using Game.Scripts.Player.Control;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Player.Inventory.ItemUsage
{
    [RequireComponent(
        typeof(PlayerMainDataComponents))]
    public class PlayerItemUserController : MonoBehaviour
    {
        [Inject] private IPlayerControls _playerControls;
        [Inject] private IToolUsingSystem _toolUsingSystem;
        
        private PlayerMainDataComponents _components;

        private bool _usingTool = false;

        private void Awake()
        {
            _components = GetComponent<PlayerMainDataComponents>();
        }

        private void OnEnable()
        {
            _playerControls.StartUsingItem += OnStartUsingItem;
            _playerControls.EndUsingItem += OnStopUsingItem;
        }

        private void OnDisable()
        {
            _playerControls.StartUsingItem -= OnStartUsingItem;
            _playerControls.EndUsingItem -= OnStopUsingItem;
            
            if (_toolUsingSystem.IsPlayerUsingTool(_components.Identity.netId))
                OnStopUsingItem();
        }

        private void OnStartUsingItem()
        {
            var item = _components.Inventory.ChosenObject;
            if (item == null || item is not ITool tool)
                return;

            var command = new StartUsingToolCommand(
                _components,
                item,
                tool,
                _components.Eyes.transform.forward);

            _usingTool = _toolUsingSystem.TryStartUsingTool(command);
        }

        private void OnStopUsingItem()
        {
            if (!_usingTool)
                return;

            _toolUsingSystem.StopUsingTool(_components.Identity.netId);
        }
    }
}
using Game.Scripts.GameSystems.Factories;
using Game.Scripts.GameSystems.Repositories;
using Game.Scripts.GameSystems.ToolUsingSystem.Data;
using Game.Scripts.Network.EventBus;
using Game.Scripts.Player.Control;
using Game.Scripts.Player.Control.Data;
using Game.Scripts.Player.Inventory.Events;
using Game.Scripts.Player.Inventory.Items;
using Game.Scripts.Player.Network;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.GameSystems.ToolUsingSystem
{
    [RequireComponent(typeof(ToolUsingSystem))]
    public class NetworkToolUsingSystem : MonoBehaviour, IToolUsingSystem
    {
        [Inject] private INetworkServerPublishing _serverPublishing;
        [Inject] private IPlayerActionCommandHandler _playerActions;
        [Inject] private IPlayerRepository _playerRepository;
        [Inject] private INetworkObjectFactory _factory;
        [Inject] private IEventBus _eventBus;
        
        private ToolUsingSystem _toolUsingSystem;

        private void Awake()
        {
            _toolUsingSystem = GetComponent<ToolUsingSystem>();
            _playerActions.RegisterHandler(OnUsingToolNetworkCommandReceived, () => new UsingToolNetworkCommand());
        }

        private void OnDestroy()
        {
            _playerActions.UnregisterHandler<UsingToolNetworkCommand>();
        }

        public bool IsPlayerUsingTool(uint playerId) => _toolUsingSystem.IsPlayerUsingTool(playerId);

        public bool TryStartUsingTool(StartUsingToolCommand command)
        {
            if (command.ToolUsingSystem == null)
                command.ToolUsingSystem = this;

            if (NetworkServer.active)
                command.Type = UsingToolCommandType.ToServer;
            else
                command.Type = UsingToolCommandType.ClientPrediction;
            
            if (!_toolUsingSystem.TryStartUsingTool(command))
                return false;

            var networkCommand = new UsingToolNetworkCommand(
                command.Player.Identity.netId,
                command.Item.NetworkIdentity.netId,
                command.LookDirection,
                true,
                0);
            
            if (NetworkServer.active)
            {
                networkCommand.Type = (int)UsingToolCommandType.Confirm;
                _serverPublishing.SendToPlayersExcludeServer(PlayerActionCommand.Create(networkCommand));
            }
            else
            {
                networkCommand.Type = (int)UsingToolCommandType.ToServer;
                NetworkClient.Send(PlayerActionCommand.Create(networkCommand));
            }

            return true;
        }

        public bool StopUsingTool(uint playerId, UsingToolCommandType type = UsingToolCommandType.ToServer)
        {
            if (type is UsingToolCommandType.Confirm)
            {
                _toolUsingSystem.StopUsingTool(playerId, type);
                return true;
            }
            
            if (NetworkServer.active)
                type = UsingToolCommandType.ToServer;
            else 
                type = UsingToolCommandType.ClientPrediction;

            _toolUsingSystem.TryGetCommandByPlayerId(playerId, out var startCommand);
            if (!_toolUsingSystem.StopUsingTool(playerId, type))
                return false;
            
            var networkCommand = new UsingToolNetworkCommand(
                playerId,
                startCommand.Item.NetworkIdentity.netId,
                Vector3.zero,
                false,
                0);

            if (NetworkServer.active)
            {
                networkCommand.Type = (int)UsingToolCommandType.Confirm;
                _serverPublishing.SendToPlayersExcludeServer(PlayerActionCommand.Create(networkCommand));
            }
            else
            {
                networkCommand.Type = (int)UsingToolCommandType.ToServer;
                NetworkClient.Send(PlayerActionCommand.Create(networkCommand));
            }

            return true;
        }

        public void StopUsingToolCauseOfCancel(StartUsingToolCommand cause)
        {
            _toolUsingSystem.StopUsingToolCauseOfCancel(cause);
            
            if (cause.Type is not UsingToolCommandType.ToServer)
            {
                if (cause.Tool.Durability <= 0)
                {
                    cause.Player.Inventory.TryRemove(cause.Item);
                    cause.Item.gameObject.SetActive(false);
                }
                
                return;
            }

            if (!NetworkServer.active)
                return;
            
            var networkCommand = new UsingToolNetworkCommand(
                cause.Player.Identity.netId,
                cause.Tool.Durability > 0 ? cause.Item.NetworkIdentity.netId : 0,
                cause.LookDirection,
                false,
                (int)UsingToolCommandType.Confirm);
            
            _serverPublishing.SendToPlayersExcludeServer(PlayerActionCommand.Create(networkCommand));
            
            if (cause.Tool.Durability <= 0)
            {
                _factory.DespawnAndDestroyServerWithEvent(
                    cause.Item.gameObject, new RemoveItemBeforeDestroyCommand(cause.Player.Identity.netId));
            }
        }

        private void OnUsingToolNetworkCommandReceived(UsingToolNetworkCommand command)
        {
            var type = (UsingToolCommandType)command.Type;
            if (type is UsingToolCommandType.ToServer && NetworkServer.active)
            {
                if (command.IsUsing)
                    StartUsingToolServerCommand(command);
                else 
                    StopUsingToolServerCommand(command);
            }
            else if (type is UsingToolCommandType.Confirm && NetworkClient.active)
            {
                if (command.IsUsing)
                    StartUsingToolConfirm(command);
                else 
                    StopUsingToolConfirm(command);
            }
            else if (type is UsingToolCommandType.Cancel && NetworkClient.active)
            {
                CancelUsingTool(command);
            }
        }

        private void StartUsingToolServerCommand(UsingToolNetworkCommand networkCommand)
        {
            var command = ConvertCommandToLocal(networkCommand);
            
            if (!_toolUsingSystem.TryStartUsingTool(command))
            {
                networkCommand.Type = (int)UsingToolCommandType.Cancel;
                _serverPublishing.SendToTargetPlayer(
                    PlayerActionCommand.Create(networkCommand), networkCommand.PlayerId);
                return;
            }

            networkCommand.Type = (int)UsingToolCommandType.Confirm;
            _serverPublishing.SendToPlayersExcludeOne(
                PlayerActionCommand.Create(networkCommand), networkCommand.PlayerId, true);
        }
        
        private void StartUsingToolConfirm(UsingToolNetworkCommand networkCommand)
        {
            var command = ConvertCommandToLocal(networkCommand);
            _toolUsingSystem.TryStartUsingTool(command);
        }
        
        private void CancelUsingTool(UsingToolNetworkCommand networkCommand)
        {
            var command = ConvertCommandToLocal(networkCommand);
            _toolUsingSystem.StopUsingTool(networkCommand.PlayerId, UsingToolCommandType.Cancel);
        }

        private void StopUsingToolServerCommand(UsingToolNetworkCommand networkCommand)
        {
            if (!_toolUsingSystem.StopUsingTool(networkCommand.PlayerId, UsingToolCommandType.ToServer))
                return;

            var player = _playerRepository.GetPlayerObject(networkCommand.PlayerId);
            var tool = NetworkObjectResolver.Resolve<ITool>(networkCommand.ItemId);
            
            networkCommand.Type = (int)UsingToolCommandType.Confirm;
            _serverPublishing.SendToPlayersExcludeServer(PlayerActionCommand.Create(networkCommand));
        }

        private void StopUsingToolConfirm(UsingToolNetworkCommand networkCommand)
        {
            if (!_playerRepository.IsMyPlayer(networkCommand.PlayerId))
            {
                _toolUsingSystem.StopUsingTool(networkCommand.PlayerId, UsingToolCommandType.Confirm);
            }
        }

        private StartUsingToolCommand ConvertCommandToLocal(UsingToolNetworkCommand networkCommand)
        {
            var player = _playerRepository.GetPlayerObject(networkCommand.PlayerId);
            var item = NetworkObjectResolver.Resolve<BaseItem>(networkCommand.ItemId);
            var type = (UsingToolCommandType)networkCommand.Type;

            return new StartUsingToolCommand(
                player,
                item,
                item != null ? item as ITool : null,
                networkCommand.LookDirection,
                type,
                this);
        }
    }
}
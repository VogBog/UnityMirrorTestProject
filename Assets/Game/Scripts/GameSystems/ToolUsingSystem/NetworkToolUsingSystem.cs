using Game.Scripts.GameSystems.Factories;
using Game.Scripts.GameSystems.Repositories;
using Game.Scripts.GameSystems.ToolUsingSystem.Data;
using Game.Scripts.Network.EventBus;
using Game.Scripts.Network.PayloadTransfer;
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
        [Inject] private IPlayerRepository _playerRepository;
        [Inject] private INetworkObjectFactory _factory;
        [Inject] private IEventBus _eventBus;
        
        private ToolUsingSystem _toolUsingSystem;

        private void Awake()
        {
            _toolUsingSystem = GetComponent<ToolUsingSystem>();
            if (NetworkServer.active)
                NetworkServer.RegisterHandler<UsingToolNetworkCommand>(CmdUseTool, false);
            else 
                NetworkClient.RegisterHandler<UsingToolNetworkCommand>(UseToolClient, false);
        }

        private void OnDestroy()
        {
            if (NetworkServer.active)
                NetworkServer.UnregisterHandler<UsingToolNetworkCommand>();
            else
                NetworkClient.UnregisterHandler<UsingToolNetworkCommand>();
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
                NetworkServerCommands.SendToOtherClients(networkCommand);
            }
            else
            {
                networkCommand.Type = (int)UsingToolCommandType.ToServer;
                NetworkClient.Send(networkCommand);
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
                NetworkServerCommands.SendToOtherClients(networkCommand);
            }
            else
            {
                networkCommand.Type = (int)UsingToolCommandType.ToServer;
                NetworkClient.Send(networkCommand);
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
            
            NetworkServerCommands.SendToOtherClients(networkCommand);
            
            if (cause.Tool.Durability <= 0)
            {
                cause.Player.Inventory.TryRemove(cause.Item);
                _factory.DespawnAndDestroyServer(cause.Item.gameObject);
            }
        }

        [Server]
        private void CmdUseTool(NetworkConnectionToClient connectionToClient, UsingToolNetworkCommand command)
        {
            var type = (UsingToolCommandType)command.Type;
            if (type is not UsingToolCommandType.ToServer)
                return;
            
            var player = _playerRepository.GetPlayerObjectByConnection(connectionToClient);
            command.PlayerId = player.Identity.netId;

            if (command.IsUsing)
                StartUsingToolServerCommand(connectionToClient, command);
            else 
                StopUsingToolServerCommand(command);
        }

        [Client]
        private void UseToolClient(UsingToolNetworkCommand command)
        {
            var type = (UsingToolCommandType)command.Type;
            if (type is UsingToolCommandType.Confirm)
            {
                if (command.IsUsing)
                    StartUsingToolConfirm(command);
                else 
                    StopUsingToolConfirm(command);
            }
            else if (type is UsingToolCommandType.Cancel)
            {
                CancelUsingTool(command);
            }
        }

        private void StartUsingToolServerCommand(
            NetworkConnectionToClient connection, UsingToolNetworkCommand networkCommand)
        {
            var command = ConvertCommandToLocal(networkCommand);
            
            if (!_toolUsingSystem.TryStartUsingTool(command))
            {
                networkCommand.Type = (int)UsingToolCommandType.Cancel;
                connection.Send(networkCommand);
                return;
            }

            networkCommand.Type = (int)UsingToolCommandType.Confirm;
            NetworkServerCommands.SendToOtherClients(networkCommand, connection);
        }
        
        private void StartUsingToolConfirm(UsingToolNetworkCommand networkCommand)
        {
            var command = ConvertCommandToLocal(networkCommand);
            _toolUsingSystem.TryStartUsingTool(command);
        }
        
        private void CancelUsingTool(UsingToolNetworkCommand networkCommand)
        {
            _toolUsingSystem.StopUsingTool(networkCommand.PlayerId, UsingToolCommandType.Cancel);
        }

        private void StopUsingToolServerCommand(UsingToolNetworkCommand networkCommand)
        {
            if (!_toolUsingSystem.StopUsingTool(networkCommand.PlayerId, UsingToolCommandType.ToServer))
                return;
            
            networkCommand.Type = (int)UsingToolCommandType.Confirm;
            NetworkServerCommands.SendToOtherClients(networkCommand);
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
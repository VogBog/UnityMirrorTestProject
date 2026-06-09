using System.Collections;
using System.Collections.Generic;
using Game.Scripts.GameSystems.ToolUsingSystem.Data;
using Game.Scripts.Network.EventBus;
using UnityEngine;
using Zenject;

namespace Game.Scripts.GameSystems.ToolUsingSystem
{
    public class ToolUsingSystem : MonoBehaviour, IToolUsingSystem
    {
        [Inject] private IEventBus _eventBus;
        
        private readonly Dictionary<uint, (StartUsingToolCommand, Coroutine)> _activeCommands = new();

        public bool TryGetCommandByPlayerId(uint playerId, out StartUsingToolCommand command)
        {
            if (_activeCommands.TryGetValue(playerId, out var handle))
            {
                command = handle.Item1;
                return true;
            }

            command = default;
            return false;
        }

        public bool IsPlayerUsingTool(uint playerId) => _activeCommands.ContainsKey(playerId);
        
        public bool TryStartUsingTool(StartUsingToolCommand command)
        {
            if (command.ToolUsingSystem == null)
                command.ToolUsingSystem = this;

            if (command.Type is UsingToolCommandType.Cancel)
            {
                StopUsingTool(command.Player.Identity.netId, command.Type);
                return true;
            }
            
            if (!CanUse(command) || command.Tool.InUse)
                return false;

            if (_activeCommands.ContainsKey(command.Player.Identity.netId))
                return false;

            if (command.Player.Inventory.ChosenObject != command.Item)
            {
                command.Player.Inventory.ChooseAtLocal(command.Player.Inventory.GetIndex(command.Item));
            }

            var coroutine = StartCoroutine(UsingToolRoutine(command));
            _activeCommands.Add(command.Player.Identity.netId, (command, coroutine));

            var useToolCommand = new UseToolCommand(
                command.Player, command.LookDirection, command.ToolUsingSystem, command.Type);
            command.Tool.InUse = true;
            command.Tool.OnStartUsing(useToolCommand);
            
            return true;
        }

        public bool StopUsingTool(uint playerId, UsingToolCommandType type = UsingToolCommandType.ToServer)
        {
            if (!_activeCommands.Remove(playerId, out var active))
                return false;

            var (command, coroutine) = active;
            if (coroutine != null)
                StopCoroutine(coroutine);

            var useToolCommand = new UseToolCommand(
                command.Player, command.LookDirection, command.ToolUsingSystem, type);
            command.Tool.InUse = false;
            command.Tool.OnStopUsing(useToolCommand);

            return true;
        }

        public void StopUsingToolCauseOfCancel(StartUsingToolCommand cause)
            => StopUsingTool(cause.Player.Identity.netId, cause.Type);

        private bool CanUse(StartUsingToolCommand command) =>
            command.Player.Inventory.Contains(command.Item) &&
            command.Tool.CanUse && command.Tool.Durability > 0 &&
            command.Player.Characteristics.Stamina >= command.Tool.UseStamina;

        private IEnumerator UsingToolRoutine(StartUsingToolCommand command)
        {
            while (CanUse(command))
            {
                yield return new WaitForSeconds(command.Tool.UsingTime);

                if (!CanUse(command) || command.Player.Inventory.ChosenObject != command.Item)
                {
                    command.ToolUsingSystem?.StopUsingToolCauseOfCancel(command);
                    yield break;
                }

                var useToolCommand = new UseToolCommand(
                    command.Player, command.LookDirection, command.ToolUsingSystem, command.Type);
                
                if (!command.Tool.ActivateUsingEffect(useToolCommand))
                    continue;

                if (command.Type is UsingToolCommandType.ToServer or UsingToolCommandType.ClientPrediction)
                {
                    command.Tool.Durability--;
                    if (command.Tool.UseStamina > 0f)
                        command.Player.Characteristics.DecreaseStamina(command.Tool.UseStamina);
                }
            }
            
            command.ToolUsingSystem?.StopUsingToolCauseOfCancel(command);
        }
    }
}
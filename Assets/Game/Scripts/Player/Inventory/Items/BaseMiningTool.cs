using Game.Scripts.GameSystems.Mining;
using Game.Scripts.GameSystems.ToolUsingSystem.Data;
using Zenject;

namespace Game.Scripts.Player.Inventory.Items
{
    public abstract class BaseMiningTool : BaseTool
    {
        [Inject] private IMiningSystem _miningSystem;

        public override void ActivateUsingEffect(UseToolCommand command)
        {
            if (command.Type is UsingToolCommandType.ToServer &&
                NetworkIdentity.isServer &&
                _miningSystem.TryMine(command.Player.Eyes.transform.position, command.LookDirection, out var ore))
            {
                _miningSystem.MineOre(command.Player.Identity.netId, ore);
            }
        }
    }
}
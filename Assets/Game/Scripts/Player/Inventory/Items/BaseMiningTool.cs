using Game.Scripts.GameSystems.Mining;
using Game.Scripts.GameSystems.ToolUsingSystem.Data;
using Zenject;

namespace Game.Scripts.Player.Inventory.Items
{
    public abstract class BaseMiningTool : BaseTool
    {
        [Inject] private IMiningSystem _miningSystem;

        public override bool ActivateUsingEffect(UseToolCommand command)
        {
            if (_miningSystem.TryMine(command.Player.Eyes.transform.position, command.LookDirection, out var ore))
            {
                if (command.Type is UsingToolCommandType.ToServer &&
                    NetworkIdentity.isServer)
                {
                    _miningSystem.MineOre(command.Player.Identity.netId, ore);
                }

                return true;
            }

            return false;
        }
    }
}
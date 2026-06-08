using Game.Scripts.GameSystems.ToolUsingSystem.Data;
using Game.Scripts.Player.Inventory.Items.ScriptableData;
using UnityEngine;

namespace Game.Scripts.Player.Inventory.Items
{
    public class StaminaIncreasingTool : BaseTool
    {
        [SerializeField] private StaminaIncreasingData _data;

        private PlayerCharacteristics _characteristics;

        public override ToolScriptableData ToolInitData => _data;
        public override BaseItemData BaseItemData => _data;

        public override void OnStartUsing(UseToolCommand command)
        {
            base.OnStartUsing(command);
            if (command.Type is not UsingToolCommandType.Cancel)
                _characteristics = command.Player.Characteristics;
        }

        public override bool ActivateUsingEffect(UseToolCommand command)
        {
            if (command.Type is UsingToolCommandType.ToServer or UsingToolCommandType.ClientPrediction)
            {
                _characteristics?.IncreaseStamina(_data.AddStamina);
                return true;
            }

            return false;
        }
    }
}
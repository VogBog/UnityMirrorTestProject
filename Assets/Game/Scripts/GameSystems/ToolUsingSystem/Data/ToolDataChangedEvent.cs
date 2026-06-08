using Game.Scripts.Player;
using Game.Scripts.Player.Inventory.Items;

namespace Game.Scripts.GameSystems.ToolUsingSystem.Data
{
    public struct ToolDataChangedEvent
    {
        public PlayerMainDataComponents PlayerComponents;
        public BaseItem Item;
        public ITool Tool;

        public ToolDataChangedEvent(PlayerMainDataComponents components, BaseItem item, ITool tool)
        {
            PlayerComponents = components;
            Item = item;
            Tool = tool;
        }
    }
}
using Game.Scripts.Player;
using Game.Scripts.Player.Inventory.Items;
using JetBrains.Annotations;

namespace Game.Scripts.GameSystems.ToolUsingSystem.Data
{
    public struct ToolDataChangedEvent
    {
        [CanBeNull] public PlayerMainDataComponents PlayerComponents;
        public BaseItem Item;
        public ITool Tool;

        public ToolDataChangedEvent([CanBeNull] PlayerMainDataComponents components, BaseItem item, ITool tool)
        {
            PlayerComponents = components;
            Item = item;
            Tool = tool;
        }
    }
}
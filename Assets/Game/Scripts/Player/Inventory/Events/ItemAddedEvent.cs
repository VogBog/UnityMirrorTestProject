using Game.Scripts.Player.Inventory.Items;

namespace Game.Scripts.Player.Inventory.Events
{
    public struct ItemAddedEvent
    {
        public PlayerMainDataComponents Player;
        public BaseItem Item;
        public int Index;

        public ItemAddedEvent(PlayerMainDataComponents player, BaseItem item, int index)
        {
            Player = player;
            Item = item;
            Index = index;
        }
    }
}
using Game.Scripts.Player.Inventory.Items;
using Mirror;

namespace Game.Scripts.Player.Inventory.Events
{
    public struct ItemRemovedEvent
    {
        public PlayerMainDataComponents Player;
        public BaseItem Item;
        public int Index;

        public ItemRemovedEvent(PlayerMainDataComponents player, BaseItem item, int index)
        {
            Player = player;
            Item = item;
            Index = index;
        }
    }
}
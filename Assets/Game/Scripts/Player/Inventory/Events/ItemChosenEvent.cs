using Game.Scripts.Player.Inventory.Items;
using JetBrains.Annotations;
using Mirror;

namespace Game.Scripts.Player.Inventory.Events
{
    public struct ItemChosenEvent
    {
        public PlayerMainDataComponents Player;
        [CanBeNull] public BaseItem Item;
        public int Index;

        public ItemChosenEvent(PlayerMainDataComponents player, BaseItem item, int index)
        {
            Player = player;
            Item = item;
            Index = index;
        }
    }
}
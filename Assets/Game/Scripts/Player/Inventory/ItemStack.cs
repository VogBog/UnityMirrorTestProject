using System.Collections.Generic;
using Game.Scripts.Player.Inventory.Items;

namespace Game.Scripts.Player.Inventory
{
    public struct ItemStack
    {
        public List<BaseItem> Items;
        public int MaxStack;

        public ItemStack(BaseItem item)
        {
            Items = new List<BaseItem>
            {
                item
            };

            MaxStack = item.BaseItemData.MaxStack;
        }
    }
}
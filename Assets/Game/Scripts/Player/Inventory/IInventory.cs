using System;
using Game.Scripts.Player.Inventory.Items;

namespace Game.Scripts.Player.Inventory
{
    public interface IInventory
    {
        const int Capacity = 9;
        
        int ChosenIndex { get; }
        BaseItem ChosenObject { get; }
        int Count { get; }
        
        bool TryAdd(BaseItem item);
        bool TryInsert(int index, BaseItem item);
        bool TryRemove(BaseItem item);
        bool TryRemoveAt(int index);
        void ChooseAt(int index);
        ItemStack GetAt(int index);
        int GetIndex(BaseItem item);
        ItemStack[] GetAllCopy();
        void ForEach(Action<ItemStack> action);
        bool Contains(BaseItem item);
        void ApplyData(ItemStack[] data);
    }
}
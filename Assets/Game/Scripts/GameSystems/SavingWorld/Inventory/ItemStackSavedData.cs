namespace Game.Scripts.GameSystems.SavingWorld.Inventory
{
    public struct ItemStackSavedData
    {
        public int MaxStack;
        public uint[] Items;

        public ItemStackSavedData(int maxStack, uint[] items)
        {
            MaxStack = maxStack;
            Items = items;
        }
    }
}
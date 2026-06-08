namespace Game.Scripts.Player.Inventory.Events
{
    public struct InventoryChangedEvent
    {
        public PlayerMainDataComponents Player;

        public InventoryChangedEvent(PlayerMainDataComponents player)
        {
            Player = player;
        }
    }
}
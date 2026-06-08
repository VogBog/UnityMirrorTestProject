using Game.Scripts.GameSystems.Factories.Data;

namespace Game.Scripts.Player.Inventory.Events
{
    public struct RemoveItemBeforeDestroyCommand : IFactoryMessage
    {
        public uint PlayerId;
        public uint FactoryItemId;

        public RemoveItemBeforeDestroyCommand(uint playerId)
        {
            PlayerId = playerId;
            FactoryItemId = 0;
        }

        public uint GetFactoryItemId() => FactoryItemId;

        public void SetFactoryItemId(uint id) => FactoryItemId = id;
    }
}
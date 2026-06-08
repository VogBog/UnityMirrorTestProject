using Game.Scripts.GameSystems.Factories.Data;

namespace Game.Scripts.GameSystems.PlayerSpawning
{
    public struct SetPlayerNameCommand : IFactoryMessage
    {
        public uint FactoryItemId;
        public string Name;

        public SetPlayerNameCommand(string name)
        {
            Name = name;
            FactoryItemId = 0;
        }

        public uint GetFactoryItemId() => FactoryItemId;

        public void SetFactoryItemId(uint id) => FactoryItemId = id;
    }
}
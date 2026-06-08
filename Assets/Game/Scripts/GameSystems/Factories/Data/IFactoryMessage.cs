using Mirror;

namespace Game.Scripts.GameSystems.Factories.Data
{
    public interface IFactoryMessage : NetworkMessage
    {
        uint GetFactoryItemId();
        void SetFactoryItemId(uint id);
    }
}
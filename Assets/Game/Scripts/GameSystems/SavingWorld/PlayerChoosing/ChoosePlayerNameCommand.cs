using Mirror;

namespace Game.Scripts.GameSystems.SavingWorld.PlayerChoosing
{
    public struct ChoosePlayerNameCommand : NetworkMessage
    {
        public string Name;

        public ChoosePlayerNameCommand(string name)
        {
            Name = name;
        }
    }
}
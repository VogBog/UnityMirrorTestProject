using Mirror;

namespace Game.Scripts.GameSystems.PlayerSpawning
{
    public struct SpawnPlayerCommand : NetworkMessage
    {
        public string Name;

        public SpawnPlayerCommand(string name)
        {
            Name = name;
        }
    }
}
using Mirror;

namespace Game.Scripts.GameSystems.Factories.Data
{
    public struct ObjectSpawnedEvent : NetworkMessage
    {
        public NetworkIdentity NetworkIdentity;
        public SpawnMessage SpawnMessage;

        public ObjectSpawnedEvent(NetworkIdentity networkIdentity, SpawnMessage spawnMessage)
        {
            NetworkIdentity = networkIdentity;
            SpawnMessage = spawnMessage;
        }
    }
}
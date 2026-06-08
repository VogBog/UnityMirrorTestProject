using Mirror;

namespace Game.Scripts.GameSystems.Factories.Data
{
    public struct ObjectDespawnedEvent : NetworkMessage
    {
        public NetworkIdentity NetworkIdentity;

        public ObjectDespawnedEvent(NetworkIdentity networkIdentity)
        {
            NetworkIdentity = networkIdentity;
        }
    }
}
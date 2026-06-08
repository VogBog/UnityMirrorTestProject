using Mirror;

namespace Game.Scripts.Player
{
    public struct PlayerCharacteristicsChangedEvent
    {
        public PlayerCharacteristics Characteristics;
        public NetworkIdentity Player;

        public PlayerCharacteristicsChangedEvent(PlayerCharacteristics characteristics, NetworkIdentity player)
        {
            Characteristics = characteristics;
            Player = player;
        }
    }
}
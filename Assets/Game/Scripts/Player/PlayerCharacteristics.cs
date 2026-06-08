using System.IO;
using Game.Scripts.GameSystems.SavingWorld.Players;
using Game.Scripts.Network.EventBus;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Player
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class PlayerCharacteristics : MonoBehaviour, IPlayerSavingComponent
    {
        [SerializeField] private PlayerScriptableData _data;
        
        private IEventBus _eventBus;
        private NetworkIdentity _networkIdentity;

        public float Stamina { get; private set; }
        public float MaxStamina { get; private set; }

        [Inject]
        private void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _networkIdentity = GetComponent<NetworkIdentity>();
            
            Stamina = _data.Stamina;
            MaxStamina = Stamina;
        }

        private void Start()
        {
            SetStamina(Stamina);
        }

        public void DecreaseStamina(float amount) => SetStamina(Stamina - amount);

        public void IncreaseStamina(float amount) => SetStamina(Stamina + amount);

        public void SetStamina(float amount)
        {
            Stamina = Mathf.Clamp(amount, 0, MaxStamina);
            _eventBus.Publish(new PlayerCharacteristicsChangedEvent(this, _networkIdentity));
        }

        public void SaveData(BinaryWriter writer)
        {
            writer.Write(Stamina);
            writer.Write(MaxStamina);
        }

        public void LoadData(BinaryReader reader)
        {
            Stamina = reader.ReadSingle();
            MaxStamina = reader.ReadSingle();
            
            SetStamina(Stamina);
        }
    }
}
using System.IO;
using Game.Scripts.GameSystems.SavingWorld.Players;
using Game.Scripts.Network.EventBus;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Player
{
    public class PlayerCharacteristics : NetworkBehaviour, IPlayerSavingComponent
    {
        [SerializeField] private PlayerScriptableData _data;
        
        private IEventBus _eventBus;

        [field: SyncVar(hook = nameof(OnStaminaChanged))]
        public float Stamina { get; private set; }

        public float MaxStamina { get; private set; }

        [Inject]
        private void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
            
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
            float oldStamina = Stamina;
            Stamina = Mathf.Clamp(amount, 0, MaxStamina);
            OnStaminaChanged(oldStamina, Stamina);
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

        private void OnStaminaChanged(float oldValue, float newValue)
        {
            _eventBus.Publish(new PlayerCharacteristicsChangedEvent(this, netIdentity));
        }
    }
}
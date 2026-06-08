using Game.Scripts.GameSystems.Repositories;
using Game.Scripts.Network.EventBus;
using Game.Scripts.Player;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Ui.PlayerCharacteristics
{
    public class PlayerCharacteristicsPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerCharacteristicsView _view;

        private IEventBus _eventBus;
        private IPlayerRepository _playerRepository;
        
        [Inject]
        private void Construct(IEventBus eventBus, IPlayerRepository playerRepository)
        {
            _eventBus = eventBus;
            _playerRepository = playerRepository;
        }

        private void OnEnable()
        {
            _eventBus.Subscribe<PlayerCharacteristicsChangedEvent>(OnPlayerCharacteristicsChanged);
        }

        private void OnDisable()
        {
            _eventBus.Unsubscribe<PlayerCharacteristicsChangedEvent>(OnPlayerCharacteristicsChanged);
        }

        private void OnPlayerCharacteristicsChanged(PlayerCharacteristicsChangedEvent ev)
        {
            if (!_playerRepository.IsMyPlayer(ev.Player))
                return;
            
            _view.SetStamina(ev.Characteristics.Stamina, ev.Characteristics.MaxStamina);
        }
    }
}
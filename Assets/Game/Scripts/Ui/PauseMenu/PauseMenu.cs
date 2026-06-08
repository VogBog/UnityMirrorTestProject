using Game.Scripts.GameSystems.GameFinishing;
using Game.Scripts.Ui.MenusSystem;
using Mirror;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Ui.PauseMenu
{
    public class PauseMenu : MonoBehaviour, IMenuPage
    {
        [SerializeField] private TMP_Text _quitBtnText;
        [SerializeField] private string _serverQuitText;
        [SerializeField] private string _clientQuitText;

        private IMenusSystem _menus;
        private IGameFinisher _gameFinisher;

        [Inject]
        private void Construct(IMenusSystem menus, IGameFinisher gameFinisher)
        {
            _menus = menus;
            _gameFinisher = gameFinisher;
        }

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public bool Open()
        {
            if (NetworkServer.active)
                _quitBtnText.text = _serverQuitText;
            else
                _quitBtnText.text = _clientQuitText;
            
            gameObject.SetActive(true);
            return true;
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void ContinueClicked()
        {
            _menus.Close();
        }

        public void QuitClicked()
        {
            _gameFinisher.QuitGame();
        }
    }
}
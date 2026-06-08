using System;
using Game.Scripts.SceneManagement;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Scripts.Network.Main
{
    public class OfflineMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _disconnectedPage;
        [SerializeField] private ChooseSaveFileMenu _chooseSaveFileMenu;

        [SerializeField] private TMP_InputField _ipInput;
        [SerializeField] private TMP_InputField _portInput;

        [SerializeField] private Button _hostBtn;
        [SerializeField] private Button _connectBtn;
        [SerializeField] private Button _quitBtn;

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (NetworkManager.singleton == null)
            {
                Debug.LogError("Cannot start OfflineMenu without NetworkManager");
                SceneManager.LoadScene(nameof(Scenes.Boot));
                return;
            }

            if (NetworkManager.singleton.transport == null)
            {
                Debug.LogError("Cannot start OfflineMenu without NetworkManager transport");
                SceneManager.LoadScene(nameof(Scenes.Boot));
                return;
            }

            var transport = NetworkManager.singleton.transport;
            transport.OnClientTransportException += OnClientTransportException;
            transport.OnServerTransportException += OnServerTransportException;
            transport.OnClientDisconnected += OnClientDisconnected;
            transport.OnServerDisconnected += OnServerDisconnected;
            
            _hostBtn.onClick.AddListener(StartHost);
            _connectBtn.onClick.AddListener(StartClient);
            _quitBtn.onClick.AddListener(Quit);

            _ipInput.text = NetworkManager.singleton.networkAddress;
            if (NetworkManager.singleton.transport is PortTransport portTransport)
                _portInput.text = portTransport.Port.ToString();

            _chooseSaveFileMenu.Started += OnGameStarted;

            if (StaticData.DisconnectedFromServer && !StaticData.DisconnectedByHimself)
            {
                _disconnectedPage.SetActive(true);
            }
            
            StaticData.SetDefault();
        }

        private void OnDisable()
        {
            if (NetworkManager.singleton != null && NetworkManager.singleton.transport != null)
            {
                var transport = NetworkManager.singleton.transport;
                transport.OnClientTransportException -= OnClientTransportException;
                transport.OnServerTransportException -= OnServerTransportException;
                transport.OnClientDisconnected -= OnClientDisconnected;
                transport.OnServerDisconnected -= OnServerDisconnected;
            }
            
            _hostBtn.onClick.RemoveListener(StartHost);
            _connectBtn.onClick.RemoveListener(StartClient);
            _quitBtn.onClick.RemoveListener(Quit);
        }

        public void StartHost()
        {
            if (!TrySetParameters())
                return;

            _chooseSaveFileMenu.ShowScreen();
        }

        public void StartClient()
        {
            if (!TrySetParameters())
                return;
            
            SetButtonsEnabled(false);
            NetworkManager.singleton.StartClient();
        }

        public void Quit()
        {
            SetButtonsEnabled(false);
            Application.Quit();
        }

        public void CloseDisconnectedPage()
        {
            _disconnectedPage.SetActive(false);
        }

        public void SetButtonsEnabled(bool enabled)
        {
            _hostBtn.interactable = enabled;
            _connectBtn.interactable = enabled;
            _quitBtn.interactable = enabled;
        }

        private bool TrySetParameters()
        {
            string ip = _ipInput.text;
            string port = _portInput.text;

            if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(port))
                return false;
            
            NetworkManager.singleton.networkAddress = ip;
            if (NetworkManager.singleton.transport is PortTransport portTransport &&
                ushort.TryParse(port, out ushort parsedPort))
                portTransport.Port = parsedPort;

            return true;
        }

        private void OnGameStarted()
        {
            SetButtonsEnabled(false);
            NetworkManager.singleton.StartHost();
            NetworkManager.singleton.ServerChangeScene(nameof(Scenes.Game));
        }

        private void OnClientTransportException(Exception exception)
        {
            SetButtonsEnabled(true);
        }

        private void OnServerTransportException(int _, Exception exception)
        {
            SetButtonsEnabled(true);
        }

        private void OnClientDisconnected()
        {
            SetButtonsEnabled(true);
        }

        private void OnServerDisconnected(int _)
        {
            SetButtonsEnabled(true);
        }
    }
}
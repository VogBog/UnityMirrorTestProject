using System;
using System.Collections;
using Game.Scripts.GameSystems.SavingWorld.Players;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Ui.PlayerChooser
{
    public class PlayerChooserView : MonoBehaviour
    {
        [SerializeField] private GameObject _page;
        [SerializeField] private LayoutGroup _layoutGroup;
        [SerializeField] private RectTransform _content;
        [SerializeField] private PlayerDataButton _playerDataButtonPrefab;

        [SerializeField] private GameObject _createNewPage;
        [SerializeField] private TMP_InputField _playerNameInput;
        [SerializeField] private TMP_Text _errorText;

        private PlayerDataButton[] _slots;

        public const string CreateNewText = "Create new...";
        
        public bool Initialized { get; private set; }

        public event Action<string> PlayerChosen;
        public event Action<string> NewPlayerCreated; 

        public void Initialize(int count)
        {
            _page.SetActive(true);
            StartCoroutine(InitializeRoutine(count));
        }

        public void ShowPlayerPreviews(PlayerPreviewData[] data)
        {
            int i = 0;
            for (; i < data.Length; i++)
            {
                _slots[i].SetText(data[i].Name);
            }
            
            _slots[i++].SetText(CreateNewText);
            _content.sizeDelta = new Vector2(0f, _playerDataButtonPrefab.Height * i);

            for (; i < _slots.Length; i++)
            {
                _slots[i].gameObject.SetActive(false);
            }
            
            _page.SetActive(true);
        }

        public void CreateNewPlayer()
        {
            string name = _playerNameInput.text;
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
            {
                _errorText.text = "Please enter a name";
                return;
            }

            foreach (var slot in _slots)
            {
                if (slot.Text == name)
                {
                    _errorText.text = "This name is already taken";
                    return;
                }
            }
            
            CloseCreatingNewPlayerPage();
            
            NewPlayerCreated?.Invoke(name);
        }

        public void CloseCreatingNewPlayerPage()
        {
            _createNewPage.SetActive(false);
        }

        public void Close()
        {
            _page.SetActive(false);
        }

        private IEnumerator InitializeRoutine(int count)
        {
            _slots = new PlayerDataButton[count];
            for (int i = 0; i < count; i++)
            {
                int index = i;
                _slots[i] = Instantiate(_playerDataButtonPrefab, _layoutGroup.transform);
                _slots[i].Button.onClick.AddListener(() => OnPlayerClicked(index));
                yield return null;
            }
            
            _createNewPage.SetActive(false);
            _errorText.text = string.Empty;
            _content.sizeDelta = new Vector2(0f, _playerDataButtonPrefab.Height * count);

            _layoutGroup.enabled = false;
            _page.SetActive(false);
            Initialized = true;
        }

        private void OnPlayerClicked(int index)
        {
            if (!string.IsNullOrEmpty(_slots[index].Text) &&
                _slots[index].Text != CreateNewText)
            {
                PlayerChosen?.Invoke(_slots[index].Text);
            }
            else
            {
                _createNewPage.SetActive(true);
            }
        }
    }
}
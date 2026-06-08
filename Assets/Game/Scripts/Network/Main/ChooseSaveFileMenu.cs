using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Game.Scripts.SceneManagement;
using Game.Scripts.Ui.PlayerChooser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Network.Main
{
    public class ChooseSaveFileMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _page;
        [SerializeField] private PlayerDataButton _buttonPrefab;
        [SerializeField] private LayoutGroup _layoutGroup;
        [SerializeField] private RectTransform _content;

        [SerializeField] private GameObject _newSaveFilePage;
        [SerializeField] private TMP_InputField _saveFileNameInputField;

        private PlayerDataButton[] _buttons;
        private Coroutine _initializeCoroutine;
        private bool _started = false;

        public event Action Started;

        public const string CreateNewText = "Create new save...";

        public void ShowScreen()
        {
            _page.SetActive(true);
            _newSaveFilePage.SetActive(false);
            
            if (_buttons == null)
            {
                if (_initializeCoroutine != null)
                    return;
                _initializeCoroutine = StartCoroutine(InitializeRoutine());
            }
        }

        public void BackClicked()
        {
            if (_newSaveFilePage.activeSelf)
            {
                _newSaveFilePage.SetActive(false);
                return;
            }
            
            _page.SetActive(false);
        }

        public void CreateNewClicked()
        {
            if (_started)
                return;
            
            string saveFile = _saveFileNameInputField.text;
            if (string.IsNullOrWhiteSpace(saveFile))
                return;

            foreach (var btn in _buttons)
            {
                if (btn.Text.Equals(saveFile))
                    return;
            }

            if (saveFile.StartsWith(' ') || saveFile.Contains('#') || saveFile.Contains('?') ||
                saveFile.Contains('!') || saveFile.Contains('@') || saveFile.Contains('/') ||
                saveFile.Contains('\\') || saveFile.Contains(';'))
                return;

            StaticData.SaveFileName = saveFile;
            _started = true;
            Started?.Invoke();
        }

        private IEnumerator InitializeRoutine()
        {
            var buttons = new List<PlayerDataButton>();
            yield return CreateButtons(buttons);

            var button = Instantiate(_buttonPrefab, _layoutGroup.transform);
            button.SetText(CreateNewText);
            button.Button.onClick.AddListener(() => _newSaveFilePage.SetActive(true));
            buttons.Add(button);

            _content.sizeDelta = new Vector2(0f, buttons.Count * _buttonPrefab.Height);

            _buttons = buttons.ToArray();
        }

        private IEnumerator CreateButtons(List<PlayerDataButton> list)
        {
            string folder = Path.Combine(Application.persistentDataPath, "saves");
            if (!Directory.Exists(folder))
            {
                yield break;
            }

            var files = Directory.GetFiles(folder);
            if (files.Length == 0)
            {
                yield break;
            }

            foreach (var file in files)
            {
                int startIndex = file.LastIndexOf('\\');
                int endIndex = file.LastIndexOf(".dat", StringComparison.CurrentCulture);
                string saveFile = file.Substring(startIndex + 1, endIndex - startIndex - 1);

                var button = Instantiate(_buttonPrefab, _layoutGroup.transform);
                button.SetText(saveFile);
                button.Button.onClick.AddListener(() => OnSaveFileClicked(saveFile));
                list.Add(button);

                yield return null;
            }
        }

        private void OnSaveFileClicked(string saveFileName)
        {
            if (_started)
                return;
            
            if (saveFileName == CreateNewText)
            {
                _newSaveFilePage.SetActive(true);
                return;
            }
            
            StaticData.SaveFileName = saveFileName;
            _started = true;
            Started?.Invoke();
        }
    }
}
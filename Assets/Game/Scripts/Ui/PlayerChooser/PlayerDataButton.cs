using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Ui.PlayerChooser
{
    public class PlayerDataButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private float _height;

        public float Height => _height;
        public Button Button => _button;
        public string Text => _name.text;
        
        public void SetText(string text) => _name.text = text;
    }
}
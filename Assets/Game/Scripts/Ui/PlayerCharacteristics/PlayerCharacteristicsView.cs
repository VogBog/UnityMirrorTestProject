using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Ui.PlayerCharacteristics
{
    public class PlayerCharacteristicsView : MonoBehaviour
    {
        [SerializeField] private Slider _staminaSlider;

        public void SetStamina(float stamina, float maxStamina)
        {
            _staminaSlider.maxValue = maxStamina;
            _staminaSlider.value = stamina;
        }
    }
}
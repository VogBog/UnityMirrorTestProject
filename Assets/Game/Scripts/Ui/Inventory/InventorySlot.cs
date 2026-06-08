using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Ui.Inventory
{
    public class InventorySlot : MonoBehaviour
    {
        [field: SerializeField] public Image Icon { get; private set; }
        [field: SerializeField] public Slider Slider { get; private set; }
        [field: SerializeField] public Image ChosenBorder { get; private set; }
        [field: SerializeField] public TMP_Text StackCountText { get; private set; }

        public virtual void SetIcon([CanBeNull] Sprite sprite)
        {
            Icon.sprite = sprite;

            if (sprite == null)
            {
                Icon.color = new Color(0, 0, 0, 0);
            }
            else
            {
                Icon.color = Color.white;
                Icon.preserveAspect = true;
            }
        }
        
        public virtual void SetSliderActive(bool isActive) => Slider.gameObject.SetActive(isActive);

        public virtual void SetSliderValue(float value, float maxValue)
        {
            Slider.maxValue = maxValue;
            Slider.value = value;
        }

        public virtual void SetChosenBorder(bool on) => ChosenBorder.gameObject.SetActive(on);

        public virtual void SetStackCount(int count)
        {
            if (count <= 1)
            {
                StackCountText.gameObject.SetActive(false);
            }
            else
            {
                StackCountText.gameObject.SetActive(true);
                StackCountText.text = count.ToString();
            }
        }

        public void SetData(InventorySlotData data)
        {
            SetIcon(data.Icon);
            SetSliderActive(data.ActiveSlider);
            SetStackCount(data.StackCount);
            if (data.ActiveSlider)
                SetSliderValue(data.SliderValue, data.SliderMaxValue);
        }

        public void SetEmpty()
        {
            SetIcon(null);
            SetSliderActive(false);
            SetChosenBorder(false);
            SetStackCount(0);
        }
    }
}
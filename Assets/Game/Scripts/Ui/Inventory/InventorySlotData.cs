using UnityEngine;

namespace Game.Scripts.Ui.Inventory
{
    public struct InventorySlotData
    {
        public Sprite Icon;
        public bool ActiveSlider;
        public float SliderValue;
        public float SliderMaxValue;
        public int StackCount;

        public InventorySlotData(Sprite icon)
        {
            Icon = icon;
            ActiveSlider = false;
            SliderValue = 0;
            SliderMaxValue = 1;
            StackCount = 1;
        }

        public InventorySlotData SetSliderValue(float value, float maxValue)
        {
            ActiveSlider = true;
            SliderValue = value;
            SliderMaxValue = maxValue;

            return this;
        }

        public InventorySlotData SetStackCount(int stackCount)
        {
            StackCount = stackCount;
            return this;
        }
    }
}
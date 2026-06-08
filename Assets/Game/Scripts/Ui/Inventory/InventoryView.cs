using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Player.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Ui.Inventory
{
    public class InventoryView : MonoBehaviour
    {
        [SerializeField] private LayoutGroup _slotsParent;
        [SerializeField] private InventorySlot _slotPrefab;

        private readonly InventorySlot[] _slots = new InventorySlot[IInventory.Capacity];
        
        public bool Initialized { get; private set; }

        private void Awake()
        {
            StartCoroutine(InitializeRoutine());
        }

        public void SetItems(IEnumerable<InventorySlotData> items)
        {
            int count = 0;
            foreach (var data in items)
            {
                _slots[count++].SetData(data);
            }

            for (int i = count; i < _slots.Length; i++)
            {
                _slots[i].SetEmpty();
            }
        }

        public void SetSlider(int index, float value, float maxValue)
        {
            var slot = _slots[index];
            slot.SetSliderActive(true);
            slot.SetSliderValue(value, maxValue);
        }

        public void SetChosen(int index)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i].SetChosenBorder(i == index);
            }
        }

        private IEnumerator InitializeRoutine()
        {
            for (int i = 0; i < IInventory.Capacity; i++)
            {
                var slotInstance = Instantiate(_slotPrefab, _slotsParent.transform);
                _slots[i] = slotInstance;
                slotInstance.SetEmpty();

                yield return null;
            }

            _slotsParent.enabled = false;
            Initialized = true;
        }
    }
}
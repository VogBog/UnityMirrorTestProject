using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Player.Inventory;
using Game.Scripts.Ui.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Ui.InventoryWindow
{
    public class InventoryWindowView : MonoBehaviour
    {
        [SerializeField] private GameObject _page;
        [SerializeField] private Transform _dragParent;
        [SerializeField] private LayoutGroup _layoutGroup;
        [SerializeField] private InventoryDragSlot _dragSlotPrefab;
        [SerializeField] private InventoryDropSlot _dropSlotPrefab;

        private readonly InventoryDropSlot[] _slots = new InventoryDropSlot[IInventory.Capacity];
        
        public bool Initialized { get; private set; }

        public event Action<InventoryDropSlot, int, InventoryDragSlot, int> Dropped; 

        private IEnumerator Start()
        {
            for (int i = 0; i < IInventory.Capacity; i++)
            {
                var dropSlot = Instantiate(_dropSlotPrefab, _layoutGroup.transform);
                _slots[i] = dropSlot;
                
                var dragSlot = Instantiate(_dragSlotPrefab, dropSlot.transform);
                dropSlot.SetSlot(dragSlot);

                dropSlot.Dropped += OnDropped;
                dragSlot.DragBegan += OnDragBegan;

                yield return null;
            }

            _layoutGroup.enabled = false;
            _page.SetActive(false);
            Initialized = true;
        }

        public void Show()
        {
            _page.SetActive(true);
        }

        public void Show(IEnumerable<InventorySlotData> items)
        {
            int i = 0;
            foreach (var item in items)
            {
                if (i >= _slots.Length)
                    break;
                
                _slots[i].Slot.SetData(item);
                ++i;
            }
            _page.SetActive(true);
        }

        public void Close()
        {
            _page.SetActive(false);
        }

        public void Set(int index, InventorySlotData slot)
        {
            _slots[index].Slot.SetData(slot);
        }

        private void OnDragBegan(InventoryDragSlot drag)
        {
            drag.transform.SetParent(_dragParent);
        }

        private void OnDropped(InventoryDropSlot drop, InventoryDragSlot drag)
        {
            int dropIndex = 0;
            int dragIndex = 0;

            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == drop)
                    dropIndex = i;
                if (_slots[i].Slot == drag)
                    dragIndex = i;
            }
            
            Dropped?.Invoke(drop, dropIndex, drag, dragIndex);
        }
    }
}
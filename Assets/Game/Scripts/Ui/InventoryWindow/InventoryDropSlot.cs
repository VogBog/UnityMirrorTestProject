using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Scripts.Ui.InventoryWindow
{
    public class InventoryDropSlot : MonoBehaviour, IDropHandler
    {
        [SerializeField] private Transform _dropParent;

        public InventoryDragSlot Slot { get; private set; }

        public event Action<InventoryDropSlot, InventoryDragSlot> Dropped; 

        public void SetSlot(InventoryDragSlot slot)
        {
            if (Slot != null)
                return;

            Slot = slot;
            Slot.transform.SetParent(_dropParent);
            Slot.transform.localPosition = Vector3.zero;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null &&
                eventData.pointerDrag.TryGetComponent<InventoryDragSlot>(out var slot))
            {
                Dropped?.Invoke(this, slot);
            }
        }
    }
}
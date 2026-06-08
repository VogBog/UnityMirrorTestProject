using System;
using Game.Scripts.Ui.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Scripts.Ui.InventoryWindow
{
    public class InventoryDragSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image _raycastTarget;
        [SerializeField] private InventorySlot _slot;
        
        private Transform _parent;
        private RectTransform _rectTransform;
        private bool _dragging = false;
        
        public event Action<InventoryDragSlot> DragBegan;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void SetData(InventorySlotData data)
        {
            _slot.SetData(data);
            _raycastTarget.raycastTarget = data.Icon != null;
            if (_dragging)
            {
                transform.SetParent(_parent);
                transform.localPosition = Vector3.zero;
                _dragging = false;
            }
        }

        public void SetEmpty()
        {
            _slot.SetEmpty();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _raycastTarget.raycastTarget = false;
            _parent = transform.parent;
            _dragging = true;
            DragBegan?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;
            
            _rectTransform.anchoredPosition += eventData.delta;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _dragging = false;
            _raycastTarget.raycastTarget = true;
            transform.SetParent(_parent);
            transform.localPosition = Vector3.zero;
        }
    }
}
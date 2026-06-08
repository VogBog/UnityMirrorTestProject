using System;
using Game.Scripts.Network.EventBus;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Ui.MenusSystem
{
    public class MenusSystem : MonoBehaviour, IMenusSystem
    {
        [Inject] private IEventBus _eventBus;
        
        public IMenuPage ActivePage { get; private set; }

        public event Action Opened;
        public event Action Closed;

        public bool TryOpen(IMenuPage page)
        {
            if (ActivePage != null)
                return false;
            
            ActivePage = page;
            if (!page.Open())
            {
                ActivePage = null;
                return false;
            }
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            Opened?.Invoke();
            return true;
        }

        public void Close()
        {
            ActivePage?.Close();
            ActivePage = null;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
            
            Closed?.Invoke();
        }
    }
}
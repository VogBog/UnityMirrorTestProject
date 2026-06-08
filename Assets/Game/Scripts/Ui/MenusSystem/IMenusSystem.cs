using System;

namespace Game.Scripts.Ui.MenusSystem
{
    public interface IMenusSystem
    {
        IMenuPage ActivePage { get; }
        
        event Action Opened;
        event Action Closed;

        bool TryOpen(IMenuPage page);
        void Close();
    }
}
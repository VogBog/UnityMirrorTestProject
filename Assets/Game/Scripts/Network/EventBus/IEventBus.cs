using System;

namespace Game.Scripts.Network.EventBus
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler);
        void Unsubscribe<T>(Action<T> handler);
        void Publish<T>(T msg);
    }
}
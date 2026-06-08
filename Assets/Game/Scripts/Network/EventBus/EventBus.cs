using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Network.EventBus
{
    public class EventBus : MonoBehaviour, IEventBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new();
        
        public void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var @delegate))
            {
                _handlers.Add(type, handler);
                return;
            }
            
            if (@delegate is not Action<T> action)
                throw new SystemException($"Unknown exception while subscribing to {type}");
            
            action += handler;
            _handlers[type] = action;
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var @delegate))
                return;
            
            if (@delegate is not Action<T> action)
                throw new SystemException($"Unknown exception while unsubscribing from {type}");
            
            action -= handler;
            if (action == null)
                _handlers.Remove(type);
            else 
                _handlers[type] = action;
        }

        public void Publish<T>(T msg)
        {
            if (!_handlers.TryGetValue(typeof(T), out var @delegate))
                return;
            
            if (@delegate is not Action<T> action)
                throw new SystemException($"Unknown exception while invoking {typeof(T)}");
            
            action.Invoke(msg);
        }
    }
}
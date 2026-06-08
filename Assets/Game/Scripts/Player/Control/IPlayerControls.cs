using System;
using UnityEngine;

namespace Game.Scripts.Player.Control
{
    public interface IPlayerControls
    {
        public Vector2 MoveVector { get; }

        public event Action JumpedFixedUpdate;
        public event Action<Vector2> Looking;
        public event Action Dropped;
        public event Action Interacted;
        public event Action StartUsingItem;
        public event Action EndUsingItem;

        public event Action<int> NumPressed;
        public event Action<int> Scrolled;
        public event Action OpenedInventory;
        public event Action PausePressed;
    }
}
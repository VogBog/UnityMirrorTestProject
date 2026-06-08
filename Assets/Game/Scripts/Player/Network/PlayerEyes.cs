using UnityEngine;

namespace Game.Scripts.Player.Network
{
    public class PlayerEyes : MonoBehaviour
    {
        [SerializeField] private Component[] _ownerComponents;
        
        public void Resolve()
        {
            foreach (var component in _ownerComponents)
            {
                Destroy(component);
            }
        }
    }
}
using UnityEngine;

namespace Game.Scripts.Animations
{
    [RequireComponent(typeof(Animator))]
    public class ToolAnimator : MonoBehaviour
    {
        private Animator _animator;
        
        private static readonly int Using = Animator.StringToHash("Using");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void SetUsing(bool inUse)
        {
            _animator.SetBool(Using, inUse);
        }
    }
}
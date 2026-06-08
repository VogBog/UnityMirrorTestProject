using UnityEngine;
using Zenject;

namespace Game.Scripts.Player.Control
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerScriptableData _data;
        
        private float _speed;
        private float _jumpHeight;
        private Vector3 _velocity;

        private IPlayerControls _playerControls;
        private CharacterController _controller;

        [Inject]
        private void Construct(IPlayerControls playerControls)
        {
            _playerControls = playerControls;
            _controller = GetComponent<CharacterController>();
            
            _speed = _data.Speed;
            _jumpHeight = _data.JumpHeight;
        }

        private void OnEnable()
        {
            _playerControls.JumpedFixedUpdate += Jump;
        }

        private void OnDisable()
        {
            _playerControls.JumpedFixedUpdate -= Jump;
        }

        public void Jump()
        {
            if (!_controller.isGrounded || _velocity.y > 0f)
                return;

            _velocity.y = Mathf.Sqrt(-2f * Physics.gravity.y * _jumpHeight);
        }

        private void FixedUpdate()
        {
            var moveVector2 = _playerControls.MoveVector;
            var moveVector = (transform.forward * moveVector2.y + transform.right * moveVector2.x) * _speed;

            _controller.Move((moveVector + _velocity) * Time.fixedDeltaTime);

            if (!_controller.isGrounded)
            {
                _velocity.y += Physics.gravity.y * Time.fixedDeltaTime;
            }
            else if (_velocity.y <= 0f)
            {
                _velocity.y = -0.2f;
            }
        }
    }
}
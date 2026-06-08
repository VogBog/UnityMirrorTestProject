using UnityEngine;
using Zenject;

namespace Game.Scripts.Player.Control
{
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] private PlayerScriptableData _data;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private Transform _playerTransform;

        private float _sensitivity;
        private float _rotationX;
        
        private IPlayerControls _playerControls;

        [Inject]
        private void Construct(IPlayerControls playerControls)
        {
            _playerControls = playerControls;

            _sensitivity = _data.MouseSensitivity;
        }

        private void OnEnable()
        {
            _playerControls.Looking += OnLook;
        }

        private void OnDisable()
        {
            _playerControls.Looking -= OnLook;
        }

        private void OnLook(Vector2 vector)
        {
            _rotationX = Mathf.Clamp(_rotationX - vector.y * _sensitivity, -90f, 90f);
            _playerTransform.Rotate(Vector3.up, vector.x * _sensitivity);
            _cameraTransform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
        }
    }
}
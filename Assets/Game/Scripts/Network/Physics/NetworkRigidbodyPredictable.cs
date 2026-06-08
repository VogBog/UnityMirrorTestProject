using System;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Network.Physics
{
    [RequireComponent(typeof(Rigidbody))]
    public class NetworkRigidbodyPredictable : NetworkBehaviour
    {
        [SerializeField] private float _positionSensitivity = 0.1f;
        [SerializeField] private float _rotationSensitivity = 0.1f;
        [SerializeField] private float _angularVelocitySensitivity = 0.1f;
        [SerializeField] private float _velocitySensitivity = 0.1f;
        
        [SerializeField] private float _syncInterval = 2f;
        
        private Rigidbody _rigidbody;

        private RigidbodySnapshot _lastSnapshot;
        private uint _sequenceNumber = 0;
        private bool _repeatingInvoke = false;

        private float _sequenceNumberStep = uint.MaxValue / 2f;
        private float _posSensitivitySqr;
        private float _velocitySensitivitySqr;
        private float _angularVelocitySensitivitySqr;
        
        public Rigidbody ClientRigidbody => _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            
            _posSensitivitySqr = _positionSensitivity * _positionSensitivity;
            _velocitySensitivitySqr = _velocitySensitivity * _velocitySensitivity;
            _angularVelocitySensitivitySqr = _angularVelocitySensitivity * _angularVelocitySensitivity;
        }
        
        private void OnEnable()
        {
            if (!_repeatingInvoke && isServer)
            {
                _repeatingInvoke = true;
                InvokeRepeating(nameof(TrySync), 0f, _syncInterval);
            }
        }

        private void OnDisable()
        {
            if (_repeatingInvoke)
                CancelInvoke(nameof(TrySync));
        }

        public override void OnStartServer()
        {
            if (!_repeatingInvoke)
            {
                _repeatingInvoke = true;
                InvokeRepeating(nameof(TrySync), _syncInterval, _syncInterval);
            }
        }

        public override void OnStartClient()
        {
            if (!isServer)
                CmdSyncRigidbody();
        }

        public void TrySync()
        {
            if (!isServer)
                return;
            
            var position = transform.position;
            var rotation = transform.rotation;
            var velocity = _rigidbody.linearVelocity;
            var angularVelocity = _rigidbody.angularVelocity;

            if (Vector3.SqrMagnitude(position - _lastSnapshot.Position) > _posSensitivitySqr ||
                Vector3.SqrMagnitude(velocity - _lastSnapshot.Velocity) > _velocitySensitivitySqr ||
                Vector3.SqrMagnitude(angularVelocity - _lastSnapshot.AngularVelocity) > _angularVelocitySensitivitySqr ||
                IsRotationDifferent(rotation, _lastSnapshot.Rotation))
            {
                SyncClientRpc(new RigidbodySnapshot(position, rotation, velocity, angularVelocity, _sequenceNumber++));
            }
        }

        public void SyncForce()
        {
            if (!isServer)
                return;

            var snapshot = new RigidbodySnapshot(
                transform.position,
                transform.rotation,
                _rigidbody.linearVelocity,
                _rigidbody.angularVelocity,
                _sequenceNumber++);
            
            SyncClientRpc(snapshot);
        }

        public void SyncForceCommand()
        {
            if (isServer)
                return;
            
            CmdSyncRigidbody();
        }

        public void SetLinearVelocity(Vector3 velocity)
        {
            _rigidbody.linearVelocity = velocity;
            if (isServer)
                SyncForce();
        }

        public void SetAngularVelocity(Vector3 angularVelocity)
        {
            _rigidbody.angularVelocity = angularVelocity;
            if (isServer)
                SyncForce();
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            if (isServer)
                SyncForce();
        }

        public void SetVelocity(Vector3 linear, Vector3 angular)
        {
            _rigidbody.linearVelocity = linear;
            _rigidbody.angularVelocity = angular;
            if (isServer)
                SyncForce();
        }

        private bool IsRotationDifferent(Quaternion a, Quaternion b)
        {
            return Mathf.Abs(a.x - b.x) > _rotationSensitivity ||
                   Mathf.Abs(a.y - b.y) > _rotationSensitivity ||
                   Mathf.Abs(a.z - b.z) > _rotationSensitivity ||
                   Mathf.Abs(a.w - b.w) > _rotationSensitivity;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (NetworkServer.active)
                TrySync();
        }

        [ClientRpc]
        private void SyncClientRpc(RigidbodySnapshot snapshot)
        {
            if (isServer)
                return;
            
            if (_sequenceNumber > snapshot.SequenceNumber &&
                Mathf.Abs(_sequenceNumber - snapshot.SequenceNumber) < _sequenceNumberStep)
                return;
            
            _sequenceNumber = snapshot.SequenceNumber;
            transform.SetPositionAndRotation(snapshot.Position, snapshot.Rotation);

            if (!_rigidbody.isKinematic)
            {
                _rigidbody.linearVelocity = snapshot.Velocity;
                _rigidbody.angularVelocity = snapshot.AngularVelocity;
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdSyncRigidbody()
        {
            SyncForce();
        }
    }
}
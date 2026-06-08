using Mirror;
using UnityEngine;

namespace Game.Scripts.Network.Physics
{
    public struct RigidbodySnapshot : NetworkMessage
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        
        public uint SequenceNumber;

        public RigidbodySnapshot(
            Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, uint sequenceNumber)
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
            
            SequenceNumber = sequenceNumber;
        }
    }
}
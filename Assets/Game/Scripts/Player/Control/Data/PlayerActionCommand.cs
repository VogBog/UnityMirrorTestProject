using Game.Scripts.Network.PayloadTransfer;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Player.Control.Data
{
    public struct PlayerActionCommand : NetworkMessage
    {
        public string Type;
        public byte[] Payload;
        
        public bool ForPlayer;
        public uint PlayerId;

        public static PlayerActionCommand Create<T>(T command)
            where T : struct, INetworkSerializable
        {
            var payload = new NetworkPayload();
            payload.Serialize(command);
            
            return new PlayerActionCommand
            {
                Type = typeof(T).Name,
                Payload = payload.Data,
                ForPlayer = false,
                PlayerId = 0
            };
        }

        public static PlayerActionCommand CreateForPlayer<T>(uint playerId, T command)
            where T : struct, INetworkSerializable
        {
            var payload = new NetworkPayload();
            payload.Serialize(command);

            return new PlayerActionCommand
            {
                Type = typeof(T).Name,
                Payload = payload.Data,
                ForPlayer = true,
                PlayerId = playerId
            };
        }

        public T GetCommand<T>(T command)
            where T : struct, INetworkSerializable
        {
            var payload = new NetworkPayload();
            payload.Data = Payload;
            command = payload.Deserialize(command);
            return command;
        }
    }
}
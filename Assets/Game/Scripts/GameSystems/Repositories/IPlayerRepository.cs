using System;
using Game.Scripts.Player;
using Mirror;

namespace Game.Scripts.GameSystems.Repositories
{
    public interface IPlayerRepository
    {
        int Count { get; }
        PlayerMainDataComponents MyPlayer { get; }
        
        void RegisterPlayer(NetworkIdentity playerObject);
        void UnRegisterPlayer(NetworkIdentity playerObject);
        PlayerMainDataComponents GetPlayerObject(uint netId);
        PlayerMainDataComponents GetPlayerObjectByConnection(NetworkConnectionToClient connection);
        bool IsPlayer(NetworkIdentity obj);
        bool IsMyPlayer(NetworkIdentity obj);
        void ForEach(Action<PlayerMainDataComponents> forEachAction);
    }
}
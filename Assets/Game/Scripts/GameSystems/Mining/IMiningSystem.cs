using Game.Scripts.Player;
using UnityEngine;

namespace Game.Scripts.GameSystems.Mining
{
    public interface IMiningSystem
    {
        bool TryMine(Vector3 eyes, Vector3 lookDirection, out IOre ore);
        void MineOre(PlayerMainDataComponents player, IOre ore);
    }
}
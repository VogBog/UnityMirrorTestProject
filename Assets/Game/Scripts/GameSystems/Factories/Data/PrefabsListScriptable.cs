using Mirror;
using UnityEngine;

namespace Game.Scripts.GameSystems.Factories.Data
{
    [CreateAssetMenu(menuName = "Data/Prefabs")]
    public class PrefabsListScriptable : ScriptableObject
    {
        [field: SerializeField] public NetworkIdentity[] Prefabs { get; private set; }
    }
}
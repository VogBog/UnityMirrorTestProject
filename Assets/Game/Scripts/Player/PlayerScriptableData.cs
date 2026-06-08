using UnityEngine;

namespace Game.Scripts.Player
{
    [CreateAssetMenu(menuName = "Data/Player")]
    public class PlayerScriptableData : ScriptableObject
    {
        [field: SerializeField] public float Speed { get; private set; }
        [field: SerializeField] public float JumpHeight { get; private set; }
        [field: SerializeField] public float Stamina { get; private set; }
        [field: SerializeField] public float MouseSensitivity { get; private set; }
        [field: SerializeField] public float InteractionDistance { get; private set; }
    }
}
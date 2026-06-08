using Game.Scripts.GameSystems.ToolUsingSystem.Data;

namespace Game.Scripts.GameSystems.ToolUsingSystem
{
    public interface ITool
    {
        public int Durability { get; set; }
        public int MaxDurability { get; }
        public float UsingTime { get; }
        public float UseStamina { get; }
        public bool CanUse { get; }
        public bool InUse { get; set; }
        
        void OnStartUsing(UseToolCommand command);
        bool ActivateUsingEffect(UseToolCommand command);
        void OnStopUsing(UseToolCommand command);
    }
}
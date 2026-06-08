using System.IO;
using Game.Scripts.Animations;
using Game.Scripts.GameSystems.ToolUsingSystem;
using Game.Scripts.GameSystems.ToolUsingSystem.Data;
using Mirror;

namespace Game.Scripts.Player.Inventory.Items
{
    public abstract class BaseTool : BaseItem, ITool
    {
        private ToolAnimator _animator;
        private bool _loaded;
        
        public abstract ToolScriptableData ToolInitData { get; }
        public int Durability { get; set; }
        public int MaxDurability { get; private set; }
        public float UsingTime { get; private set; }
        public float UseStamina { get; private set; }
        public bool CanUse => Durability > 0;
        public bool InUse { get; set; }

        protected virtual void Start()
        {
            _animator = GetComponentInChildren<ToolAnimator>(true);

            if (!_loaded)
            {
                Durability = ToolInitData.Durability;
            }
            
            MaxDurability = ToolInitData.Durability;
            UsingTime = ToolInitData.UsingTime;
            UseStamina = ToolInitData.UseStamina;
        }

        public virtual void OnStartUsing(UseToolCommand command)
        {
            _animator?.SetUsing(true);
        }

        public abstract void ActivateUsingEffect(UseToolCommand command);

        public virtual void OnStopUsing(UseToolCommand command)
        {
            _animator?.SetUsing(false);
        }

        public override void OnSerialize(NetworkWriter writer, bool initialState)
        {
            writer.WriteInt(Durability);
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            Durability = reader.ReadInt();
            _loaded = true;
        }

        public override void SaveData(BinaryWriter writer)
        {
            writer.Write(Durability);
        }

        public override void LoadData(BinaryReader reader)
        {
            Durability = reader.ReadInt32();
            MaxDurability = ToolInitData.Durability;
            _loaded = true;
        }
    }
}
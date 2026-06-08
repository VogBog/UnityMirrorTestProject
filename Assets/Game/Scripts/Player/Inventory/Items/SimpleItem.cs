using System.IO;
using Game.Scripts.Player.Inventory.Items.ScriptableData;
using UnityEngine;

namespace Game.Scripts.Player.Inventory.Items
{
    public class SimpleItem : BaseItem
    {
        [SerializeField] private BaseItemData _itemData;
        
        public override BaseItemData BaseItemData => _itemData;
        
        public override void SaveData(BinaryWriter writer)
        {
            //Nothing to save
        }

        public override void LoadData(BinaryReader reader)
        {
            //Nothing to load
        }
    }
}
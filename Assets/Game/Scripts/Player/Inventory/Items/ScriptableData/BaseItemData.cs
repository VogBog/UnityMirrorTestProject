using UnityEngine;

namespace Game.Scripts.Player.Inventory.Items.ScriptableData
{
    [CreateAssetMenu(menuName = "Data/Items/Base")]
    public class BaseItemData : ScriptableObject
    {
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public int MaxStack { get; private set; }
    }
}
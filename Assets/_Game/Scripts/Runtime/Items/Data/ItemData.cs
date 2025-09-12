using UnityEngine;
using Game.Runtime.Core.Data;

namespace Game.Runtime.Items.Data
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Game/Items/Item Data")]
    public class ItemData : BaseDataModel
    {
        [Header("Item Info")]
        public ItemType ItemType;
        public string DisplayName = "New Item";
        public Sprite ItemIcon;

        [Header("Economics")]
        public int SellPrice = 10;
        public int ProductionCost = 5;

        [Header("Stacking")]
        public float StackHeight = 0.1f;
        public int MaxStackSize = 10;

        [Header("Visuals")]
        public GameObject ItemPrefab;
        public Color ItemColor = Color.white;
    }
}

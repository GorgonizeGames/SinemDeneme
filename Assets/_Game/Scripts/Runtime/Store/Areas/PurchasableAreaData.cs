using UnityEngine;
using Game.Runtime.Core.Data;

namespace Game.Runtime.Store.Areas
{
    [CreateAssetMenu(fileName = "PurchasableArea", menuName = "Game/Store/Purchasable Area Data")]
    public class PurchasableAreaData : BaseDataModel
    {
        [Header("Area Info")]
        public string AreaName = "New Area";
        public AreaType AreaType = AreaType.Machine;

        [Header("Purchase")]
        public int PurchaseCost = 100;
        public int UnlockLevel = 1;

        [Header("Visuals")]
        public Sprite AreaIcon;
        public Color AreaColor = Color.white;
    }

    public enum AreaType
    {
        Machine,
        Shelf,
        Decoration,
        Expansion
    }
}
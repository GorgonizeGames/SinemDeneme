using UnityEngine;
using Game.Runtime.Store.Areas;
using Game.Runtime.Items;
using Game.Runtime.Items.Data;

namespace Game.Runtime.Production
{
    [CreateAssetMenu(fileName = "MachineData", menuName = "Game/Production/Machine Data")]
    public class ProductionMachineData : PurchasableAreaData
    {
        [Header("Production")]
        public ItemType ProducedItemType = ItemType.Comic;
        public float ProductionInterval = 3f;
        public int MaxCapacity = 10;

        [Header("Upgrade")]
        public int SpeedUpgradeCost = 50;
        public int CapacityUpgradeCost = 75;
        public float SpeedMultiplier = 0.8f;
        public int CapacityIncrease = 5;
    }
}


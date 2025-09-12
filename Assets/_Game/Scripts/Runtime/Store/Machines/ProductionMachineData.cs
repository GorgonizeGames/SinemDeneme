using UnityEngine;
using Game.Runtime.Store.Areas;
using Game.Runtime.Items.Data;

namespace Game.Runtime.Store.Machines
{
    [CreateAssetMenu(fileName = "MachineData", menuName = "Game/Store/Machine Data")]
    public class ProductionMachineData : PurchasableAreaData
    {
        [Header("Production")]
        [SerializeField] private MachineCatagory machineCatagory;
        [SerializeField] private ItemType producedItemType;
        [SerializeField] private float productionInterval = 3f;
        [SerializeField] private int maxCapacity = 10;

        [Header("Upgrade")]
        [SerializeField] private int speedUpgradeCost = 50;
        [SerializeField] private int capacityUpgradeCost = 75;
        [SerializeField] private float speedMultiplier = 0.8f;
        [SerializeField] private int capacityIncrease = 5;

        // --- Public Properties (Değişkenlere dışarıdan erişim için) ---

        public MachineCatagory MachineCatagory => machineCatagory;
        public ItemType ProducedItemType => producedItemType;
        public float ProductionInterval => productionInterval;
        public int MaxCapacity => maxCapacity;
        public int SpeedUpgradeCost => speedUpgradeCost;
        public int CapacityUpgradeCost => capacityUpgradeCost;
        public float SpeedMultiplier => speedMultiplier;
        public int CapacityIncrease => capacityIncrease;
    }

    public enum MachineCatagory
    {
        None = 0,
        Comic = 1,
        Figure = 2,
    }
}
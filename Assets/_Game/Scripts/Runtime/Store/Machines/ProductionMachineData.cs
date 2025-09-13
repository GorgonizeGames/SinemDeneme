using UnityEngine;
using Game.Runtime.Store.Areas;
using Game.Runtime.Items.Data;

namespace Game.Runtime.Store.Machines
{
    [CreateAssetMenu(fileName = "MachineData", menuName = "Game/Store/Machine Data")]
    public class ProductionMachineData : BasePurchasableData
    {
        [Header("Production Settings")]
        [SerializeField] private MachineCatagory machineCatagory;
        [SerializeField] private ItemType producedItemType;
        [SerializeField] private float productionInterval = 3f;
        [SerializeField] private int maxCapacity = 10;
        [SerializeField] private float itemRemovalDuration = 0.5f;

        [Header("Machine Interaction")]
        [SerializeField] private float interactionProcessingInterval = 0.5f;

        [SerializeField] private float interactionScaleAmount;

        [Header("Upgrade Settings")]
        [SerializeField] private int speedUpgradeCost = 50;
        [SerializeField] private int capacityUpgradeCost = 75;
        [SerializeField] private float speedUpgradeMultiplier = 0.8f;
        [SerializeField] private int capacityUpgradeIncrease = 5;
        [SerializeField] private int maxUpgradeLevel = 10;

        [Header("Production Animation")]
        [SerializeField] private float figureProductionAnimDuration = 2f;
        [SerializeField] private float comicProductionAnimDuration = 1.5f;
        [SerializeField] private float coloringArmAnimSpeed = 0.2f;
        [SerializeField] private float productionLineRotateSpeed = 0.2f;

        [Header("Machine Visual Effects")]
        [SerializeField] private bool enableMachineVFX = true;
        [SerializeField] private bool enableGearAnimation = true;
        [SerializeField] private bool enableConveyorMovement = true;
        [SerializeField] private float gearRotationSpeed = 2f;
        [SerializeField] private float conveyorScrollSpeed = 1f;

        [Header("Performance Settings")]
        [SerializeField] private bool enablePerformanceTracking = false;
        [SerializeField] private float performanceUpdateInterval = 1f;

        // Machine Properties
        public MachineCatagory MachineCatagory => machineCatagory;
        public ItemType ProducedItemType => producedItemType;
        public float ProductionInterval => productionInterval;
        public int MaxCapacity => maxCapacity;
        public float ItemRemovalDuration => itemRemovalDuration;
        public float InteractionProcessingInterval => interactionProcessingInterval;
        public int SpeedUpgradeCost => speedUpgradeCost;
        public int CapacityUpgradeCost => capacityUpgradeCost;
        public float SpeedUpgradeMultiplier => speedUpgradeMultiplier;
        public int CapacityUpgradeIncrease => capacityUpgradeIncrease;
        public int MaxUpgradeLevel => maxUpgradeLevel;
        public float FigureProductionAnimDuration => figureProductionAnimDuration;
        public float ComicProductionAnimDuration => comicProductionAnimDuration;
        public float ColoringArmAnimSpeed => coloringArmAnimSpeed;
        public float ProductionLineRotateSpeed => productionLineRotateSpeed;
        public bool EnableMachineVFX => enableMachineVFX;
        public bool EnableGearAnimation => enableGearAnimation;
        public bool EnableConveyorMovement => enableConveyorMovement;
        public float GearRotationSpeed => gearRotationSpeed;
        public float ConveyorScrollSpeed => conveyorScrollSpeed;
        public bool EnablePerformanceTracking => enablePerformanceTracking;
        public float PerformanceUpdateInterval => performanceUpdateInterval;

        public float InteractionScaleAmount => interactionScaleAmount;

        public override AreaType AreaType => AreaType.Machine;
        public override bool AllowEmployeeInteraction => true; // Machines allow employees

        // Machine-specific helper methods
        public float GetUpgradedProductionInterval(int upgradeLevel)
        {
            float multiplier = Mathf.Pow(speedUpgradeMultiplier, upgradeLevel);
            return productionInterval * multiplier;
        }

        public int GetUpgradedCapacity(int upgradeLevel)
        {
            return maxCapacity + (capacityUpgradeIncrease * upgradeLevel);
        }

        public int GetUpgradeCost(UpgradeType upgradeType, int currentLevel)
        {
            int baseCost = upgradeType == UpgradeType.Speed ? speedUpgradeCost : capacityUpgradeCost;
            return Mathf.RoundToInt(baseCost * Mathf.Pow(1.5f, currentLevel));
        }

        public bool CanUpgrade(UpgradeType upgradeType, int currentLevel)
        {
            return currentLevel < maxUpgradeLevel;
        }

        public float GetProductionEfficiency(int speedUpgradeLevel)
        {
            float originalInterval = productionInterval;
            float upgradedInterval = GetUpgradedProductionInterval(speedUpgradeLevel);
            return originalInterval / upgradedInterval;
        }

        public string GetMachineDisplayName()
        {
            return $"{machineCatagory} Machine ({producedItemType})";
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            // Machine-specific validations
            productionInterval = Mathf.Max(0.1f, productionInterval);
            maxCapacity = Mathf.Max(1, maxCapacity);
            itemRemovalDuration = Mathf.Max(0.1f, itemRemovalDuration);
            interactionProcessingInterval = Mathf.Max(0.1f, interactionProcessingInterval);

            speedUpgradeMultiplier = Mathf.Clamp(speedUpgradeMultiplier, 0.1f, 0.95f);
            capacityUpgradeIncrease = Mathf.Max(1, capacityUpgradeIncrease);
            maxUpgradeLevel = Mathf.Max(1, maxUpgradeLevel);

            figureProductionAnimDuration = Mathf.Max(0.5f, figureProductionAnimDuration);
            comicProductionAnimDuration = Mathf.Max(0.5f, comicProductionAnimDuration);
            coloringArmAnimSpeed = Mathf.Max(0.1f, coloringArmAnimSpeed);
            productionLineRotateSpeed = Mathf.Max(0.1f, productionLineRotateSpeed);

            gearRotationSpeed = Mathf.Max(0.1f, gearRotationSpeed);
            conveyorScrollSpeed = Mathf.Max(0.1f, conveyorScrollSpeed);
        }
    }

    // Enums from original code
    public enum MachineCatagory
    {
        None = 0,
        Comic = 1,
        Figure = 2,
    }

    public enum UpgradeType
    {
        Speed,
        Capacity
    }
}
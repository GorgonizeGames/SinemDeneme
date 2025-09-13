using UnityEngine;
using Game.Runtime.Store.Areas;
using Game.Runtime.Items.Data;

namespace Game.Runtime.Store.Shelves
{
    [CreateAssetMenu(fileName = "ShelfData", menuName = "Game/Store/Shelf Data")]
    public class DisplayShelfData : BasePurchasableData
    {
        [Header("Shelf Configuration")]
        [SerializeField] private ItemType acceptedItemType;
        [SerializeField] private int maxDisplayItems = 12;
        [SerializeField] private float restockThreshold = 0.3f;
        [SerializeField] private bool autoArrange = true;

        [Header("Shelf Interaction")]
        [SerializeField] private float stockingAnimationDuration = 0.3f;
        [SerializeField] private float purchaseAnimationDuration = 0.2f;
        [SerializeField] private float rearrangeAnimationDuration = 0.3f;

        [Header("Customer Behavior")]
        [SerializeField] private float customerAttraction = 1f;
        [SerializeField] private float browsingTime = 2f;
        [SerializeField] private int maxCustomersAtOnce = 3;

        [Header("Shelf Visual Effects")]
        [SerializeField] private bool enableStockingVFX = true;
        [SerializeField] private bool enableShelfPurchaseVFX = true;
        [SerializeField] private float interactionScaleAmount = 1.02f;

        // Shelf Properties
        public ItemType AcceptedItemType => acceptedItemType;
        public int MaxDisplayItems => maxDisplayItems;
        public float RestockThreshold => restockThreshold;
        public bool AutoArrange => autoArrange;
        public float StockingAnimationDuration => stockingAnimationDuration;
        public float PurchaseAnimationDuration => purchaseAnimationDuration;
        public float RearrangeAnimationDuration => rearrangeAnimationDuration;
        public float CustomerAttraction => customerAttraction;
        public float BrowsingTime => browsingTime;
        public int MaxCustomersAtOnce => maxCustomersAtOnce;
        public bool EnableStockingVFX => enableStockingVFX;
        public bool EnableShelfPurchaseVFX => enableShelfPurchaseVFX;
        public float InteractionScaleAmount => interactionScaleAmount;
        public override AreaType AreaType => AreaType.Shelf;
        public override bool AllowEmployeeInteraction => true; // Shelves allow employees for stocking

        // Shelf-specific helper methods
        public bool NeedsRestock(int currentItems)
        {
            if (maxDisplayItems <= 0) return false;
            float currentPercentage = (float)currentItems / maxDisplayItems;
            return currentPercentage <= restockThreshold;
        }

        public int GetRestockAmount(int currentItems)
        {
            return Mathf.Max(0, maxDisplayItems - currentItems);
        }

        public float GetStockPercentage(int currentItems)
        {
            return maxDisplayItems > 0 ? (float)currentItems / maxDisplayItems : 0f;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            // Shelf-specific validations
            maxDisplayItems = Mathf.Max(1, maxDisplayItems);
            restockThreshold = Mathf.Clamp01(restockThreshold);
            stockingAnimationDuration = Mathf.Max(0.1f, stockingAnimationDuration);
            purchaseAnimationDuration = Mathf.Max(0.1f, purchaseAnimationDuration);
            rearrangeAnimationDuration = Mathf.Max(0.1f, rearrangeAnimationDuration);
            customerAttraction = Mathf.Max(0f, customerAttraction);
            browsingTime = Mathf.Max(0.5f, browsingTime);
            maxCustomersAtOnce = Mathf.Max(1, maxCustomersAtOnce);
            interactionScaleAmount = Mathf.Max(1f, interactionScaleAmount);
        }
    }
}
using UnityEngine;
using Game.Runtime.Core.Data;
using Game.Runtime.Interactions.Interfaces;

namespace Game.Runtime.Store.Areas
{
    /// <summary>
    /// Base purchasable data - tüm satın alınabilir alanlar için temel özellikler
    /// Simplified architecture'dan esinlenerek, interface gerektirmeden data-driven yaklaşım
    /// </summary>
    public abstract class BasePurchasableData : BaseDataModel
    {
        [Header("Area Information")]
        [SerializeField] protected AreaType areaType;
        [SerializeField] protected bool isLastArea = false;

        [Header("Purchase Settings")]
        [SerializeField] protected int purchaseCost = 100;
        [SerializeField] protected int unlockLevel = 1;
        [SerializeField] protected bool requiresOfflineProgress = false;

        [Header("Purchase Animation & Timing")]
        [SerializeField] protected float purchaseDuration = 2f;
        [SerializeField] protected float purchaseScaleAmount = 1.1f;
        [SerializeField] protected float purchaseAnimDuration = 0.3f;
        [SerializeField] protected float activationScaleDuration = 0.5f;
        [SerializeField] protected float visualResetDuration = 0.2f;

        [Header("Interaction Settings")]
        [SerializeField] protected InteractionType interactionType;
        [SerializeField] protected bool allowPlayerInteraction = true;
        [SerializeField] protected bool allowEmployeeInteraction = false;

        [Header("Visual Effects")]
        [SerializeField] protected bool enablePurchaseVFX = true;
        [SerializeField] protected bool enableMoneyDropEffect = true;
        [SerializeField] protected bool enableScaleAnimation = true;
        [SerializeField] protected Color areaColor = Color.white;

        [Header("UI Settings")]
        [SerializeField] protected bool showCostUI = true;
        [SerializeField] protected bool showProgressBar = true;
        [SerializeField] protected bool showAreaInfo = true;
        [SerializeField] protected string costFormat = "${0}";

        [Header("Audio Settings")]
        [SerializeField] protected bool enablePurchaseAudio = true;
        [SerializeField] protected bool enableMoneyDropAudio = true;
        [SerializeField] protected float audioVolume = 1f;

        [Header("Economy Settings")]
        [SerializeField] protected bool allowPartialPayment = true;
        [SerializeField] protected float partialPaymentRate = 0.5f;
        [SerializeField] protected bool refundOnCancel = false;

        // Public Properties - Virtual yaparak override edilebilir
        public virtual AreaType AreaType => areaType;
        public virtual bool IsLastArea => isLastArea;
        public virtual int PurchaseCost => purchaseCost;
        public virtual int UnlockLevel => unlockLevel;
        public virtual bool RequiresOfflineProgress => requiresOfflineProgress;
        public virtual float PurchaseDuration => purchaseDuration;
        public virtual float PurchaseScaleAmount => purchaseScaleAmount;
        public virtual float PurchaseAnimDuration => purchaseAnimDuration;
        public virtual float ActivationScaleDuration => activationScaleDuration;
        public virtual float VisualResetDuration => visualResetDuration;
        public virtual InteractionType InteractionType => interactionType;
        public virtual bool AllowPlayerInteraction => allowPlayerInteraction;
        public virtual bool AllowEmployeeInteraction => allowEmployeeInteraction;
        public virtual bool EnablePurchaseVFX => enablePurchaseVFX;
        public virtual bool EnableMoneyDropEffect => enableMoneyDropEffect;
        public virtual bool EnableScaleAnimation => enableScaleAnimation;
        public virtual bool ShowCostUI => showCostUI;
        public virtual bool ShowProgressBar => showProgressBar;
        public virtual bool ShowAreaInfo => showAreaInfo;
        public virtual string CostFormat => costFormat;
        public virtual bool EnablePurchaseAudio => enablePurchaseAudio;
        public virtual bool EnableMoneyDropAudio => enableMoneyDropAudio;
        public virtual float AudioVolume => audioVolume;
        public virtual bool AllowPartialPayment => allowPartialPayment;
        public virtual float PartialPaymentRate => partialPaymentRate;
        public virtual bool RefundOnCancel => refundOnCancel;

        // Helper Methods
        public virtual string GetFormattedCost()
        {
            return string.Format(costFormat, purchaseCost);
        }

        public virtual float GetPurchaseProgressRate()
        {
            return purchaseDuration > 0 ? 1f / purchaseDuration : 1f;
        }

        public virtual float GetPartialPaymentAmount(int availableMoney)
        {
            if (!allowPartialPayment) return 0f;
            return Mathf.Min(availableMoney, Mathf.RoundToInt(partialPaymentRate));
        }

        public virtual bool CanAfford(int availableMoney)
        {
            return availableMoney >= purchaseCost;
        }

        public virtual bool CanStartPurchase(int availableMoney)
        {
            return allowPartialPayment ? availableMoney > 0 : CanAfford(availableMoney);
        }

        public virtual string GetAreaDisplayName()
        {
            return $" ({areaType})";
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            // Positive değerler sağla
            purchaseCost = Mathf.Max(1, purchaseCost);
            unlockLevel = Mathf.Max(1, unlockLevel);
            purchaseDuration = Mathf.Max(0.1f, purchaseDuration);
            
            // Animasyon değerleri makul olsun
            purchaseScaleAmount = Mathf.Max(1f, purchaseScaleAmount);
            purchaseAnimDuration = Mathf.Max(0.1f, purchaseAnimDuration);
            activationScaleDuration = Mathf.Max(0.1f, activationScaleDuration);
            visualResetDuration = Mathf.Max(0.1f, visualResetDuration);

            audioVolume = Mathf.Clamp01(audioVolume);
            partialPaymentRate = Mathf.Max(0.1f, partialPaymentRate);
        }
    }

    public enum AreaType
    {
        Machine,
        Shelf,
    }
}
using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Interfaces;
using Game.Runtime.Interactions.Interfaces;
using DG.Tweening;
using Game.Runtime.Character;
using Game.Runtime.Core.Extensions;

namespace Game.Runtime.Store.Areas
{
    public abstract class PurchasableArea : MonoBehaviour, IInteractable
    {
        [Header("Area Settings")]
        [SerializeField] protected PurchasableAreaData areaData;
        [SerializeField] protected Transform purchaseVisual;
        [SerializeField] protected GameObject activeVisual;

        [Header("Purchase Timing")]
        [SerializeField] protected float purchaseDuration = 2f;

        [Header("Visual Effects")]
        [SerializeField] protected float purchaseScaleAmount = 1.1f;
        [SerializeField] protected float purchaseAnimDuration = 0.3f;
        [SerializeField] protected float activationScaleDuration = 0.5f;
        [SerializeField] protected float visualResetDuration = 0.2f;

        [Header("UI Elements")]
        [SerializeField] protected Canvas areaCanvas;
        [SerializeField] protected UnityEngine.UI.Image progressBar;
        [SerializeField] protected TMPro.TextMeshProUGUI costText;

        [Header("Interaction Settings")]
        [SerializeField] protected InteractionType interactionType = InteractionType.Purchasable;
        [SerializeField] protected InteractionPriority interactionPriority = InteractionPriority.Medium;

        [Inject] protected IGameManager _gameManager;
        [Inject] protected IEconomyService _economyService;

        protected PurchasableAreaState _currentState = PurchasableAreaState.Locked;
        protected float _purchaseProgress = 0f;
        protected IInteractor _purchasingInteractor;

        // Performance optimizations
        private readonly System.Collections.Generic.List<Tween> _activeTweens = new System.Collections.Generic.List<Tween>();
        private Tween _purchaseVisualTween;
        private Tween _activationTween;

        // Cached strings to avoid allocation in hot paths
        private string _cachedCostText;
        private bool _costTextDirty = true;

        // IInteractable Implementation
        public virtual InteractionType InteractionType => interactionType;
        public virtual InteractionPriority Priority => interactionPriority;

        // Events
        public System.Action<PurchasableArea> OnPurchased;
        public System.Action<PurchasableArea> OnActivated;

        // Properties
        public PurchasableAreaData Data => areaData;
        public PurchasableAreaState State => _currentState;
        public bool IsActive => _currentState == PurchasableAreaState.Active;
        public bool IsLocked => _currentState == PurchasableAreaState.Locked;
        public bool IsPurchasing => _currentState == PurchasableAreaState.Purchasing;

        protected virtual void Awake()
        {
            try
            {
                this.InjectDependencies();
                PrepareCachedStrings();
                LoadState();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during PurchasableArea Awake: {e.Message}", this);
            }
        }

        protected virtual void Start()
        {
            try
            {
                UpdateVisuals();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during PurchasableArea Start: {e.Message}", this);
            }
        }

        private void PrepareCachedStrings()
        {
            if (areaData != null)
            {
                _cachedCostText = $"${areaData.PurchaseCost}";
                _costTextDirty = false;
            }
        }

        // ==================== IInteractable Implementation ====================

        public virtual bool CanInteract(IInteractor interactor)
        {
            if (interactor?.Character == null) return false;

            try
            {
                if (IsLocked)
                {
                    if (interactor.Character is PlayerCharacterController)
                    {
                        return _economyService != null && _economyService.CanAfford(areaData.PurchaseCost);
                    }
                    return false;
                }

                if (IsActive)
                {
                    return CanInteractWhenActive(interactor);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in CanInteract: {e.Message}", this);
            }

            return false;
        }

        public virtual void OnInteractionStart(IInteractor interactor)
        {
            if (interactor?.Character == null) return;

            try
            {
                if (IsLocked && interactor.Character is PlayerCharacterController)
                {
                    StartPurchaseProcess(interactor);
                }
                else if (IsActive)
                {
                    OnActiveInteractionStart(interactor);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in OnInteractionStart: {e.Message}", this);
            }
        }

        public virtual void OnInteractionContinue(IInteractor interactor)
        {
            try
            {
                if (IsPurchasing && _purchasingInteractor == interactor)
                {
                    ContinuePurchaseProcess();
                }
                else if (IsActive)
                {
                    OnActiveInteractionContinue(interactor);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in OnInteractionContinue: {e.Message}", this);
            }
        }

        public virtual void OnInteractionEnd(IInteractor interactor)
        {
            try
            {
                if (IsPurchasing && _purchasingInteractor == interactor)
                {
                    CancelPurchaseProcess();
                }
                else if (IsActive)
                {
                    OnActiveInteractionEnd(interactor);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in OnInteractionEnd: {e.Message}", this);
            }
        }

        // ==================== Override Methods for Active State ====================

        protected abstract bool CanInteractWhenActive(IInteractor interactor);
        protected abstract void OnActiveInteractionStart(IInteractor interactor);
        protected abstract void OnActiveInteractionContinue(IInteractor interactor);
        protected abstract void OnActiveInteractionEnd(IInteractor interactor);

        // ==================== Purchase Process ====================

        protected virtual void StartPurchaseProcess(IInteractor interactor)
        {
            try
            {
                _currentState = PurchasableAreaState.Purchasing;
                _purchasingInteractor = interactor;
                _purchaseProgress = 0f;

                // Visual feedback
                if (progressBar != null)
                {
                    progressBar.fillAmount = 0f;
                    progressBar.transform.parent.gameObject.SetActive(true);
                }

                if (purchaseVisual != null)
                {
                    CleanupPurchaseVisualTween();
                    _purchaseVisualTween = purchaseVisual.transform.DOScale(purchaseScaleAmount, purchaseAnimDuration)
                        .SetLoops(-1, LoopType.Yoyo);
                    
                    if (_purchaseVisualTween != null)
                    {
                        _activeTweens.Add(_purchaseVisualTween);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error starting purchase process: {e.Message}", this);
                CancelPurchaseProcess();
            }
        }

        protected virtual void ContinuePurchaseProcess()
        {
            try
            {
                _purchaseProgress += Time.deltaTime / purchaseDuration;

                if (progressBar != null)
                    progressBar.fillAmount = _purchaseProgress;

                if (_purchaseProgress >= 1f)
                {
                    CompletePurchase();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error continuing purchase process: {e.Message}", this);
                CancelPurchaseProcess();
            }
        }

        protected virtual void CancelPurchaseProcess()
        {
            try
            {
                _currentState = PurchasableAreaState.Locked;
                _purchasingInteractor = null;
                _purchaseProgress = 0f;

                // Reset visuals
                if (progressBar != null)
                {
                    progressBar.fillAmount = 0f;
                    progressBar.transform.parent.gameObject.SetActive(false);
                }

                CleanupPurchaseVisualTween();

                if (purchaseVisual != null)
                {
                    Tween resetTween = purchaseVisual.transform.DOScale(1f, visualResetDuration);
                    if (resetTween != null)
                    {
                        _activeTweens.Add(resetTween);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error canceling purchase process: {e.Message}", this);
            }
        }

        protected virtual void CompletePurchase()
        {
            try
            {
                if (_economyService != null && _economyService.TrySpend(areaData.PurchaseCost))
                {
                    SetState(PurchasableAreaState.Active);
                    OnPurchased?.Invoke(this);

                    // Visual feedback
                    if (activeVisual != null)
                    {
                        activeVisual.transform.localScale = Vector3.zero;
                        CleanupActivationTween();
                        _activationTween = activeVisual.transform.DOScale(1f, activationScaleDuration).SetEase(Ease.OutBack);
                        
                        if (_activationTween != null)
                        {
                            _activeTweens.Add(_activationTween);
                        }
                    }

                    CleanupPurchaseVisualTween();
                    _purchasingInteractor = null;
                }
                else
                {
                    CancelPurchaseProcess();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error completing purchase: {e.Message}", this);
                CancelPurchaseProcess();
            }
        }

        // ==================== State Management ====================

        protected virtual void LoadState()
        {
            try
            {
                SetState(_currentState);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading state: {e.Message}", this);
            }
        }

        public virtual void SetState(PurchasableAreaState newState)
        {
            try
            {
                _currentState = newState;
                UpdateVisuals();

                if (newState == PurchasableAreaState.Active)
                {
                    OnActivated?.Invoke(this);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error setting state: {e.Message}", this);
            }
        }

        protected virtual void UpdateVisuals()
        {
            try
            {
                if (purchaseVisual != null)
                    purchaseVisual.gameObject.SetActive(_currentState == PurchasableAreaState.Locked);

                if (activeVisual != null)
                    activeVisual.gameObject.SetActive(_currentState == PurchasableAreaState.Active);

                if (areaCanvas != null)
                    areaCanvas.gameObject.SetActive(_currentState == PurchasableAreaState.Locked);

                // Use cached string instead of creating new one
                if (costText != null && _costTextDirty && !string.IsNullOrEmpty(_cachedCostText))
                {
                    costText.text = _cachedCostText;
                    _costTextDirty = false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error updating visuals: {e.Message}", this);
            }
        }

        // ==================== CLEANUP ====================

        private void CleanupPurchaseVisualTween()
        {
            if (_purchaseVisualTween != null && _purchaseVisualTween.IsActive())
            {
                _purchaseVisualTween.Kill();
                _activeTweens.Remove(_purchaseVisualTween);
                _purchaseVisualTween = null;
            }
        }

        private void CleanupActivationTween()
        {
            if (_activationTween != null && _activationTween.IsActive())
            {
                _activationTween.Kill();
                _activeTweens.Remove(_activationTween);
                _activationTween = null;
            }
        }

        private void CleanupAllTweens()
        {
            try
            {
                foreach (var tween in _activeTweens)
                {
                    if (tween != null && tween.IsActive())
                    {
                        tween.Kill();
                    }
                }
                _activeTweens.Clear();

                CleanupPurchaseVisualTween();
                CleanupActivationTween();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error cleaning up tweens: {e.Message}");
            }
        }

        protected virtual void OnDestroy()
        {
            CleanupAllTweens();
        }

        void OnDisable()
        {
            CleanupAllTweens();
        }
    }

    public enum PurchasableAreaState
    {
        Locked,
        Purchasing,
        Active
    }
}
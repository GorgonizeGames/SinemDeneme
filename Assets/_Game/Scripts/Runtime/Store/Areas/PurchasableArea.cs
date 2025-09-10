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
            this.InjectDependencies();
            LoadState();
        }

        protected virtual void Start()
        {
            UpdateVisuals();
        }

        // ==================== IInteractable Implementation ====================

        public virtual bool CanInteract(IInteractor interactor)
        {
            // Locked state - sadece Player satın alabilir
            if (IsLocked)
            {
                // Sadece Player satın alabilir
                if (interactor.Character is PlayerCharacterController)
                {
                    return _economyService != null && _economyService.CanAfford(areaData.PurchaseCost);
                }
                return false;
            }

            // Active state - alt sınıflar override edecek
            if (IsActive)
            {
                return CanInteractWhenActive(interactor);
            }

            return false;
        }

        public virtual void OnInteractionStart(IInteractor interactor)
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

        public virtual void OnInteractionContinue(IInteractor interactor)
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

        public virtual void OnInteractionEnd(IInteractor interactor)
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

        // ==================== Override Methods for Active State ====================

        protected abstract bool CanInteractWhenActive(IInteractor interactor);
        protected abstract void OnActiveInteractionStart(IInteractor interactor);
        protected abstract void OnActiveInteractionContinue(IInteractor interactor);
        protected abstract void OnActiveInteractionEnd(IInteractor interactor);

        // ==================== Purchase Process ====================

        protected virtual void StartPurchaseProcess(IInteractor interactor)
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

            purchaseVisual?.transform.DOScale(1.1f, 0.3f).SetLoops(-1, LoopType.Yoyo);
        }

        protected virtual void ContinuePurchaseProcess()
        {
            _purchaseProgress += Time.deltaTime / 2f; // 2 saniyede tamamlanır

            if (progressBar != null)
                progressBar.fillAmount = _purchaseProgress;

            if (_purchaseProgress >= 1f)
            {
                CompletePurchase();
            }
        }

        protected virtual void CancelPurchaseProcess()
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

            DOTween.Kill(purchaseVisual);
            purchaseVisual?.transform.DOScale(1f, 0.2f);
        }

        protected virtual void CompletePurchase()
        {
            if (_economyService.TrySpend(areaData.PurchaseCost))
            {
                SetState(PurchasableAreaState.Active);
                OnPurchased?.Invoke(this);

                // Visual feedback
                if (activeVisual != null)
                {
                    activeVisual.transform.DOScale(0f, 0f);
                    activeVisual.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
                }

                DOTween.Kill(purchaseVisual);

                _purchasingInteractor = null;
            }
            else
            {
                CancelPurchaseProcess();
            }
        }

        // ==================== State Management ====================

        protected virtual void LoadState()
        {
            // Load from save system if needed
            // For now, default to locked
            SetState(_currentState);
        }

        public virtual void SetState(PurchasableAreaState newState)
        {
            _currentState = newState;
            UpdateVisuals();

            if (newState == PurchasableAreaState.Active)
            {
                OnActivated?.Invoke(this);
            }
        }

        protected virtual void UpdateVisuals()
        {
            if (purchaseVisual != null)
                purchaseVisual.gameObject.SetActive(_currentState == PurchasableAreaState.Locked);

            if (activeVisual != null)
                activeVisual.gameObject.SetActive(_currentState == PurchasableAreaState.Active);

            if (areaCanvas != null)
                areaCanvas.gameObject.SetActive(_currentState == PurchasableAreaState.Locked);

            if (costText != null)
                costText.text = $"${areaData.PurchaseCost}";
        }
    }

    public enum PurchasableAreaState
    {
        Locked,
        Purchasing,
        Active
    }
}
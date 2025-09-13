using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Interfaces;
using Game.Runtime.Interactions.Interfaces;
using Game.Runtime.Character;
using Game.Runtime.Core.Extensions;

namespace Game.Runtime.Store.Areas
{
    /// <summary>
    /// Generic purchasable area that works with any BasePurchasableData
    /// Eski kodlarınızdaki logic'i simplified architecture ile birleştirir
    /// </summary>
    public abstract class PurchasableArea<T> : MonoBehaviour, IInteractable where T : BasePurchasableData
    {
        [Header("Area Configuration")]
        [SerializeField] protected T areaData;
        [SerializeField] protected string areaId;

        [Header("Visual References - Colliders")]
        [SerializeField] protected Collider activeCollider;
        [SerializeField] protected Collider passiveCollider;

        [Header("Visual References - GameObjects")]
        [SerializeField] protected GameObject activeVisual;
        [SerializeField] protected GameObject passiveVisual;
        [SerializeField] protected GameObject nextObjectCamera;
        [SerializeField] protected List<GameObject> objectsToOpenOnPurchase = new List<GameObject>();
        [SerializeField] protected List<GameObject> objectsToCloseOnPurchase = new List<GameObject>();

        [Header("Visual References - UI Elements")]
        [SerializeField] protected Canvas areaCanvas;
        [SerializeField] protected TMPro.TextMeshProUGUI costText;
        [SerializeField] protected UnityEngine.UI.Image outlineImage;
        [SerializeField] protected UnityEngine.UI.Image backgroundColorImage;
        [SerializeField] protected UnityEngine.UI.Image progressBarImage;
        [SerializeField] protected UnityEngine.UI.Image moneyIconImage;

        [Header("Visual References - Effects")]
        [SerializeField] protected ParticleSystem purchaseVFX;
        [SerializeField] protected Transform moneyDropTarget;

        [Inject] protected IGameManager _gameManager;
        [Inject] protected IEconomyService _economyService;

        // Core State
        protected PurchasableAreaState _currentState = PurchasableAreaState.Locked;
        protected IInteractor _purchasingInteractor;

        // Purchase Progress - Eski koddan inspired
        protected float _purchaseProgress = 0f;
        protected float _totalSpent = 0f;
        protected bool _canInteract = false;

        // Animation Management
        protected readonly List<Tween> _activeTweens = new List<Tween>();
        protected Tween _purchaseTween;
        protected Tween _activationTween;
        protected Tween _interactionTween;

        // Cached Values for Performance
        private string _cachedCostText;
        private bool _costTextDirty = true;

        // Eski koddan - character tracking
        protected List<BaseCharacterController> _interactingCharacters = new List<BaseCharacterController>();

        // IInteractable Implementation
        public virtual InteractionType InteractionType => areaData?.InteractionType ?? InteractionType.Purchasable;

        // Events
        public System.Action<PurchasableArea<T>> OnAreaPurchased;
        public System.Action<PurchasableArea<T>> OnAreaActivated;
        public System.Action<PurchasableArea<T>, float> OnPurchaseProgress;

        // Properties
        public T Data => areaData;
        public PurchasableAreaState CurrentState => _currentState;
        public bool IsActive => _currentState == PurchasableAreaState.Active;
        public bool IsLocked => _currentState == PurchasableAreaState.Locked;
        public bool IsPurchasing => _currentState == PurchasableAreaState.Purchasing;
        public int Cost => areaData?.PurchaseCost ?? 100;
        public string AreaId => areaId;
        public float PurchaseProgress => _purchaseProgress;
        public bool CanInteract => _canInteract;

        protected virtual void Awake()
        {
            try
            {
                this.InjectDependencies();
                ValidateAreaData();
                InitializeColliders();
                PrepareCachedStrings();
                LoadAreaState(); // Eski koddan - save/load system
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
                UpdateVisualState();
                SetupGameStateHandling();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during PurchasableArea Start: {e.Message}", this);
            }
        }

        private void ValidateAreaData()
        {
            if (areaData == null)
            {
                Debug.LogError($"[{gameObject.name}] AreaData of type {typeof(T).Name} is not assigned!", this);
                enabled = false;
                return;
            }

            // Auto-generate ID if empty
            if (string.IsNullOrEmpty(areaId))
            {
                areaId = gameObject.name.Replace(" ", "_").Replace("(", "").Replace(")", "");
            }
        }

        private void InitializeColliders()
        {
            if (activeCollider != null)
                activeCollider.enabled = false;

            if (passiveCollider != null)
                passiveCollider.enabled = false;
        }

        private void PrepareCachedStrings()
        {
            if (areaData != null)
            {
                _cachedCostText = areaData.GetFormattedCost();
                _costTextDirty = false;
            }
        }

        private void SetupGameStateHandling()
        {
            if (_gameManager != null)
            {
                _gameManager.OnStateChanged += HandleGameStateChange;
            }
        }

        protected virtual void HandleGameStateChange(GameState newState)
        {
            // Game durdurulduğunda purchase işlemini iptal et
            if (newState != GameState.Playing && IsPurchasing)
            {
                CancelPurchaseProcess();
            }
        }

        // ==================== IInteractable Implementation ====================

        public virtual bool OnCanInteract(IInteractor interactor)
        {
            if (interactor?.Character == null || areaData == null) return false;

            try
            {
                var character = interactor.Character;

                // Character type kontrolü - base class properties kullanarak
                if (character is PlayerCharacterController && !areaData.AllowPlayerInteraction)
                    return false;

                if (IsEmployee(character) && !areaData.AllowEmployeeInteraction)
                    return false;

                if (IsLocked)
                {
                    // Sadece player satın alabilir
                    if (character is PlayerCharacterController)
                    {
                        return _economyService != null && areaData.CanStartPurchase(_economyService.CurrentMoney);
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
                // Character'ı listeye ekle
                if (!_interactingCharacters.Contains(interactor.Character))
                {
                    _interactingCharacters.Add(interactor.Character);
                }

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
                // Character'ı listeden çıkar
                if (_interactingCharacters.Contains(interactor.Character))
                {
                    _interactingCharacters.Remove(interactor.Character);
                }

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

        // ==================== Abstract Methods ====================

        protected abstract bool CanInteractWhenActive(IInteractor interactor);
        protected abstract void OnActiveInteractionStart(IInteractor interactor);
        protected abstract void OnActiveInteractionContinue(IInteractor interactor);
        protected abstract void OnActiveInteractionEnd(IInteractor interactor);

        // ==================== Purchase Process - Eski koddan inspired ====================

        protected virtual void StartPurchaseProcess(IInteractor interactor)
        {
            if (areaData == null) return;

            try
            {
                _currentState = PurchasableAreaState.Purchasing;
                _purchasingInteractor = interactor;
                _purchaseProgress = 0f;
                _totalSpent = 0f;
                _canInteract = true;

                // Visual feedback - eski koddan inspired
                if (areaData.ShowProgressBar && backgroundColorImage != null)
                {
                    backgroundColorImage.fillAmount = 0f;
                }

                // Outline scale animation - eski koddan
                if (areaData.EnableScaleAnimation && outlineImage != null)
                {
                    CleanupInteractionTween();
                    _interactionTween = outlineImage.transform.DOScale(
                        Vector3.one * areaData.PurchaseScaleAmount,
                        areaData.PurchaseAnimDuration);
                    if (_interactionTween != null)
                        _activeTweens.Add(_interactionTween);
                }

                StartPurchaseAnimation();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error starting purchase process: {e.Message}", this);
                CancelPurchaseProcess();
            }
        }

        protected virtual void ContinuePurchaseProcess()
        {
            if (_economyService == null || areaData == null) return;

            try
            {
                int currentMoney = _economyService.CurrentMoney;

                if (areaData.AllowPartialPayment)
                {
                    // Partial payment mode - eski koddan inspired
                    float paymentAmount = areaData.GetPartialPaymentAmount(currentMoney) * Time.deltaTime;
                    int intPaymentAmount = Mathf.FloorToInt(paymentAmount);

                    if (intPaymentAmount > 0 && _economyService.TrySpend(intPaymentAmount))
                    {
                        _totalSpent += intPaymentAmount;
                        _purchaseProgress = _totalSpent / areaData.PurchaseCost;

                        UpdatePurchaseVisuals();
                        OnPurchaseProgress?.Invoke(this, _purchaseProgress);

                        if (areaData.EnableMoneyDropAudio)
                        {
                            CreateMoneyDropEffect();
                        }
                    }
                }
                else
                {
                    // Traditional progress mode
                    _purchaseProgress += Time.deltaTime * areaData.GetPurchaseProgressRate();
                    UpdatePurchaseVisuals();
                    OnPurchaseProgress?.Invoke(this, _purchaseProgress);
                }

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
            if (areaData == null) return;

            try
            {
                // Handle refund if enabled
                if (areaData.RefundOnCancel && _totalSpent > 0)
                {
                    _economyService?.AddMoney(Mathf.FloorToInt(_totalSpent));
                }

                _currentState = PurchasableAreaState.Locked;
                _purchasingInteractor = null;
                _purchaseProgress = 0f;
                _totalSpent = 0f;
                _canInteract = false;

                UpdatePurchaseVisuals();
                ResetPurchaseAnimations();
                SaveAreaState(); // Eski koddan
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error canceling purchase process: {e.Message}", this);
            }
        }

        protected virtual void CompletePurchase()
        {
            if (areaData == null) return;

            try
            {
                // Final payment calculation
                if (areaData.AllowPartialPayment)
                {
                    int remainingCost = areaData.PurchaseCost - Mathf.FloorToInt(_totalSpent);
                    if (remainingCost > 0 && !_economyService.TrySpend(remainingCost))
                    {
                        CancelPurchaseProcess();
                        return;
                    }
                }
                else
                {
                    if (!_economyService.TrySpend(areaData.PurchaseCost))
                    {
                        CancelPurchaseProcess();
                        return;
                    }
                }

                SetAreaState(PurchasableAreaState.Active);
                OnAreaPurchased?.Invoke(this);

                // Visual feedback
                if (areaData.EnablePurchaseVFX && purchaseVFX != null)
                    purchaseVFX.Play();

                if (areaData.EnablePurchaseAudio)
                {
                    // SoundManager.PlaySound - eski koddan
                }

                _purchasingInteractor = null;
                OnPurchaseCompleted();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error completing purchase: {e.Message}", this);
                CancelPurchaseProcess();
            }
        }

        protected virtual void OnPurchaseCompleted()
        {
            if (areaData == null) return;

            // Handle objects to open/close - eski koddan
            if (objectsToOpenOnPurchase != null)
            {
                foreach (GameObject obj in objectsToOpenOnPurchase)
                {
                    if (obj != null) obj.SetActive(true);
                }
            }

            if (objectsToCloseOnPurchase != null)
            {
                foreach (GameObject obj in objectsToCloseOnPurchase)
                {
                    if (obj != null) obj.SetActive(false);
                }
            }

            if (nextObjectCamera != null)
                nextObjectCamera.SetActive(true);

            // Last area logic - eski koddan
            if (areaData.IsLastArea)
            {
                
                // Floor progression logic burada olabilir
            }
        }

        // ==================== Animation Methods ====================

        private void StartPurchaseAnimation()
        {
            if (areaData == null) return;

            CleanupPurchaseTween();

            if (!areaData.AllowPartialPayment)
            {
                // Traditional progress bar animation
                _purchaseTween = DOTween.To(() => _purchaseProgress, x => _purchaseProgress = x, 1f, areaData.PurchaseDuration)
                    .OnUpdate(() =>
                    {
                        UpdatePurchaseVisuals();
                        OnPurchaseProgress?.Invoke(this, _purchaseProgress);
                    })
                    .OnComplete(() => CompletePurchase());

                if (_purchaseTween != null)
                    _activeTweens.Add(_purchaseTween);
            }
        }

        private void UpdatePurchaseVisuals()
        {
            if (areaData == null) return;

            // Progress bar update
            if (areaData.ShowProgressBar && backgroundColorImage != null)
            {
                backgroundColorImage.fillAmount = _purchaseProgress;
            }

            // Cost text for partial payment - eski koddan inspired
            if (areaData.AllowPartialPayment && costText != null)
            {
                int remainingCost = Mathf.Max(0, areaData.PurchaseCost - Mathf.FloorToInt(_totalSpent));
                costText.text = string.Format(areaData.CostFormat, remainingCost);
            }
        }

        private void ResetPurchaseAnimations()
        {
            if (areaData == null) return;

            if (areaData.ShowProgressBar && backgroundColorImage != null)
            {
                backgroundColorImage.fillAmount = 0f;
            }

            if (areaData.EnableScaleAnimation && outlineImage != null)
            {
                CleanupInteractionTween();
                _interactionTween = outlineImage.transform.DOScale(Vector3.one, areaData.VisualResetDuration);
                if (_interactionTween != null)
                    _activeTweens.Add(_interactionTween);
            }

            if (costText != null && !string.IsNullOrEmpty(_cachedCostText))
            {
                costText.text = _cachedCostText;
            }
        }

        private void CreateMoneyDropEffect()
        {
            if (!areaData.EnableMoneyDropEffect || moneyDropTarget == null) return;

            // Eski koddan money drop animation
            // Money pool'dan money objesi al, animasyon yap
        }

        // ==================== State Management - Eski koddan inspired ====================

        public virtual void SetAreaState(PurchasableAreaState newState)
        {
            try
            {
                _currentState = newState;
                UpdateVisualState();
                SaveAreaState(); // Her state değişiminde kaydet

                if (newState == PurchasableAreaState.Active)
                {
                    OnAreaActivated?.Invoke(this);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error setting area state: {e.Message}", this);
            }
        }

        protected virtual void UpdateVisualState()
        {
            if (areaData == null) return;

            try
            {
                // Reset colliders
                if (passiveCollider != null) passiveCollider.enabled = false;
                if (activeCollider != null) activeCollider.enabled = false;

                // Reset visuals
                if (activeVisual != null) activeVisual.SetActive(false);
                if (passiveVisual != null) passiveVisual.SetActive(false);

                switch (_currentState)
                {
                    case PurchasableAreaState.Locked:
                        if (passiveVisual != null) passiveVisual.SetActive(true);
                        if (passiveCollider != null) passiveCollider.enabled = true;
                        if (areaCanvas != null) areaCanvas.gameObject.SetActive(areaData.ShowCostUI);
                        UpdateCostText();
                        break;

                    case PurchasableAreaState.Active:
                        if (activeVisual != null)
                        {
                            activeVisual.SetActive(true);

                            if (areaData.EnableScaleAnimation)
                            {
                                // Eski koddan inspired - scale animation
                                activeVisual.transform.localScale = Vector3.zero;
                                CleanupActivationTween();
                                _activationTween = activeVisual.transform
                                    .DOScale(Vector3.one, areaData.ActivationScaleDuration)
                                    .SetEase(Ease.OutBack)
                                    .OnComplete(() =>
                                    {
                                        if (activeCollider != null)
                                            activeCollider.enabled = true;
                                    });

                                if (_activationTween != null)
                                    _activeTweens.Add(_activationTween);
                            }
                            else
                            {
                                if (activeCollider != null)
                                    activeCollider.enabled = true;
                            }
                        }
                        if (areaCanvas != null) areaCanvas.gameObject.SetActive(false);
                        break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error updating visual state: {e.Message}", this);
            }
        }

        private void UpdateCostText()
        {
            if (areaData == null || !areaData.ShowCostUI) return;

            if (costText != null && _costTextDirty)
            {
                costText.text = _cachedCostText;
                _costTextDirty = false;
            }
        }

        // ==================== Save/Load System - Eski koddan ====================

        protected virtual void SaveAreaState()
        {
            if (string.IsNullOrEmpty(areaId)) return;

            try
            {
                string stateKey = areaId + "_State";
                string costKey = areaId + "_Cost";

                PlayerPrefs.SetInt(stateKey, (int)_currentState);
                PlayerPrefs.SetInt(costKey, Cost);
                PlayerPrefs.Save();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error saving area state: {e.Message}", this);
            }
        }

        protected virtual void LoadAreaState()
        {
            if (string.IsNullOrEmpty(areaId)) return;

            try
            {
                string stateKey = areaId + "_State";
                string costKey = areaId + "_Cost";

                if (PlayerPrefs.HasKey(stateKey))
                {
                    _currentState = (PurchasableAreaState)PlayerPrefs.GetInt(stateKey);
                }

                if (PlayerPrefs.HasKey(costKey))
                {
                    // Cost'u data'dan override etmeyelim, data'daki değer geçerli olsun
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading area state: {e.Message}", this);
            }
        }

        // ==================== Utility Methods ====================

        private bool IsEmployee(BaseCharacterController character)
        {
            if (character == null) return false;

            var aiController = character as Character.AI.AICharacterController;
            return aiController != null &&
                   aiController.Data?.CharacterType == Character.Interfaces.CharacterType.AI_Employee;
        }

        // ==================== Public API ====================

        public bool CanAfford() => areaData != null && _economyService != null && areaData.CanAfford(_economyService.CurrentMoney);
        public bool CanStartPurchase() => areaData != null && _economyService != null && areaData.CanStartPurchase(_economyService.CurrentMoney);
        public string GetDisplayName() => areaData?.GetAreaDisplayName() ?? gameObject.name;
        public float GetPurchaseProgressPercentage() => _purchaseProgress * 100f;

        // ==================== Cleanup ====================

        private void CleanupPurchaseTween()
        {
            if (_purchaseTween != null && _purchaseTween.IsActive())
            {
                _purchaseTween.Kill();
                _activeTweens.Remove(_purchaseTween);
                _purchaseTween = null;
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

        private void CleanupInteractionTween()
        {
            if (_interactionTween != null && _interactionTween.IsActive())
            {
                _interactionTween.Kill();
                _activeTweens.Remove(_interactionTween);
                _interactionTween = null;
            }
        }

        private void CleanupAllTweens()
        {
            foreach (var tween in _activeTweens)
            {
                if (tween != null && tween.IsActive())
                    tween.Kill();
            }
            _activeTweens.Clear();

            CleanupPurchaseTween();
            CleanupActivationTween();
            CleanupInteractionTween();
        }

        protected virtual void OnDestroy()
        {
            CleanupAllTweens();
            SaveAreaState();

            if (_gameManager != null)
            {
                _gameManager.OnStateChanged -= HandleGameStateChange;
            }
        }

        protected virtual void OnDisable()
        {
            CleanupAllTweens();
            SaveAreaState();
        }

        // ==================== Debug ====================

#if UNITY_EDITOR
        [ContextMenu("Show Area Stats")]
        private void ShowAreaStats()
        {
            if (areaData == null)
            {
                Debug.LogWarning("No area data assigned!");
                return;
            }

            Debug.Log($"Area Stats ({typeof(T).Name}):\n" +
                     $"Type: {areaData.AreaType}\n" +
                     $"Cost: {areaData.GetFormattedCost()}\n" +
                     $"State: {_currentState}\n" +
                     $"Progress: {GetPurchaseProgressPercentage():F1}%\n" +
                     $"Allow Player: {areaData.AllowPlayerInteraction}\n" +
                     $"Allow Employee: {areaData.AllowEmployeeInteraction}");
        }

        void OnValidate()
        {
            if (string.IsNullOrEmpty(areaId))
            {
                areaId = gameObject.name.Replace(" ", "_").Replace("(", "").Replace(")", "");
            }

            if (areaData != null)
            {
                _costTextDirty = true;
            }
        }

        bool IInteractable.CanInteract(IInteractor interactor)
        {
            throw new System.NotImplementedException();
        }
#endif
    }

    public enum PurchasableAreaState
    {
        Locked,
        Purchasing,
        Active
    }
}
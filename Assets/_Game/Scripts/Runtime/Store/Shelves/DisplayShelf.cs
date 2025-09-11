using UnityEngine;
using System.Collections.Generic;
using Game.Runtime.Store.Areas;
using Game.Runtime.Interactions.Interfaces;
using Game.Runtime.Items;
using Game.Runtime.Items.Data;
using Game.Runtime.Character;
using Game.Runtime.Character.Components;
using Game.Runtime.Character.Interfaces;
using Game.Runtime.Character.AI;
using Game.Runtime.Core.Extensions;
using Game.Runtime.Core.Performance;
using DG.Tweening;

namespace Game.Runtime.Store.Shelves
{
    public class DisplayShelf : PurchasableArea
    {
        [Header("Shelf Configuration")]
        [SerializeField] private DisplayShelfData shelfData;

        [Header("Display Settings")]
        [SerializeField] private Transform[] displaySlots;
        [SerializeField] private bool autoArrange = true;

        [Header("Customer Positions")]
        [SerializeField] private Transform[] customerBrowsePoints;

        [Header("Visual Timing")]
        [SerializeField] private float interactionScaleAmount = 1.02f;
        [SerializeField] private float interactionScaleDuration = 0.2f;
        [SerializeField] private float stockAnimationDuration = 0.3f;
        [SerializeField] private float rearrangeAnimationDuration = 0.3f;

        private List<Item> _displayedItems = new List<Item>();
        private Dictionary<Transform, Item> _slotItemMap = new Dictionary<Transform, Item>();

        // ✅ Performance optimizations - cached components and reusable collections
        private readonly Dictionary<BaseCharacterController, ICarryingController> _cachedCarryControllers =
            new Dictionary<BaseCharacterController, ICarryingController>();
        private readonly List<Item> _tempItemList = new List<Item>();
        private readonly List<Tween> _activeTweens = new List<Tween>();
        private Tween _interactionTween;

        // ✅ Performance tracking
        private int _totalItemsStocked = 0;
        private int _totalItemsSold = 0;
        private float _lastStockTime = 0f;

        // Events
        public System.Action<Item> OnItemStocked;
        public System.Action<Item> OnItemPurchased;

        // Properties
        public bool IsFull => _displayedItems.Count >= displaySlots.Length;
        public bool IsEmpty => _displayedItems.Count == 0;
        public int ItemCount => _displayedItems.Count;
        public float StockPercentage => displaySlots.Length > 0 ? (float)_displayedItems.Count / displaySlots.Length : 0f;

        protected override void Start()
        {
            base.Start();
            interactionType = InteractionType.Shelf;
            interactionPriority = InteractionPriority.Medium;
            InitializeSlots();
        }

        private void InitializeSlots()
        {
            try
            {
                _slotItemMap.Clear();
                if (displaySlots != null)
                {
                    foreach (var slot in displaySlots)
                    {
                        if (slot != null)
                            _slotItemMap[slot] = null;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error initializing slots: {e.Message}", this);
            }
        }

        // ==================== PurchasableArea Overrides ====================

        protected override bool CanInteractWhenActive(IInteractor interactor)
        {
            if (interactor?.Character == null) return false;

            try
            {
                // ✅ Performance profiling
                using (PerformanceMonitoringSystem.ProfileInteraction())
                {
                    var controller = interactor as InteractionController;
                    if (controller == null) return false;

                    var character = interactor.Character;

                    return IsEmployeeOrPlayer(character)
                        ? CanEmployeeInteract(controller, character)
                        : IsCustomer(character) && CanCustomerInteract(controller);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error checking interaction capability: {e.Message}", this);
                return false;
            }
        }

        private bool CanEmployeeInteract(InteractionController controller, BaseCharacterController character)
        {
            try
            {
                // For stocking: need items, correct type, shelf not full
                if (!controller.HasItemsInHand() || IsFull) return false;

                var carryController = GetCachedCarryController(character);
                if (carryController == null || shelfData == null) return false;

                return carryController.CurrentItemType() == shelfData.AcceptedItemType;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error checking employee interaction: {e.Message}", this);
                return false;
            }
        }

        private bool CanCustomerInteract(InteractionController controller)
        {
            try
            {
                // For purchasing: shelf has items, customer can carry
                return !IsEmpty && controller.CanCarryMore();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error checking customer interaction: {e.Message}", this);
                return false;
            }
        }

        // ✅ Cache carry controllers to avoid repeated GetComponent calls
        private ICarryingController GetCachedCarryController(BaseCharacterController character)
        {
            if (character == null) return null;

            if (!_cachedCarryControllers.TryGetValue(character, out var carryController))
            {
                carryController = character.CarryingController;
                if (carryController != null)
                {
                    _cachedCarryControllers[character] = carryController;
                }
            }

            return carryController;
        }

        protected override void OnActiveInteractionStart(IInteractor interactor)
        {
            try
            {
                // Clean up previous interaction tween
                CleanupInteractionTween();

                _interactionTween = transform.DOScale(Vector3.one * interactionScaleAmount, interactionScaleDuration);
                if (_interactionTween != null)
                {
                    _activeTweens.Add(_interactionTween);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error starting shelf interaction: {e.Message}", this);
            }
        }

        protected override void OnActiveInteractionContinue(IInteractor interactor)
        {
            if (interactor?.Character == null) return;

            try
            {
                // ✅ Performance profiling
                using (PerformanceMonitoringSystem.ProfileInteraction())
                {
                    var character = interactor.Character;

                    if (IsEmployeeOrPlayer(character))
                    {
                        TryStockItem(interactor);
                    }
                    else if (IsCustomer(character))
                    {
                        TryTakeItem(interactor);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during shelf interaction: {e.Message}", this);
            }
        }

        protected override void OnActiveInteractionEnd(IInteractor interactor)
        {
            try
            {
                CleanupInteractionTween();

                _interactionTween = transform.DOScale(Vector3.one, interactionScaleDuration);
                if (_interactionTween != null)
                {
                    _activeTweens.Add(_interactionTween);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error ending shelf interaction: {e.Message}", this);
            }
        }

        // ==================== STOCKING ====================

        private void TryStockItem(IInteractor interactor)
        {
            if (IsFull || interactor?.Character == null || shelfData == null) return;

            try
            {
                var carryController = GetCachedCarryController(interactor.Character);
                if (carryController != null && carryController.CurrentItemType() == shelfData.AcceptedItemType)
                {
                    Item item = carryController.RemoveTopItem();
                    if (item != null)
                    {
                        PlaceItemOnShelf(item);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error stocking item: {e.Message}", this);
            }
        }

        private void PlaceItemOnShelf(Item item)
        {
            if (item == null) return;

            try
            {
                Transform slot = GetEmptySlot();
                if (slot != null)
                {
                    item.OnPlacedOnShelf(slot);

                    // ✅ Animate placement with proper tween tracking
                    item.transform.localScale = Vector3.zero;
                    Tween placementTween = item.transform.DOScale(1f, stockAnimationDuration)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() =>
                        {
                            // ✅ Performance tracking
                            _totalItemsStocked++;
                            _lastStockTime = Time.time;
                        });
                    
                    if (placementTween != null)
                    {
                        _activeTweens.Add(placementTween);
                    }

                    _slotItemMap[slot] = item;
                    _displayedItems.Add(item);

                    OnItemStocked?.Invoke(item);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error placing item on shelf: {e.Message}", this);
            }
        }

        // ==================== CUSTOMER PURCHASE ====================

        private void TryTakeItem(IInteractor interactor)
        {
            if (IsEmpty || interactor?.Character == null) return;

            try
            {
                var carryController = GetCachedCarryController(interactor.Character);
                if (carryController != null && !carryController.IsFull())
                {
                    Item item = GetLastItem();
                    if (item != null && carryController.TryPickupItem(item))
                    {
                        RemoveItemFromShelf(item);
                        
                        // ✅ Performance tracking
                        _totalItemsSold++;
                        
                        OnItemPurchased?.Invoke(item);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error taking item: {e.Message}", this);
            }
        }

        private void RemoveItemFromShelf(Item item)
        {
            if (item == null) return;

            try
            {
                // Find and clear slot
                foreach (var kvp in _slotItemMap)
                {
                    if (kvp.Value == item)
                    {
                        _slotItemMap[kvp.Key] = null;
                        break;
                    }
                }

                _displayedItems.Remove(item);

                if (autoArrange)
                {
                    RearrangeItems();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error removing item from shelf: {e.Message}", this);
            }
        }

        // ==================== UTILITY ====================

        private Transform GetEmptySlot()
        {
            if (_slotItemMap == null) return null;

            foreach (var kvp in _slotItemMap)
            {
                if (kvp.Value == null && kvp.Key != null)
                    return kvp.Key;
            }
            return null;
        }

        private Item GetLastItem()
        {
            return _displayedItems.Count > 0 ? _displayedItems[_displayedItems.Count - 1] : null;
        }

        private void RearrangeItems()
        {
            if (displaySlots == null) return;

            try
            {
                // ✅ Use temp list to avoid allocation
                _tempItemList.Clear();
                _tempItemList.AddRange(_displayedItems);

                InitializeSlots();

                int index = 0;
                foreach (var item in _tempItemList)
                {
                    if (item != null && index < displaySlots.Length)
                    {
                        Transform slot = displaySlots[index];
                        if (slot != null)
                        {
                            Tween moveTween = item.transform.DOMove(slot.position, rearrangeAnimationDuration)
                                .SetEase(Ease.OutQuad);
                            
                            if (moveTween != null)
                            {
                                _activeTweens.Add(moveTween);
                            }
                            _slotItemMap[slot] = item;
                        }
                        index++;
                    }
                }

                _tempItemList.Clear();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error rearranging items: {e.Message}", this);
            }
        }

        private bool IsEmployeeOrPlayer(BaseCharacterController character)
        {
            if (character is PlayerCharacterController) return true;

            var aiController = character as AICharacterController;
            return aiController != null && aiController.Data?.CharacterType == CharacterType.AI_Employee;
        }

        private bool IsCustomer(BaseCharacterController character)
        {
            var aiController = character as AICharacterController;
            return aiController != null && aiController.Data?.CharacterType == CharacterType.AI_Customer;
        }

        public Transform GetCustomerBrowsePoint()
        {
            if (customerBrowsePoints != null && customerBrowsePoints.Length > 0)
            {
                return customerBrowsePoints[Random.Range(0, customerBrowsePoints.Length)];
            }
            return transform;
        }

        // ✅ Performance monitoring methods
        public float GetStockingRate()
        {
            if (_totalItemsStocked == 0) return 0f;
            return _totalItemsStocked / (Time.time - _lastStockTime);
        }

        public int GetTotalItemsStocked()
        {
            return _totalItemsStocked;
        }

        public int GetTotalItemsSold()
        {
            return _totalItemsSold;
        }

        public float GetSalesRate()
        {
            if (_totalItemsSold == 0) return 0f;
            return _totalItemsSold / Time.time;
        }

        // ==================== CLEANUP ====================

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

                CleanupInteractionTween();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error cleaning up shelf tweens: {e.Message}");
            }
        }

        protected override void OnDestroy()
        {
            CleanupAllTweens();
            _cachedCarryControllers.Clear();
            base.OnDestroy();
        }

        void OnDisable()
        {
            CleanupAllTweens();
        }

        // ✅ Debug information
        #if UNITY_EDITOR
        [ContextMenu("Show Shelf Stats")]
        private void ShowShelfStats()
        {
            Debug.Log($"Shelf Stats:\n" +
                     $"Items Stocked: {_totalItemsStocked}\n" +
                     $"Items Sold: {_totalItemsSold}\n" +
                     $"Current Stock: {ItemCount}/{displaySlots?.Length ?? 0}\n" +
                     $"Stock Percentage: {StockPercentage:P}\n" +
                     $"Sales Rate: {GetSalesRate():F2} items/sec");
        }
        #endif
    }
}
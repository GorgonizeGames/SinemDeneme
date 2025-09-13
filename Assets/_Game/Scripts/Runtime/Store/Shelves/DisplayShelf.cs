using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
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
using Game.Runtime.Store.Machines;

namespace Game.Runtime.Store.Shelves
{
    /// <summary>
    /// Updated DisplayShelf - eski kodunuzdan logic'i alıp simplified architecture ile birleştirir
    /// Generic PurchasableArea<DisplayShelfData> kullanır
    /// </summary>
    public class DisplayShelf : PurchasableArea<DisplayShelfData>
    {
        [Header("Shelf Visual Components")]
        [SerializeField] private GameObject carpet;
        [SerializeField] private Transform[] displaySlots;
        [SerializeField] private List<ItemPlacer> itemPlacerList = new List<ItemPlacer>();

        [Header("Customer & Employee Positions")]
        [SerializeField] private Transform[] customerBrowsePoints;
        [SerializeField] private List<ItemPlacer> employeeTargets = new List<ItemPlacer>();
        [SerializeField] private List<ItemPlacer> customerTargets = new List<ItemPlacer>();

        [Header("Next Objects - Eski koddan")]
        [SerializeField] private List<GameObject> nextObjects = new List<GameObject>();

        // Shelf State - eski koddan
        private List<Item> _items = new List<Item>();
        private List<Customer> _customers = new List<Customer>();
        private Dictionary<Transform, Item> _slotItemMap = new Dictionary<Transform, Item>();

        // Performance optimizations
        private readonly Dictionary<BaseCharacterController, ICarryingController> _cachedCarryControllers = 
            new Dictionary<BaseCharacterController, ICarryingController>();
        private readonly List<Item> _tempItemList = new List<Item>();
        private readonly List<Tween> _shelfActiveTweens = new List<Tween>();

        // Events - eski koddan
        public System.Action<Item> OnItemStocked;
        public System.Action<Item> OnItemPurchased;
        public System.Action OnPlayerIn; // Eski koddan
        public System.Action OnActiveChanged; // Eski koddan

        // Properties
        public List<Item> Items => _items;
        public List<Customer> Customers => _customers;
        public ItemType CurrentItemType => Data?.AcceptedItemType ?? ItemType.None;
        public bool IsFull => _items.Count >= (Data?.MaxDisplayItems ?? displaySlots.Length);
        public bool IsEmpty => _items.Count == 0;
        public int ItemCount => _items.Count;
        public float StockPercentage => (Data?.MaxDisplayItems ?? displaySlots.Length) > 0 ? 
            (float)_items.Count / (Data?.MaxDisplayItems ?? displaySlots.Length) : 0f;

        protected override void Start()
        {
            base.Start();
            InitializeSlots();

            // Eski koddan - manager'a kayıt
            // GameManager.Instance.LevelManager.AddShelvesList(this);
        }

        protected void OnEnable()
        {
            RemoveItemOnEnable(); // Eski koddan

            // Manager'a tekrar kayıt
            // if (GameManager.Instance.LevelManager)
            //     GameManager.Instance.LevelManager.AddShelvesList(this);
        }

        protected override void OnDisable()
        {
            SaveShelfData(); // Eski koddan
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
                Debug.LogError($"Error initializing shelf slots: {e.Message}", this);
            }
        }

        // Eski koddan - clear items on enable
        private void RemoveItemOnEnable()
        {
            foreach (Item item in _items)
            {
                if (item != null)
                    item.OnReset();
            }

            _items.Clear();

            foreach (ItemPlacer placer in itemPlacerList)
            {
                if (placer != null)
                    placer.IsItFull = false;
            }

            _customers.Clear();
        }

        // ==================== PurchasableArea Overrides ====================

        protected override bool CanInteractWhenActive(IInteractor interactor)
        {
            if (interactor?.Character == null || Data == null) return false;

            try
            {
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
                Debug.LogError($"Error checking shelf interaction capability: {e.Message}", this);
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
                if (carryController == null) return false;

                return carryController.CurrentItemType() == Data.AcceptedItemType;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error checking employee shelf interaction: {e.Message}", this);
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
                Debug.LogError($"Error checking customer shelf interaction: {e.Message}", this);
                return false;
            }
        }

        protected override void OnActiveInteractionStart(IInteractor interactor)
        {
            try
            {
                // Carpet scale animation - eski koddan
                if (carpet != null && interactor.Character is PlayerCharacterController)
                {
                    carpet.transform.DOScale(Vector3.one * 1.2f, 0.3f);
                    OnPlayerIn?.Invoke(); // Eski koddan event
                }

                // Shelf scale feedback
                if (Data?.EnableShelfPurchaseVFX == true)
                {
                    CleanupShelfInteractionTween();
                    var scaleTween = transform.DOScale(Vector3.one * Data.InteractionScaleAmount, Data.StockingAnimationDuration);
                    if (scaleTween != null)
                    {
                        _shelfActiveTweens.Add(scaleTween);
                    }
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
                // Reset carpet scale - eski koddan
                if (carpet != null && interactor.Character is PlayerCharacterController)
                {
                    carpet.transform.DOScale(Vector3.one, 0.3f);
                }

                // Reset shelf scale
                CleanupShelfInteractionTween();
                var resetTween = transform.DOScale(Vector3.one, Data?.StockingAnimationDuration ?? 0.3f);
                if (resetTween != null)
                {
                    _shelfActiveTweens.Add(resetTween);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error ending shelf interaction: {e.Message}", this);
            }
        }

        // ==================== Stocking Logic - Eski koddan ====================

        private void TryStockItem(IInteractor interactor)
        {
            if (IsFull || interactor?.Character == null || Data == null) return;

            try
            {
                var carryController = GetCachedCarryController(interactor.Character);
                if (carryController != null && carryController.CurrentItemType() == Data.AcceptedItemType)
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
                Debug.LogError($"Error stocking item on shelf: {e.Message}", this);
            }
        }

        private void PlaceItemOnShelf(Item item)
        {
            if (item == null) return;

            try
            {
                ItemPlacer placer = ChoosePlace();
                if (placer != null)
                {
                    item.transform.parent = placer.transform;

                    // Animation - eski koddan inspired with data-driven values
                    var animDuration = Data?.StockingAnimationDuration ?? 1f;
                    var jumpPower = 1f; // Data'dan alınabilir
                    var ease = Ease.OutFlash; // Data'dan alınabilir

                    var jumpTween = item.transform.DOLocalJump(Vector3.zero, jumpPower, 1, animDuration)
                        .SetEase(ease)
                        .OnComplete(() =>
                        {
                            if (item != null)
                            {
                                _items.Add(item);
                                SaveShelfData();
                            }
                        });

                    if (jumpTween != null)
                    {
                        _shelfActiveTweens.Add(jumpTween);
                    }

                    var rotateTween = item.transform.DOLocalRotate(Vector3.zero, animDuration).SetEase(ease);
                    if (rotateTween != null)
                    {
                        _shelfActiveTweens.Add(rotateTween);
                    }

                    placer.IsItFull = true;
                    item.Placer = placer; // Eski koddan property

                    OnItemStocked?.Invoke(item);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error placing item on shelf: {e.Message}", this);
            }
        }

        // ==================== Customer Purchase Logic - Eski koddan ====================

        private void TryTakeItem(IInteractor interactor)
        {
            if (IsEmpty || interactor?.Character == null) return;

            try
            {
                var character = interactor.Character;
                var carryController = GetCachedCarryController(character);

                if (carryController != null && !carryController.IsFull())
                {
                    // Customer capacity check - eski koddan
                    if (character is Customer customer)
                    {
                        if (_items.Count > 0 && character.ItemsCount.Count < customer.Capacity)
                        {
                            // Position check - eski koddan
                            if (IsCustomerInPosition(customer))
                            {
                                Item item = RemoveItemFromShelf();
                                if (item != null)
                                {
                                    customer.AddItemList(item); // Eski koddan method
                                    OnItemPurchased?.Invoke(item);
                                }
                            }
                        }
                        else if (character.Items.Count == customer.Capacity)
                        {
                            // Customer is full - send to cash register - eski koddan
                            // GameManager.Instance.LevelManager.CashRegister.AddCustomers(customer);
                            RemoveCustomerList(customer);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error customer taking item: {e.Message}", this);
            }
        }

        private bool IsCustomerInPosition(Customer customer)
        {
            if (customer?.Target == null) return false;

            float threshold = 0.1f;
            return Mathf.Abs(customer.transform.position.x - customer.Target.position.x) < threshold &&
                   Mathf.Abs(customer.transform.position.z - customer.Target.position.z) < threshold;
        }

        private Item RemoveItemFromShelf()
        {
            if (_items.Count == 0) return null;

            try
            {
                Item item = _items[_items.Count - 1];
                
                if (item?.Placer != null)
                {
                    item.Placer.IsItFull = false;
                }

                _items.Remove(item);
                SaveShelfData();

                if (Data?.AutoArrange == true)
                {
                    RearrangeItems();
                }

                return item;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error removing item from shelf: {e.Message}", this);
                return null;
            }
        }

        // ==================== Item Management - Eski koddan ====================

        public void AddItemFromItems(Item item, bool isLoad = false)
        {
            if (item == null) return;

            ItemPlacer placer = ChoosePlace();
            if (placer != null)
            {
                item.transform.parent = placer.transform;
                placer.IsItFull = true;
                item.Placer = placer;
                _items.Add(item);
                SaveShelfData();

                if (isLoad)
                {
                    item.transform.localPosition = Vector3.zero;
                    item.transform.localRotation = Quaternion.identity;
                }
            }
        }

        public void RemoveAllItems()
        {
            foreach (Item item in _items)
            {
                if (item != null)
                    item.OnReset();
            }

            _items.Clear();
            ClearPlacers();
            _customers.Clear();
        }

        private void RearrangeItems()
        {
            if (displaySlots == null || Data?.AutoArrange != true) return;

            try
            {
                _tempItemList.Clear();
                _tempItemList.AddRange(_items);

                InitializeSlots();

                int index = 0;
                foreach (var item in _tempItemList)
                {
                    if (item != null && index < displaySlots.Length)
                    {
                        Transform slot = displaySlots[index];
                        if (slot != null)
                        {
                            var moveTween = item.transform.DOMove(slot.position, Data.RearrangeAnimationDuration)
                                .SetEase(Ease.OutQuad);
                            
                            if (moveTween != null)
                            {
                                _shelfActiveTweens.Add(moveTween);
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
                Debug.LogError($"Error rearranging shelf items: {e.Message}", this);
            }
        }

        private ItemPlacer ChoosePlace()
        {
            for (int i = 0; i < itemPlacerList.Count; i++)
            {
                if (itemPlacerList[i] != null && !itemPlacerList[i].IsItFull)
                    return itemPlacerList[i];
            }
            return null;
        }

        // ==================== Customer Management - Eski koddan ====================

        public void AddCustomerList(Customer customer)
        {
            if (customer != null && !_customers.Contains(customer))
            {
                _customers.Add(customer);
            }
        }

        public void RemoveCustomerList(Customer customer)
        {
            if (_customers.Contains(customer))
            {
                if (customer.ShelfPlacer != null)
                    customer.ShelfPlacer.IsItFull = false;

                _customers.Remove(customer);
            }
        }

        public ItemPlacer ChooseEmployeePlace()
        {
            for (int i = 0; i < employeeTargets.Count; i++)
            {
                if (employeeTargets[i] != null && !employeeTargets[i].IsItFull)
                {
                    employeeTargets[i].IsItFull = true;
                    return employeeTargets[i];
                }
            }
            return null;
        }

        public ItemPlacer ChooseCustomerPlace()
        {
            for (int i = 0; i < customerTargets.Count; i++)
            {
                if (customerTargets[i] != null && !customerTargets[i].IsItFull)
                {
                    customerTargets[i].IsItFull = true;
                    return customerTargets[i];
                }
            }
            return null;
        }

        public void ClearPlacers()
        {
            foreach (ItemPlacer placer in customerTargets)
            {
                if (placer != null)
                    placer.IsItFull = false;
            }

            foreach (ItemPlacer placer in employeeTargets)
            {
                if (placer != null)
                    placer.IsItFull = false;
            }
        }

        public Transform GetCustomerBrowsePoint()
        {
            if (customerBrowsePoints != null && customerBrowsePoints.Length > 0)
            {
                return customerBrowsePoints[Random.Range(0, customerBrowsePoints.Length)];
            }
            return transform;
        }

        // ==================== Utility Methods ====================

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

        // ==================== Save/Load - Eski koddan ====================

        private void SaveShelfData()
        {
            if (string.IsNullOrEmpty(AreaId)) return;

            try
            {
                string itemCountKey = AreaId + "_ItemCount";
                PlayerPrefs.SetInt(itemCountKey, _items.Count);
                PlayerPrefs.Save();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error saving shelf data: {e.Message}", this);
            }
        }

        private void LoadShelfData()
        {
            if (string.IsNullOrEmpty(AreaId)) return;

            try
            {
                string itemCountKey = AreaId + "_ItemCount";
                
                if (PlayerPrefs.HasKey(itemCountKey))
                {
                    int itemCount = PlayerPrefs.GetInt(itemCountKey);
                    
                    // Load items - bu kısım item pool system ile güncellenebilir
                    // for (int i = 0; i < itemCount; i++)
                    // {
                    //     Item item = GameManager.Instance.ObjectPoolingManager.GetItemFromPool(CurrentItemType);
                    //     if (item != null)
                    //     {
                    //         item.gameObject.SetActive(true);
                    //         AddItemFromItems(item, true);
                    //     }
                    // }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading shelf data: {e.Message}", this);
            }
        }

        // ==================== Restock Logic - Data-driven ====================

        public bool NeedsRestock()
        {
            return Data?.NeedsRestock(_items.Count) ?? false;
        }

        public int GetRestockAmount()
        {
            return Data?.GetRestockAmount(_items.Count) ?? 0;
        }

        public float GetRestockPercentage()
        {
            return Data?.GetStockPercentage(_items.Count) ?? 0f;
        }

        // ==================== Performance Monitoring ====================

        public float GetStockingRate()
        {
            // Bu implement edilebilir - kaç item/saniye stoklandığı
            return 0f;
        }

        public int GetTotalItemsStocked()
        {
            return _items.Count;
        }

        public int GetTotalItemsSold()
        {
            // Bu track edilebilir
            return 0;
        }

        // ==================== Cleanup ====================

        private void CleanupShelfInteractionTween()
        {
            // Shelf-specific interaction tweens cleanup
            var interactionTweens = _shelfActiveTweens.FindAll(t => t != null && t.IsActive());
            foreach (var tween in interactionTweens)
            {
                if (tween.IsActive()) tween.Kill();
            }
            
            _shelfActiveTweens.RemoveAll(t => t == null || !t.IsActive());
        }

        private void CleanupAllShelfTweens()
        {
            try
            {
                foreach (var tween in _shelfActiveTweens)
                {
                    if (tween != null && tween.IsActive())
                    {
                        tween.Kill();
                    }
                }
                _shelfActiveTweens.Clear();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error cleaning up shelf tweens: {e.Message}");
            }
        }

        protected override void OnDestroy()
        {
            CleanupAllShelfTweens();
            _cachedCarryControllers.Clear();
            base.OnDestroy();
        }

        // ==================== Debug ====================

#if UNITY_EDITOR
        [ContextMenu("Show Shelf Stats")]
        private void ShowShelfStats()
        {
            if (Data == null)
            {
                Debug.LogWarning("No shelf data assigned!");
                return;
            }

            Debug.Log($"Shelf Stats:\n" +
                     $"Accepted Item: {Data.AcceptedItemType}\n" +
                     $"Current Stock: {ItemCount}/{Data.MaxDisplayItems}\n" +
                     $"Stock Percentage: {GetRestockPercentage():P}\n" +
                     $"Needs Restock: {NeedsRestock()}\n" +
                     $"Customers: {_customers.Count}/{Data.MaxCustomersAtOnce}\n" +
                     $"Auto Arrange: {Data.AutoArrange}\n" +
                     $"State: {CurrentState}");
        }

        [ContextMenu("Clear All Items")]
        private void ClearAllItems()
        {
            RemoveAllItems();
            Debug.Log("All shelf items cleared!");
        }

        [ContextMenu("Force Restock")]
        private void ForceRestock()
        {
            if (Data == null) return;

            int restockAmount = GetRestockAmount();
            Debug.Log($"Need to restock {restockAmount} items of type {Data.AcceptedItemType}");
        }
#endif
    }

    // ==================== Support Classes - Eski koddan ====================

    [System.Serializable]
    public class Customer : BaseCharacterController
    {
        [Header("Customer Settings")]
        [SerializeField] private int capacity = 5;
        [SerializeField] private Transform target;
        [SerializeField] private ItemPlacer shelfPlacer;

        private List<Item> items = new List<Item>();

        public int Capacity => capacity;
        public Transform Target => target;
        public ItemPlacer ShelfPlacer 
        { 
            get => shelfPlacer; 
            set => shelfPlacer = value; 
        }
        public List<Item> Items => items;
        public List<Item> ItemsCount => items; // Eski koddan compatibility

        // Eski koddan methods
        public void AddItemList(Item item)
        {
            if (item != null && items.Count < capacity)
            {
                items.Add(item);
            }
        }

        public Item RemoveItemList(ItemType itemType)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i].ItemType == itemType)
                {
                    Item item = items[i];
                    items.RemoveAt(i);
                    return item;
                }
            }
            return null;
        }

        // Base class overrides - placeholder
        protected override void OnInitialize() { }
        protected override void HandleInput() { }
    }
}
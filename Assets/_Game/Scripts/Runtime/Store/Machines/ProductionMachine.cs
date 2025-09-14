using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Runtime.Store.Areas;
using Game.Runtime.Interactions.Interfaces;
using Game.Runtime.Items;
using Game.Runtime.Items.Data;
using Game.Runtime.Character;
using Game.Runtime.Character.Components;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Performance;
using Game.Runtime.Items.Services;
using Game.Runtime.Core.Extensions;
using Game.Runtime.Character.Interfaces;

namespace Game.Runtime.Store.Machines
{
    /// <summary>
    /// Updated ProductionMachine - eski kodunuzdan logic'i alıp simplified architecture ile birleştirir
    /// Generic PurchasableArea<ProductionMachineData> kullanır
    /// </summary>
    public class ProductionMachine : PurchasableArea<ProductionMachineData>
    {
        [Header("Machine Visual Components - Figure Production")]
        [SerializeField] private GameObject nonPaintedObject;
        [SerializeField] private GameObject coloringArm;
        [SerializeField] private GameObject bounceObject;

        [Header("Machine Visual Components - General")]
        [SerializeField] private GameObject productionLine;
        [SerializeField] private GameObject carpet;
        [SerializeField] private Transform[] gears;

        [Header("Machine Transform References")]
        [SerializeField] private Transform productionPoint;
        [SerializeField] private Transform itemTarget;
        [SerializeField] private Transform itemParent;
        [SerializeField] private Transform nonPaintedTarget;

        [Header("Machine Visual Effects")]
        [SerializeField] private ParticleSystem machineVFX;
        [SerializeField] private Renderer conveyorRenderer;

        [Header("Employee Positions")]
        [SerializeField] private Transform[] employeeWaitPoints;
        [SerializeField] private List<ItemPlacer> employeeTargets = new List<ItemPlacer>();

        [Inject] private IItemPoolService _itemPool;

        // Machine State - eski koddan
        private List<Item> _items = new List<Item>();
        private Coroutine _productionCoroutine;
        private bool _isProducing = false;

        // Performance optimizations
        private readonly List<Tween> _machineActiveTweens = new List<Tween>();
        private readonly Dictionary<BaseCharacterController, ICarryingController> _cachedCarryControllers = new Dictionary<BaseCharacterController, ICarryingController>();

        // Performance tracking - eski koddan inspired
        private float _lastProductionTime;
        private int _totalItemsProduced = 0;

        // Events - eski koddan
        public System.Action<Item> OnItemProduced;
        public System.Action<Item> OnItemCollected;

        // Properties
        public List<Item> Items => _items;
        public bool IsFull => _items.Count >= Data.MaxCapacity;
        public bool IsEmpty => _items.Count == 0;
        public bool IsProducing => _isProducing;

        protected override void Start()
        {
            base.Start();

            if (IsActive)
            {
                StartProduction();
            }

            // Eski koddan - manager'a kayıt
            // GameManager.Instance.LevelManager.AddMachine(this);
        }

        protected void OnEnable()
        {
            ClearOnEnable(); // Eski koddan
            
            if (IsActive)
            {
                StartProduction();
            }
        }

        protected override void OnDisable()
        {
            StopProduction();
            SaveMachineData(); // Eski koddan
        }

        // Eski koddan - clear items on enable
        private void ClearOnEnable()
        {
            StopProduction();

            foreach (Item item in _items)
            {
                if (item != null)
                    item.OnReset(); // Eski koddan method
            }

            _items.Clear();
        }

        // ==================== PurchasableArea Overrides ====================

        protected override bool CanInteractWhenActive(IInteractor interactor)
        {
            if (interactor?.Character == null || Data == null) return false;

            var controller = interactor as InteractionController;
            if (controller == null) return false;

            // Machine'de item yoksa alamaz
            if (_items.Count == 0) return false;

            // Daha fazla taşıyamazsa alamaz
            if (!controller.CanCarryMore()) return false;

            // Type check - eski koddan logic
            if (controller.HasItemsInHand())
            {
                var carryController = GetCachedCarryController(interactor.Character);
                if (carryController != null && carryController.CurrentItemType() != Data.ProducedItemType)
                {
                    return false;
                }
            }

            return true;
        }

        protected override void OnActiveInteractionStart(IInteractor interactor)
        {
            // Carpet scale animation - eski koddan
            if (carpet != null)
            {
                carpet.transform.DOScale(Vector3.one * 1.2f, 0.3f);
            }

            // Machine scale feedback
            CleanupMachineInteractionTween();
            var scaleTween = transform.DOScale(Vector3.one * Data.InteractionScaleAmount, Data.PurchaseAnimDuration);
            if (scaleTween != null)
            {
                _machineActiveTweens.Add(scaleTween);
            }
        }

        protected override void OnActiveInteractionContinue(IInteractor interactor)
        {
            using (PerformanceMonitoringSystem.ProfileInteraction())
            {
                TryGiveItem(interactor);
            }
        }

        protected override void OnActiveInteractionEnd(IInteractor interactor)
        {
            // Reset carpet scale - eski koddan
            if (carpet != null)
            {
                carpet.transform.DOScale(Vector3.one, 0.3f);
            }

            // Reset machine scale
            CleanupMachineInteractionTween();
            var resetTween = transform.DOScale(Vector3.one, Data.PurchaseAnimDuration);
            if (resetTween != null)
            {
                _machineActiveTweens.Add(resetTween);
            }
        }

        protected override void OnPurchaseCompleted()
        {
            base.OnPurchaseCompleted();
            StartProduction();
        }

        // ==================== Production Logic - Eski koddan ====================

        public void StartProduction()
        {
            if (!IsActive || _isProducing || Data == null) return;

            _isProducing = true;

            if (_productionCoroutine != null)
            {
                StopCoroutine(_productionCoroutine);
            }

            _productionCoroutine = StartCoroutine(ProductionCycle());
        }

        public void StopProduction()
        {
            _isProducing = false;

            if (_productionCoroutine != null)
            {
                StopCoroutine(_productionCoroutine);
                _productionCoroutine = null;
            }

            StopAllVisualEffects();
        }

        private IEnumerator ProductionCycle()
        {
            while (_isProducing && IsActive && Data != null)
            {
                using (PerformanceMonitoringSystem.ProfileProduction())
                {
                    if (_items.Count < Data.MaxCapacity)
                    {
                        yield return StartCoroutine(ProduceItemSequence());
                    }
                    else
                    {
                        yield return new WaitForSeconds(1f); // Full capacity wait
                    }
                }

                yield return new WaitForSeconds(Data.ProductionInterval);
            }
        }

        private IEnumerator ProduceItemSequence()
        {
            if (Data == null || _itemPool == null) yield break;

            // Eski koddan - production animation
            switch (Data.MachineCatagory)
            {
                case MachineCatagory.Figure:
                    yield return StartCoroutine(ProduceFigureItem());
                    break;
                case MachineCatagory.Comic:
                    yield return StartCoroutine(ProduceComicItem());
                    break;
            }
        }

        // Eski koddan - figure production
        private IEnumerator ProduceFigureItem()
        {
            if (nonPaintedObject == null || itemTarget == null) yield break;

            Vector3 originalPos = nonPaintedObject.transform.position;

            // Get item from pool
            Item item = _itemPool.GetItem(Data.ProducedItemType);
            if (item == null) yield break;

            item.transform.parent = itemParent;
            item.Rigidbody.useGravity = false;

            // Enable colliders - eski koddan
            foreach (var collider in item.Colliders)
            {
                if (collider != null) collider.enabled = true;
            }

            item.gameObject.SetActive(false);

            // Production line animation
            if (productionLine != null)
            {
                productionLine.transform.DOLocalRotate(new Vector3(60, 0, 0), Data.ProductionLineRotateSpeed);
            }

            // Move non-painted object
            if (nonPaintedObject != null)
            {
                yield return nonPaintedObject.transform.DOMove(itemParent.position, 0.5f).WaitForCompletion();

                // Coloring arm animation
                if (coloringArm != null)
                {
                    yield return coloringArm.transform.DOLocalRotate(new Vector3(5, 0, 0), Data.ColoringArmAnimSpeed).WaitForCompletion();

                    // Reset non-painted object
                    nonPaintedObject.transform.position = originalPos;

                    // Setup item
                    item.transform.localPosition = Vector3.zero;
                    item.transform.localRotation = Quaternion.identity;
                    item.gameObject.SetActive(true);

                    // Bounce animation
                    if (bounceObject != null)
                    {
                        yield return bounceObject.transform.DOShakeScale(0.3f, 0.5f, 3, 30).WaitForCompletion();

                        // Reset coloring arm
                        coloringArm.transform.DOLocalRotate(new Vector3(-45, 0, 0), Data.ColoringArmAnimSpeed);

                        // Play VFX
                        if (machineVFX != null && Data.EnableMachineVFX)
                            machineVFX.Play();

                        // Move item to target
                        Vector3 targetPos = itemTarget.position + Vector3.left * Random.Range(0, 0.5f);
                        yield return item.transform.DOMove(targetPos, 0.5f).WaitForCompletion();

                        // Enable physics
                        item.Rigidbody.useGravity = true;
                        item.Rigidbody.isKinematic = false;

                        AddItemToMachine(item);

                        // Reset production line
                        if (productionLine != null)
                        {
                            productionLine.transform.DOLocalRotate(Vector3.zero, Data.ProductionLineRotateSpeed);
                        }
                    }
                }
            }
        }

        // Eski koddan - comic production
        private IEnumerator ProduceComicItem()
        {
            Item item = _itemPool.GetItem(Data.ProducedItemType);
            if (item == null) yield break;

            item.gameObject.transform.SetParent(itemParent);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            item.Rigidbody.useGravity = false;

            // Enable colliders
            foreach (var collider in item.Colliders)
            {
                if (collider != null) collider.enabled = true;
            }

            item.gameObject.SetActive(true);

            // Production line animation
            if (productionLine != null)
            {
                productionLine.transform.DOLocalRotate(new Vector3(60, 0, 0), Data.ProductionLineRotateSpeed);
            }

            // Move to target
            Vector3 targetPos = itemTarget.position + Vector3.left * Random.Range(0, 0.1f);
            yield return item.transform.DOMove(targetPos, 0.7f).WaitForCompletion();

            // Enable physics
            item.Rigidbody.useGravity = true;
            item.Rigidbody.isKinematic = false;

            // Reset production line
            if (productionLine != null)
            {
                productionLine.transform.DOLocalRotate(Vector3.zero, Data.ProductionLineRotateSpeed);
            }

            AddItemToMachine(item);
        }

        private void AddItemToMachine(Item item)
        {
            if (item == null) return;

            _items.Add(item);
            _totalItemsProduced++;
            _lastProductionTime = Time.time;

            OnItemProduced?.Invoke(item);
        }

        private void TryGiveItem(IInteractor interactor)
        {
            if (_items.Count == 0 || interactor?.Character == null) return;

            var controller = interactor as InteractionController;
            var carryController = GetCachedCarryController(interactor.Character);

            if (carryController != null && !carryController.IsFull())
            {
                Item item = GetLastProducedItem();
                if (item == null) return;

                // Type check - eski koddan
                if (carryController.CurrentItemType() != ItemType.None &&
                    carryController.CurrentItemType() != item.ItemType)
                {
                    return;
                }

                if (carryController.TryPickupItem(item))
                {
                    RemoveItemFromMachine(item);
                    OnItemCollected?.Invoke(item);
                }
            }
        }

        private Item GetLastProducedItem()
        {
            return _items.Count > 0 ? _items[_items.Count - 1] : null;
        }

        private void RemoveItemFromMachine(Item item)
        {
            if (item == null || !_items.Contains(item)) return;

            // Disable physics - eski koddan
            item.Rigidbody.useGravity = false;
            item.Rigidbody.isKinematic = true;

            // Disable colliders
            foreach (var collider in item.Colliders)
            {
                if (collider != null) collider.enabled = false;
            }

            _items.Remove(item);
        }

        // Cache carry controllers to avoid repeated GetComponent calls
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

        // ==================== Visual Effects - Eski koddan ====================

        private void StopAllVisualEffects()
        {
            SetConveyorMovement(false);
        }

        private void SetConveyorMovement(bool moving)
        {
            if (conveyorRenderer?.material != null && Data != null)
            {
                float scrollSpeed = moving ? Data.ConveyorScrollSpeed : 0f;
                conveyorRenderer.material.SetFloat("_ScrollSpeed", scrollSpeed);
            }
        }

        // ==================== Employee Support - Eski koddan ====================

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

        public Transform GetEmployeeWaitPoint()
        {
            if (employeeWaitPoints != null && employeeWaitPoints.Length > 0)
            {
                return employeeWaitPoints[0];
            }
            return transform;
        }

        // ==================== Upgrade System - Data-driven ====================

        public void UpdateCapacity(int newCapacity)
        {
            // Bu artık data üzerinden kontrol edilebilir
            // Data.GetUpgradedCapacity(upgradeLevel) kullanılabilir
        }

        public void UpdateProductionSpeed(float newDuration)
        {
            // Bu artık data üzerinden kontrol edilebilir
            // Data.GetUpgradedProductionInterval(upgradeLevel) kullanılabilir
        }

        // ==================== Save/Load - Eski koddan ====================

        private void SaveMachineData()
        {
            if (string.IsNullOrEmpty(AreaId)) return;

            try
            {
                string itemCountKey = AreaId + "_ItemCount";
                string itemTypeKey = AreaId + "_ItemType";
                string produceItemKey = AreaId + "_ProduceItem";
                string capacityKey = AreaId + "_Capacity";

                PlayerPrefs.SetInt(itemCountKey, _items.Count);
                if (Data != null)
                {
                    PlayerPrefs.SetInt(itemTypeKey, (int)Data.ProducedItemType);
                }
                PlayerPrefs.SetInt(produceItemKey, _totalItemsProduced);
                
                PlayerPrefs.Save();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error saving machine data: {e.Message}", this);
            }
        }

        private void LoadMachineData()
        {
            if (string.IsNullOrEmpty(AreaId)) return;

            try
            {
                string itemCountKey = AreaId + "_ItemCount";
                string produceItemKey = AreaId + "_ProduceItem";

                if (PlayerPrefs.HasKey(produceItemKey))
                {
                    _totalItemsProduced = PlayerPrefs.GetInt(produceItemKey);
                }

                // Item count loading - eski kodda bu vardi ama şimdilik skip edelim
                // Çünkü item pool system değişti
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading machine data: {e.Message}", this);
            }
        }

        // ==================== Performance Monitoring ====================

        public float GetProductionRate()
        {
            if (_totalItemsProduced == 0) return 0f;
            return _totalItemsProduced / (Time.time - _lastProductionTime);
        }

        public int GetTotalItemsProduced()
        {
            return _totalItemsProduced;
        }

        public float GetCapacityPercentage()
        {
            return Data != null ? (float)_items.Count / Data.MaxCapacity : 0f;
        }

        // ==================== Cleanup ====================

        private void CleanupMachineInteractionTween()
        {
            // Machine-specific interaction tweens cleanup
            var interactionTweens = _machineActiveTweens.FindAll(t => t != null && t.IsActive());
            foreach (var tween in interactionTweens)
            {
                if (tween.IsActive()) tween.Kill();
            }
            
            _machineActiveTweens.RemoveAll(t => t == null || !t.IsActive());
        }

        private void CleanupAllMachineTweens()
        {
            try
            {
                foreach (var tween in _machineActiveTweens)
                {
                    if (tween != null && tween.IsActive())
                    {
                        tween.Kill();
                    }
                }
                _machineActiveTweens.Clear();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error cleaning up machine tweens: {e.Message}");
            }
        }

        protected override void OnDestroy()
        {
            StopProduction();
            CleanupAllMachineTweens();
            _cachedCarryControllers.Clear();
            base.OnDestroy();
        }

        // ==================== Debug ====================

#if UNITY_EDITOR
        [ContextMenu("Show Machine Stats")]
        private void ShowMachineStats()
        {
            if (Data == null)
            {
                Debug.LogWarning("No machine data assigned!");
                return;
            }

            Debug.Log($"Machine Stats:\n" +
                     $"Type: {Data.MachineCatagory} - {Data.ProducedItemType}\n" +
                     $"Total Produced: {_totalItemsProduced}\n" +
                     $"Current Rate: {GetProductionRate():F2} items/sec\n" +
                     $"Items in Queue: {_items.Count}/{Data.MaxCapacity}\n" +
                     $"Capacity: {GetCapacityPercentage():P}\n" +
                     $"Is Producing: {_isProducing}\n" +
                     $"State: {CurrentState}");
        }

        [ContextMenu("Clear All Items")]
        private void ClearAllItems()
        {
            ClearOnEnable();
            Debug.Log("All machine items cleared!");
        }
#endif
    }

    // ==================== Support Classes ====================

    [System.Serializable]
    public class ItemPlacer : MonoBehaviour
    {
        [SerializeField] private bool isItFull = false;
        
        public bool IsItFull 
        { 
            get => isItFull; 
            set => isItFull = value; 
        }
    }
}
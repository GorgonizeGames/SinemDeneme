using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Runtime.Store.Areas;
using Game.Runtime.Interactions.Interfaces;
using Game.Runtime.Items;
using Game.Runtime.Items.Data;
using Game.Runtime.Character;
using Game.Runtime.Character.Components;
using Game.Runtime.Core.DI;
using DG.Tweening;
using Game.Runtime.Items.Services;
using Game.Runtime.Core.Extensions;

namespace Game.Runtime.Store.Machines
{
    public class ProductionMachine : PurchasableArea
    {
        [Header("Machine Configuration")]
        [SerializeField] private ProductionMachineData machineData;

        [Header("Production Settings")]
        [SerializeField] private Transform productionPoint;
        [SerializeField] private Transform outputPoint;
        [SerializeField] private Transform[] outputSlots;

        [Header("Visual Elements")]
        [SerializeField] private Transform[] gears;
        [SerializeField] private ParticleSystem productionVFX;
        [SerializeField] private Renderer conveyorRenderer;

        [Header("Employee Positions")]
        [SerializeField] private Transform[] employeeWaitPoints;

        [Header("Visual Timing")]
        [SerializeField] private float interactionScaleAmount = 1.05f;
        [SerializeField] private float interactionScaleDuration = 0.2f;
        [SerializeField] private float itemMoveAnimationDuration = 0.5f;
        [SerializeField] private float productionVisualDelay = 0.5f;

        [Header("Production Delays")]
        [SerializeField] private float fullCapacityWaitTime = 1f;

        [Inject] private IItemPoolService _itemPool;

        private Queue<Item> _producedItems = new Queue<Item>();
        private Coroutine _productionCoroutine;
        private bool _isProducing = false;
        private int _currentSlotIndex = 0;

        // Performance optimizations - resource management
        private readonly List<Tween> _activeTweens = new List<Tween>();
        private readonly List<Transform> _activeGearAnimations = new List<Transform>();
        private Tween _interactionTween;

        // Cached strings for performance
        private string _cachedCostText;
        private bool _costTextDirty = true;

        // Events
        public System.Action<Item> OnItemProduced;
        public System.Action<Item> OnItemCollected;

        protected override void Start()
        {
            base.Start();
            interactionType = InteractionType.Machine;
            interactionPriority = InteractionPriority.High;

            PrepareCachedStrings();

            if (IsActive)
            {
                StartProduction();
            }
        }

        private void PrepareCachedStrings()
        {
            if (areaData != null)
            {
                _cachedCostText = $"${areaData.PurchaseCost}";
            }
        }

        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();

            // Use cached string instead of creating new one every time
            if (costText != null && _costTextDirty)
            {
                costText.text = _cachedCostText;
                _costTextDirty = false;
            }
        }

        // ==================== PurchasableArea Overrides ====================

        protected override bool CanInteractWhenActive(IInteractor interactor)
        {
            if (interactor?.Character == null) return false;

            var controller = interactor as InteractionController;
            if (controller == null) return false;

            // Machine'de item yoksa alamaz
            if (_producedItems.Count == 0) return false;

            // Daha fazla taşıyamazsa alamaz
            if (!controller.CanCarryMore()) return false;

            // Elinde farklı tip item varsa alamaz
            if (controller.HasItemsInHand())
            {
                var carryController = controller.GetCachedCarryController();
                if (carryController != null && carryController.CurrentItemType() != machineData.ProducedItemType)
                {
                    return false;
                }
            }

            return true;
        }

        protected override void OnActiveInteractionStart(IInteractor interactor)
        {
            // Clean up previous tween to prevent leaks
            CleanupInteractionTween();

            _interactionTween = transform.DOScale(Vector3.one * interactionScaleAmount, interactionScaleDuration);
            if (_interactionTween != null)
            {
                _activeTweens.Add(_interactionTween);
            }
        }

        protected override void OnActiveInteractionContinue(IInteractor interactor)
        {
            TryGiveItem(interactor);
        }

        protected override void OnActiveInteractionEnd(IInteractor interactor)
        {
            CleanupInteractionTween();

            _interactionTween = transform.DOScale(Vector3.one, interactionScaleDuration);
            if (_interactionTween != null)
            {
                _activeTweens.Add(_interactionTween);
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

        protected override void CompletePurchase()
        {
            base.CompletePurchase();
            StartProduction();
        }

        // ==================== PRODUCTION ====================

        public void StartProduction()
        {
            if (!IsActive || _isProducing || machineData == null) return;

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

            // Safe coroutine cleanup
            if (_productionCoroutine != null)
            {
                StopCoroutine(_productionCoroutine);
                _productionCoroutine = null;
            }

            // Stop visual effects safely
            StopAllVisualEffects();
        }

        private void StopAllVisualEffects()
        {
            AnimateGears(false);
            SetConveyorMovement(false);
        }

        private IEnumerator ProductionCycle()
        {
            while (_isProducing && IsActive && machineData != null)
            {
                if (_producedItems.Count < machineData.MaxCapacity)
                {
                    yield return StartCoroutine(ProduceItemSequence());
                }
                else
                {
                    yield return new WaitForSeconds(fullCapacityWaitTime);
                }

                yield return new WaitForSeconds(machineData.ProductionInterval);
            }
        }

        private IEnumerator ProduceItemSequence()
        {
            if (machineData == null || _itemPool == null) yield break;

            // Start visual feedback
            AnimateGears(true);
            SetConveyorMovement(true);

            float halfProductionTime = machineData.ProductionInterval * 0.5f;
            yield return new WaitForSeconds(halfProductionTime);

            // Spawn item with null checks
            Item item = _itemPool.GetItem(machineData.ProducedItemType);
            if (item != null && productionPoint != null)
            {
                item.transform.position = productionPoint.position;
                item.transform.rotation = Quaternion.identity;

                // Find available slot
                Transform targetSlot = GetAvailableSlot();

                // Animate to output with proper cleanup handling
                if (targetSlot != null)
                {
                    Tween moveTween = item.transform.DOMove(targetSlot.position, itemMoveAnimationDuration)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            if (item != null && this != null) // Null check for safety
                            {
                                _producedItems.Enqueue(item);

                                if (productionVFX != null)
                                    productionVFX.Play();

                                OnItemProduced?.Invoke(item);
                            }
                        });

                    if (moveTween != null)
                    {
                        _activeTweens.Add(moveTween);
                    }
                }
                else
                {
                    _producedItems.Enqueue(item);
                    OnItemProduced?.Invoke(item);
                }
            }

            yield return new WaitForSeconds(productionVisualDelay);

            // Stop visual feedback
            StopAllVisualEffects();
        }

        private void TryGiveItem(IInteractor interactor)
        {
            if (_producedItems.Count == 0 || interactor?.Character == null) return;

            var controller = interactor as InteractionController;
            var carryController = controller?.GetCachedCarryController();

            if (carryController != null && !carryController.IsFull())
            {
                // Safe peek operation
                Item item = null;
                if (_producedItems.Count > 0)
                {
                    item = _producedItems.Peek();
                }

                if (item == null) return;

                // Item type check
                if (carryController.CurrentItemType() != ItemType.None &&
                    carryController.CurrentItemType() != item.ItemType)
                {
                    return;
                }

                if (carryController.TryPickupItem(item))
                {
                    _producedItems.Dequeue();
                    OnItemCollected?.Invoke(item);
                }
            }
        }

        // ==================== VISUAL FEEDBACK ====================

        private void AnimateGears(bool animate)
        {
            if (gears == null) return;

            if (animate)
            {
                foreach (var gear in gears)
                {
                    if (gear != null && !_activeGearAnimations.Contains(gear))
                    {
                        float rotationDuration = 2f; // 360 degrees in 2 seconds
                        Tween gearTween = gear.DORotate(new Vector3(0, 360, 0), rotationDuration, RotateMode.LocalAxisAdd)
                            .SetLoops(-1, LoopType.Restart)
                            .SetEase(Ease.Linear);

                        if (gearTween != null)
                        {
                            _activeTweens.Add(gearTween);
                            _activeGearAnimations.Add(gear);
                        }
                    }
                }
            }
            else
            {
                // Stop all gear animations
                foreach (var gear in _activeGearAnimations)
                {
                    if (gear != null)
                    {
                        DOTween.Kill(gear);
                    }
                }
                _activeGearAnimations.Clear();
            }
        }

        private void SetConveyorMovement(bool moving)
        {
            if (conveyorRenderer?.material != null)
            {
                float scrollSpeed = moving ? 1f : 0f;
                conveyorRenderer.material.SetFloat("_ScrollSpeed", scrollSpeed);
            }
        }

        private Transform GetAvailableSlot()
        {
            if (outputSlots != null && outputSlots.Length > 0)
            {
                Transform slot = outputSlots[_currentSlotIndex % outputSlots.Length];
                _currentSlotIndex++;
                return slot;
            }
            return outputPoint;
        }

        public Transform GetEmployeeWaitPoint()
        {
            if (employeeWaitPoints != null && employeeWaitPoints.Length > 0)
            {
                return employeeWaitPoints[0];
            }
            return transform;
        }

        // ==================== CLEANUP ====================

        private void CleanupAllTweens()
        {
            // Clean up all tracked tweens
            foreach (var tween in _activeTweens)
            {
                if (tween != null && tween.IsActive())
                {
                    tween.Kill();
                }
            }
            _activeTweens.Clear();

            // Clean up gear animations
            foreach (var gear in _activeGearAnimations)
            {
                if (gear != null)
                {
                    DOTween.Kill(gear);
                }
            }
            _activeGearAnimations.Clear();

            // Clean up interaction tween
            CleanupInteractionTween();
        }

        protected override void OnDestroy()
        {
            StopProduction();
            CleanupAllTweens();
            base.OnDestroy();
        }

        void OnDisable()
        {
            StopProduction();
            CleanupAllTweens();
        }
    }
}
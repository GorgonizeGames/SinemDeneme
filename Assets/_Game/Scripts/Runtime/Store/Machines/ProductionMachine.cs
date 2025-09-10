using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Runtime.Store.Areas;
using Game.Runtime.Interactions.Interfaces;
using Game.Runtime.Items;
using Game.Runtime.Items.Data;
using Game.Runtime.Character.Components;
using Game.Runtime.Core.DI;
using DG.Tweening;
using Game.Runtime.Items.Services;

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

        [Inject] private IItemPoolService _itemPool;

        private Queue<Item> _producedItems = new Queue<Item>();
        private Coroutine _productionCoroutine;
        private bool _isProducing = false;
        private int _currentSlotIndex = 0;

        // Events
        public System.Action<Item> OnItemProduced;
        public System.Action<Item> OnItemCollected;

        protected override void Start()
        {
            base.Start();
            interactionType = InteractionType.Machine;
            interactionPriority = InteractionPriority.High;

            if (IsActive)
            {
                StartProduction();
            }
        }

        // ==================== PurchasableArea Overrides ====================

        protected override bool CanInteractWhenActive(IInteractor interactor)
        {
            // Machine'den item alabilmek için koşullar
            var controller = interactor as InteractionController;
            if (controller == null) return false;

            // Elinde item varsa ve Machine'de item varsa interaction yapmasın
            if (controller.HasItemsInHand() && _producedItems.Count > 0)
            {
                // Farklı item tipiyse alamaz
                var carryController = interactor.Character.GetComponentInChildren<StackingCarryController>();
                if (carryController != null && carryController.CurrentItemType != machineData.ProducedItemType)
                {
                    return false;
                }
            }

            // Daha fazla taşıyamazsa alamaz
            if (!controller.CanCarryMore())
            {
                return false;
            }

            // Machine'de item yoksa alamaz
            return _producedItems.Count > 0;
        }

        protected override void OnActiveInteractionStart(IInteractor interactor)
        {
            // Visual feedback
            DOTween.Kill(transform);
            transform.DOScale(Vector3.one * 1.05f, 0.2f);
        }

        protected override void OnActiveInteractionContinue(IInteractor interactor)
        {
            TryGiveItem(interactor);
        }

        protected override void OnActiveInteractionEnd(IInteractor interactor)
        {
            // Reset visual
            transform.DOScale(Vector3.one, 0.2f);
        }

        protected override void CompletePurchase()
        {
            base.CompletePurchase();
            StartProduction();
        }

        // ==================== PRODUCTION ====================

        public void StartProduction()
        {
            if (!IsActive || _isProducing) return;

            _isProducing = true;
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
        }

        private IEnumerator ProductionCycle()
        {
            while (_isProducing && IsActive)
            {
                if (_producedItems.Count < machineData.MaxCapacity)
                {
                    yield return StartCoroutine(ProduceItemSequence());
                }
                else
                {
                    yield return new WaitForSeconds(1f); // Wait if full
                }

                yield return new WaitForSeconds(machineData.ProductionInterval);
            }
        }

        private IEnumerator ProduceItemSequence()
        {
            // Start visual feedback
            AnimateGears(true);
            SetConveyorMovement(true);

            yield return new WaitForSeconds(machineData.ProductionInterval * 0.5f);

            // Spawn item
            Item item = _itemPool.GetItem(machineData.ProducedItemType);
            if (item != null)
            {
                item.transform.position = productionPoint.position;
                item.transform.rotation = Quaternion.identity;

                // Find available slot
                Transform targetSlot = GetAvailableSlot();

                // Animate to output
                item.transform.DOMove(targetSlot.position, 0.5f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        _producedItems.Enqueue(item);

                        if (productionVFX != null)
                            productionVFX.Play();

                        OnItemProduced?.Invoke(item);
                    });
            }

            yield return new WaitForSeconds(0.5f);

            // Stop visual feedback
            AnimateGears(false);
            SetConveyorMovement(false);
        }

        private void TryGiveItem(IInteractor interactor)
        {
            if (_producedItems.Count == 0) return;

            var carryController = interactor.Character.GetComponentInChildren<StackingCarryController>();
            if (carryController != null && !carryController.IsFull)
            {
                Item item = _producedItems.Peek();

                // Item tipi kontrolü
                if (carryController.CurrentItemType != ItemType.None &&
                    carryController.CurrentItemType != item.ItemType)
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

            foreach (var gear in gears)
            {
                if (gear != null)
                {
                    if (animate)
                    {
                        gear.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.LocalAxisAdd)
                            .SetLoops(-1, LoopType.Restart)
                            .SetEase(Ease.Linear);
                    }
                    else
                    {
                        DOTween.Kill(gear);
                    }
                }
            }
        }

        private void SetConveyorMovement(bool moving)
        {
            if (conveyorRenderer != null && conveyorRenderer.material != null)
            {
                float speed = moving ? 1f : 0f;
                conveyorRenderer.material.SetFloat("_ScrollSpeed", speed);
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
    }
}
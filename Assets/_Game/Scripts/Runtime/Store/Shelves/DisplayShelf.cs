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

        // ==================== PurchasableArea Overrides ====================

        protected override bool CanInteractWhenActive(IInteractor interactor)
        {
            if (interactor?.Character == null) return false;

            var controller = interactor as InteractionController;
            if (controller == null) return false;

            var character = interactor.Character;

            return IsEmployeeOrPlayer(character) 
                ? CanEmployeeInteract(controller, character)
                : IsCustomer(character) && CanCustomerInteract(controller);
        }

        private bool CanEmployeeInteract(InteractionController controller, BaseCharacterController character)
        {
            // For stocking: need items, correct type, shelf not full
            if (!controller.HasItemsInHand() || IsFull) return false;

            var carryController = character.GetComponentInChildren<StackingCarryController>();
            if (carryController == null || shelfData == null) return false;

            return carryController.CurrentItemType == shelfData.AcceptedItemType;
        }

        private bool CanCustomerInteract(InteractionController controller)
        {
            // For purchasing: shelf has items, customer can carry
            return !IsEmpty && controller.CanCarryMore();
        }

        protected override void OnActiveInteractionStart(IInteractor interactor)
        {
            // Visual feedback
            transform.DOScale(Vector3.one * interactionScaleAmount, interactionScaleDuration);
        }

        protected override void OnActiveInteractionContinue(IInteractor interactor)
        {
            if (interactor?.Character == null) return;

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

        protected override void OnActiveInteractionEnd(IInteractor interactor)
        {
            transform.DOScale(Vector3.one, interactionScaleDuration);
        }

        // ==================== STOCKING ====================

        private void TryStockItem(IInteractor interactor)
        {
            if (IsFull || interactor?.Character == null || shelfData == null) return;

            var carryController = interactor.Character.GetComponentInChildren<StackingCarryController>();
            if (carryController != null && carryController.CurrentItemType == shelfData.AcceptedItemType)
            {
                Item item = carryController.RemoveTopItem();
                if (item != null)
                {
                    PlaceItemOnShelf(item);
                }
            }
        }

        private void PlaceItemOnShelf(Item item)
        {
            if (item == null) return;

            Transform slot = GetEmptySlot();
            if (slot != null)
            {
                item.OnPlacedOnShelf(slot);

                // Animate placement
                item.transform.DOScale(0f, 0f);
                item.transform.DOScale(1f, stockAnimationDuration).SetEase(Ease.OutBack);

                _slotItemMap[slot] = item;
                _displayedItems.Add(item);

                OnItemStocked?.Invoke(item);
            }
        }

        // ==================== CUSTOMER PURCHASE ====================

        private void TryTakeItem(IInteractor interactor)
        {
            if (IsEmpty || interactor?.Character == null) return;

            var carryController = interactor.Character.GetComponentInChildren<StackingCarryController>();
            if (carryController != null && !carryController.IsFull)
            {
                Item item = GetLastItem();
                if (item != null && carryController.TryPickupItem(item))
                {
                    RemoveItemFromShelf(item);
                    OnItemPurchased?.Invoke(item);
                }
            }
        }

        private void RemoveItemFromShelf(Item item)
        {
            if (item == null) return;

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

        // ==================== UTILITY ====================

        private Transform GetEmptySlot()
        {
            foreach (var kvp in _slotItemMap)
            {
                if (kvp.Value == null)
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

            // Compact items to fill gaps
            var items = new List<Item>(_displayedItems);
            InitializeSlots();

            int index = 0;
            foreach (var item in items)
            {
                if (item != null && index < displaySlots.Length)
                {
                    Transform slot = displaySlots[index];
                    if (slot != null)
                    {
                        item.transform.DOMove(slot.position, rearrangeAnimationDuration);
                        _slotItemMap[slot] = item;
                    }
                    index++;
                }
            }
        }

        private bool IsEmployeeOrPlayer(BaseCharacterController character)
        {
            if (character is PlayerCharacterController) return true;

            var aiController = character as AICharacterController;
            return aiController != null && aiController.Data.CharacterType == CharacterType.AI_Employee;
        }

        private bool IsCustomer(BaseCharacterController character)
        {
            var aiController = character as AICharacterController;
            return aiController != null && aiController.Data.CharacterType == CharacterType.AI_Customer;
        }

        public Transform GetCustomerBrowsePoint()
        {
            if (customerBrowsePoints != null && customerBrowsePoints.Length > 0)
            {
                return customerBrowsePoints[Random.Range(0, customerBrowsePoints.Length)];
            }
            return transform;
        }
    }
}
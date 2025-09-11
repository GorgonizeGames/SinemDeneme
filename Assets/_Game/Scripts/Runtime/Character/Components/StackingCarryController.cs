using UnityEngine;
using System.Collections.Generic;
using Game.Runtime.Items;
using Game.Runtime.Items.Data;
using Game.Runtime.Character.Interfaces;
using Game.Runtime.Core.Interfaces;
using Game.Runtime.Items.Interfaces;

namespace Game.Runtime.Character.Components
{
    public class StackingCarryController : MonoBehaviour, ICarryingController
    {
        [Header("Stacking Settings")]
        [SerializeField] private Transform stackPoint;
        [SerializeField] private int maxStackSize = 10;
        [SerializeField] private float stackHeight = 0.1f;

        private Stack<Item> _carriedItems = new Stack<Item>();
        private ItemType _currentItemType = ItemType.None;

        // ICarryingController Implementation
        public bool IsCarrying => _carriedItems.Count > 0;
        public GameObject CarriedItem => _carriedItems.Count > 0 ? _carriedItems.Peek().gameObject : null;
        public Transform CarryPoint => stackPoint;
        public Transform Transform => transform;

        // Stacking Properties
        public ItemType CurrentItemType => _currentItemType;
        public int CarriedCount => _carriedItems.Count;
        public bool IsFull => _carriedItems.Count >= maxStackSize;

        void Awake()
        {
            if (stackPoint == null)
            {
                Debug.LogError($"[{gameObject.name}] StackPoint is not assigned!", this);
            }
        }

        public bool CanPickupItem(IPickupable pickupable)
        {
            if (pickupable == null || !pickupable.CanBePickedUp) return false;
            if (IsFull) return false;

            var item = pickupable as Item;
            if (item == null) return false;

            // First item sets the type, subsequent items must match
            if (_carriedItems.Count == 0) return true;

            return item.ItemType == _currentItemType;
        }

        public bool TryPickupItem(IPickupable pickupable)
        {
            if (!CanPickupItem(pickupable)) return false;

            var item = pickupable as Item;
            if (item == null) return false;

            // Set item type if this is the first item
            if (_carriedItems.Count == 0)
            {
                _currentItemType = item.ItemType;
            }

            // Calculate stack position
            Vector3 stackPosition = stackPoint.position + Vector3.up * (_carriedItems.Count * stackHeight);

            // Add to stack
            _carriedItems.Push(item);
            item.OnPickedUp(this);

            // Position the item in the stack
            item.transform.position = stackPosition;

            return true;
        }

        public bool TryDropItem()
        {
            if (_carriedItems.Count == 0) return false;

            Item topItem = _carriedItems.Pop();
            Vector3 dropPosition = transform.position + transform.forward * 1f;

            topItem.OnDropped(dropPosition);

            // Clear item type if stack is empty
            if (_carriedItems.Count == 0)
            {
                _currentItemType = ItemType.None;
            }

            return true;
        }

        public Item RemoveTopItem()
        {
            if (_carriedItems.Count == 0) return null;

            Item topItem = _carriedItems.Pop();

            // Clear item type if stack is empty
            if (_carriedItems.Count == 0)
            {
                _currentItemType = ItemType.None;
            }

            return topItem;
        }

        public void ForceDropItem()
        {
            while (_carriedItems.Count > 0)
            {
                TryDropItem();
            }
        }

        public void DropAllItems()
        {
            ForceDropItem();
        }

        void OnDestroy()
        {
            ForceDropItem();
        }
    }
}
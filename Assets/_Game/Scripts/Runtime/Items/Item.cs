using UnityEngine;
using Game.Runtime.Items.Data;
using Game.Runtime.Items.Interfaces;
using Game.Runtime.Core.Interfaces;
using Game.Runtime.Store.Machines;
using System.Collections.Generic;

namespace Game.Runtime.Items
{
    public class Item : MonoBehaviour, IPickupable
    {
        [Header("Item Configuration")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private string itemId;

        private bool _isBeingCarried = false;
        private bool _isOnShelf = false;
        private Rigidbody _rigidbody;

        // IPickupable Implementation
        public string ItemId => itemId;
        public Transform Transform => transform;
        public bool CanBePickedUp => !_isBeingCarried && !_isOnShelf;
        public ItemType ItemType => itemData?.ItemType ?? ItemType.None;

        // Properties
        public ItemData Data => itemData;
        public bool IsOnShelf => _isOnShelf;

        public ItemPlacer Placer { get; set; }
        public List<Collider> Colliders { get; }

        public Rigidbody Rigidbody;
        public void OnReset() { /* implementation */ }

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            if (string.IsNullOrEmpty(itemId))
                itemId = System.Guid.NewGuid().ToString();
        }

        public void OnPickedUp(ICarrier carrier)
        {
            _isBeingCarried = true;
            _isOnShelf = false;

            if (_rigidbody != null)
                _rigidbody.isKinematic = true;

            transform.SetParent(carrier.CarryPoint);
            transform.localPosition = GetCarryOffset();
            transform.localRotation = Quaternion.Euler(GetCarryRotation());
        }

        public void OnDropped(Vector3 dropPosition)
        {
            _isBeingCarried = false;
            _isOnShelf = false;

            transform.SetParent(null);
            transform.position = dropPosition;

            if (_rigidbody != null)
                _rigidbody.isKinematic = false;
        }

        public void OnPlacedOnShelf(Transform shelfSlot)
        {
            _isBeingCarried = false;
            _isOnShelf = true;

            transform.SetParent(shelfSlot);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            if (_rigidbody != null)
                _rigidbody.isKinematic = true;
        }

        public void ReturnToPool()
        {
            _isBeingCarried = false;
            _isOnShelf = false;

            transform.SetParent(null);
            gameObject.SetActive(false);
        }

        public Vector3 GetCarryOffset()
        {
            return Vector3.zero; // Can be customized per item type
        }

        public Vector3 GetCarryRotation()
        {
            return Vector3.zero; // Can be customized per item type
        }

        public void SetItemData(ItemData data)
        {
            itemData = data;
        }
    }
}
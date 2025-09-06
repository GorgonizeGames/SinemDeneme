using UnityEngine;
using System.Collections.Generic;
using Game.Runtime.Items;
using Game.Runtime.Zones;
using Game.Runtime.Items.Interfaces;
using Game.Runtime.Character.Interfaces;

namespace Game.Runtime.Character.Components
{
    [RequireComponent(typeof(Collider))]
    public class CharacterTriggerDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private LayerMask itemLayerMask = 1;
        [SerializeField] private LayerMask zoneLayerMask = 1;
        [SerializeField] private bool enableDebugLogs = false;

        // Detected objects
        private List<IPickupable> _nearbyItems = new List<IPickupable>();
        private List<PickupZone> _nearbyPickupZones = new List<PickupZone>();
        private List<DropZone> _nearbyDropZones = new List<DropZone>();

        // Component references
        private ICarryingController _carryingController;
        private ICharacterController _characterController;

        // Public properties for other components to query
        public IReadOnlyList<IPickupable> NearbyItems => _nearbyItems;
        public IReadOnlyList<PickupZone> NearbyPickupZones => _nearbyPickupZones;
        public IReadOnlyList<DropZone> NearbyDropZones => _nearbyDropZones;

        void Awake()
        {
            // Setup trigger collider
            var collider = GetComponent<Collider>();
            collider.isTrigger = true;

            // Get components
            _carryingController = GetComponent<ICarryingController>();
            _characterController = GetComponent<ICharacterController>();

            if (_carryingController == null)
            {
                Debug.LogError("❌ CharacterTriggerDetector requires ICarryingController!", this);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            // Check layer masks
            int otherLayer = 1 << other.gameObject.layer;

            // Detect items
            if ((itemLayerMask & otherLayer) != 0)
            {
                var pickupable = other.GetComponent<IPickupable>();
                if (pickupable != null && !_nearbyItems.Contains(pickupable))
                {
                    _nearbyItems.Add(pickupable);
                    OnItemDetected(pickupable);
                }
            }

            // Detect zones
            if ((zoneLayerMask & otherLayer) != 0)
            {
                var pickupZone = other.GetComponent<PickupZone>();
                if (pickupZone != null && !_nearbyPickupZones.Contains(pickupZone))
                {
                    _nearbyPickupZones.Add(pickupZone);
                    OnPickupZoneEntered(pickupZone);
                }

                var dropZone = other.GetComponent<DropZone>();
                if (dropZone != null && !_nearbyDropZones.Contains(dropZone))
                {
                    _nearbyDropZones.Add(dropZone);
                    OnDropZoneEntered(dropZone);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            // Remove items
            var pickupable = other.GetComponent<IPickupable>();
            if (pickupable != null)
            {
                _nearbyItems.Remove(pickupable);
                OnItemLost(pickupable);
            }

            // Remove zones
            var pickupZone = other.GetComponent<PickupZone>();
            if (pickupZone != null)
            {
                _nearbyPickupZones.Remove(pickupZone);
                OnPickupZoneExited(pickupZone);
            }

            var dropZone = other.GetComponent<DropZone>();
            if (dropZone != null)
            {
                _nearbyDropZones.Remove(dropZone);
                OnDropZoneExited(dropZone);
            }
        }

        private void OnItemDetected(IPickupable item)
        {
            if (enableDebugLogs)
                Debug.Log($"📦 Item detected: {item.ItemId}");

            // Try auto-pickup if character is in a pickup zone and hands are free
            if (!_carryingController.IsCarrying && _nearbyPickupZones.Count > 0)
            {
                TryAutoPickupInZone(item);
            }
        }

        private void OnItemLost(IPickupable item)
        {
            if (enableDebugLogs)
                Debug.Log($"📦 Item lost: {item.ItemId}");
        }

        private void OnPickupZoneEntered(PickupZone zone)
        {
            if (enableDebugLogs)
                Debug.Log($"🟢 Entered pickup zone: {zone.ZoneId}");

            // Try to pickup available items
            if (!_carryingController.IsCarrying && zone.AutoPickup)
            {
                TryAutoPickupInZone();
            }
        }

        private void OnPickupZoneExited(PickupZone zone)
        {
            if (enableDebugLogs)
                Debug.Log($"🟢 Exited pickup zone: {zone.ZoneId}");
        }

        private void OnDropZoneEntered(DropZone zone)
        {
            if (enableDebugLogs)
                Debug.Log($"🔴 Entered drop zone: {zone.ZoneId}");

            // Try to drop carried item
            if (_carryingController.IsCarrying && zone.AutoDrop)
            {
                TryAutoDropInZone(zone);
            }
        }

        private void OnDropZoneExited(DropZone zone)
        {
            if (enableDebugLogs)
                Debug.Log($"🔴 Exited drop zone: {zone.ZoneId}");
        }

        private void TryAutoPickupInZone(IPickupable specificItem = null)
        {
            // Get the first pickup zone (could be enhanced to prioritize)
            var activeZone = _nearbyPickupZones.Count > 0 ? _nearbyPickupZones[0] : null;
            if (activeZone == null) return;

            var itemsToCheck = specificItem != null ? 
                new List<IPickupable> { specificItem } : _nearbyItems;

            foreach (var item in itemsToCheck)
            {
                if (activeZone.IsItemAllowed(item) && _carryingController.CanPickupItem(item))
                {
                    bool success = _carryingController.TryPickupItem(item);
                    if (success)
                    {
                        if (enableDebugLogs)
                            Debug.Log($"✅ Auto-pickup successful: {item.ItemId}");
                        break; // Only pick up one item
                    }
                }
            }
        }

        private void TryAutoDropInZone(DropZone zone)
        {
            if (_carryingController.CarriedItem != null)
            {
                var carryingController = _carryingController as CarryingController;
                var carriedItemComponent = carryingController?.GetCarriedItemComponent();
                
                if (carriedItemComponent != null && zone.IsItemAccepted(carriedItemComponent))
                {
                    bool success = _carryingController.TryDropItem();
                    if (success)
                    {
                        zone.NotifyItemDropped(carriedItemComponent);
                        if (enableDebugLogs)
                            Debug.Log($"✅ Auto-drop successful: {carriedItemComponent.ItemId}");
                    }
                }
            }
        }

        // Public methods for manual pickup/drop
        public IPickupable GetBestAvailableItem()
        {
            foreach (var item in _nearbyItems)
            {
                if (_carryingController.CanPickupItem(item))
                {
                    return item;
                }
            }
            return null;
        }

        public bool IsInPickupZone()
        {
            return _nearbyPickupZones.Count > 0;
        }

        public bool IsInDropZone()
        {
            return _nearbyDropZones.Count > 0;
        }

        // Debug visualization
        void OnDrawGizmosSelected()
        {
            // Draw detection radius
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                Gizmos.color = Color.cyan;
                if (collider is SphereCollider sphere)
                {
                    Gizmos.DrawWireSphere(transform.position, sphere.radius);
                }
                else if (collider is BoxCollider box)
                {
                    Gizmos.DrawWireCube(transform.position, box.size);
                }
            }

            // Draw lines to nearby items
            Gizmos.color = Color.green;
            foreach (var item in _nearbyItems)
            {
                if (item?.Transform != null)
                {
                    Gizmos.DrawLine(transform.position, item.Transform.position);
                }
            }
        }
    }
}
using UnityEngine;
using Game.Runtime.Items.Interfaces;
using Game.Runtime.Character.Interfaces;

namespace Game.Runtime.Items
{
    public class PickupableItem : MonoBehaviour, IPickupable
    {
        [Header("Item Settings")]
        [SerializeField] private string itemId = "DefaultItem";
        [SerializeField] private Vector3 carryOffset = Vector3.zero;
        [SerializeField] private Vector3 carryRotation = Vector3.zero;
        [SerializeField] private bool canBePickedUp = true;

        [Header("Visual Settings")]
        [SerializeField] private GameObject visualObject;
        [SerializeField] private ParticleSystem pickupEffect;

        private Rigidbody _rigidbody;
        private bool _isBeingCarried = false;

        public string ItemId => itemId;
        public Transform Transform => transform;
        public bool CanBePickedUp => canBePickedUp && !_isBeingCarried;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void OnPickedUp(ICarryingController carrier)
        {
            _isBeingCarried = true;
            canBePickedUp = false;
            
            // Disable physics while carrying
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
            }
            
            // Play pickup effect
            if (pickupEffect != null)
            {
                pickupEffect.Play();
            }
            
            Debug.Log($"📦 Item '{itemId}' picked up");
        }

        public void OnDropped(Vector3 dropPosition)
        {
            _isBeingCarried = false;
            canBePickedUp = true;
            
            // Detach from parent
            transform.SetParent(null);
            transform.position = dropPosition;
            
            // Re-enable physics
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = false;
            }
            
            Debug.Log($"📦 Item '{itemId}' dropped at {dropPosition}");
        }

        public Vector3 GetCarryOffset()
        {
            return carryOffset;
        }

        public Vector3 GetCarryRotation()
        {
            return carryRotation;
        }
    }
}
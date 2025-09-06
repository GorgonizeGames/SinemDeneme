using UnityEngine;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.Character.States.UpperBody;
using Game.Runtime.Character.Interfaces;
using Game.Runtime.Items.Interfaces;

namespace Game.Runtime.Character.Components
{
    public class CarryingController : MonoBehaviour, ICarryingController
    {
        [Header("Carrying Settings")]
        [SerializeField] private Transform carryPoint;
        [SerializeField] private LayerMask pickupLayerMask = 1;
        [SerializeField] private float pickupRange = 1.5f;
        [SerializeField] private bool enableDebugLogs = false;

        // Simplified state machine - only 2 states
        private StateMachine<ICarryingController> _upperBodyStateMachine;
        private GameObject _carriedItem;
        private IPickupable _carriedItemComponent;

        // Public Properties
        public bool IsCarrying => _carriedItem != null;
        public GameObject CarriedItem => _carriedItem;
        public Transform CarryPoint => carryPoint;

        void Awake()
        {
            SetupUpperBodyStateMachine();
            ValidateComponents();
        }

        void Start()
        {
            // Start with hands free
            _upperBodyStateMachine.ChangeState<HandsFreeState>();
        }

        private void SetupUpperBodyStateMachine()
        {
            _upperBodyStateMachine = new StateMachine<ICarryingController>(this);
            _upperBodyStateMachine.AddState(new HandsFreeState());
            _upperBodyStateMachine.AddState(new CarryingState());
        }

        private void ValidateComponents()
        {
            if (carryPoint == null)
            {
                Debug.LogError("❌ CarryPoint is not assigned!", this);
            }
        }

        void Update()
        {
            _upperBodyStateMachine.Update();
        }

        public bool CanPickupItem(IPickupable item)
        {
            if (IsCarrying) return false;
            if (item == null || !item.CanBePickedUp) return false;
            
            float distance = Vector3.Distance(transform.position, item.Transform.position);
            return distance <= pickupRange;
        }

        public bool TryPickupItem(IPickupable item)
        {
            if (!CanPickupItem(item)) return false;

            _carriedItemComponent = item;
            _carriedItem = item.Transform.gameObject;
            
            // Immediately attach and transition to carrying state
            AttachItemToCarryPoint();
            _carriedItemComponent.OnPickedUp(this);
            
            // Transition to carrying state (animator handles the transition)
            bool success = _upperBodyStateMachine.ChangeState<CarryingState>();
            
            if (enableDebugLogs && success)
                Debug.Log($"📦 Item picked up: {_carriedItemComponent.ItemId}");
                
            return success;
        }

        public bool TryDropItem()
        {
            if (!IsCarrying) return false;
            
            // Drop the item and transition to hands free
            DropCurrentItem();
            bool success = _upperBodyStateMachine.ChangeState<HandsFreeState>();
            
            if (enableDebugLogs && success)
                Debug.Log($"📦 Item dropped");
                
            return success;
        }

        public void ForceDropItem()
        {
            if (!IsCarrying) return;

            DropCurrentItem();
            _upperBodyStateMachine.ChangeState<HandsFreeState>();
            
            if (enableDebugLogs)
                Debug.Log("📦 Item force dropped");
        }

        private void AttachItemToCarryPoint()
        {
            if (_carriedItem != null && carryPoint != null)
            {
                // Position item at carry point
                _carriedItem.transform.SetParent(carryPoint);
                _carriedItem.transform.localPosition = _carriedItemComponent?.GetCarryOffset() ?? Vector3.zero;
                _carriedItem.transform.localEulerAngles = _carriedItemComponent?.GetCarryRotation() ?? Vector3.zero;
                
                // Disable physics while carrying
                var rb = _carriedItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }
            }
        }

        private void DropCurrentItem()
        {
            if (_carriedItemComponent != null)
            {
                Vector3 dropPosition = transform.position + transform.forward * 1f;
                _carriedItemComponent.OnDropped(dropPosition);
            }
            
            _carriedItem = null;
            _carriedItemComponent = null;
        }

        // Internal method for states to access carried item component
        internal IPickupable GetCarriedItemComponent()
        {
            return _carriedItemComponent;
        }

        // Debug
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, pickupRange);
            
            if (carryPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(carryPoint.position, Vector3.one * 0.2f);
            }
        }
    }
}
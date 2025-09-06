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

        private StateMachine<ICarryingController> _upperBodyStateMachine;
        private GameObject _carriedItem;
        private IPickupable _carriedItemComponent;
        private bool _isInitialized = false;

        // ICarryingController Implementation
        public bool IsCarrying => _carriedItem != null;
        public GameObject CarriedItem => _carriedItem;
        public Transform CarryPoint => carryPoint;
        public Transform Transform => transform;

        void Awake()
        {
            SetupUpperBodyStateMachine();
            ValidateComponents();
            _isInitialized = true;
        }

        void Start()
        {
            if (_isInitialized && _upperBodyStateMachine != null)
            {
                _upperBodyStateMachine.ChangeState<HandsFreeState>();
            }
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
                Debug.LogError($"[{gameObject.name}] CarryPoint is not assigned!", this);
            }
        }

        void Update()
        {
            if (_isInitialized)
            {
                _upperBodyStateMachine?.Update();
            }
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

            AttachItemToCarryPoint();
            _carriedItemComponent.OnPickedUp(this);

            bool success = _upperBodyStateMachine?.ChangeState<CarryingState>() ?? false;

            if (enableDebugLogs && success)
                Debug.Log($"📦 Item picked up: {_carriedItemComponent.ItemId}");

            return success;
        }

        public bool TryDropItem()
        {
            if (!IsCarrying) return false;

            DropCurrentItem();
            bool success = _upperBodyStateMachine?.ChangeState<HandsFreeState>() ?? false;

            if (enableDebugLogs && success)
                Debug.Log($"📦 Item dropped");

            return success;
        }

        public void ForceDropItem()
        {
            if (!IsCarrying) return;

            DropCurrentItem();
            _upperBodyStateMachine?.ChangeState<HandsFreeState>();

            if (enableDebugLogs)
                Debug.Log("📦 Item force dropped");
        }

        private void AttachItemToCarryPoint()
        {
            if (_carriedItem != null && carryPoint != null)
            {
                _carriedItem.transform.SetParent(carryPoint);
                _carriedItem.transform.localPosition = _carriedItemComponent?.GetCarryOffset() ?? Vector3.zero;
                _carriedItem.transform.localEulerAngles = _carriedItemComponent?.GetCarryRotation() ?? Vector3.zero;

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

        internal IPickupable GetCarriedItemComponent()
        {
            return _carriedItemComponent;
        }

        void OnDestroy()
        {
            if (IsCarrying)
            {
                ForceDropItem();
            }
            _upperBodyStateMachine?.Cleanup();
        }

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
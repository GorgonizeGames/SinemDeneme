using UnityEngine;
using UnityEngine.AI;
using Game.Runtime.Items.Interfaces;
using Game.Runtime.Character.Interfaces;
using Game.Runtime.Character.AI.Factory;

namespace Game.Runtime.Character.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AICharacterController : BaseCharacterController
    {
        [Header("AI Navigation")]
        [SerializeField] protected NavMeshAgent navMeshAgent;
        [SerializeField] protected float stoppingDistance = 0.5f;
        [SerializeField] protected float navMeshSampleDistance = 1f;

        [Header("AI Behavior")]
        [SerializeField] protected float decisionInterval = 1f;
        [SerializeField] protected bool enableDebugLogs = false;

        [Header("AI Pickup Settings")]
        [SerializeField] protected float pickupRange = 1.5f;
        [SerializeField] protected bool enableSmartPickup = true;

        // AI Behavior System
        protected IAIBehavior _currentBehavior;
        protected float _lastDecisionTime;
        protected Vector3 _targetDestination;
        protected bool _isMovingToTarget;

        // AI Task System
        protected IPickupable _targetItem;
        protected Transform _targetDropZone;

        // Public properties
        public NavMeshAgent NavAgent => navMeshAgent;
        public bool IsMoving => navMeshAgent != null && navMeshAgent.velocity.magnitude > 0.1f;
        public bool HasReachedDestination => navMeshAgent != null && !navMeshAgent.pathPending && navMeshAgent.remainingDistance < stoppingDistance;

        protected override void Awake()
        {
            base.Awake();

            if (navMeshAgent == null)
                navMeshAgent = GetComponent<NavMeshAgent>();

            SetupNavMeshAgent();
        }

        protected override void OnInitialize()
        {
            SetupAIBehavior();

            if (enableDebugLogs)
                Debug.Log($"🤖 AI Character initialized - Role: {Data.CharacterType}, Type: {Data.CharacterType}");
        }


        protected virtual void SetupNavMeshAgent()
        {
            if (navMeshAgent == null) return;

            navMeshAgent.stoppingDistance = stoppingDistance;
            navMeshAgent.updateRotation = false;
            navMeshAgent.updatePosition = false;
        }

        protected virtual void SetupAIBehavior()
        {
            _currentBehavior = AIBehaviorFactory.CreateBehavior(Data.CharacterType, this);
            _currentBehavior?.Initialize();
        }

        protected override void HandleInput()
        {
            // AI decision making
            if (Time.time - _lastDecisionTime >= decisionInterval)
            {
                _currentBehavior?.UpdateBehavior();

                // AI pickup logic
                if (enableSmartPickup)
                {
                    HandleAIPickupLogic();
                }

                _lastDecisionTime = Time.time;
            }

            // Convert NavMesh movement to CharacterMotor input
            UpdateMovementFromNavMesh();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            // Sync NavMesh position in FixedUpdate for physics consistency
            if (navMeshAgent != null && !navMeshAgent.updatePosition)
            {
                navMeshAgent.nextPosition = transform.position;
            }
        }

        protected virtual void HandleAIPickupLogic()
        {
            // Only employees should actively pickup items for now
            if (Data.CharacterType != CharacterType.AI_Employee) return;
            if (_carryingController == null || _triggerDetector == null) return;

            if (!_carryingController.IsCarrying && _targetItem == null)
            {
                FindNearbyItems();
            }
            else if (_carryingController.IsCarrying && _targetDropZone == null)
            {
                FindNearbyDropZones();
            }
        }

        private void FindNearbyItems()
        {
            if (_triggerDetector == null) return;

            var nearbyItems = _triggerDetector.NearbyItems;

            foreach (var item in nearbyItems)
            {
                if (_carryingController.CanPickupItem(item))
                {
                    _targetItem = item;
                    MoveTo(item.Transform.position);

                    if (enableDebugLogs)
                        Debug.Log($"🤖 AI found target item: {item.ItemId}");
                    break;
                }
            }
        }

        private void FindNearbyDropZones()
        {
            if (_triggerDetector == null) return;

            var nearbyDropZones = _triggerDetector.NearbyDropZones;

            foreach (var dropZone in nearbyDropZones)
            {
                _targetDropZone = dropZone.transform;
                MoveTo(dropZone.transform.position);

                if (enableDebugLogs)
                    Debug.Log($"🤖 AI found target drop zone: {dropZone.ZoneId}");
                break;
            }
        }

        protected virtual void UpdateMovementFromNavMesh()
        {
            if (navMeshAgent == null) return;

            if (navMeshAgent.hasPath && navMeshAgent.velocity.magnitude > 0.1f)
            {
                Vector3 velocity = navMeshAgent.desiredVelocity.normalized;
                Vector2 movementInput = new Vector2(velocity.x, velocity.z);
                SetMovementInput(movementInput);
            }
            else
            {
                SetMovementInput(Vector2.zero);
                CheckTargetReached();
            }
        }

        private void CheckTargetReached()
        {
            if (_carryingController == null) return;

            // Check if we reached target item
            if (_targetItem != null && !_carryingController.IsCarrying)
            {
                float distance = Vector3.Distance(transform.position, _targetItem.Transform.position);
                if (distance <= stoppingDistance)
                {
                    bool success = _carryingController.TryPickupItem(_targetItem);
                    if (success && enableDebugLogs)
                    {
                        Debug.Log($"🤖 AI picked up item: {_targetItem.ItemId}");
                    }
                    _targetItem = null;
                }
            }

            // Check if we reached drop zone
            if (_targetDropZone != null && _carryingController.IsCarrying)
            {
                float distance = Vector3.Distance(transform.position, _targetDropZone.position);
                if (distance <= stoppingDistance)
                {
                    _targetDropZone = null;
                }
            }
        }

        public virtual bool MoveTo(Vector3 destination)
        {
            if (navMeshAgent == null) return false;

            if (IsValidDestination(destination))
            {
                navMeshAgent.SetDestination(destination);
                _targetDestination = destination;
                _isMovingToTarget = true;

                if (enableDebugLogs)
                    Debug.Log($"🎯 {Data.CharacterType} moving to: {destination}");

                return true;
            }
            return false;
        }

        public virtual void Stop()
        {
            navMeshAgent?.ResetPath();
            _isMovingToTarget = false;
            SetMovementInput(Vector2.zero);
            _motor?.Stop();
        }

        protected virtual bool IsValidDestination(Vector3 destination)
        {
            NavMeshHit hit;
            return NavMesh.SamplePosition(destination, out hit, navMeshSampleDistance, NavMesh.AllAreas);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _currentBehavior?.OnBehaviorEnd();
        }

        void OnDrawGizmosSelected()
        {
            if (navMeshAgent != null && navMeshAgent.hasPath)
            {
                Gizmos.color = Color.yellow;
                Vector3[] path = navMeshAgent.path.corners;
                for (int i = 0; i < path.Length - 1; i++)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }

            if (_isMovingToTarget)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_targetDestination, 0.5f);
            }

            // Draw pickup range
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, pickupRange);
        }
    }
}
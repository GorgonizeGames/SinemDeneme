using UnityEngine;
using UnityEngine.AI;
using Game.Runtime.Items.Interfaces;
using Game.Runtime.Character.Interfaces;

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
        [SerializeField] protected AIRole aiRole;
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
        public AIRole Role => aiRole;
        public bool IsMoving => navMeshAgent.velocity.magnitude > 0.1f;
        public bool HasReachedDestination => !navMeshAgent.pathPending && navMeshAgent.remainingDistance < stoppingDistance;

        protected override void OnInitialize()
        {
            SetupNavMeshAgent();
            SetupAIBehavior();
            SyncCharacterType();
            
            if (enableDebugLogs)
                Debug.Log($"🤖 AI Character initialized - Role: {aiRole}, Type: {characterType}");
        }

        private void SyncCharacterType()
        {
            switch (aiRole)
            {
                case AIRole.Customer:
                    characterType = CharacterType.AI_Customer;
                    break;
                case AIRole.Employee:
                    characterType = CharacterType.AI_Employee;
                    break;
                case AIRole.Cashier:
                    characterType = CharacterType.AI_Cashier;
                    break;
            }
        }

        protected virtual void SetupNavMeshAgent()
        {
            if (navMeshAgent == null)
                navMeshAgent = GetComponent<NavMeshAgent>();

            navMeshAgent.stoppingDistance = stoppingDistance;
            navMeshAgent.updateRotation = false;
            navMeshAgent.updatePosition = false;
        }

        protected virtual void SetupAIBehavior()
        {
            _currentBehavior = AIBehaviorFactory.CreateBehavior(aiRole, this);
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

        protected virtual void HandleAIPickupLogic()
        {
            // Only employees should actively pickup items for now
            if (aiRole != AIRole.Employee) return;

            if (!_carryingController.IsCarrying && _targetItem == null)
            {
                // Look for items to pickup using trigger detector
                FindNearbyItems();
            }
            else if (_carryingController.IsCarrying && _targetDropZone == null)
            {
                // Look for drop zones using trigger detector
                FindNearbyDropZones();
            }
        }

        private void FindNearbyItems()
        {
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
            if (navMeshAgent.hasPath && navMeshAgent.velocity.magnitude > 0.1f)
            {
                Vector3 velocity = navMeshAgent.desiredVelocity.normalized;
                Vector2 movementInput = new Vector2(velocity.x, velocity.z);
                SetMovementInput(movementInput);

                if (!navMeshAgent.updatePosition)
                {
                    navMeshAgent.nextPosition = transform.position;
                }
            }
            else
            {
                SetMovementInput(Vector2.zero);
                
                // Check if we reached target item or drop zone
                CheckTargetReached();
            }
        }

        private void CheckTargetReached()
        {
            // Check if we reached target item
            if (_targetItem != null && !_carryingController.IsCarrying)
            {
                float distance = Vector3.Distance(transform.position, _targetItem.Transform.position);
                if (distance <= stoppingDistance)
                {
                    bool success = _carryingController.TryPickupItem(_targetItem);
                    if (success)
                    {
                        if (enableDebugLogs)
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
                    // Drop zones handle auto-drop, just clear target
                    _targetDropZone = null;
                }
            }
        }

        public virtual bool MoveTo(Vector3 destination)
        {
            if (IsValidDestination(destination))
            {
                navMeshAgent.SetDestination(destination);
                _targetDestination = destination;
                _isMovingToTarget = true;
                
                if (enableDebugLogs)
                    Debug.Log($"🎯 {aiRole} moving to: {destination}");
                
                return true;
            }
            return false;
        }

        public virtual void Stop()
        {
            navMeshAgent.ResetPath();
            _isMovingToTarget = false;
            SetMovementInput(Vector2.zero);
            
            if (_motor != null)
            {
                _motor.Stop();
            }
            else
            {
                Debug.LogError("❌ CharacterMotor is null in AICharacterController!", this);
            }
        }

        public virtual void SetRole(AIRole newRole)
        {
            if (aiRole != newRole)
            {
                aiRole = newRole;
                SyncCharacterType();
                SetupAIBehavior();
            }
        }

        protected virtual bool IsValidDestination(Vector3 destination)
        {
            NavMeshHit hit;
            return NavMesh.SamplePosition(destination, out hit, navMeshSampleDistance, NavMesh.AllAreas);
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

    public enum AIRole
    {
        Customer,
        Employee,
        Cashier
    }
}
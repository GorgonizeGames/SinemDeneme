using UnityEngine;
using UnityEngine.AI;
using Game.Runtime.Items.Interfaces;
using Game.Runtime.Character.Interfaces;
using Game.Runtime.Character.AI.Factory;
using Game.Runtime.Character.Components;
using Game.Runtime.Character.Animation;

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

        [Header("AI Movement Thresholds")]
        [SerializeField] protected float velocityThreshold = 0.1f;

        // AI Behavior System
        protected IAIBehavior _currentBehavior;
        protected float _lastDecisionTime;
        protected Vector3 _targetDestination;
        protected bool _isMovingToTarget;

        // AI Task System
        protected IPickupable _targetItem;
        protected Transform _targetDropZone;

        // Performance optimization - cached values to avoid allocations
        private Vector2 _cachedMovementInput = Vector2.zero;
        
        // Cached debug strings to avoid string allocation in hot paths
        private string _cachedInitMessage;
        private string _cachedMoveMessage;
        private string _cachedPickupMessage;
        private bool _debugStringsDirty = true;

        // Cached distance calculations (squared to avoid sqrt)
        private float _stoppingDistanceSquared;
        private float _velocityThresholdSquared;

        // Public properties
        public NavMeshAgent NavAgent => navMeshAgent;
        public bool IsMoving => navMeshAgent != null && navMeshAgent.velocity.sqrMagnitude > _velocityThresholdSquared;
        public bool HasReachedDestination => navMeshAgent != null && !navMeshAgent.pathPending && navMeshAgent.remainingDistance < stoppingDistance;

        protected override void Awake()
        {
            base.Awake();

            if (navMeshAgent == null)
                navMeshAgent = GetComponent<NavMeshAgent>();

            // Cache squared values for performance
            _stoppingDistanceSquared = stoppingDistance * stoppingDistance;
            _velocityThresholdSquared = velocityThreshold * velocityThreshold;

            SetupNavMeshAgent();
        }

        protected override void OnInitialize()
        {
            SetupAIBehavior();
            PrepareDebugStrings();

            if (enableDebugLogs && !string.IsNullOrEmpty(_cachedInitMessage))
            {
                Debug.Log(_cachedInitMessage);
            }
        }

        private void PrepareDebugStrings()
        {
            if (enableDebugLogs && Data != null && _debugStringsDirty)
            {
                _cachedInitMessage = $"🤖 AI Character initialized - Role: {Data.CharacterType}";
                _debugStringsDirty = false;
            }
        }

        protected virtual void SetupNavMeshAgent()
        {
            if (navMeshAgent == null) return;

            try
            {
                navMeshAgent.stoppingDistance = stoppingDistance;
                navMeshAgent.updateRotation = false;
                navMeshAgent.updatePosition = false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error setting up NavMeshAgent: {e.Message}", this);
            }
        }

        protected virtual void SetupAIBehavior()
        {
            try
            {
                if (Data != null)
                {
                    _currentBehavior = AIBehaviorFactory.CreateBehavior(Data.CharacterType, this);
                    _currentBehavior?.Initialize();
                }
                else
                {
                    Debug.LogError($"[{gameObject.name}] Cannot setup AI behavior without CharacterData!", this);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error setting up AI behavior: {e.Message}", this);
            }
        }

        protected override void HandleInput()
        {
            // AI decision making
            if (Time.time - _lastDecisionTime >= decisionInterval)
            {
                try
                {
                    _currentBehavior?.UpdateBehavior();

                    // AI pickup logic
                    if (enableSmartPickup)
                    {
                        HandleAIPickupLogic();
                    }
                }
                catch (System.Exception e)
                {
                    if (enableDebugLogs)
                        Debug.LogError($"AI Behavior Error: {e.Message}", this);
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
            try
            {
                if (navMeshAgent != null && !navMeshAgent.updatePosition && navMeshAgent.isActiveAndEnabled)
                {
                    navMeshAgent.nextPosition = transform.position;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error syncing NavMesh position: {e.Message}", this);
            }
        }

        protected virtual void HandleAIPickupLogic()
        {
            // Only employees should actively pickup items for now
            if (Data?.CharacterType != CharacterType.AI_Employee) return;
            if (_carryingController == null || _interactionController == null) return;

            try
            {
                if (!_carryingController.IsCarrying && _targetItem == null)
                {
                    FindNearbyItems();
                }
                else if (_carryingController.IsCarrying && _targetDropZone == null)
                {
                    FindNearbyDropZones();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in AI pickup logic: {e.Message}", this);
            }
        }

        private void FindNearbyItems()
        {
            if (_interactionController == null) return;

            // TODO: Implement proper item detection system with spatial partitioning
            // This will use octree or similar spatial data structure to avoid
            // expensive distance calculations every frame
            
            // Placeholder for now - will be implemented with performance in mind
        }

        private void FindNearbyDropZones()
        {
            // TODO: Implement drop zone detection with caching
            // Will use cached zone positions and only recalculate when zones change
        }

        protected virtual void UpdateMovementFromNavMesh()
        {
            if (navMeshAgent == null) return;

            try
            {
                if (navMeshAgent.hasPath && navMeshAgent.velocity.sqrMagnitude > _velocityThresholdSquared)
                {
                    Vector3 velocity = navMeshAgent.desiredVelocity.normalized;
                    
                    // Reuse cached Vector2 to avoid allocation
                    _cachedMovementInput.x = velocity.x;
                    _cachedMovementInput.y = velocity.z;
                    SetMovementInput(_cachedMovementInput);
                }
                else
                {
                    // Use static Vector2.zero to avoid allocation
                    SetMovementInput(Vector2.zero);
                    CheckTargetReached();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error updating movement from NavMesh: {e.Message}", this);
            }
        }

        private void CheckTargetReached()
        {
            if (_carryingController == null) return;

            try
            {
                // Check if we reached target item (use squared distance for performance)
                if (_targetItem != null && !_carryingController.IsCarrying)
                {
                    float distanceSquared = Vector3.SqrMagnitude(transform.position - _targetItem.Transform.position);
                    
                    if (distanceSquared <= _stoppingDistanceSquared)
                    {
                        bool success = _carryingController.TryPickupItem(_targetItem);
                        if (success && enableDebugLogs)
                        {
                            // Only create string if logging is enabled
                            if (string.IsNullOrEmpty(_cachedPickupMessage))
                            {
                                _cachedPickupMessage = $"🤖 AI picked up item: {_targetItem.ItemId}";
                            }
                            Debug.Log(_cachedPickupMessage);
                        }
                        _targetItem = null;
                    }
                }

                // Check if we reached drop zone (use squared distance for performance)
                if (_targetDropZone != null && _carryingController.IsCarrying)
                {
                    float distanceSquared = Vector3.SqrMagnitude(transform.position - _targetDropZone.position);
                    
                    if (distanceSquared <= _stoppingDistanceSquared)
                    {
                        _targetDropZone = null;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error checking target reached: {e.Message}", this);
            }
        }

        public virtual bool MoveTo(Vector3 destination)
        {
            if (navMeshAgent == null) return false;

            try
            {
                if (IsValidDestination(destination))
                {
                    navMeshAgent.SetDestination(destination);
                    _targetDestination = destination;
                    _isMovingToTarget = true;

                    if (enableDebugLogs && Data != null)
                    {
                        // Create debug string only when needed and cache it
                        if (string.IsNullOrEmpty(_cachedMoveMessage))
                        {
                            _cachedMoveMessage = $"🎯 {Data.CharacterType} moving to: {destination}";
                        }
                        Debug.Log(_cachedMoveMessage);
                    }

                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error moving to destination: {e.Message}", this);
            }
            
            return false;
        }

        public virtual void Stop()
        {
            try
            {
                if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
                {
                    navMeshAgent.ResetPath();
                }
                _isMovingToTarget = false;
                SetMovementInput(Vector2.zero);
                _motor?.Stop();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error stopping AI character: {e.Message}", this);
            }
        }

        protected virtual bool IsValidDestination(Vector3 destination)
        {
            try
            {
                NavMeshHit hit;
                return NavMesh.SamplePosition(destination, out hit, navMeshSampleDistance, NavMesh.AllAreas);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error validating destination: {e.Message}", this);
                return false;
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                // Safely end AI behavior
                if (_currentBehavior != null)
                {
                    _currentBehavior.OnBehaviorEnd();
                    _currentBehavior = null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error ending AI behavior during destroy: {e.Message}");
            }
            
            base.OnDestroy();
        }

        void OnDisable()
        {
            try
            {
                // Stop navigation when disabled
                if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
                {
                    navMeshAgent.ResetPath();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error disabling AI character: {e.Message}");
            }
        }

        // Conditional compilation to exclude debug gizmos in release builds
        #if UNITY_EDITOR
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
        #endif
    }
}
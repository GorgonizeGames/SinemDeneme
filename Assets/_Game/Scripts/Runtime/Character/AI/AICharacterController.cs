using UnityEngine;
using UnityEngine.AI;
using Game.Runtime.Core.DI;

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

        // AI Behavior System
        protected IAIBehavior _currentBehavior;
        protected float _lastDecisionTime;
        protected Vector3 _targetDestination;
        protected bool _isMovingToTarget;

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
            navMeshAgent.updateRotation = false; // CharacterMotor handles rotation
            navMeshAgent.updatePosition = false; // CharacterMotor handles position
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
                _lastDecisionTime = Time.time;
            }

            // Convert NavMesh movement to CharacterMotor input
            UpdateMovementFromNavMesh();
        }

        protected virtual void UpdateMovementFromNavMesh()
        {
            if (navMeshAgent.hasPath && navMeshAgent.velocity.magnitude > 0.1f)
            {
                Vector3 velocity = navMeshAgent.desiredVelocity.normalized;
                Vector2 movementInput = new Vector2(velocity.x, velocity.z);
                SetMovementInput(movementInput);

                // Sync position with NavMeshAgent
                if (!navMeshAgent.updatePosition)
                {
                    navMeshAgent.nextPosition = transform.position;
                }
            }
            else
            {
                SetMovementInput(Vector2.zero);
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

        // ✅ FIXED: Null reference exception eliminated
        public virtual void Stop()
        {
            navMeshAgent.ResetPath();
            _isMovingToTarget = false;
            SetMovementInput(Vector2.zero);
            
            // ✅ SAFE: _motor is now accessible from base class (protected)
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
        }
    }

    public enum AIRole
    {
        Customer,
        Employee,
        Cashier
    }
}
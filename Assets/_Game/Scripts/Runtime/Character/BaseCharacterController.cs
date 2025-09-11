using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Extensions;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.Character.Motor;
using Game.Runtime.Character.States;
using Game.Runtime.Character.Components;
using Game.Runtime.Character.Interfaces;
using Game.Runtime.Core.Interfaces;
using Game.Runtime.Character.Data;

namespace Game.Runtime.Character
{
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(StackingCarryController))]
    [RequireComponent(typeof(InteractionController))]
    public abstract class BaseCharacterController : MonoBehaviour, ICharacterController
    {
        [Inject] protected IGameManager _gameManager;

        // Cached components to avoid repeated GetComponent calls
        protected CharacterMotor _motor;
        protected StateMachine<ICharacterController> _stateMachine;
        protected InteractionController _interactionController;
        protected StackingCarryController _carryingController;

        // Input caching to avoid Vector2 allocations
        protected Vector2 _currentMovementInput = Vector2.zero;

        // Component validation flags
        private bool _componentsValidated = false;
        private bool _hasValidComponents = false;
        private bool _isInitialized = false;

        // Cached properties for performance
        public Transform Transform => transform;
        public Animator Animator => _motor?.CharacterAnimator;
        public Vector2 MovementInput => _currentMovementInput;
        public CharacterData Data => _motor?.Data;
        public ICarryingController CarryingController => _carryingController;

        protected virtual void Awake()
        {
            ValidateAndCacheComponents();
        }

        protected virtual void Start()
        {
            if (!_isInitialized && _hasValidComponents)
            {
                try
                {
                    this.InjectDependencies();
                    Initialize();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error during character initialization: {e.Message}", this);
                }
            }
        }

        private void ValidateAndCacheComponents()
        {
            if (_componentsValidated) return;

            try
            {
                // Cache all required components
                _motor = GetComponent<CharacterMotor>();
                _carryingController = GetComponent<StackingCarryController>();
                _interactionController = GetComponent<InteractionController>();

                // Validate components
                _hasValidComponents = ValidateComponents();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error validating components: {e.Message}", this);
                _hasValidComponents = false;
            }
            finally
            {
                _componentsValidated = true;
            }
        }

        protected virtual bool ValidateComponents()
        {
            bool isValid = true;

            if (_motor == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterMotor component is missing!", this);
                isValid = false;
            }

            if (_carryingController == null)
            {
                Debug.LogError($"[{gameObject.name}] StackingCarryController component is missing!", this);
                isValid = false;
            }

            if (_interactionController == null)
            {
                Debug.LogError($"[{gameObject.name}] InteractionController component is missing!", this);
                isValid = false;
            }

            return isValid;
        }

        protected virtual void Initialize()
        {
            if (!_hasValidComponents)
            {
                Debug.LogError($"[{gameObject.name}] Cannot initialize without valid components!", this);
                return;
            }

            try
            {
                SetupStateMachine();
                SetupGameStateHandling();

                // Start with idle state
                _stateMachine.ChangeState<CharacterIdleState>();

                // Call derived class initialization
                OnInitialize();
                _isInitialized = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during character initialization: {e.Message}", this);
            }
        }

        protected virtual void SetupStateMachine()
        {
            try
            {
                _stateMachine = new StateMachine<ICharacterController>(this);
                _stateMachine.AddState(new CharacterIdleState());
                _stateMachine.AddState(new CharacterMovingState());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error setting up state machine: {e.Message}", this);
                throw; // Re-throw to prevent invalid initialization
            }
        }

        protected virtual void SetupGameStateHandling()
        {
            try
            {
                if (_gameManager != null)
                {
                    _gameManager.OnStateChanged += HandleGameStateChange;
                }
                else
                {
                    Debug.LogWarning($"[{gameObject.name}] GameManager is not injected yet!");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error setting up game state handling: {e.Message}", this);
            }
        }

        // Abstract methods for derived classes
        protected abstract void OnInitialize();
        protected abstract void HandleInput();

        protected virtual void Update()
        {
            if (!_isInitialized || !_hasValidComponents) return;
            
            // Check game state once per frame instead of accessing property multiple times
            var currentGameState = _gameManager?.CurrentState ?? GameState.Playing;
            if (currentGameState != GameState.Playing) return;

            try
            {
                HandleInput();
                _stateMachine?.Update();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during character update: {e.Message}", this);
            }
        }

        protected virtual void FixedUpdate()
        {
            if (!_isInitialized || !_hasValidComponents) return;
            
            // Check game state once per FixedUpdate frame
            var currentGameState = _gameManager?.CurrentState ?? GameState.Playing;
            if (currentGameState != GameState.Playing) return;

            try
            {
                _stateMachine?.FixedUpdate();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during character fixed update: {e.Message}", this);
            }
        }

        // ICharacterController Implementation
        public virtual bool ChangeState<T>() where T : BaseState<ICharacterController>
        {
            try
            {
                return _stateMachine?.ChangeState<T>() ?? false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error changing character state: {e.Message}", this);
                return false;
            }
        }

        public virtual void SetMovementInput(Vector2 input)
        {
            // Reuse the same Vector2 reference to avoid allocation
            _currentMovementInput.x = input.x;
            _currentMovementInput.y = input.y;
        }

        // Character control methods
        public void ApplySpeedBoost(float multiplier, float duration)
        {
            try
            {
                _motor?.ApplySpeedBoost(multiplier, duration);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error applying speed boost: {e.Message}", this);
            }
        }

        public void ChangeCharacterSettings(CharacterData newSettings)
        {
            try
            {
                _motor?.SetCharacterSettings(newSettings);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error changing character settings: {e.Message}", this);
            }
        }

        protected virtual void HandleGameStateChange(GameState newState)
        {
            try
            {
                bool isEnabled = (newState == GameState.Playing);

                if (!isEnabled)
                {
                    SetMovementInput(Vector2.zero);
                    _motor?.Stop();

                    // Force drop items when game is not playing
                    if (_carryingController?.IsCarrying == true)
                    {
                        _carryingController.ForceDropItem();
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error handling game state change: {e.Message}", this);
            }
        }

        // Cleanup
        protected virtual void OnDestroy()
        {
            try
            {
                // Unsubscribe from events to prevent memory leaks
                if (_gameManager != null)
                {
                    _gameManager.OnStateChanged -= HandleGameStateChange;
                }

                // Clean up state machine
                _stateMachine?.Cleanup();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during character cleanup: {e.Message}");
            }
        }

        void OnDisable()
        {
            try
            {
                // Stop movement when disabled
                SetMovementInput(Vector2.zero);
                _motor?.Stop();

                // Force drop items when disabled
                if (_carryingController?.IsCarrying == true)
                {
                    _carryingController.ForceDropItem();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during character disable: {e.Message}");
            }
        }

        // Debug helper for inspector
        #if UNITY_EDITOR
        [ContextMenu("Validate Components")]
        private void ValidateComponentsInEditor()
        {
            _componentsValidated = false;
            ValidateAndCacheComponents();
            
            if (_hasValidComponents)
            {
                Debug.Log($"[{gameObject.name}] All components are valid!", this);
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] Some components are missing or invalid!", this);
            }
        }
        #endif
    }
}
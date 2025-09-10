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
    [RequireComponent(typeof(CharacterMotor), typeof(StackingCarryController), typeof(InteractionController))]
    public abstract class BaseCharacterController : MonoBehaviour, ICharacterController
    {
        [Inject] protected IGameManager _gameManager;

        protected CharacterMotor _motor;
        protected StateMachine<ICharacterController> _stateMachine;
        protected Vector2 _currentMovementInput;

        protected InteractionController _interactionController;
        protected StackingCarryController _carryingController;

        private bool _isInitialized = false;

        public Transform Transform => transform;
        public Animator Animator => _motor?.CharacterAnimator;
        public Vector2 MovementInput => _currentMovementInput;
        public CharacterData Data => _motor?.Data;
        public ICarryingController CarryingController => _carryingController;

        protected virtual void Awake()
        {
            // Get components early
            _motor = GetComponent<CharacterMotor>();
            _carryingController = GetComponent<StackingCarryController>();
            _interactionController = GetComponent<InteractionController>();

            ValidateComponents();
        }

        protected virtual void Start()
        {
            if (!_isInitialized)
            {
                this.InjectDependencies();
                Initialize();
            }
        }

        protected virtual void Initialize()
        {
            SetupStateMachine();
            SetupGameStateHandling();

            _stateMachine.ChangeState<CharacterIdleState>();

            OnInitialize();
            _isInitialized = true;
        }

        protected virtual void ValidateComponents()
        {
            if (_motor == null)
                Debug.LogError($"[{gameObject.name}] CharacterMotor component is missing!", this);
            if (_carryingController == null)
                Debug.LogError($"[{gameObject.name}] CarryingController component is missing!", this);
            if (_interactionController == null)
                Debug.LogError($"[{gameObject.name}] InteractionController component is missing!", this);
        }

        protected virtual void SetupStateMachine()
        {
            _stateMachine = new StateMachine<ICharacterController>(this);
            _stateMachine.AddState(new CharacterIdleState());
            _stateMachine.AddState(new CharacterMovingState());
        }

        protected virtual void SetupGameStateHandling()
        {
            if (_gameManager != null)
                _gameManager.OnStateChanged += HandleGameStateChange;
            else
                Debug.LogWarning($"[{gameObject.name}] GameManager is not injected yet!");
        }

        protected abstract void OnInitialize();
        protected abstract void HandleInput();

        protected virtual void Update()
        {
            if (!_isInitialized) return;
            if (_gameManager?.CurrentState != GameState.Playing) return;

            HandleInput();
            _stateMachine?.Update();
        }

        protected virtual void FixedUpdate()
        {
            if (!_isInitialized) return;
            if (_gameManager?.CurrentState != GameState.Playing) return;

            _stateMachine?.FixedUpdate();
        }

        public virtual bool ChangeState<T>() where T : BaseState<ICharacterController>
        {
            return _stateMachine?.ChangeState<T>() ?? false;
        }

        public virtual void SetMovementInput(Vector2 input)
        {
            _currentMovementInput = input;
        }

        public void ApplySpeedBoost(float multiplier, float duration)
        {
            _motor?.ApplySpeedBoost(multiplier, duration);
        }

        public void ChangeCharacterSettings(CharacterData newSettings)
        {
            _motor?.SetCharacterSettings(newSettings);
        }

        protected virtual void HandleGameStateChange(GameState newState)
        {
            bool isEnabled = (newState == GameState.Playing);

            if (!isEnabled)
            {
                SetMovementInput(Vector2.zero);
                _motor?.Stop();

                if (_carryingController?.IsCarrying == true)
                {
                    _carryingController.ForceDropItem();
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (_gameManager != null)
                _gameManager.OnStateChanged -= HandleGameStateChange;

            _stateMachine?.Cleanup();
        }
    }
}
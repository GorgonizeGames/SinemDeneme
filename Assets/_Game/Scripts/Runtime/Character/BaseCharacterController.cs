using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Extensions;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.Game;
using Game.Runtime.Character.Motor;
using Game.Runtime.Character.States;
using Game.Runtime.Character.Components;
using Game.Runtime.Character.Interfaces;

namespace Game.Runtime.Character
{
    [RequireComponent(typeof(CharacterMotor), typeof(CarryingController), typeof(CharacterTriggerDetector))]
    public abstract class BaseCharacterController : MonoBehaviour, ICharacterController
    {
        [Header("Character Info")]
        [SerializeField] protected CharacterType characterType;

        [Inject] protected IGameManager _gameManager;

        protected CharacterMotor _motor;
        protected CarryingController _carryingController; // ✅ Added
        protected CharacterTriggerDetector _triggerDetector; // ✅ Added
        protected StateMachine<ICharacterController> _stateMachine;
        protected Vector2 _currentMovementInput;

        public Transform Transform => transform;
        public Animator Animator => _motor.CharacterAnimator;
        public Vector2 MovementInput => _currentMovementInput;
        public CharacterType CharacterType => characterType;
        public CharacterSettings Settings => _motor.Settings;
        public ICarryingController CarryingController => _carryingController; // ✅ Added

        void Start()
        {
            this.InjectDependencies();
            Initialize();
        }

        protected virtual void Initialize()
        {
            _motor = GetComponent<CharacterMotor>();
            _carryingController = GetComponent<CarryingController>(); // ✅ Added
            _triggerDetector = GetComponent<CharacterTriggerDetector>(); // ✅ Added
            
            SetupStateMachine();
            SetupGameStateHandling();
            
            _stateMachine.ChangeState<CharacterIdleState>();
            
            OnInitialize();
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
        }

        protected abstract void OnInitialize();
        protected abstract void HandleInput();

        void Update()
        {
            if (_gameManager.CurrentState != GameState.Playing) return;
            
            HandleInput();
            _stateMachine.Update();
        }

        void FixedUpdate()
        {
            if (_gameManager.CurrentState != GameState.Playing) return;
            
            _stateMachine.FixedUpdate();
        }

        public virtual bool ChangeState<T>() where T : BaseState<ICharacterController>
        {
            return _stateMachine.ChangeState<T>();
        }

        public virtual void SetMovementInput(Vector2 input)
        {
            _currentMovementInput = input;
        }

        public void ApplySpeedBoost(float multiplier, float duration)
        {
            _motor.ApplySpeedBoost(multiplier, duration);
        }

        public void ChangeCharacterSettings(CharacterSettings newSettings)
        {
            _motor.SetCharacterSettings(newSettings);
        }

        protected virtual void HandleGameStateChange(GameState newState)
        {
            bool isEnabled = (newState == GameState.Playing);
            
            if (!isEnabled)
            {
                SetMovementInput(Vector2.zero);
                _motor.Stop();
                
                // Force drop item if game paused/stopped
                if (_carryingController.IsCarrying)
                {
                    _carryingController.ForceDropItem();
                }
            }
        }

        void OnDestroy()
        {
            if (_gameManager != null)
                _gameManager.OnStateChanged -= HandleGameStateChange;
        }
    }
}
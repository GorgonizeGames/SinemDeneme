using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Extensions;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.Game;
using Game.Runtime.Character.Motor;
using Game.Runtime.Character.States;

namespace Game.Runtime.Character
{
    [RequireComponent(typeof(CharacterMotor))]
    public abstract class BaseCharacterController : MonoBehaviour, ICharacterController
    {
        [Header("Character Info")]
        [SerializeField] protected CharacterType characterType;

        [Inject] protected IGameManager _gameManager;

        protected CharacterMotor _motor;
        protected StateMachine<ICharacterController> _stateMachine;
        protected Vector2 _currentMovementInput;

        public Transform Transform => transform;
        public Animator Animator => _motor.CharacterAnimator;
        public Vector2 MovementInput => _currentMovementInput;
        public CharacterType CharacterType => characterType;
        public CharacterSettings Settings => _motor.Settings;

        void Start()
        {
            this.InjectDependencies();
            Initialize();
        }

        protected virtual void Initialize()
        {
            _motor = GetComponent<CharacterMotor>();
            
            SetupStateMachine();
            SetupGameStateHandling();
            
            // ✅ Idle state ile başla
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
            
            // ✅ Önce input'u al
            HandleInput();
            
            // ✅ Sonra state machine'i güncelle
            _stateMachine.Update();
        }

        void FixedUpdate()
        {
            if (_gameManager.CurrentState != GameState.Playing) return;
            
            // ✅ State machine fiziksel güncellemeleri handle eder
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
            }
        }

        void OnDestroy()
        {
            if (_gameManager != null)
                _gameManager.OnStateChanged -= HandleGameStateChange;
        }
    }
}
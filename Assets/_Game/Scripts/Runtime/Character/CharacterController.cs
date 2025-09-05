using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Extensions;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.Input;
using Game.Runtime.Game;
using Game.Runtime.Character.Motor;
using Game.Runtime.Character.States;

namespace Game.Runtime.Character
{
    [RequireComponent(typeof(CharacterMotor))]
    public class CharacterController : MonoBehaviour, ICharacterController
    {
        [Inject] private IInputService _inputService;
        [Inject] private IGameManager _gameManager;

        private CharacterMotor _motor;
        private StateMachine<ICharacterController> _stateMachine;
        
        public Transform Transform => transform;
        public Animator Animator => _motor.CharacterAnimator;
        public Vector2 MovementInput => _inputService.MovementInput;

        void Awake()
        {
            this.InjectDependencies();
            _motor = GetComponent<CharacterMotor>();
            
            _stateMachine = new StateMachine<ICharacterController>(this);
            _stateMachine.AddState(new CharacterIdleState());
            _stateMachine.AddState(new CharacterMovingState());
        }

        void Start()
        {
            _stateMachine.ChangeState<CharacterIdleState>();
            
            if (_gameManager != null)
                _gameManager.OnStateChanged += HandleGameStateChange;
        }

        void OnDestroy()
        {
            if (_gameManager != null)
                _gameManager.OnStateChanged -= HandleGameStateChange;
        }

        void Update()
        {
            if (_gameManager.CurrentState != GameState.Playing) return;
            
            _stateMachine.Update();
        }

        void FixedUpdate()
        {
            if (_gameManager.CurrentState != GameState.Playing) return;
            
            _stateMachine.FixedUpdate();
        }

        public bool ChangeState<T>() where T : BaseState<ICharacterController>
        {
            return _stateMachine.ChangeState<T>();
        }
        
        private void HandleGameStateChange(GameState newState)
        {
            bool isInputEnabled = (newState == GameState.Playing);
            _inputService.EnableInput(isInputEnabled);

            if (!isInputEnabled)
            {
                _motor.Move(Vector2.zero);
            }
        }
    }
}
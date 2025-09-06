using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Input;
using Game.Runtime.Core.Extensions;
using Game.Runtime.Game;

namespace Game.Runtime.Character
{
    public class PlayerCharacterController : BaseCharacterController
    {
        [Header("Player Settings")]
        [SerializeField] private bool enableInputDebug = false;

        [Inject] private IInputService _inputService;

        protected override void OnInitialize()
        {
            this.InjectDependencies();
            
            if (_inputService == null)
            {
                Debug.LogError("❌ Player needs IInputService!", this);
                enabled = false;
                return;
            }

            Debug.Log($"🎮 Player initialized with settings: {Settings?.name ?? "None"}");
        }

        protected override void HandleInput()
        {
            if (_inputService == null) return;

            Vector2 input = _inputService.MovementInput;
            SetMovementInput(input);

            if (enableInputDebug && input.magnitude > 0.1f)
            {
                Debug.Log($"🎮 Player Input: {input} | Speed: {Settings?.MoveSpeed ?? 0}");
            }
        }

        protected override void HandleGameStateChange(GameState newState)
        {
            base.HandleGameStateChange(newState);
            
            bool isInputEnabled = (newState == GameState.Playing);
            _inputService?.EnableInput(isInputEnabled);
        }
    }
}
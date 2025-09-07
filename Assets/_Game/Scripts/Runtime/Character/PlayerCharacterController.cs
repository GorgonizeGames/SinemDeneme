using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Interfaces;

namespace Game.Runtime.Character
{
    public class PlayerCharacterController : BaseCharacterController
    {
        [Header("Player Settings")]
        [SerializeField] private bool enableInputDebug = false;

        [Inject] private IInputService _inputService;

        protected override void OnInitialize()
        {
            // Dependency injection BaseCharacterController'da yapıldı, tekrar yapma!

            if (_inputService == null)
            {
                Debug.LogError($"[{gameObject.name}] Player needs IInputService!", this);
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
                Debug.Log($"🎮 Player Input: {input} | Speed: {Settings?.MoveSpeed ?? 0} | Carrying: {_carryingController?.IsCarrying ?? false}");
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
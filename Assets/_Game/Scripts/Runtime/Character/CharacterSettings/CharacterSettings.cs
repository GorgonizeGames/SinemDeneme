using UnityEngine;

namespace Game.Runtime.Character
{
    [CreateAssetMenu(fileName = "NewCharacterSettings", menuName = "Game/Character Settings")]
    public class CharacterSettings : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField, Range(1f, 10f)] private float _moveSpeed = 5f;
        [SerializeField, Range(1f, 20f)] private float _rotationSpeed = 15f;
        [SerializeField, Range(5f, 20f)] private float _acceleration = 10f;

        [Header("Animation")]
        [SerializeField, Range(0.1f, 2f)] private float _animationSpeedMultiplier = 1f;
        [SerializeField] private bool _useRootMotion = false;

        [Header("Physics")]
        [SerializeField] private bool _freezeYPosition = true;
        [SerializeField, Range(0f, 1f)] private float _drag = 0.1f;

        // Public properties
        public float MoveSpeed => _moveSpeed;
        public float RotationSpeed => _rotationSpeed;
        public float Acceleration => _acceleration;
        public float AnimationSpeedMultiplier => _animationSpeedMultiplier;
        public bool UseRootMotion => _useRootMotion;
        public bool FreezeYPosition => _freezeYPosition;
        public float Drag => _drag;

        // Runtime değişiklik için metodlar
        public void SetMoveSpeed(float speed)
        {
            _moveSpeed = Mathf.Clamp(speed, 1f, 10f);
        }

        public void SetRotationSpeed(float speed)
        {
            _rotationSpeed = Mathf.Clamp(speed, 1f, 20f);
        }

        // Power-up'lar için geçici değişiklikler
        [System.NonSerialized] private float _speedMultiplier = 1f;
        
        public float EffectiveMoveSpeed => _moveSpeed * _speedMultiplier;
        
        public void ApplySpeedBoost(float multiplier, float duration)
        {
            _speedMultiplier = multiplier;
            // Duration için timer gerekebilir (Coroutine veya DOTween ile)
        }

        public void ResetSpeedBoost()
        {
            _speedMultiplier = 1f;
        }
    }
}
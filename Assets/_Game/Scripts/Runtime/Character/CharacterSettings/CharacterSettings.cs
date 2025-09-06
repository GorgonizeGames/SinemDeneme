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

        // Public properties (READ ONLY - ScriptableObjects should be immutable)
        public float MoveSpeed => _moveSpeed;
        public float RotationSpeed => _rotationSpeed;
        public float Acceleration => _acceleration;
        public float AnimationSpeedMultiplier => _animationSpeedMultiplier;
        public bool UseRootMotion => _useRootMotion;
        public bool FreezeYPosition => _freezeYPosition;
        public float Drag => _drag;
    }
}
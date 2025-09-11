using UnityEngine;
using Game.Runtime.Character.Animation;
using Game.Runtime.Character.Settings;
using Game.Runtime.Character.Data;

namespace Game.Runtime.Character.Motor
{
    public class CharacterMotor : MonoBehaviour
    {
        [Header("Character Settings")]
        [SerializeField] private CharacterData data;

        [Header("Movement Thresholds")]
        [SerializeField] private float movementThreshold = 0.1f;
        [SerializeField] private float rotationThreshold = 0.1f;
        
        private Rigidbody _rigidbody;
        private Vector3 _currentVelocity;
        private Vector3 _movementInput;
        private CharacterRuntimeSettings _runtimeSettings;

        public Animator CharacterAnimator { get; private set; }
        public CharacterData Data => data;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            CharacterAnimator = GetComponentInChildren<Animator>();

            if (data != null)
            {
                _runtimeSettings = new CharacterRuntimeSettings(data);
            }

            ApplyPhysicsSettings();
        }

        private void ApplyPhysicsSettings()
        {
            if (_rigidbody == null) return;

            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            if (_runtimeSettings != null)
            {
                // Unity version compatibility for drag property
#if UNITY_2023_1_OR_NEWER
                _rigidbody.linearDamping = _runtimeSettings.Drag;
#else
                _rigidbody.drag = _runtimeSettings.Drag;
#endif

                if (_runtimeSettings.FreezeYPosition)
                {
                    _rigidbody.constraints |= RigidbodyConstraints.FreezePositionY;
                }
            }
        }

        public void SetMovementInput(Vector2 input)
        {
            _movementInput = new Vector3(input.x, 0, input.y);
        }

        public void ExecuteMovement()
        {
            if (_runtimeSettings == null || _rigidbody == null) return;

            Vector3 targetVelocity = _movementInput * _runtimeSettings.EffectiveMoveSpeed;

            _currentVelocity = Vector3.Lerp(
                _currentVelocity,
                targetVelocity,
                _runtimeSettings.Acceleration * Time.fixedDeltaTime
            );

            Vector3 newPosition = _rigidbody.position + _currentVelocity * Time.fixedDeltaTime;
            _rigidbody.MovePosition(newPosition);

            UpdateRotation(_movementInput);
            UpdateAnimation();
        }

        public void Stop()
        {
            _movementInput = Vector3.zero;
            _currentVelocity = Vector3.zero;
            UpdateAnimation();
        }

        private void UpdateRotation(Vector3 direction)
        {
            if (_runtimeSettings == null || _rigidbody == null || direction.magnitude <= rotationThreshold) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.Slerp(
                _rigidbody.rotation,
                targetRotation,
                _runtimeSettings.RotationSpeed * Time.fixedDeltaTime
            );
            _rigidbody.MoveRotation(newRotation);
        }

        private void UpdateAnimation()
        {
            if (CharacterAnimator == null || _runtimeSettings == null) return;

            float speed = _currentVelocity.magnitude;
            float normalizedSpeed = _runtimeSettings.EffectiveMoveSpeed > 0 
                ? speed / _runtimeSettings.EffectiveMoveSpeed 
                : 0f;
            bool isMoving = normalizedSpeed > movementThreshold;

            // Base Layer - Sadece yürüme ve idle
            CharacterAnimator.SetFloat(AnimationParameters.Speed, 
                normalizedSpeed * _runtimeSettings.AnimationSpeedMultiplier);
            CharacterAnimator.SetBool(AnimationParameters.IsMoving, isMoving);
        }

        public void SetCharacterSettings(CharacterData newSettings)
        {
            if (newSettings == null) return;

            data = newSettings;
            if (_runtimeSettings != null)
            {
                _runtimeSettings.ChangeBaseSettings(newSettings);
            }
            else
            {
                _runtimeSettings = new CharacterRuntimeSettings(newSettings);
            }
            ApplyPhysicsSettings();
        }

        public void ApplySpeedBoost(float multiplier, float duration)
        {
            _runtimeSettings?.ApplySpeedBoost(multiplier, duration);
        }

        public void ResetSpeedBoost()
        {
            _runtimeSettings?.ResetSpeedBoost();
        }

        public bool IsMoving => _currentVelocity.magnitude > movementThreshold;
        public Vector3 CurrentVelocity => _currentVelocity;
    }
}
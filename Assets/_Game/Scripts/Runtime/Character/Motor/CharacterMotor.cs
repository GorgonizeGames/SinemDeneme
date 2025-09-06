using UnityEngine;

namespace Game.Runtime.Character.Motor
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class CharacterMotor : MonoBehaviour
    {
        [Header("Character Settings")]
        [SerializeField] private CharacterSettings characterSettings;

        private Rigidbody _rigidbody;
        private Vector3 _currentVelocity;
        private Vector3 _movementInput;
        
        public Animator CharacterAnimator { get; private set; }
        public CharacterSettings Settings => characterSettings;

        private int _speedHash;
        private int _isMovingHash;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            CharacterAnimator = GetComponent<Animator>();

            _speedHash = Animator.StringToHash("Speed");
            _isMovingHash = Animator.StringToHash("IsMoving");
            
            ApplyPhysicsSettings();
        }

        private void ApplyPhysicsSettings()
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            
            if (characterSettings != null)
            {
                _rigidbody.linearDamping = characterSettings.Drag;
                
                if (characterSettings.FreezeYPosition)
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
            if (characterSettings == null) return;

            Vector3 targetVelocity = _movementInput * characterSettings.EffectiveMoveSpeed;

            _currentVelocity = Vector3.Lerp(
                _currentVelocity,
                targetVelocity,
                characterSettings.Acceleration * Time.fixedDeltaTime
            );

            Vector3 newPosition = _rigidbody.position + _currentVelocity * Time.fixedDeltaTime;
            _rigidbody.MovePosition(newPosition);

            UpdateRotation(_movementInput);
            UpdateAnimation(_movementInput.magnitude);
        }

        // ✅ ADDED - Stop method that was missing
        public void Stop()
        {
            _movementInput = Vector3.zero;
            _currentVelocity = Vector3.zero;
            UpdateAnimation(0f);
        }

        private void UpdateRotation(Vector3 direction)
        {
            if (characterSettings == null || direction.magnitude <= 0.1f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.Slerp(
                _rigidbody.rotation, 
                targetRotation, 
                characterSettings.RotationSpeed * Time.fixedDeltaTime
            );
            _rigidbody.MoveRotation(newRotation);
        }

        private void UpdateAnimation(float moveMagnitude)
        {
            if (CharacterAnimator == null || characterSettings == null) return;
            
            float normalizedSpeed = _currentVelocity.magnitude / characterSettings.EffectiveMoveSpeed;
            bool isMoving = normalizedSpeed > 0.1f;

            float animationSpeed = normalizedSpeed * characterSettings.AnimationSpeedMultiplier;

            CharacterAnimator.SetFloat(_speedHash, animationSpeed);
            CharacterAnimator.SetBool(_isMovingHash, isMoving);
        }

        // Backward compatibility
        public void Move(Vector2 input)
        {
            SetMovementInput(input);
            ExecuteMovement();
        }

        public void SetCharacterSettings(CharacterSettings newSettings)
        {
            characterSettings = newSettings;
            ApplyPhysicsSettings();
        }

        public void ApplySpeedBoost(float multiplier, float duration)
        {
            characterSettings?.ApplySpeedBoost(multiplier, duration);
        }

        public void ResetSpeedBoost()
        {
            characterSettings?.ResetSpeedBoost();
        }

        // Public properties for states
        public bool IsMoving => _currentVelocity.magnitude > 0.1f;
        public Vector3 CurrentVelocity => _currentVelocity;
    }
}
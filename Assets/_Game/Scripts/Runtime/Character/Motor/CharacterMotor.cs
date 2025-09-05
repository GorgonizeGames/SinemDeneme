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
                _rigidbody.linearDamping= characterSettings.Drag;
                
                if (characterSettings.FreezeYPosition)
                {
                    _rigidbody.constraints |= RigidbodyConstraints.FreezePositionY;
                }
            }
        }

        public void Move(Vector2 input)
        {
            _movementInput = new Vector3(input.x, 0, input.y);
        }

        void FixedUpdate()
        {
            if (characterSettings == null) return;

            // Hedef hızı ScriptableObject'ten al
            Vector3 targetVelocity = _movementInput * characterSettings.EffectiveMoveSpeed;

            // Hızı ivmelenme ile yumuşat
            _currentVelocity = Vector3.Lerp(
                _currentVelocity,
                targetVelocity,
                characterSettings.Acceleration * Time.fixedDeltaTime
            );

            // Yeni pozisyonu hesapla ve MovePosition ile uygula
            Vector3 newPosition = _rigidbody.position + _currentVelocity * Time.fixedDeltaTime;
            _rigidbody.MovePosition(newPosition);

            UpdateRotation(_movementInput);
            UpdateAnimation(_movementInput.magnitude);
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

            // Animation speed multiplier'ı uygula
            float animationSpeed = normalizedSpeed * characterSettings.AnimationSpeedMultiplier;

            CharacterAnimator.SetFloat(_speedHash, animationSpeed);
            CharacterAnimator.SetBool(_isMovingHash, isMoving);
        }

        // Public methods for external use
        public void SetCharacterSettings(CharacterSettings newSettings)
        {
            characterSettings = newSettings;
            ApplyPhysicsSettings();
        }

        public CharacterSettings GetCharacterSettings()
        {
            return characterSettings;
        }

        // Power-up metodları
        public void ApplySpeedBoost(float multiplier, float duration)
        {
            characterSettings?.ApplySpeedBoost(multiplier, duration);
        }

        public void ResetSpeedBoost()
        {
            characterSettings?.ResetSpeedBoost();
        }
    }
}
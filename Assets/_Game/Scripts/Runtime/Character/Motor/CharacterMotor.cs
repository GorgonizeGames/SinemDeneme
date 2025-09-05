using UnityEngine;

namespace Game.Runtime.Character.Motor
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class CharacterMotor : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float acceleration = 10f;

        private Rigidbody _rigidbody;
        private Vector3 _currentVelocity;
        private Vector3 _movementInput; // Input'u saklamak için yeni değişken
        
        public Animator CharacterAnimator { get; private set; }

        private int _speedHash;
        private int _isMovingHash;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            CharacterAnimator = GetComponent<Animator>();

            _speedHash = Animator.StringToHash("Speed");
            _isMovingHash = Animator.StringToHash("IsMoving");
            
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        // Move metodu artık sadece input'u alıp bir değişkene atayacak.
        public void Move(Vector2 input)
        {
            _movementInput = new Vector3(input.x, 0, input.y);
        }

        // Tüm fizik işlemleri FixedUpdate'e taşındı.
        void FixedUpdate()
        {
            // Hedef hızı hesapla
            Vector3 targetVelocity = _movementInput * moveSpeed;

            // Hızı ivmelenme ile yumuşat
            _currentVelocity = Vector3.Lerp(
                _currentVelocity,
                targetVelocity,
                acceleration * Time.fixedDeltaTime
            );

            // Yeni pozisyonu hesapla ve MovePosition ile uygula
            Vector3 newPosition = _rigidbody.position + _currentVelocity * Time.fixedDeltaTime;
            _rigidbody.MovePosition(newPosition);

            UpdateRotation(_movementInput);
            UpdateAnimation(_movementInput.magnitude);
        }

        private void UpdateRotation(Vector3 direction)
        {
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                // Yeni rotasyonu hesapla ve MoveRotation ile uygula
                Quaternion newRotation = Quaternion.Slerp(_rigidbody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                _rigidbody.MoveRotation(newRotation);
            }
        }

        private void UpdateAnimation(float moveMagnitude)
        {
            if (CharacterAnimator == null) return;
            
            // Animasyon hızı için mevcut hızın büyüklüğünü kullanmak daha doğru sonuç verir
            float normalizedSpeed = _currentVelocity.magnitude / moveSpeed;
            bool isMoving = normalizedSpeed > 0.1f;

            CharacterAnimator.SetFloat(_speedHash, normalizedSpeed);
            CharacterAnimator.SetBool(_isMovingHash, isMoving);
        }
    }
}
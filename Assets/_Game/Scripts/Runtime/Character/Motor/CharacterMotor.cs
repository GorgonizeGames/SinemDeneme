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

        public void Move(Vector2 input)
        {
            Vector3 movementDirection = new Vector3(input.x, 0, input.y);
            Vector3 targetVelocity = movementDirection * moveSpeed;

            _currentVelocity = Vector3.Lerp(
                _currentVelocity,
                targetVelocity,
                acceleration * Time.fixedDeltaTime
            );

            _rigidbody.linearVelocity = new Vector3(_currentVelocity.x, _rigidbody.linearVelocity.y, _currentVelocity.z);

            UpdateRotation(movementDirection);
            UpdateAnimation(movementDirection.magnitude);
        }

        private void UpdateRotation(Vector3 direction)
        {
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }

        private void UpdateAnimation(float moveMagnitude)
        {
            if (CharacterAnimator == null) return;
            
            float normalizedSpeed = moveMagnitude;
            bool isMoving = normalizedSpeed > 0.1f;

            CharacterAnimator.SetFloat(_speedHash, normalizedSpeed);
            CharacterAnimator.SetBool(_isMovingHash, isMoving);
        }
    }
}

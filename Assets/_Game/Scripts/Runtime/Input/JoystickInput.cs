using UnityEngine;
using Game.Runtime.Core.Interfaces;

namespace Game.Runtime.Input
{
    public class JoystickInput : MonoBehaviour, IInputService
    {
        [SerializeField] private Joystick joystick;

        private bool _isInputEnabled = true;

        public Vector2 MovementInput
        {
            get
            {
                if (!_isInputEnabled || joystick == null)
                    return Vector2.zero;

                return new Vector2(joystick.Horizontal, joystick.Vertical);
            }
        }

        void Awake()
        {
            ValidateComponents();
        }

        private void ValidateComponents()
        {
            if (joystick == null)
            {
                Debug.LogError($"[{gameObject.name}] Joystick reference is not assigned!", this);
                _isInputEnabled = false;
            }
        }

        public void EnableInput(bool enabled)
        {
            _isInputEnabled = enabled;

            if (!enabled && joystick != null && joystick.isActiveAndEnabled)
            {
                joystick.OnPointerUp(null);
            }
        }
    }
}

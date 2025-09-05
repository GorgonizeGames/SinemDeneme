using UnityEngine;

namespace Game.Runtime.Input
{
    public class JoystickInput : MonoBehaviour, IInputService
    {
        [SerializeField] private Joystick joystick;

        private bool _isInputEnabled = true;

        public Vector2 MovementInput => _isInputEnabled ? 
            new Vector2(joystick.Horizontal, joystick.Vertical) : Vector2.zero;

        void Awake()
        {
            if (joystick == null)
            {
                Debug.LogError("Joystick referansı atanmamış!", this);
                _isInputEnabled = false;
            }
        }

        public void EnableInput(bool enabled)
        {
            _isInputEnabled = enabled;
            if (!enabled && joystick.isActiveAndEnabled)
            {
                joystick.OnPointerUp(null); 
            }
        }
    }
}

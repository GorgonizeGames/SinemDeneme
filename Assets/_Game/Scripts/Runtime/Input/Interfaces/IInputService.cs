using UnityEngine;

namespace Game.Runtime.Input.Interfaces
{
    public interface IInputService
    {
        Vector2 MovementInput { get; }
        void EnableInput(bool enabled);
    }
}

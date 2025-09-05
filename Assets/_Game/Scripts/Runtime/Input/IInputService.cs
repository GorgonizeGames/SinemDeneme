using UnityEngine;

namespace Game.Runtime.Input
{
    public interface IInputService
    {
        Vector2 MovementInput { get; }
        void EnableInput(bool enabled);
    }
}

using UnityEngine;

namespace Game.Runtime.Core.Interfaces
{
    public interface IInputService
    {
        Vector2 MovementInput { get; }
        void EnableInput(bool enabled);
    }
}

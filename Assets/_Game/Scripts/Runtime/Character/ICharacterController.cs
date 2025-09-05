using UnityEngine;
using Game.Runtime.Core.StateMachine;

namespace Game.Runtime.Character
{
    public interface ICharacterController
    {
        Transform Transform { get; }
        Animator Animator { get; }
        Vector2 MovementInput { get; }
        bool ChangeState<T>() where T : BaseState<ICharacterController>;
    }
}
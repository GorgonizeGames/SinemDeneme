using UnityEngine;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.Character.Components;

namespace Game.Runtime.Character.Interfaces
{
    public interface ICharacterController
    {
        Transform Transform { get; }
        Animator Animator { get; }
        Vector2 MovementInput { get; }
        ICarryingController CarryingController { get; } 
        bool ChangeState<T>() where T : BaseState<ICharacterController>;
        void SetMovementInput(Vector2 input);
    }

    public enum CharacterType
    {
        Player,
        AI_Customer,    
        AI_Employee,    
        AI_Cashier      
    }
}
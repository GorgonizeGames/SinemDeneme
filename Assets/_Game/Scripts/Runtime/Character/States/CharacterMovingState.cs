using UnityEngine;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.Character.Motor;

namespace Game.Runtime.Character.States
{
    public class CharacterMovingState : BaseState<ICharacterController>
    {
        private CharacterMotor _motor;

        public override void OnEnter(ICharacterController owner)
        {
            // Motor referansını bir kere al, her kare arama.
            _motor = owner.Transform.GetComponent<CharacterMotor>();
        }

        public override void OnUpdate(ICharacterController owner)
        {
            if (owner.MovementInput.magnitude < 0.1f)
            {
                owner.ChangeState<CharacterIdleState>();
            }
        }

        public override void OnFixedUpdate(ICharacterController owner)
        {
            _motor.Move(owner.MovementInput);
        }
    }
}
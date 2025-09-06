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
            if (_motor == null)
                _motor = owner.Transform.GetComponent<CharacterMotor>();
            
            Debug.Log("🏃 Character entered MOVING state");
        }

        public override void OnUpdate(ICharacterController owner)
        {
            // ✅ Input kontrolü - hareket yoksa Idle state'e geç
            if (owner.MovementInput.magnitude < 0.1f)
            {
                owner.ChangeState<CharacterIdleState>();
            }
        }

        public override void OnFixedUpdate(ICharacterController owner)
        {
            // ✅ SADECE Moving state'te fiziksel hareket
            _motor.SetMovementInput(owner.MovementInput);
            _motor.ExecuteMovement();
        }

        public override void OnExit(ICharacterController owner)
        {
            Debug.Log("💤 Character exiting MOVING state");
        }
    }
}
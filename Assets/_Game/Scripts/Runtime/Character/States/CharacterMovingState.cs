using UnityEngine;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.Character.Motor;
using Game.Runtime.Character.Interfaces;

namespace Game.Runtime.Character.States
{
    public class CharacterMovingState : BaseState<ICharacterController>
    {
        private CharacterMotor _motor;

        public override void OnEnter(ICharacterController owner)
        {
            if (_motor == null)
            {
                _motor = owner.Transform.GetComponent<CharacterMotor>();
                if (_motor == null)
                {
                    Debug.LogError($"[{owner.Transform.name}] CharacterMotor component not found!");
                    return;
                }
            }

            Debug.Log("🏃 Character entered MOVING state");
        }

        public override void OnUpdate(ICharacterController owner)
        {
            if (_motor != null)
            {
                _motor.SetMovementInput(owner.MovementInput);
            }

            if (owner.MovementInput.magnitude < 0.1f)
            {
                owner.ChangeState<CharacterIdleState>();
            }
        }

        public override void OnFixedUpdate(ICharacterController owner)
        {
            _motor?.ExecuteMovement();
        }

        public override void OnExit(ICharacterController owner)
        {
            Debug.Log("💤 Character exiting MOVING state");
        }
    }
}
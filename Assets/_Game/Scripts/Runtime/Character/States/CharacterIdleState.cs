using UnityEngine;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.Character.Motor;
using Game.Runtime.Character.Interfaces;
using Game.Runtime.Character.Animation;

namespace Game.Runtime.Character.States
{
    public class CharacterIdleState : BaseState<ICharacterController>
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

            _motor.Stop();

            // ✅ AnimationHelper kullanarak doğru animasyon set etme
            if (owner.Animator != null)
            {
                AnimationHelper.SetMovementState(owner.Animator, 0f, false);
            }

            Debug.Log("💤 Character entered IDLE state");
        }

        public override void OnUpdate(ICharacterController owner)
        {
            if (owner.MovementInput.magnitude > 0.1f)
            {
                owner.ChangeState<CharacterMovingState>();
            }
        }

        public override void OnExit(ICharacterController owner)
        {
            Debug.Log("🏃 Character exiting IDLE state");
        }
    }
}
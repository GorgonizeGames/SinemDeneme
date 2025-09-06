using UnityEngine;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.Character.Motor;
using Game.Runtime.Character.Interfaces;

namespace Game.Runtime.Character.States
{
    public class CharacterIdleState : BaseState<ICharacterController>
    {
        private CharacterMotor _motor;

        public override void OnEnter(ICharacterController owner)
        {
            if (_motor == null)
                _motor = owner.Transform.GetComponent<CharacterMotor>();
            
            // ✅ State'e girerken dur
            _motor.Stop();
            
            Debug.Log("💤 Character entered IDLE state");
        }

        public override void OnUpdate(ICharacterController owner)
        {
            // ✅ Input kontrolü - hareket varsa Moving state'e geç
            if (owner.MovementInput.magnitude > 0.1f)
            {
                owner.ChangeState<CharacterMovingState>();
            }
        }

        public override void OnFixedUpdate(ICharacterController owner)
        {
            // ✅ Idle state'te fiziksel hareket YOK
            // Motor sadece animasyonu güncelliyor
        }

        public override void OnExit(ICharacterController owner)
        {
            Debug.Log("🏃 Character exiting IDLE state");
        }
    }
}
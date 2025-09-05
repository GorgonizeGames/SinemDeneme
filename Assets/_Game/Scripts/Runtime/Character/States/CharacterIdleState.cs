using UnityEngine;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.Character.Motor; // Bu satırı ekleyin

namespace Game.Runtime.Character.States
{
    public class CharacterIdleState : BaseState<ICharacterController>
    {
        private CharacterMotor _motor;

        // OnEnter metodu, bu duruma girildiği anda bir kere çalışır.
        public override void OnEnter(ICharacterController owner)
        {
            // Motor component'ine referansı al
            if (_motor == null)
                _motor = owner.Transform.GetComponent<CharacterMotor>();
            
            // Motora hareketin sıfır olduğunu söyleyerek animasyonu güncellemesini sağla
            _motor.Move(Vector2.zero);
        }

        public override void OnUpdate(ICharacterController owner)
        {
            if (owner.MovementInput.magnitude > 0.1f)
            {
                owner.ChangeState<CharacterMovingState>();
            }
        }
    }
}
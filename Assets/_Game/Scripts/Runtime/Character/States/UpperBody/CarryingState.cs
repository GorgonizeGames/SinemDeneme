using UnityEngine;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.Character.Components;
using Game.Runtime.Character.Interfaces;
using Game.Runtime.Character.Animation;

namespace Game.Runtime.Character.States.UpperBody
{
    public class CarryingState : BaseState<ICarryingController>
    {
        public override void OnEnter(ICarryingController owner)
        {
            Debug.Log("📦 Upper body: CARRYING");

            // ✅ AnimationHelper kullanarak doğru animasyon set etme
            SetCarryingAnimation(owner, true);
        }

        public override void OnUpdate(ICarryingController owner)
        {
            // Item zaten parenting ile takip ediyor
            // Ek bir işlem gerekmiyor
        }

        public override void OnExit(ICarryingController owner)
        {
            Debug.Log("📦 Exiting carrying state");
            
            // ✅ Exit sırasında animasyonu temizle
            SetCarryingAnimation(owner, false);
        }

        private void SetCarryingAnimation(ICarryingController owner, bool isCarrying)
        {
            var characterController = (owner as MonoBehaviour)?.GetComponent<ICharacterController>();
            if (characterController?.Animator != null)
            {
                // ✅ AnimationHelper kullanarak carrying state'i set et
                AnimationHelper.SetCarryingState(characterController.Animator, isCarrying);
            }
        }
    }
}
using UnityEngine;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.Character.Components;
using Game.Runtime.Character.Interfaces;
using Game.Runtime.Character.Animation;

namespace Game.Runtime.Character.States.UpperBody
{
    public class HandsFreeState : BaseState<ICarryingController>
    {
        public override void OnEnter(ICarryingController owner)
        {
            Debug.Log("🙌 Upper body: HANDS FREE");

            // ✅ AnimationHelper kullanarak doğru animasyon set etme
            SetHandsFreeAnimation(owner);
        }

        public override void OnUpdate(ICarryingController owner)
        {
            // Bu state'te özel bir işlem yok
            // Pickup komutu dışarıdan gelecek
        }

        public override void OnExit(ICarryingController owner)
        {
            Debug.Log("🙌 Exiting hands free state");
        }

        private void SetHandsFreeAnimation(ICarryingController owner)
        {
            var characterController = (owner as MonoBehaviour)?.GetComponent<ICharacterController>();
            if (characterController?.Animator != null)
            {
                // ✅ AnimationHelper kullanarak carrying state'i false yap
                AnimationHelper.SetCarryingState(characterController.Animator, false);
            }
        }
    }
}
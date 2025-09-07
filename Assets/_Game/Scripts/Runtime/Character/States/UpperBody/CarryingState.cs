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

            // Upper Body Layer'da carrying animasyonunu aktif et
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
        }

        private void SetCarryingAnimation(ICarryingController owner, bool isCarrying)
        {
            var characterController = (owner as MonoBehaviour)?.GetComponent<ICharacterController>();
            if (characterController?.Animator != null)
            {
                // Upper Body Layer parametresi
                characterController.Animator.SetBool(AnimationParameters.IsCarrying, isCarrying);

                // Upper Body Layer weight ayarla (taşırken layer aktif)
                characterController.Animator.SetLayerWeight(AnimationLayers.UpperBodyLayer, isCarrying ? 1f : 0f);
            }
        }
    }
}

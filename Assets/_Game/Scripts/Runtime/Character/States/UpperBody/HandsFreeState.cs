using UnityEngine;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.Character.Components;
using Game.Runtime.Character.Interfaces;

namespace Game.Runtime.Character.States.UpperBody
{
    public class HandsFreeState : BaseState<ICarryingController>
    {
        public override void OnEnter(ICarryingController owner)
        {
            Debug.Log("🙌 Upper body: HANDS FREE");
            
            // Set animator to hands free state
            SetHandsFreeAnimation(owner);
        }

        public override void OnUpdate(ICarryingController owner)
        {
            // State transition happens externally via TryPickupItem()
            // This state just maintains the hands free pose
        }

        public override void OnExit(ICarryingController owner)
        {
            Debug.Log("🙌 Exiting hands free state");
        }

        private void SetHandsFreeAnimation(ICarryingController owner)
        {
            // Get character controller to access animator
            var characterController = (owner as MonoBehaviour)?.GetComponent<ICharacterController>();
            if (characterController?.Animator != null)
            {
                // Simple bool - animator handles the transition
                characterController.Animator.SetBool("IsCarrying", false);
            }
        }
    }
}
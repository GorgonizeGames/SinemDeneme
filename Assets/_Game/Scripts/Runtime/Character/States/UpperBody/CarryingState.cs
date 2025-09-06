using UnityEngine;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.Character.Components;
using Game.Runtime.Character.Interfaces;

namespace Game.Runtime.Character.States.UpperBody
{
   public class CarryingState : BaseState<ICarryingController>
    {
        public override void OnEnter(ICarryingController owner)
        {
            Debug.Log("📦 Upper body: CARRYING");
            
            // Set animator to carrying state
            SetCarryingAnimation(owner);
        }

        public override void OnUpdate(ICarryingController owner)
        {
            // State transition happens externally via TryDropItem()
            // This state just maintains the carrying pose
            
            // Keep item synced with carry point (optional, parenting should handle this)
            UpdateCarriedItemPosition(owner);
        }

        public override void OnExit(ICarryingController owner)
        {
            Debug.Log("📦 Exiting carrying state");
        }

        private void SetCarryingAnimation(ICarryingController owner)
        {
            var characterController = (owner as MonoBehaviour)?.GetComponent<ICharacterController>();
            if (characterController?.Animator != null)
            {
                // Simple bool - animator handles the transition
                characterController.Animator.SetBool("IsCarrying", true);
            }
        }

        private void UpdateCarriedItemPosition(ICarryingController owner)
        {
            // Items follow carry point automatically due to parenting
            // This method can be used for additional adjustments if needed
            
            if (owner.CarriedItem != null && owner.CarryPoint != null)
            {
                // Optional: Apply any runtime adjustments
                // For example, slight sway during movement, etc.
            }
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

namespace Game.Runtime.Character.AI
{
    public class CustomerBehavior : BaseAIBehavior
    {
        private CustomerState currentState = CustomerState.Entering;
        private Queue<Vector3> shoppingTargets = new Queue<Vector3>();
        private float stateTimer = 0f;
        private float shoppingTime = 5f; // Time spent shopping

        public CustomerBehavior(AICharacterController aiController) : base(aiController) { }

        public override void UpdateBehavior()
        {
            stateTimer += Time.deltaTime;

            switch (currentState)
            {
                case CustomerState.Entering:
                    HandleEntering();
                    break;
                case CustomerState.Shopping:
                    HandleShopping();
                    break;
                case CustomerState.GoingToQueue:
                    HandleGoingToQueue();
                    break;
                case CustomerState.InQueue:
                    HandleInQueue();
                    break;
                case CustomerState.Paying:
                    HandlePaying();
                    break;
                case CustomerState.Leaving:
                    HandleLeaving();
                    break;
            }
        }

        private void HandleEntering()
        {
            // Find random shopping spots
            GenerateShoppingTargets();
            currentState = CustomerState.Shopping;
            stateTimer = 0f;
        }

        private void HandleShopping()
        {
            if (shoppingTargets.Count > 0 && controller.HasReachedDestination)
            {
                Vector3 nextTarget = shoppingTargets.Dequeue();
                controller.MoveTo(nextTarget);
                stateTimer = 0f;
            }
            else if (shoppingTargets.Count == 0 && stateTimer > shoppingTime)
            {
                currentState = CustomerState.GoingToQueue;
            }
        }

        private void HandleGoingToQueue()
        {
            // Find queue position
            Vector3 queuePosition = FindQueuePosition();
            controller.MoveTo(queuePosition);
            currentState = CustomerState.InQueue;
        }

        private void HandleInQueue()
        {
            // Wait in queue logic
            if (IsMyTurnToPay())
            {
                currentState = CustomerState.Paying;
                stateTimer = 0f;
            }
        }

        private void HandlePaying()
        {
            if (stateTimer > 3f) // Payment duration
            {
                currentState = CustomerState.Leaving;
            }
        }

        private void HandleLeaving()
        {
            Vector3 exitPoint = FindExitPoint();
            controller.MoveTo(exitPoint);
            
            // Customer completed their journey
            if (controller.HasReachedDestination)
            {
                // Destroy or return to pool
                Object.Destroy(controller.gameObject);
            }
        }

        private void GenerateShoppingTargets()
        {
            // TODO: Get shopping points from store manager
            // For now, generate random points
            for (int i = 0; i < Random.Range(2, 5); i++)
            {
                Vector3 randomPoint = GetRandomShoppingPoint();
                shoppingTargets.Enqueue(randomPoint);
            }
        }

        private Vector3 GetRandomShoppingPoint()
        {
            // TODO: Get from store layout system
            return Vector3.zero; // Placeholder
        }

        private Vector3 FindQueuePosition()
        {
            // TODO: Get from queue management system
            return Vector3.zero; // Placeholder
        }

        private bool IsMyTurnToPay()
        {
            // TODO: Check with queue system
            return stateTimer > 10f; // Placeholder
        }

        private Vector3 FindExitPoint()
        {
            // TODO: Get from store layout
            return Vector3.zero; // Placeholder
        }
    }

    public enum CustomerState
    {
        Entering,
        Shopping,
        GoingToQueue,
        InQueue,
        Paying,
        Leaving
    }
}

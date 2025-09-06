using UnityEngine;
using System.Collections.Generic;

namespace Game.Runtime.Character.AI
{
    public class CustomerBehavior : BaseAIBehavior
    {
        private CustomerState currentState = CustomerState.Entering;
        private Queue<Vector3> shoppingTargets = new Queue<Vector3>();
        private float stateTimer = 0f;
        private float shoppingTime = 5f;

        public CustomerBehavior(AICharacterController aiController) : base(aiController) { }

        public override void UpdateBehavior()
        {
            if (!IsActive) return;

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
            GenerateShoppingTargets();
            currentState = CustomerState.Shopping;
            stateTimer = 0f;
        }

        private void HandleShopping()
        {
            if (controller == null) return;

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
            Vector3 queuePosition = FindQueuePosition();
            controller?.MoveTo(queuePosition);
            currentState = CustomerState.InQueue;
        }

        private void HandleInQueue()
        {
            if (IsMyTurnToPay())
            {
                currentState = CustomerState.Paying;
                stateTimer = 0f;
            }
        }

        private void HandlePaying()
        {
            if (stateTimer > 3f)
            {
                currentState = CustomerState.Leaving;
            }
        }

        private void HandleLeaving()
        {
            if (controller == null) return;

            Vector3 exitPoint = FindExitPoint();
            controller.MoveTo(exitPoint);

            if (controller.HasReachedDestination)
            {
                // Clean up before destroy
                OnBehaviorEnd();
                Object.Destroy(controller.gameObject);
            }
        }

        public override void OnBehaviorEnd()
        {
            base.OnBehaviorEnd();
            shoppingTargets.Clear();
        }

        private void GenerateShoppingTargets()
        {
            for (int i = 0; i < Random.Range(2, 5); i++)
            {
                Vector3 randomPoint = GetRandomShoppingPoint();
                shoppingTargets.Enqueue(randomPoint);
            }
        }

        private Vector3 GetRandomShoppingPoint()
        {
            // TODO: Get from store layout system
            return Vector3.zero;
        }

        private Vector3 FindQueuePosition()
        {
            // TODO: Get from queue management system
            return Vector3.zero;
        }

        private bool IsMyTurnToPay()
        {
            // TODO: Check with queue system
            return stateTimer > 10f;
        }

        private Vector3 FindExitPoint()
        {
            // TODO: Get from store layout
            return Vector3.zero;
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
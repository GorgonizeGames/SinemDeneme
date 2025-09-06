using UnityEngine;

namespace Game.Runtime.Character.AI
{
    public class CashierBehavior : BaseAIBehavior
    {
        private CashierState currentState = CashierState.WaitingForCustomer;
        private Transform cashierStation;
        private GameObject currentCustomer;
        private float serviceTimer = 0f;

        public CashierBehavior(AICharacterController aiController) : base(aiController) { }

        public override void Initialize()
        {
            base.Initialize();
            // Find cashier station
            cashierStation = FindCashierStation();
            if (cashierStation != null)
            {
                controller.MoveTo(cashierStation.position);
            }
        }

        public override void UpdateBehavior()
        {
            switch (currentState)
            {
                case CashierState.GoingToStation:
                    HandleGoingToStation();
                    break;
                case CashierState.WaitingForCustomer:
                    HandleWaitingForCustomer();
                    break;
                case CashierState.ServingCustomer:
                    HandleServingCustomer();
                    break;
            }
        }

        private void HandleGoingToStation()
        {
            if (controller.HasReachedDestination)
            {
                currentState = CashierState.WaitingForCustomer;
                controller.Stop(); // Stay at station
            }
        }

        private void HandleWaitingForCustomer()
        {
            // Check for customers in queue
            GameObject nextCustomer = FindNextCustomerInQueue();
            if (nextCustomer != null)
            {
                currentCustomer = nextCustomer;
                currentState = CashierState.ServingCustomer;
                serviceTimer = 0f;
            }
        }

        private void HandleServingCustomer()
        {
            serviceTimer += Time.deltaTime;
            
            if (serviceTimer >= GetServiceDuration())
            {
                // Service completed
                OnCustomerServiceCompleted();
                currentCustomer = null;
                currentState = CashierState.WaitingForCustomer;
            }
        }

        private Transform FindCashierStation()
        {
            // TODO: Find from store layout system
            return null; // Placeholder
        }

        private GameObject FindNextCustomerInQueue()
        {
            // TODO: Get from queue management system
            return null; // Placeholder
        }

        private float GetServiceDuration()
        {
            return Random.Range(2f, 5f); // Random service time
        }

        private void OnCustomerServiceCompleted()
        {
            Debug.Log("💰 Cashier completed customer service");
            // TODO: Notify customer and queue system
        }
    }

    public enum CashierState
    {
        GoingToStation,
        WaitingForCustomer,
        ServingCustomer
    }
}

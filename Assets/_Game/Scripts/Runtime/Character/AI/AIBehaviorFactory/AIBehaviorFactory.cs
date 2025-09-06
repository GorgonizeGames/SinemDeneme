namespace Game.Runtime.Character.AI.Factory
{
    public static class AIBehaviorFactory
    {
        public static IAIBehavior CreateBehavior(AIRole role, AICharacterController controller)
        {
            switch (role)
            {
                case AIRole.Customer:
                    return new CustomerBehavior(controller);
                case AIRole.Employee:
                    return new EmployeeBehavior(controller);
                case AIRole.Cashier:
                    return new CashierBehavior(controller);
                default:
                    UnityEngine.Debug.LogError($"Unknown AI Role: {role}");
                    return null;
            }
        }
    }
}
using Game.Runtime.Character.Interfaces;

namespace Game.Runtime.Character.AI.Factory
{
    public static class AIBehaviorFactory
    {
        public static IAIBehavior CreateBehavior(CharacterType role, AICharacterController controller)
        {
            switch (role)
            {
                case CharacterType.AI_Customer:
                    return new CustomerBehavior(controller);
                case CharacterType.AI_Employee:
                    return new EmployeeBehavior(controller);
                case CharacterType.AI_Cashier:
                    return new CashierBehavior(controller);
                default:
                    UnityEngine.Debug.LogError($"Unknown AI Role: {role}");
                    return null;
            }
        }
    }
}
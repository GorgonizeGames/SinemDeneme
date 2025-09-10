namespace Game.Runtime.Interactions.Interfaces
{
    public interface IInteractable
    {
        bool CanInteract(IInteractor interactor);
        void OnInteractionStart(IInteractor interactor);
        void OnInteractionContinue(IInteractor interactor);
        void OnInteractionEnd(IInteractor interactor);
        InteractionType InteractionType { get; }
        InteractionPriority Priority { get; }
    }

    public enum InteractionType
    {
        Machine,
        Shelf,
        CashRegister,
        Zone,
        Purchasable
    }

    public enum InteractionPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }
}
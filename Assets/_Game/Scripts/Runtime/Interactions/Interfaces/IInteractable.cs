namespace Game.Runtime.Interactions.Interfaces
{
    public interface IInteractable
    {
        bool CanInteract(IInteractor interactor);
        void OnInteractionStart(IInteractor interactor);
        void OnInteractionContinue(IInteractor interactor);
        void OnInteractionEnd(IInteractor interactor);
        InteractionType InteractionType { get; }
    }

    public enum InteractionType
    {
        Machine,
        Shelf,
        CashRegister,
        Zone,
        Purchasable
    }
}
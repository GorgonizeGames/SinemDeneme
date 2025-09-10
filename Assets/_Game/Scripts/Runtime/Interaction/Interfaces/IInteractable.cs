using Game.Runtime.Character.Components;
using Game.Runtime.Core.Interfaces;
using Game.Runtime.Economy;
using Game.Runtime.Interaction.Interfaces;
using Game.Runtime.Items;
using Game.Runtime.Items.Data;
using Game.Runtime.Items.Services;
using Game.Runtime.Store.Areas;

namespace Game.Runtime.Interaction.Interfaces
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
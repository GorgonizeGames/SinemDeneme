using Game.Runtime.Character;

namespace Game.Runtime.Interaction.Interfaces
{
    public interface IInteractor
    {
        BaseCharacterController Character { get; }
        bool IsInteracting { get; }
        bool CanStartInteraction { get; }
        void StartInteraction(IInteractable interactable);
        void EndInteraction(IInteractable interactable);
    }
}

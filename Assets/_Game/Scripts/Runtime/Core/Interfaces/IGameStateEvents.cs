using System;

namespace Game.Runtime.Core.Interfaces
{
    public interface IGameStateEvents
    {
        // State Enter Events
        event Action OnMainMenuEntered;
        event Action OnPlayingEntered;
        event Action OnPausedEntered;
        event Action OnGameOverEntered;

        // State Exit Events
        event Action OnMainMenuExited;
        event Action OnPlayingExited;
        event Action OnPausedExited;
        event Action OnGameOverExited;

        // Trigger Methods
        void TriggerMainMenuEntered();
        void TriggerMainMenuExited();
        void TriggerPlayingEntered();
        void TriggerPlayingExited();
        void TriggerPausedEntered();
        void TriggerPausedExited();
        void TriggerGameOverEntered();
        void TriggerGameOverExited();
    }

    public class GameStateEvents : IGameStateEvents
    {
        // Events
        public event Action OnMainMenuEntered;
        public event Action OnPlayingEntered;
        public event Action OnPausedEntered;
        public event Action OnGameOverEntered;
        public event Action OnMainMenuExited;
        public event Action OnPlayingExited;
        public event Action OnPausedExited;
        public event Action OnGameOverExited;

        // Trigger Methods
        public void TriggerMainMenuEntered() => OnMainMenuEntered?.Invoke();
        public void TriggerMainMenuExited() => OnMainMenuExited?.Invoke();
        public void TriggerPlayingEntered() => OnPlayingEntered?.Invoke();
        public void TriggerPlayingExited() => OnPlayingExited?.Invoke();
        public void TriggerPausedEntered() => OnPausedEntered?.Invoke();
        public void TriggerPausedExited() => OnPausedExited?.Invoke();
        public void TriggerGameOverEntered() => OnGameOverEntered?.Invoke();
        public void TriggerGameOverExited() => OnGameOverExited?.Invoke();
    }
}
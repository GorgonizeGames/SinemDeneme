using System;

namespace Game.Runtime.UI.Signals
{
    public interface IUISignals
    {
        // Events
        event Action OnPlayButtonClicked;
        event Action OnMainMenuRequested;
        event Action OnPauseToggleRequested;
        event Action OnResumeRequested;
        event Action OnRestartRequested;
        event Action OnSettingsRequested;
        event Action OnQuitRequested;
        event Action<int> OnCheatMoneyRequested;
        event Action OnCheatClearDataRequested;

        // Trigger Methods
        void TriggerPlayButton();
        void TriggerMainMenuRequest();
        void TriggerPauseToggle();
        void TriggerResumeRequest();
        void TriggerRestartRequest();
        void TriggerSettingsRequest();
        void TriggerQuitRequest();
        void TriggerCheatMoney(int amount);
        void TriggerCheatClearData();
    }

    public class UISignals : IUISignals
    {
        // Events
        public event Action OnPlayButtonClicked;
        public event Action OnMainMenuRequested;
        public event Action OnPauseToggleRequested;
        public event Action OnResumeRequested;
        public event Action OnRestartRequested;
        public event Action OnSettingsRequested;
        public event Action OnQuitRequested;
        public event Action<int> OnCheatMoneyRequested;
        public event Action OnCheatClearDataRequested;

        // Trigger Methods
        public void TriggerPlayButton() => OnPlayButtonClicked?.Invoke();
        public void TriggerMainMenuRequest() => OnMainMenuRequested?.Invoke();
        public void TriggerPauseToggle() => OnPauseToggleRequested?.Invoke();
        public void TriggerResumeRequest() => OnResumeRequested?.Invoke();
        public void TriggerRestartRequest() => OnRestartRequested?.Invoke();
        public void TriggerSettingsRequest() => OnSettingsRequested?.Invoke();
        public void TriggerQuitRequest() => OnQuitRequested?.Invoke();
        public void TriggerCheatMoney(int amount) => OnCheatMoneyRequested?.Invoke(amount);
        public void TriggerCheatClearData() => OnCheatClearDataRequested?.Invoke();
    }
}

using UnityEngine;
using UnityEngine.UI;
using Game.Runtime.UI.Core;
using Game.Runtime.UI.Signals;
using Game.Runtime.Core.DI;

namespace Game.Runtime.UI.Panels
{
    public class PausePanel : BaseUIPanel
    {
        [Header("Pause Menu")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;

        protected override void OnInitialize()
        {
            layer = UILayer.Menu;
            
            resumeButton?.onClick.AddListener(OnResumeClicked);
            settingsButton?.onClick.AddListener(OnSettingsClicked);
            mainMenuButton?.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnResumeClicked()
        {
            Debug.Log("▶️ Resume button clicked!");
            _uiSignals?.TriggerResumeRequest();
        }

        private void OnSettingsClicked()
        {
            Debug.Log("⚙️ Settings from pause clicked!");
            _uiSignals?.TriggerSettingsRequest();
        }

        private void OnMainMenuClicked()
        {
            Debug.Log("🏠 Main menu from pause clicked!");
            _uiSignals?.TriggerMainMenuRequest();
        }
    }
}
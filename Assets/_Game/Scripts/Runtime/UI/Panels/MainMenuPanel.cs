using UnityEngine;
using UnityEngine.UI;
using Game.Runtime.UI.Core;
using Game.Runtime.UI.Signals;
using Game.Runtime.Core.DI;

namespace Game.Runtime.UI.Panels
{
    public class MainMenuPanel : BaseUIPanel
    {
        [Header("Main Menu")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        protected override void OnInitialize()
        {
            layer = UILayer.Menu;
            
            playButton?.onClick.AddListener(OnTapToPlay);
            settingsButton?.onClick.AddListener(OnSettingsClicked);
            quitButton?.onClick.AddListener(OnQuitClicked);
        }

        private void OnTapToPlay()
        {
            Debug.Log("🎮 Play button clicked!");
            _uiSignals?.TriggerPlayButton();
        }

        private void OnSettingsClicked()
        {
            Debug.Log("⚙️ Settings button clicked!");
            _uiSignals?.TriggerSettingsRequest();
        }

        private void OnQuitClicked()
        {
            Debug.Log("🚪 Quit button clicked!");
            _uiSignals?.TriggerQuitRequest();
        }
    }
}

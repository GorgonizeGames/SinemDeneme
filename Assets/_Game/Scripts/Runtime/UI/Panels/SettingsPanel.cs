using UnityEngine;
using UnityEngine.UI;
using Game.Runtime.UI.Core;

namespace Game.Runtime.UI.Panels
{
    public class SettingsPanel : BaseUIPanel
    {
        [Header("Settings")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle fullscreenToggle;

        protected override void OnInitialize()
        {
            layer = UILayer.Popup;
            
            closeButton?.onClick.AddListener(OnCloseClicked);
            musicSlider?.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxSlider?.onValueChanged.AddListener(OnSFXVolumeChanged);
            fullscreenToggle?.onValueChanged.AddListener(OnFullscreenToggled);
        }

        private void OnCloseClicked()
        {
            Debug.Log("❌ Close settings clicked!");
            _uiService?.HidePanelAsync<SettingsPanel>();
        }

        private void OnMusicVolumeChanged(float value)
        {
            Debug.Log($"🎵 Music volume: {value}");
            // Set music volume
        }

        private void OnSFXVolumeChanged(float value)
        {
            Debug.Log($"🔊 SFX volume: {value}");
            // Set SFX volume
        }

        private void OnFullscreenToggled(bool isFullscreen)
        {
            Debug.Log($"🖥️ Fullscreen: {isFullscreen}");
            Screen.fullScreen = isFullscreen;
        }
    }
}

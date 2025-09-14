using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.UI.Signals;
using Game.Runtime.UI.Core;
using Game.Runtime.UI.Panels;
using Game.Runtime.Core.Extensions;

namespace Game.Runtime.UI
{
    /// <summary>
    /// Settings UI Handler - Settings panel açma/kapama işlemleri
    /// </summary>
    public class SettingsUIHandler : MonoBehaviour
    {
        [Inject] private IUIService _uiService;
        [Inject] private IUISignals _uiSignals;

        private void Start()
        {
            this.InjectDependencies();
            SubscribeToUISignals();
        }

        private void SubscribeToUISignals()
        {
            if (_uiSignals != null)
            {
                _uiSignals.OnSettingsRequested += HandleSettingsRequested;
            }
        }

        private async void HandleSettingsRequested()
        {
            Debug.Log("⚙️ Opening Settings Panel");
            await _uiService?.ShowPanelAsync<SettingsPanel>(UITransition.Scale);
        }

        private void OnDestroy()
        {
            if (_uiSignals != null)
            {
                _uiSignals.OnSettingsRequested -= HandleSettingsRequested;
            }
        }
    }
}
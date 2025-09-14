using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Interfaces;
using Game.Runtime.UI.Core;
using Game.Runtime.UI.Panels;
using Game.Runtime.Core.Extensions;


namespace Game.Runtime.UI
{
    /// <summary>
    /// UI State Handler - Game state değişikliklerini dinler ve UI'ı yönetir
    /// State'lerden tamamen bağımsız çalışır
    /// </summary>
    public class UIStateHandler : MonoBehaviour
    {
        [Inject] private IUIService _uiService;
        [Inject] private IGameStateEvents _stateEvents;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        private void Start()
        {
            this.InjectDependencies();
            SubscribeToStateEvents();
        }

        private void SubscribeToStateEvents()
        {
            if (_stateEvents == null)
            {
                Debug.LogError("GameStateEvents is not injected!");
                return;
            }

            // Main Menu State
            _stateEvents.OnMainMenuEntered += HandleMainMenuEntered;
            _stateEvents.OnMainMenuExited += HandleMainMenuExited;

            // Playing State  
            _stateEvents.OnPlayingEntered += HandlePlayingEntered;
            _stateEvents.OnPlayingExited += HandlePlayingExited;

            // Paused State
            _stateEvents.OnPausedEntered += HandlePausedEntered;
            _stateEvents.OnPausedExited += HandlePausedExited;

            // Game Over State
            _stateEvents.OnGameOverEntered += HandleGameOverEntered;
            _stateEvents.OnGameOverExited += HandleGameOverExited;

            if (enableDebugLogs)
            {
                Debug.Log("🎭 UIStateHandler subscribed to all state events");
            }
        }

        // ===========================================
        // UI STATE HANDLERS
        // ===========================================

        private async void HandleMainMenuEntered()
        {
            if (enableDebugLogs) Debug.Log("🎭 UI: Showing Main Menu");
            
            await _uiService?.ShowPanelAsync<MainMenuPanel>(UITransition.Fade);
            await _uiService?.ShowPanelAsync<HUDPanel>(UITransition.None); // HUD always visible
        }

        private async void HandleMainMenuExited()
        {
            if (enableDebugLogs) Debug.Log("🎭 UI: Hiding Main Menu");
            
            await _uiService?.HidePanelAsync<MainMenuPanel>(UITransition.Fade);
        }

        private async void HandlePlayingEntered()
        {
            if (enableDebugLogs) Debug.Log("🎭 UI: Showing Game UI");
            
            await _uiService?.ShowPanelAsync<GameUIPanel>(UITransition.Fade);
            await _uiService?.ShowPanelAsync<HUDPanel>(UITransition.None); // HUD always visible
        }

        private async void HandlePlayingExited()
        {
            if (enableDebugLogs) Debug.Log("🎭 UI: Hiding Game UI");
            
            await _uiService?.HidePanelAsync<GameUIPanel>(UITransition.Fade);
        }

        private async void HandlePausedEntered()
        {
            if (enableDebugLogs) Debug.Log("🎭 UI: Showing Pause Menu");
            
            await _uiService?.ShowPanelAsync<PausePanel>(UITransition.Scale);
        }

        private async void HandlePausedExited()
        {
            if (enableDebugLogs) Debug.Log("🎭 UI: Hiding Pause Menu");
            
            await _uiService?.HidePanelAsync<PausePanel>(UITransition.Scale);
        }

        private async void HandleGameOverEntered()
        {
            if (enableDebugLogs) Debug.Log("🎭 UI: Showing Game Over");
            
            // Wait a bit for dramatic effect
            await System.Threading.Tasks.Task.Delay(1000);
            await _uiService?.ShowPanelAsync<GameOverPanel>(UITransition.FadeScale);
        }

        private async void HandleGameOverExited()
        {
            if (enableDebugLogs) Debug.Log("🎭 UI: Hiding Game Over");
            
            await _uiService?.HidePanelAsync<GameOverPanel>(UITransition.Fade);
        }

        private void OnDestroy()
        {
            if (_stateEvents != null)
            {
                // Unsubscribe to prevent memory leaks
                _stateEvents.OnMainMenuEntered -= HandleMainMenuEntered;
                _stateEvents.OnMainMenuExited -= HandleMainMenuExited;
                _stateEvents.OnPlayingEntered -= HandlePlayingEntered;
                _stateEvents.OnPlayingExited -= HandlePlayingExited;
                _stateEvents.OnPausedEntered -= HandlePausedEntered;
                _stateEvents.OnPausedExited -= HandlePausedExited;
                _stateEvents.OnGameOverEntered -= HandleGameOverEntered;
                _stateEvents.OnGameOverExited -= HandleGameOverExited;
            }
        }
    }
}
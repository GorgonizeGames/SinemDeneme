using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Runtime.UI.Core;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Interfaces;

namespace Game.Runtime.UI.Panels
{
    public class GameOverPanel : BaseUIPanel
    {
        [Header("Game Over")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private TextMeshProUGUI scoreText;

        // ✅ We need economy for final score display only
        [Inject] private IEconomyService _economyService;

        protected override void OnInitialize()
        {
            layer = UILayer.Menu;
            
            restartButton?.onClick.AddListener(OnRestartClicked);
            mainMenuButton?.onClick.AddListener(OnMainMenuClicked);
        }

        protected override void OnShow()
        {
            // Show final score when panel becomes visible
            if (scoreText != null && _economyService != null)
            {
                scoreText.text = $"Final Money: ${_economyService.CurrentMoney}";
            }
        }

        private void OnRestartClicked()
        {
            Debug.Log("🔄 Restart button clicked!");
            _uiSignals?.TriggerRestartRequest();
        }

        private void OnMainMenuClicked()
        {
            Debug.Log("🏠 Main menu from game over clicked!");
            _uiSignals?.TriggerMainMenuRequest();
        }
    }
}
using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Interfaces;
using Game.Runtime.UI.Core;
using Game.Runtime.UI.Panels;
using Game.Runtime.Core.Extensions;

namespace Game.Runtime.UI
{
    /// <summary>
    /// Cheat UI Handler - Cheat feedback ve money animasyonları
    /// </summary>
    public class CheatUIHandler : MonoBehaviour
    {
        [Inject] private IUIService _uiService;
        
        [Header("Settings")]
        [SerializeField] private bool showCheatFeedback = true;

        private void Start()
        {
            this.InjectDependencies();
        }

        /// <summary>
        /// Show money gain animation from specific position
        /// </summary>
        public async void ShowMoneyGainFromPosition(Vector3 sourcePosition, float amount)
        {
            var hudPanel = _uiService?.GetPanel<HUDPanel>();
            if (hudPanel is ICurrencyDisplay currencyDisplay)
            {
                await currencyDisplay.PlayMoneyGainAnimation(sourcePosition, amount);
            }
        }

        /// <summary>
        /// Show cheat feedback in cheat panel
        /// </summary>
        public void ShowCheatFeedback(string message, Color color)
        {
            if (!showCheatFeedback) return;

            // This could trigger cheat panel feedback
            // For now, just log it
            Debug.Log($"🐛 Cheat: {message}");
        }
    }
}

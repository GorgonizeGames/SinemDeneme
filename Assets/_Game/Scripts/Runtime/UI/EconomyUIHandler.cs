using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Interfaces;
using Game.Runtime.UI.Core;
using Game.Runtime.UI.Panels;
using Game.Runtime.Core.Extensions;

namespace Game.Runtime.UI
{
    /// <summary>
    /// Economy UI Handler - Economy değişikliklerini dinler ve UI'ı günceller
    /// </summary>
    public class EconomyUIHandler : MonoBehaviour
    {
        [Inject] private IUIService _uiService;
        [Inject] private IEconomyService _economyService;

        [Header("Settings")]
        [SerializeField] private bool enableDebugLogs = true;

        private void Start()
        {
            this.InjectDependencies();
            SubscribeToEconomyEvents();
            InitializeUI();
        }

        private void SubscribeToEconomyEvents()
        {
            if (_economyService != null)
            {
                _economyService.OnMoneyChanged += HandleMoneyChanged;
                
                if (enableDebugLogs)
                    Debug.Log("💰 EconomyUIHandler subscribed to money changes");
            }
        }

        private void InitializeUI()
        {
            // Initialize HUD with current money amount
            if (_economyService != null)
            {
                var hudPanel = _uiService?.GetPanel<HUDPanel>();
                if (hudPanel is ICurrencyDisplay currencyDisplay)
                {
                    currencyDisplay.UpdateMoney(_economyService.CurrentMoney, false);
                }
            }
        }

        private void HandleMoneyChanged(int newAmount)
        {
            if (enableDebugLogs) 
                Debug.Log($"💰 Money changed to: ${newAmount}");

            // Update HUD display
            var hudPanel = _uiService?.GetPanel<HUDPanel>();
            if (hudPanel is ICurrencyDisplay currencyDisplay)
            {
                currencyDisplay.UpdateMoney(newAmount, true);
            }
        }

        private void OnDestroy()
        {
            if (_economyService != null)
            {
                _economyService.OnMoneyChanged -= HandleMoneyChanged;
            }
        }
    }
}

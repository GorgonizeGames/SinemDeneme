using UnityEngine;
using System.Threading.Tasks;
using Game.Runtime.UI.Core;
using Game.Runtime.UI.Signals;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Interfaces;
using Game.Runtime.UI.Panels;

namespace Game.Runtime.UI.Extensions
{
    public static class UIExtensions
    {
        public static IUIService GetUIService(this MonoBehaviour mono)
        {
            return Dependencies.Container.TryResolve<IUIService>(out var service) ? service : null;
        }

        public static IUISignals GetUISignals(this MonoBehaviour mono)
        {
            return Dependencies.Container.TryResolve<IUISignals>(out var signals) ? signals : null;
        }

        public static async Task ShowPanel<T>(this MonoBehaviour mono, UITransition transition = UITransition.Fade) 
            where T : Component, IUIPanel
        {
            var uiService = mono.GetUIService();
            if (uiService != null)
            {
                await uiService.ShowPanelAsync<T>(transition);
            }
        }

        public static async Task HidePanel<T>(this MonoBehaviour mono, UITransition transition = UITransition.Fade) 
            where T : Component, IUIPanel
        {
            var uiService = mono.GetUIService();
            if (uiService != null)
            {
                await uiService.HidePanelAsync<T>(transition);
            }
        }

        // ✅ Simplified money gain animation
        public static async Task ShowMoneyGain(this MonoBehaviour mono, Vector3 sourcePos, int amount)
        {
            var uiService = mono.GetUIService();
            if (uiService != null)
            {
                var hudPanel = uiService.GetPanel<HUDPanel>();
                if (hudPanel is ICurrencyDisplay currencyDisplay)
                {
                    await currencyDisplay.PlayMoneyGainAnimation(sourcePos, amount);
                }
            }
        }

        // Signal shortcuts
        public static void TriggerPlay(this MonoBehaviour mono)
        {
            mono.GetUISignals()?.TriggerPlayButton();
        }

        public static void TriggerPause(this MonoBehaviour mono)
        {
            mono.GetUISignals()?.TriggerPauseToggle();
        }

        public static void TriggerMainMenu(this MonoBehaviour mono)
        {
            mono.GetUISignals()?.TriggerMainMenuRequest();
        }

        public static void TriggerSettings(this MonoBehaviour mono)
        {
            mono.GetUISignals()?.TriggerSettingsRequest();
        }

        public static void AddMoney(this MonoBehaviour mono, int amount)
        {
            mono.GetUISignals()?.TriggerCheatMoney(amount);
        }
    }
}
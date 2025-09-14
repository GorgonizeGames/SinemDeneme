using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.UI.Core;
using Game.Runtime.Core.Extensions;
using Game.Runtime.UI.Panels;

namespace Game.Runtime.UI.Input
{
    public class CheatInputController : MonoBehaviour
    {
        [Inject] private IUIService _uiService;

        private void Start()
        {
            this.InjectDependencies();
        }

        private async void ToggleCheatPanel()
        {
            if (_uiService == null) return;

            var cheatPanel = _uiService.GetPanel<CheatPanel>();
            if (cheatPanel != null)
            {
                if (cheatPanel.IsVisible)
                {
                    await _uiService.HidePanelAsync<CheatPanel>();
                }
                else
                {
                    await _uiService.ShowPanelAsync<CheatPanel>();
                }
            }
        }
    }
}
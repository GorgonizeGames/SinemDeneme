using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Interfaces;
using Game.Runtime.Core.Extensions;

namespace Game.Runtime.UI.Core
{
    public class UIService : MonoBehaviour, IUIService
    {
        [Header("UI Service Settings")]
        [SerializeField] private Canvas[] uiCanvases;
        [SerializeField] private Camera uiCamera;

        [Inject] private IGameManager _gameManager;

        private readonly Dictionary<string, IUIPanel> _registeredPanels = new Dictionary<string, IUIPanel>();
        private readonly Dictionary<System.Type, IUIPanel> _panelsByType = new Dictionary<System.Type, IUIPanel>();
        private readonly List<IUIPanel> _visiblePanels = new List<IUIPanel>();

        private void Awake()
        {
            this.InjectDependencies();
            InitializeCanvases();
        }

        private void InitializeCanvases()
        {
            if (uiCanvases == null) return;

            foreach (var canvas in uiCanvases)
            {
                if (canvas != null && canvas.worldCamera == null && uiCamera != null)
                {
                    canvas.worldCamera = uiCamera;
                }
            }
        }

        public void RegisterPanel(IUIPanel panel)
        {
            if (panel == null) return;

            var panelId = panel.PanelId;
            var panelType = panel.GetType();

            if (!_registeredPanels.ContainsKey(panelId))
            {
                _registeredPanels[panelId] = panel;
                _panelsByType[panelType] = panel;
            }
        }

        public async Task ShowPanelAsync<T>(UITransition transition = UITransition.Fade) where T : Component, IUIPanel
        {
            if (_panelsByType.TryGetValue(typeof(T), out var panel))
            {
                await ShowPanelInternal(panel, transition);
            }
        }

        public async Task ShowPanelAsync(string panelId, UITransition transition = UITransition.Fade)
        {
            if (_registeredPanels.TryGetValue(panelId, out var panel))
            {
                await ShowPanelInternal(panel, transition);
            }
        }

        public async Task HidePanelAsync<T>(UITransition transition = UITransition.Fade) where T : Component, IUIPanel
        {
            if (_panelsByType.TryGetValue(typeof(T), out var panel))
            {
                await HidePanelInternal(panel, transition);
            }
        }

        public async Task HidePanelAsync(string panelId, UITransition transition = UITransition.Fade)
        {
            if (_registeredPanels.TryGetValue(panelId, out var panel))
            {
                await HidePanelInternal(panel, transition);
            }
        }

        public T GetPanel<T>() where T : Component, IUIPanel
        {
            return _panelsByType.TryGetValue(typeof(T), out var panel) ? panel as T : null;
        }

        private async Task ShowPanelInternal(IUIPanel panel, UITransition transition)
        {
            if (panel?.IsVisible != false) return;

            await panel.ShowAsync(transition);
            if (!_visiblePanels.Contains(panel))
            {
                _visiblePanels.Add(panel);
            }
        }

        private async Task HidePanelInternal(IUIPanel panel, UITransition transition)
        {
            if (panel?.IsVisible != true) return;

            await panel.HideAsync(transition);
            _visiblePanels.Remove(panel);
        }

        public void SetGameState(GameState gameState)
        {
            // This method kept for compatibility but not used in our new system
        }
    }
}
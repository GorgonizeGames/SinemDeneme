using UnityEngine;
using System.Threading.Tasks;
using Game.Runtime.Core.Interfaces;

namespace Game.Runtime.UI.Core
{
    public interface IUIService
    {
        Task ShowPanelAsync<T>(UITransition transition = UITransition.Fade) where T : Component, IUIPanel;
        Task HidePanelAsync<T>(UITransition transition = UITransition.Fade) where T : Component, IUIPanel;
        Task ShowPanelAsync(string panelId, UITransition transition = UITransition.Fade);
        Task HidePanelAsync(string panelId, UITransition transition = UITransition.Fade);
        void RegisterPanel(IUIPanel panel);
        void SetGameState(GameState gameState);
        T GetPanel<T>() where T : Component, IUIPanel;
    }
}

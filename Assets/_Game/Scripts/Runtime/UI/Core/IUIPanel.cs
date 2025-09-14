using UnityEngine;
using System.Threading.Tasks;

namespace Game.Runtime.UI.Core
{
    public interface IUIPanel
    {
        string PanelId { get; }
        bool IsVisible { get; }
        UILayer Layer { get; }
        
        Task ShowAsync(UITransition transition = UITransition.Fade);
        Task HideAsync(UITransition transition = UITransition.Fade);
        void Initialize();
        void Cleanup();
    }
}
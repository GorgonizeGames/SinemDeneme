using UnityEngine;
using UnityEngine.UI;
using Game.Runtime.UI.Core;
using Game.Runtime.UI.Signals;
using Game.Runtime.Core.DI;

namespace Game.Runtime.UI.Panels
{
    public class GameUIPanel : BaseUIPanel
    {
        [Header("Game Controls")]
        [SerializeField] private Joystick joystick;
        [SerializeField] private Button pauseButton;

        public Joystick Joystick => joystick;

        protected override void OnInitialize()
        {
            layer = UILayer.Game;
            
            pauseButton?.onClick.AddListener(OnPauseClicked);
        }

        private void OnPauseClicked()
        {
            Debug.Log("⏸️ Pause button clicked!");
            _uiSignals?.TriggerPauseToggle();
        }
    }
}
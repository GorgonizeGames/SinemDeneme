using Game.Runtime.Core.StateMachine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Interfaces;
using UnityEngine;

namespace Game.Runtime.Game
{
    public class MainMenuState : BaseState<GameStateController>
    {
        private IGameStateEvents _stateEvents;

        public override void OnEnter(GameStateController owner)
        {
            _stateEvents = Dependencies.Container.Resolve<IGameStateEvents>();
            
            Debug.Log("🏠 Entered Main Menu State");
            _stateEvents?.TriggerMainMenuEntered();
        }

        public override void OnExit(GameStateController owner)
        {
            Debug.Log("🏠 Exiting Main Menu State");
            _stateEvents?.TriggerMainMenuExited();
        }
    }

    public class PlayingState : BaseState<GameStateController>
    {
        private IGameStateEvents _stateEvents;

        public override void OnEnter(GameStateController owner)
        {
            _stateEvents = Dependencies.Container.Resolve<IGameStateEvents>();
            
            Debug.Log("🎮 Entered Playing State");
            _stateEvents?.TriggerPlayingEntered();
        }

        public override void OnExit(GameStateController owner)
        {
            Debug.Log("🎮 Exiting Playing State");
            _stateEvents?.TriggerPlayingExited();
        }
    }

    public class PausedState : BaseState<GameStateController>
    {
        private IGameStateEvents _stateEvents;

        public override void OnEnter(GameStateController owner)
        {
            _stateEvents = Dependencies.Container.Resolve<IGameStateEvents>();
            
            Debug.Log("⏸️ Entered Paused State");
            Time.timeScale = 0f;
            _stateEvents?.TriggerPausedEntered();
        }

        public override void OnExit(GameStateController owner)
        {
            Time.timeScale = 1f;
            Debug.Log("⏸️ Exiting Paused State");
            _stateEvents?.TriggerPausedExited();
        }
    }

    public class GameOverState : BaseState<GameStateController>
    {
        private IGameStateEvents _stateEvents;

        public override void OnEnter(GameStateController owner)
        {
            _stateEvents = Dependencies.Container.Resolve<IGameStateEvents>();
            
            Debug.Log("💀 Entered Game Over State");
            _stateEvents?.TriggerGameOverEntered();
        }

        public override void OnExit(GameStateController owner)
        {
            Debug.Log("💀 Exiting Game Over State");
            _stateEvents?.TriggerGameOverExited();
        }
    }
}
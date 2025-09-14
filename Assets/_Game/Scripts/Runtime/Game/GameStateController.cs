using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Interfaces;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.UI.Signals;
using Game.Runtime.Core.Extensions;

namespace Game.Runtime.Game
{
    public class GameStateController : MonoBehaviour
    {
        [Inject] private IEconomyService _economyService;
        [Inject] private IUISignals _uiSignals;
        [Inject] private IGameStateEvents _stateEvents;

        private StateMachine<GameStateController> _gameStateMachine;

        private void Start()
        {
            this.InjectDependencies();
            SetupStateMachine();
            SubscribeToUISignals();
            
            // Start with main menu
            _gameStateMachine.ChangeState<MainMenuState>();
        }

        private void SetupStateMachine()
        {
            _gameStateMachine = new StateMachine<GameStateController>(this);
            _gameStateMachine.AddState(new MainMenuState());
            _gameStateMachine.AddState(new PlayingState());
            _gameStateMachine.AddState(new PausedState());
            _gameStateMachine.AddState(new GameOverState());
        }

        private void SubscribeToUISignals()
        {
            if (_uiSignals == null) return;

            // UI Signals - State transitions
            _uiSignals.OnPlayButtonClicked += () => _gameStateMachine.ChangeState<PlayingState>();
            _uiSignals.OnMainMenuRequested += () => _gameStateMachine.ChangeState<MainMenuState>();
            _uiSignals.OnPauseToggleRequested += TogglePause;
            _uiSignals.OnResumeRequested += () => _gameStateMachine.ChangeState<PlayingState>();
            _uiSignals.OnRestartRequested += () => _gameStateMachine.ChangeState<PlayingState>();
            _uiSignals.OnQuitRequested += QuitGame;

            // Cheat signals - Direct handling (not state related)
            _uiSignals.OnCheatMoneyRequested += HandleCheatMoney;
            _uiSignals.OnCheatClearDataRequested += HandleCheatClearData;
        }

        private void Update()
        {
            _gameStateMachine?.Update();
        }

        private void FixedUpdate()
        {
            _gameStateMachine?.FixedUpdate();
        }

        private void TogglePause()
        {
            var currentState = _gameStateMachine.CurrentState;
            if (currentState is PlayingState)
                _gameStateMachine.ChangeState<PausedState>();
            else if (currentState is PausedState)
                _gameStateMachine.ChangeState<PlayingState>();
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void HandleCheatMoney(int amount)
        {
            if (_economyService != null)
            {
                _economyService.AddMoney(amount);
                // Money animation will be handled by UIStateHandler when it detects economy change
            }
        }

        private void HandleCheatClearData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("💾 Save data cleared via cheat!");
        }

        private void OnDestroy()
        {
            if (_uiSignals != null)
            {
                _uiSignals.OnPlayButtonClicked -= () => _gameStateMachine.ChangeState<PlayingState>();
                _uiSignals.OnMainMenuRequested -= () => _gameStateMachine.ChangeState<MainMenuState>();
                _uiSignals.OnPauseToggleRequested -= TogglePause;
                _uiSignals.OnResumeRequested -= () => _gameStateMachine.ChangeState<PlayingState>();
                _uiSignals.OnRestartRequested -= () => _gameStateMachine.ChangeState<PlayingState>();
                _uiSignals.OnQuitRequested -= QuitGame;
                _uiSignals.OnCheatMoneyRequested -= HandleCheatMoney;
                _uiSignals.OnCheatClearDataRequested -= HandleCheatClearData;
            }

            _gameStateMachine?.Cleanup();
        }
    }
}

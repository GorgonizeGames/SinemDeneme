using System;

namespace Game.Runtime.Game
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public interface IGameManager
    {
        GameState CurrentState { get; }
        void SetState(GameState newState);
        event Action<GameState> OnStateChanged;
    }
}

using System;

namespace Game.Runtime.Core.Interfaces
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

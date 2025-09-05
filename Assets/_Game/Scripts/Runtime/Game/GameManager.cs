using System;
using UnityEngine;

namespace Game.Runtime.Game
{
    public class GameManager : MonoBehaviour, IGameManager
    {
        public GameState CurrentState { get; private set; }

        // 'event' anahtar kelimesi eklendi.
        public event Action<GameState> OnStateChanged;

        public void SetState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;
            Debug.Log($"Game State changed to: {newState}");
            OnStateChanged?.Invoke(newState);
        }
    }
}
using System;
using System.Collections.Generic;

namespace Game.Runtime.Core.StateMachine
{
    public class StateMachine<T>
    {
        private readonly T _owner;
        private IState<T> _currentState;
        private readonly Dictionary<Type, IState<T>> _states = new Dictionary<Type, IState<T>>();

        public IState<T> CurrentState => _currentState;

        public StateMachine(T owner)
        {
            _owner = owner;
        }

        public void AddState(IState<T> state)
        {
            _states[state.GetType()] = state;
        }

        public void Update()
        {
            _currentState?.OnUpdate(_owner);
        }

        public void FixedUpdate()
        {
            _currentState?.OnFixedUpdate(_owner);
        }

        public bool ChangeState<TState>() where TState : IState<T>
        {
            if (!_states.ContainsKey(typeof(TState)))
            {
                UnityEngine.Debug.LogWarning($"State {typeof(TState).Name} not found in state machine");
                return false;
            }

            _currentState?.OnExit(_owner);
            _currentState = _states[typeof(TState)];
            _currentState.OnEnter(_owner);
            return true;
        }

        public void Cleanup()
        {
            _currentState?.OnExit(_owner);
            _currentState = null;
            _states.Clear();
        }
    }

    public interface IState<T>
    {
        void OnEnter(T owner);
        void OnUpdate(T owner);
        void OnFixedUpdate(T owner);
        void OnExit(T owner);
    }

    public abstract class BaseState<T> : IState<T>
    {
        public virtual void OnEnter(T owner) { }
        public virtual void OnUpdate(T owner) { }
        public virtual void OnFixedUpdate(T owner) { }
        public virtual void OnExit(T owner) { }
    }
}
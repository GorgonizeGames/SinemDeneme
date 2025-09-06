using UnityEngine;

namespace Game.Runtime.Character.AI
{
    public interface IAIBehavior
    {
        void Initialize();
        void UpdateBehavior();
        void OnBehaviorStart();
        void OnBehaviorEnd();
        bool IsActive { get; }
    }

    public abstract class BaseAIBehavior : IAIBehavior
    {
        protected AICharacterController controller;
        protected bool isActive = false;

        public bool IsActive => isActive;

        public BaseAIBehavior(AICharacterController aiController)
        {
            controller = aiController;
        }

        public virtual void Initialize()
        {
            OnBehaviorStart();
            isActive = true;
        }

        public abstract void UpdateBehavior();

        public virtual void OnBehaviorStart() { }
        
        public virtual void OnBehaviorEnd() 
        {
            isActive = false;
        }
    }
}
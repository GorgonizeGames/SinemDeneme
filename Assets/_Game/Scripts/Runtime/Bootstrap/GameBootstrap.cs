using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Input;
using Game.Runtime.Game;
using Game.Runtime.Input.Interfaces;

namespace Game.Runtime.Bootstrap
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Scene Service References")]
        [SerializeField] private JoystickInput inputService;
        [SerializeField] private GameManager gameManager;

        private bool _servicesRegistered = false;

        void Awake()
        {
            ValidateReferences();
            RegisterAllServices();
        }

        void Start()
        {
            if (_servicesRegistered)
            {
                StartGame();
            }
        }

        private void ValidateReferences()
        {
            if (inputService == null)
            {
                Debug.LogError("[GameBootstrap] Input Service is not assigned!", this);
            }

            if (gameManager == null)
            {
                Debug.LogError("[GameBootstrap] Game Manager is not assigned!", this);
            }
        }

        private void RegisterAllServices()
        {
            if (inputService == null || gameManager == null)
            {
                Debug.LogError("[GameBootstrap] Cannot register services - missing references!");
                return;
            }

            Dependencies.Container.Register<IInputService>(inputService);
            Dependencies.Container.Register<IGameManager>(gameManager);

            _servicesRegistered = true;
            Debug.Log("🚀 All services registered!");
        }

        private void StartGame()
        {
            if (gameManager != null)
            {
                gameManager.SetState(GameState.Playing);
                Debug.Log("🎮 Game started!");
            }
        }
    }
}
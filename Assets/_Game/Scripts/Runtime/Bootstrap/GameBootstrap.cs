using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Interfaces;
using Game.Runtime.Input;
using Game.Runtime.Game;
using Game.Runtime.Economy;
using Game.Runtime.Items;
using Game.Runtime.Items.Services;

namespace Game.Runtime.Bootstrap
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Core Services")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private JoystickInput inputService;

        [Header("Game Services")]
        [SerializeField] private EconomyService economyService;
        [SerializeField] private ItemPoolService itemPoolService;

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
            if (gameManager == null)
                Debug.LogError("[GameBootstrap] GameManager is not assigned!", this);

            if (inputService == null)
                Debug.LogError("[GameBootstrap] InputService is not assigned!", this);

            if (economyService == null)
                Debug.LogError("[GameBootstrap] EconomyService is not assigned!", this);

            if (itemPoolService == null)
                Debug.LogError("[GameBootstrap] ItemPoolService is not assigned!", this);
        }

        private void RegisterAllServices()
        {
            // Core services
            if (gameManager != null)
                Dependencies.Container.Register<IGameManager>(gameManager);

            if (inputService != null)
                Dependencies.Container.Register<IInputService>(inputService);

            // Game services
            if (economyService != null)
                Dependencies.Container.Register<IEconomyService>(economyService);

            if (itemPoolService != null)
                Dependencies.Container.Register<IItemPoolService>(itemPoolService);

            _servicesRegistered = true;
            Debug.Log("🚀 All services registered successfully!");
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
using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Interfaces;
using Game.Runtime.Input;
using Game.Runtime.Game;
using Game.Runtime.Economy;
using Game.Runtime.Items.Services;
using Game.Runtime.UI.Core;
using Game.Runtime.UI.Signals;

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

        [Header("UI Services")]
        [SerializeField] private UIService uiService;

        [Header("Controllers")]
        [SerializeField] private GameStateController gameStateController;

        [Header("UI Handlers - All Event-Driven")]
        [SerializeField] private UI.UIStateHandler uiStateHandler;
        [SerializeField] private UI.EconomyUIHandler economyUIHandler; // ← YENİ!
        [SerializeField] private UI.CheatUIHandler cheatUIHandler;
        [SerializeField] private UI.SettingsUIHandler settingsUIHandler;

        [Header("Input Controllers")]
        [SerializeField] private UI.Input.CheatInputController cheatInputController;

        void Awake()
        {
            ValidateReferences();
            RegisterAllServices();
        }

        void Start()
        {
            Debug.Log("🚀 Complete Event-Driven System Started!");
        }

        private void ValidateReferences()
        {
            // Core validations
            if (gameManager == null) Debug.LogError("[GameBootstrap] GameManager missing!");
            if (inputService == null) Debug.LogError("[GameBootstrap] InputService missing!");
            if (economyService == null) Debug.LogError("[GameBootstrap] EconomyService missing!");
            if (itemPoolService == null) Debug.LogError("[GameBootstrap] ItemPoolService missing!");
            if (uiService == null) Debug.LogError("[GameBootstrap] UIService missing!");
            if (gameStateController == null) Debug.LogError("[GameBootstrap] GameStateController missing!");
            
            // Handler validations
            if (uiStateHandler == null) Debug.LogError("[GameBootstrap] UIStateHandler missing!");
            if (economyUIHandler == null) Debug.LogError("[GameBootstrap] EconomyUIHandler missing!");
            if (cheatUIHandler == null) Debug.LogError("[GameBootstrap] CheatUIHandler missing!");
            if (settingsUIHandler == null) Debug.LogError("[GameBootstrap] SettingsUIHandler missing!");
            if (cheatInputController == null) Debug.LogError("[GameBootstrap] CheatInputController missing!");
        }

        private void RegisterAllServices()
        {
            // Core services
            Dependencies.Container.Register<IGameManager>(gameManager);
            Dependencies.Container.Register<IInputService>(inputService);
            Dependencies.Container.Register<IEconomyService>(economyService);
            Dependencies.Container.Register<IItemPoolService>(itemPoolService);

            // UI services
            Dependencies.Container.Register<IUIService>(uiService);
            
            // Signal services
            var uiSignals = new UISignals();
            Dependencies.Container.Register<IUISignals>(uiSignals);
            
            // Game state events
            var gameStateEvents = new GameStateEvents();
            Dependencies.Container.Register<IGameStateEvents>(gameStateEvents);

            Debug.Log("🚀 Complete Event-Driven Architecture registered!");
            Debug.Log("📊 Total Services: " + Dependencies.Container.ServiceCount);
        }

#if UNITY_EDITOR
        [ContextMenu("Validate Event-Driven Architecture")]
        private void ValidateArchitecture()
        {
            Debug.Log("🔍 Event-Driven Architecture Validation:");
            
            bool allValid = true;
            
            // Check all required services
            allValid &= LogServiceStatus<IGameManager>("GameManager");
            allValid &= LogServiceStatus<IEconomyService>("EconomyService");
            allValid &= LogServiceStatus<IUIService>("UIService");
            allValid &= LogServiceStatus<IUISignals>("UISignals");
            allValid &= LogServiceStatus<IGameStateEvents>("GameStateEvents");
            
            // Check all handlers are assigned
            allValid &= LogHandlerStatus("UIStateHandler", uiStateHandler);
            allValid &= LogHandlerStatus("EconomyUIHandler", economyUIHandler);
            allValid &= LogHandlerStatus("CheatUIHandler", cheatUIHandler);
            allValid &= LogHandlerStatus("SettingsUIHandler", settingsUIHandler);
            
            if (allValid)
                Debug.Log("✅ Event-Driven Architecture is complete!");
            else
                Debug.LogError("❌ Some components are missing!");
        }
        
        private bool LogServiceStatus<T>(string serviceName) where T : class
        {
            bool isRegistered = Dependencies.Container.IsRegistered<T>();
            Debug.Log($"  - {serviceName}: {(isRegistered ? "✅" : "❌")}");
            return isRegistered;
        }
        
        private bool LogHandlerStatus(string handlerName, MonoBehaviour handler)
        {
            bool isAssigned = handler != null;
            Debug.Log($"  - {handlerName}: {(isAssigned ? "✅" : "❌")}");
            return isAssigned;
        }
#endif
    }
}
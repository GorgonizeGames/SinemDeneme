using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Input;
using Game.Runtime.Game;

namespace Game.Runtime.Bootstrap
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Scene Service References")]
        [SerializeField] private JoystickInput inputService;
        [SerializeField] private GameManager gameManager;

        void Awake()
        {
            RegisterAllServices();
        }

        void Start()
        {
            StartGame();
        }

        private void RegisterAllServices()
        {
            Dependencies.Container.Register<IInputService>(inputService);
            Dependencies.Container.Register<IGameManager>(gameManager);

            Debug.Log("🚀 All services registered!");
        }
        
         private void StartGame()
        {
            gameManager.SetState(GameState.Playing);
            Debug.Log("🎮 Game started!");
        }
    }
}
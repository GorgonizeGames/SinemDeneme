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
            // Servisleri DI Container'a kaydet
            Dependencies.Container.Register<IInputService>(inputService);
            Dependencies.Container.Register<IGameManager>(gameManager);
            
            Debug.Log("🚀 Game Bootstrap Completed! Services are registered.");
        }

        void Start()
        {
            // Oyunu başlat
            gameManager.SetState(GameState.Playing);
        }
    }
}
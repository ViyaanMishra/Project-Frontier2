using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frontier.Core
{
    /// <summary>
    /// Composition root for the entire game application.
    /// Manages dependency injection container and service locator pattern.
    /// </summary>
    public class GameSession : MonoBehaviour
    {
        private static GameSession _instance;
        public static GameSession Instance => _instance;

        private ServiceRegistry _serviceRegistry;
        private EventBus _eventBus;
        private TimeController _timeController;
        private GameState _gameState;

        public ServiceRegistry Services => _serviceRegistry;
        public EventBus Events => _eventBus;
        public TimeController Time => _timeController;
        public GameState State => _gameState;

        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        public void Initialize()
        {
            if (IsInitialized) return;

            // Initialize core services in dependency order
            _eventBus = new EventBus();
            _serviceRegistry = new ServiceRegistry();
            _timeController = new TimeController();
            _gameState = new GameState();

            // Register core services
            _serviceRegistry.Register<EventBus>(_eventBus);
            _serviceRegistry.Register<TimeController>(_timeController);
            _serviceRegistry.Register<GameState>(_gameState);

            // Initialize time controller
            _timeController.Initialize();

            // Set initial state
            _gameState.SetState(GameState.GameStateType.MainMenu);

            IsInitialized = true;
            Debug.Log("[GameSession] Initialized successfully");
        }

        private void Update()
        {
            if (!IsInitialized) return;

            _timeController.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (!IsInitialized) return;

            _timeController.FixedUpdate();
        }

        private void OnApplicationQuit()
        {
            _serviceRegistry?.Dispose();
            _eventBus?.ClearAllListeners();
            _instance = null;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!IsInitialized) return;

            if (pauseStatus && _gameState.CurrentState == GameState.GameStateType.Playing)
            {
                _gameState.SetState(GameState.GameStateType.Paused);
            }
            else if (!pauseStatus && _gameState.CurrentState == GameState.GameStateType.Paused)
            {
                _gameState.SetState(GameState.GameStateType.Playing);
            }
        }
    }
}

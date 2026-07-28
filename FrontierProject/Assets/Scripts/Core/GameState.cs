using System;
using UnityEngine;

namespace Frontier.Core
{
    /// <summary>
    /// Global state machine for game flow control.
    /// Handles transitions between MainMenu, Playing, Paused, and Debug states.
    /// </summary>
    public class GameState
    {
        public enum GameStateType
        {
            None,
            MainMenu,
            Loading,
            Playing,
            Paused,
            Debug,
            GameOver
        }

        private GameStateType _currentState;
        private GameStateType _previousState;
        private float _stateEnterTime;
        private bool _isTransitioning;

        public GameStateType CurrentState => _currentState;
        public GameStateType PreviousState => _previousState;
        public float StateDuration => Time.time - _stateEnterTime;
        public bool IsPlaying => _currentState == GameStateType.Playing;
        public bool IsPaused => _currentState == GameStateType.Paused;
        public bool IsInMainMenu => _currentState == GameStateType.MainMenu;
        public bool IsTransitioning => _isTransitioning;

        public event Action<GameStateType, GameStateType> OnStateChanged;

        public GameState()
        {
            _currentState = GameStateType.None;
            _previousState = GameStateType.None;
            _stateEnterTime = 0f;
            _isTransitioning = false;
        }

        /// <summary>
        /// Set the current game state with optional transition.
        /// </summary>
        public void SetState(GameStateType newState, bool force = false)
        {
            if (_isTransitioning && !force)
            {
                Debug.LogWarning($"[GameState] Cannot change state while transitioning from {_currentState} to {newState}");
                return;
            }

            if (_currentState == newState && !force)
            {
                return;
            }

            // Validate state transitions
            if (!IsValidTransition(_currentState, newState))
            {
                Debug.LogError($"[GameState] Invalid state transition: {_currentState} -> {newState}");
                return;
            }

            _previousState = _currentState;
            _currentState = newState;
            _stateEnterTime = Time.time;
            _isTransitioning = false;

            OnStateChanged?.Invoke(_previousState, _currentState);

            Debug.Log($"[GameState] Transitioned from {_previousState} to {_currentState}");

            // Handle state-specific behavior
            OnEnterState(newState);
        }

        /// <summary>
        /// Begin an asynchronous state transition (for loading screens).
        /// </summary>
        public void BeginTransition(GameStateType newState)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[GameState] Already transitioning");
                return;
            }

            if (!IsValidTransition(_currentState, newState))
            {
                Debug.LogError($"[GameState] Invalid state transition: {_currentState} -> {newState}");
                return;
            }

            _previousState = _currentState;
            _isTransitioning = true;
            
            // Fire event for UI to show loading screen
            OnStateChanged?.Invoke(_previousState, newState);
        }

        /// <summary>
        /// Complete an asynchronous state transition.
        /// </summary>
        public void CompleteTransition()
        {
            if (!_isTransitioning)
            {
                Debug.LogWarning("[GameState] Not transitioning");
                return;
            }

            _currentState = _previousState == GameStateType.Loading ? GameStateType.Playing : _previousState;
            _stateEnterTime = Time.time;
            _isTransitioning = false;

            OnStateChanged?.Invoke(GameStateType.Loading, _currentState);
            OnEnterState(_currentState);

            Debug.Log($"[GameState] Transition complete: {_currentState}");
        }

        /// <summary>
        /// Toggle pause state.
        /// </summary>
        public void TogglePause()
        {
            if (_currentState == GameStateType.Playing)
            {
                SetState(GameStateType.Paused);
            }
            else if (_currentState == GameStateType.Paused)
            {
                SetState(GameStateType.Playing);
            }
        }

        /// <summary>
        /// Enter debug mode (slows time, enables overlays).
        /// </summary>
        public void EnterDebugMode()
        {
            if (_currentState == GameStateType.Playing || _currentState == GameStateType.Paused)
            {
                SetState(GameStateType.Debug);
            }
        }

        /// <summary>
        /// Exit debug mode.
        /// </summary>
        public void ExitDebugMode()
        {
            if (_currentState == GameStateType.Debug)
            {
                SetState(_previousState == GameStateType.Debug ? GameStateType.Playing : _previousState);
            }
        }

        /// <summary>
        /// Check if a state transition is valid.
        /// </summary>
        private bool IsValidTransition(GameStateType from, GameStateType to)
        {
            // Define valid transitions
            switch (from)
            {
                case GameStateType.None:
                    return to == GameStateType.MainMenu;
                
                case GameStateType.MainMenu:
                    return to == GameStateType.Loading || to == GameStateType.GameOver;
                
                case GameStateType.Loading:
                    return to == GameStateType.Playing || to == GameStateType.MainMenu;
                
                case GameStateType.Playing:
                    return to == GameStateType.Paused || to == GameStateType.Debug || 
                           to == GameStateType.GameOver || to == GameStateType.MainMenu;
                
                case GameStateType.Paused:
                    return to == GameStateType.Playing || to == GameStateType.Debug || 
                           to == GameStateType.MainMenu;
                
                case GameStateType.Debug:
                    return to == GameStateType.Playing || to == GameStateType.Paused || 
                           to == GameStateType.MainMenu;
                
                case GameStateType.GameOver:
                    return to == GameStateType.MainMenu;
                
                default:
                    return false;
            }
        }

        /// <summary>
        /// Handle state entry behavior.
        /// </summary>
        private void OnEnterState(GameStateType state)
        {
            switch (state)
            {
                case GameStateType.Playing:
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    break;

                case GameStateType.Paused:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;

                case GameStateType.Debug:
                    Time.timeScale = 0.25f; // Slow motion for debugging
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;

                case GameStateType.MainMenu:
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;

                case GameStateType.GameOver:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
            }
        }

        /// <summary>
        /// Serialize state for save system.
        /// </summary>
        public SaveData ToSaveData()
        {
            return new SaveData
            {
                currentState = (int)_currentState,
                previousState = (int)_previousState,
                stateEnterTime = _stateEnterTime
            };
        }

        /// <summary>
        /// Deserialize state from save data.
        /// </summary>
        public void FromSaveData(SaveData data)
        {
            _currentState = (GameStateType)data.currentState;
            _previousState = (GameStateType)data.previousState;
            _stateEnterTime = data.stateEnterTime;
            _isTransitioning = false;
        }

        [Serializable]
        public struct SaveData
        {
            public int currentState;
            public int previousState;
            public float stateEnterTime;
        }
    }
}

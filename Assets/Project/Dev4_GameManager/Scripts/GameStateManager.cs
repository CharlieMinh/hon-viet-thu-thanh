using System;
using UnityEngine;
using HonVietThuThanh.Shared;

namespace HonVietThuThanh.Dev4
{
    /// <summary>Các trạng thái có thể có của game.</summary>
    public enum GameState
    {
        Preparation,
        WaveInProgress,
        WaveComplete,
        Win,
        Lose
    }

    /// <summary>
    /// GameStateManager — máy trạng thái trung tâm của game.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        [Header("Cấu hình")]
        [Tooltip("Số wave cần qua để thắng")]
        [SerializeField] private int totalWaves = 3;

        public GameState CurrentState { get; private set; } = GameState.Preparation;

        /// <summary>Phát mỗi khi trạng thái game thay đổi.</summary>
        public static event Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnWaveStarted += HandleWaveStarted;
            GameEvents.OnWaveCompleted += HandleWaveCompleted;
            BaseHealthManager.OnBaseDestroyed += HandleBaseDestroyed;
        }

        private void OnDisable()
        {
            GameEvents.OnWaveStarted -= HandleWaveStarted;
            GameEvents.OnWaveCompleted -= HandleWaveCompleted;
            BaseHealthManager.OnBaseDestroyed -= HandleBaseDestroyed;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // --- Handlers ---

        private void HandleWaveStarted(int waveIndex)
        {
            if (IsTerminalState())
            {
                Debug.Log($"[GameStateManager] Ignore WaveStarted because game already ended: {CurrentState}");
                return;
            }

            SetState(GameState.WaveInProgress);
        }

        private void HandleWaveCompleted(int waveIndex)
        {
            if (IsTerminalState())
            {
                Debug.Log($"[GameStateManager] Ignore WaveCompleted because game already ended: {CurrentState}");
                return;
            }

            // waveIndex 0-based: wave 1=0, wave 2=1, wave 3=2
            bool isLastWave = waveIndex >= totalWaves - 1;
            SetState(isLastWave ? GameState.Win : GameState.WaveComplete);
        }

        private void HandleBaseDestroyed()
        {
            // Lose có thể xảy ra bất kỳ lúc nào, kể cả giữa wave.
            // Nếu đã Win rồi thì không đổi lại Lose nữa.
            if (CurrentState == GameState.Win)
            {
                Debug.Log("[GameStateManager] Ignore Lose because game already ended with Win.");
                return;
            }

            SetState(GameState.Lose);
        }

        // --- Core ---

        private bool IsTerminalState()
        {
            return CurrentState == GameState.Win || CurrentState == GameState.Lose;
        }

        private void SetState(GameState newState)
        {
            if (CurrentState == newState) return;

            if (IsTerminalState())
            {
                Debug.Log($"[GameStateManager] Ignore state change {CurrentState} -> {newState} because game already ended.");
                return;
            }

            CurrentState = newState;
            Debug.Log($"[GameStateManager] State → {newState}");
            OnGameStateChanged?.Invoke(newState);
        }
#if UNITY_EDITOR
        [ContextMenu("TEST - Set Lose")]
        private void TestSetLose()
        {
            SetState(GameState.Lose);
        }

        [ContextMenu("TEST - Set Win")]
        private void TestSetWin()
        {
            SetState(GameState.Win);
        }

        [ContextMenu("TEST - Wave Completed 0")]
        private void TestWaveCompleted0()
        {
            HandleWaveCompleted(0);
        }

        [ContextMenu("TEST - Wave Completed Last")]
        private void TestWaveCompletedLast()
        {
            HandleWaveCompleted(totalWaves - 1);
        }

        [ContextMenu("TEST - Lose Then Wave Complete Last")]
        private void TestLoseThenWaveCompleteLast()
        {
            HandleBaseDestroyed();
            HandleWaveCompleted(totalWaves - 1);
        }

        [ContextMenu("TEST - Win Then Lose")]
        private void TestWinThenLose()
        {
            HandleWaveCompleted(totalWaves - 1);
            HandleBaseDestroyed();
        }
#endif
    }
}
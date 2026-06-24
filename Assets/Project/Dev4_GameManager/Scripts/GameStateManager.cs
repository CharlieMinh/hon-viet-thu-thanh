using System;
using UnityEngine;
using HonVietThuThanh.Shared;

namespace HonVietThuThanh.Dev4
{
    /// <summary>Các trạng thái có thể có của game.</summary>
    public enum GameState
    {
        Preparation,    // Giai đoạn đặt hero, chưa bắt đầu wave
        WaveInProgress, // Đang trong wave, enemy đang spawn và di chuyển
        WaveComplete,   // Wave vừa xong, chuẩn bị wave tiếp
        Win,            // Qua hết 3 wave → THẮNG
        Lose            // Thành hết máu → THUA
    }

    /// <summary>
    /// GameStateManager — máy trạng thái trung tâm của game.
    ///
    /// LẮNG NGHE:
    ///   GameEvents.OnWaveStarted(waveIndex)   → 0-based index từ Dev2, chuyển sang WaveInProgress
    ///   GameEvents.OnWaveCompleted(waveNumber) → 1-based number từ Dev2 (= waveIndex + 1)
    ///                                            nếu waveNumber >= totalWaves → Win
    ///                                            ngược lại → WaveComplete
    ///   BaseHealthManager.OnBaseDestroyed → chuyển sang Lose
    ///
    /// PHÁT RA:
    ///   OnGameStateChanged(GameState) → UIManager hiện/ẩn panel, button
    ///
    /// CONVENTION đã xác nhận với Dev2:
    ///   OnWaveStarted  → tham số là waveIndex (0-based): wave 1 = 0, wave 2 = 1, wave 3 = 2
    ///   OnWaveCompleted → tham số là waveNumber (1-based): wave 1 = 1, wave 2 = 2, wave 3 = 3
    ///   (Dev2 WaveManager line 225: RaiseWaveCompleted(completedWaveNumber) = completedWaveIndex + 1)
    ///
    /// SETUP trong Inspector:
    ///   - totalWaves = 3
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
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnWaveStarted         += HandleWaveStarted;
            GameEvents.OnWaveCompleted       += HandleWaveCompleted;
            BaseHealthManager.OnBaseDestroyed += HandleBaseDestroyed;
        }

        private void OnDisable()
        {
            GameEvents.OnWaveStarted         -= HandleWaveStarted;
            GameEvents.OnWaveCompleted       -= HandleWaveCompleted;
            BaseHealthManager.OnBaseDestroyed -= HandleBaseDestroyed;
        }

        // --- Handlers ---

        private void HandleWaveStarted(int waveIndex)
        {
            lastStartedWaveIndex = waveIndex;
            SetState(GameState.WaveInProgress);
        }

        private void HandleWaveCompleted(int waveNumber)
        {
            int completedWaveIndex = ResolveCompletedWaveIndex(waveIndex);
            bool isLastWave = (completedWaveIndex >= totalWaves - 1);
            SetState(isLastWave ? GameState.Win : GameState.WaveComplete);
        }

        private void HandleBaseDestroyed()
        {
            SetState(GameState.Lose);
        }

        // --- Core ---

        private void SetState(GameState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;
            Debug.Log($"[GameStateManager] State → {newState}");
            OnGameStateChanged?.Invoke(newState);
        }
    }
}

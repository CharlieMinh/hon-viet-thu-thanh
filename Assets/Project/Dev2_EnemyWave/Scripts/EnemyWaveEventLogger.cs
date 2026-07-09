using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Dev2_EnemyWave
{
    /// <summary>
    /// Logs phase 1 events so the module can be verified in isolation.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyWaveEventLogger : MonoBehaviour
    {
        private void OnEnable()
        {
            GameEvents.OnEnemySpawned += HandleEnemySpawned;
            GameEvents.OnEnemyDied += HandleEnemyDied;
            GameEvents.OnEnemyReachedBase += HandleEnemyReachedBase;
            GameEvents.OnWaveStarted += HandleWaveStarted;
            GameEvents.OnWaveCompleted += HandleWaveCompleted;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemySpawned -= HandleEnemySpawned;
            GameEvents.OnEnemyDied -= HandleEnemyDied;
            GameEvents.OnEnemyReachedBase -= HandleEnemyReachedBase;
            GameEvents.OnWaveStarted -= HandleWaveStarted;
            GameEvents.OnWaveCompleted -= HandleWaveCompleted;
        }

        private void HandleEnemySpawned(EnemyType enemyType, GameObject enemy)
        {
            Debug.Log($"[Dev2] Spawned {enemyType} -> {enemy.name}", enemy);
        }

        private void HandleEnemyDied(GameObject enemy, int goldReward)
        {
            Debug.Log($"[Dev2] Enemy died -> {enemy.name}, gold reward: {goldReward}", enemy);
        }

        private void HandleEnemyReachedBase(GameObject enemy)
        {
            Debug.Log($"[Dev2] Enemy reached base -> {enemy.name}", enemy);
        }

        private void HandleWaveStarted(int waveIndex)
        {
            Debug.Log($"[Dev2] Wave started -> {waveIndex}", this);
        }

        private void HandleWaveCompleted(int waveIndex)
        {
            Debug.Log($"[Dev2] Wave completed -> {waveIndex}", this);
        }
    }
}

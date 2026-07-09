using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HonVietThuThanh.Dev2_EnemyWave
{
    /// <summary>
    /// Debug controls for the Dev2 prototype scene.
    /// Supports both New Input System and Legacy Input Manager via compile guards.
    /// This script is for testing/debug only — production wave flow should use
    /// GameEvents.OnWaveStartRequested from Dev4 UI.
    /// </summary>
    /// <remarks>
    /// Known limitation (Minor): With the New Input System, debug keys only respond
    /// when the Game View window has OS-level keyboard focus. Clicking Inspector,
    /// Console, or Scene View during Play Mode causes keys to stop responding until
    /// the Game View is re-focused (click on it or Alt+Tab). This is standard Unity
    /// Editor behavior, not a bug in this script. It does not affect production flow
    /// since production waves are triggered via GameEvents, not keyboard input.
    /// </remarks>
    [DisallowMultipleComponent]
    public class EnemyWaveDebugInput : MonoBehaviour
    {
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private float debugDamageAmount = 5f;
        [SerializeField] private bool restartPrototypeOnReset = true;

        private void Awake()
        {
            if (enemySpawner == null)
            {
                enemySpawner = GetComponentInParent<EnemySpawner>();
            }

            if (waveManager == null)
            {
                waveManager = GetComponentInParent<WaveManager>();
            }
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            UpdateNewInputSystem();
#elif ENABLE_LEGACY_INPUT_MANAGER
            UpdateLegacyInput();
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private void UpdateNewInputSystem()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            var keyboard = Keyboard.current;

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                Debug.Log("[Dev2 Debug] Spawning single enemy...");
                enemySpawner?.SpawnSingleEnemy();
            }

            if (keyboard.kKey.wasPressedThisFrame)
            {
                Debug.Log("[Dev2 Debug] Applying debug damage to first alive enemy...");
                enemySpawner?.DamageFirstAliveEnemy(debugDamageAmount);
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                Debug.Log("[Dev2 Debug] Resetting prototype...");
                enemySpawner?.ResetPrototype();

                if (restartPrototypeOnReset)
                {
                    enemySpawner?.StartPrototype();
                }
            }

            if (keyboard.nKey.wasPressedThisFrame)
            {
                if (waveManager != null && !waveManager.HasMoreWaves && !waveManager.IsWaveActive)
                {
                    Debug.Log("[Dev2 Debug] No more waves to start. Reset/restart controls remain available.");
                    return;
                }

                Debug.Log("[Dev2 Debug] Manually starting next wave...");
                waveManager?.StartNextWaveManually();
            }
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        private void UpdateLegacyInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("[Dev2 Debug] Spawning single enemy...");
                enemySpawner?.SpawnSingleEnemy();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                Debug.Log("[Dev2 Debug] Applying debug damage to first alive enemy...");
                enemySpawner?.DamageFirstAliveEnemy(debugDamageAmount);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("[Dev2 Debug] Resetting prototype...");
                enemySpawner?.ResetPrototype();

                if (restartPrototypeOnReset)
                {
                    enemySpawner?.StartPrototype();
                }
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                if (waveManager != null && !waveManager.HasMoreWaves && !waveManager.IsWaveActive)
                {
                    Debug.Log("[Dev2 Debug] No more waves to start. Reset/restart controls remain available.");
                    return;
                }

                Debug.Log("[Dev2 Debug] Manually starting next wave...");
                waveManager?.StartNextWaveManually();
            }
        }
#endif
    }
}

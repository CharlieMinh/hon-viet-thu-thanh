using UnityEngine;
using UnityEngine.InputSystem;

namespace HonVietThuThanh.Dev2_EnemyWave
{
    /// <summary>
    /// Debug controls for the Dev2 prototype scene.
    /// </summary>
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
            // Robust check for New Input System
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
                Debug.Log("[Dev2 Debug] Manually starting next wave...");
                waveManager?.StartNextWaveManually();
            }
        }
    }
}

using UnityEngine;

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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                enemySpawner?.SpawnSingleEnemy();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                enemySpawner?.DamageFirstAliveEnemy(debugDamageAmount);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                enemySpawner?.ResetPrototype();

                if (restartPrototypeOnReset)
                {
                    enemySpawner?.StartPrototype();
                }
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                waveManager?.StartNextWaveManually();
            }
        }
    }
}

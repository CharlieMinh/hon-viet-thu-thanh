using System;
using UnityEngine;

namespace HonVietThuThanh.Shared
{
    /// <summary>
    /// Central event hub for Phase 1 module communication. Dev1, Dev2, and
    /// Dev3 raise gameplay events here; Dev4 listens to update game state and
    /// logs; Dev5 may listen for integration and debugging without owning
    /// gameplay logic.
    /// </summary>
    public static class GameEvents
    {
        /// <summary>
        /// Raised by Dev1 after a hero is successfully placed on the grid.
        /// Dev4 listens to update placement logs, rules, and future economy.
        /// </summary>
        public static event Action<HeroType, Vector2Int> OnHeroPlaced;

        /// <summary>
        /// Raised by Dev2 after an enemy is spawned. Dev3 may listen for combat
        /// awareness, while Dev4 and Dev5 may use it for logs and integration.
        /// </summary>
        public static event Action<EnemyType, GameObject> OnEnemySpawned;

        /// <summary>
        /// Raised by Dev2 when an enemy dies. Dev4 listens to update gold,
        /// rewards, logs, and future wave status.
        /// </summary>
        public static event Action<GameObject, int> OnEnemyDied;

        /// <summary>
        /// Raised by Dev2 when an enemy reaches the base. Dev4 listens to
        /// subtract base HP and update logs.
        /// </summary>
        public static event Action<GameObject> OnEnemyReachedBase;

        /// <summary>
        /// Raised by Dev2 when a wave starts. Dev4 listens to update wave
        /// status and UI/log output.
        /// </summary>
        public static event Action<int> OnWaveStarted;

        /// <summary>
        /// Raised by Dev2 when a wave is completed. Dev4 listens to update
        /// wave status and future win-condition checks.
        /// </summary>
        public static event Action<int> OnWaveCompleted;

        /// <summary>
        /// Raised by Dev3 when a hero attacks a target. Dev4 and Dev5 may
        /// listen for logs, UI feedback, or integration debugging.
        /// </summary>
        public static event Action<HeroType, GameObject> OnHeroAttacked;

        /// <summary>
        /// Raises the hero placed event after Dev1 validates placement.
        /// </summary>
        /// <param name="heroType">The type of hero that was placed.</param>
        /// <param name="gridPosition">The grid coordinate where the hero was placed.</param>
        public static void RaiseHeroPlaced(HeroType heroType, Vector2Int gridPosition)
        {
            OnHeroPlaced?.Invoke(heroType, gridPosition);
        }

        /// <summary>
        /// Raises the enemy spawned event after Dev2 creates an enemy instance.
        /// </summary>
        /// <param name="enemyType">The type of enemy that was spawned.</param>
        /// <param name="enemy">The spawned enemy GameObject.</param>
        public static void RaiseEnemySpawned(EnemyType enemyType, GameObject enemy)
        {
            OnEnemySpawned?.Invoke(enemyType, enemy);
        }

        /// <summary>
        /// Raises the enemy died event after Dev2 confirms an enemy has died.
        /// </summary>
        /// <param name="enemy">The enemy GameObject that died.</param>
        /// <param name="goldReward">The reward value Dev4 may add to gold.</param>
        public static void RaiseEnemyDied(GameObject enemy, int goldReward)
        {
            OnEnemyDied?.Invoke(enemy, goldReward);
        }

        /// <summary>
        /// Raises the enemy reached base event after Dev2 confirms an enemy
        /// reached the lane end or base trigger.
        /// </summary>
        /// <param name="enemy">The enemy GameObject that reached the base.</param>
        public static void RaiseEnemyReachedBase(GameObject enemy)
        {
            OnEnemyReachedBase?.Invoke(enemy);
        }

        /// <summary>
        /// Raises the wave started event after Dev2 starts spawning a wave.
        /// </summary>
        /// <param name="waveIndex">The zero-based or team-agreed wave index.</param>
        public static void RaiseWaveStarted(int waveIndex)
        {
            OnWaveStarted?.Invoke(waveIndex);
        }

        /// <summary>
        /// Raises the wave completed event after Dev2 confirms a wave has
        /// finished.
        /// </summary>
        /// <param name="waveIndex">The zero-based or team-agreed wave index.</param>
        public static void RaiseWaveCompleted(int waveIndex)
        {
            OnWaveCompleted?.Invoke(waveIndex);
        }

        /// <summary>
        /// Raises the hero attacked event when Dev3 confirms a hero has
        /// attacked a target.
        /// </summary>
        /// <param name="heroType">The type of hero that attacked.</param>
        /// <param name="target">The target GameObject that was attacked.</param>
        public static void RaiseHeroAttacked(HeroType heroType, GameObject target)
        {
            OnHeroAttacked?.Invoke(heroType, target);
        }
    }
}

using UnityEngine;

namespace HonVietThuThanh.Dev2_EnemyWave
{
    /// <summary>
    /// Keeps enemy spawn initialization in one place so pool/spawner logic
    /// stays small and easy to hand off.
    /// </summary>
    public static class EnemyBootstrapper
    {
        public static Enemy PrepareEnemy(
            Enemy enemy,
            EnemyData data,
            LanePath lanePath,
            EnemyPool pool,
            EnemySpawner spawner,
            Transform activeParent,
            bool shouldCountTowardWave)
        {
            if (enemy == null)
            {
                return null;
            }

            if (activeParent != null)
            {
                enemy.transform.SetParent(activeParent, false);
            }

            enemy.Initialize(data, lanePath, pool, spawner, shouldCountTowardWave);
            return enemy;
        }
    }
}

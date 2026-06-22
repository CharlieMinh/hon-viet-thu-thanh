using System.Collections;
using HonVietThuThanh.Dev2_EnemyWave;
using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Combat
{
    public class Dev3CombatPreviewBootstrap : MonoBehaviour
    {
        [SerializeField] private bool createPreviewSetup = true;
        [SerializeField] private bool logCombatEvents = true;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject dev2EnemyPrefab;

        private const string PreviewRootName = "Dev3_CombatPreview_Runtime";
        private const float ThanhGiongTargetInitialHp = 20f;

        private GameObject obstaclePrefab;
        private EnemyStub thanhGiongTarget;
        private Enemy dev2Target;
        private float dev2TargetInitialHp;
        private int heroAttackEventCount;
        private int projectileHitCleanupCount;
        private int projectileTimeoutCleanupCount;

        private void OnEnable()
        {
            if (logCombatEvents)
            {
                GameEvents.OnHeroAttacked += HandleHeroAttacked;
            }

            Projectile.OnProjectileCleanedUp += HandleProjectileCleanedUp;
        }

        private void OnDisable()
        {
            GameEvents.OnHeroAttacked -= HandleHeroAttacked;
            Projectile.OnProjectileCleanedUp -= HandleProjectileCleanedUp;
        }

        private void Start()
        {
            if (!createPreviewSetup)
            {
                return;
            }

            if (projectilePrefab == null)
            {
                Debug.LogError("[Dev3 Combat] Preview requires the stable projectile prefab asset.", this);
                return;
            }

            if (FindAnyObjectByType<HeroBase>() != null)
            {
                Debug.Log("[Dev3 Combat] Preview setup skipped because a combat hero already exists in the scene.", this);
                return;
            }

            GameObject root = new GameObject(PreviewRootName);
            obstaclePrefab = CreateObstaclePrefab(root.transform);

            CreateThanhGiongLane(root.transform);
            CreateSonTinhLane(root.transform);
            CreateChuDongTuLane(root.transform);
            CreateOutOfRangeEnemy(root.transform);
            CreateDev2IntegrationLane(root.transform);
            CreateProjectileTimeoutProbe(root.transform);
            StartCoroutine(ReportPreviewResult());

            Debug.Log("[Dev3 Combat] Preview setup spawned test heroes, enemies, and cleanup probes.", this);
        }

        private void HandleHeroAttacked(HeroType heroType, GameObject target)
        {
            heroAttackEventCount++;
            string targetName = target != null ? target.name : "target";
            Debug.Log($"[Dev3 Combat] OnHeroAttacked confirmed: {heroType} acted on {targetName}.", this);
        }

        private void HandleProjectileCleanedUp(Projectile.CleanupReason reason)
        {
            if (reason == Projectile.CleanupReason.Hit)
            {
                projectileHitCleanupCount++;
            }
            else if (reason == Projectile.CleanupReason.Timeout)
            {
                projectileTimeoutCleanupCount++;
            }
        }

        private void CreateThanhGiongLane(Transform parent)
        {
            GameObject hero = CreateHeroShell("Dev3_TestHero_ThanhGiong", new Vector3(-4f, 1f, 0f), parent);
            var attack = hero.AddComponent<ThanhGiong_Attack>();
            attack.attackDamage = 10f;
            attack.attackRange = 5f;
            attack.attackSpeed = 2f;
            attack.projectilePrefab = projectilePrefab;

            thanhGiongTarget = CreateEnemy("Dev3_TestEnemy_ThanhGiong_Target", new Vector3(-4f, 0.5f, 3f), ThanhGiongTargetInitialHp, parent);
        }

        private void CreateSonTinhLane(Transform parent)
        {
            GameObject hero = CreateHeroShell("Dev3_TestHero_SonTinh", new Vector3(0f, 1f, 0f), parent);
            var attack = hero.AddComponent<SonTinh_Attack>();
            attack.attackDamage = 10f;
            attack.attackRange = 5f;
            attack.attackSpeed = 1f;
            attack.obstaclePrefab = obstaclePrefab;

            CreateEnemy("Dev3_TestEnemy_SonTinh_Target", new Vector3(0f, 0.5f, 3f), 40f, parent);
        }

        private void CreateChuDongTuLane(Transform parent)
        {
            GameObject hero = CreateHeroShell("Dev3_TestHero_ChuDongTu", new Vector3(4f, 1f, 0f), parent);
            var attack = hero.AddComponent<ChuDongTu_Attack>();
            attack.attackDamage = 8f;
            attack.attackRange = 5f;
            attack.attackSpeed = 1f;

            GameObject ally = CreateHeroShell("Dev3_TestHero_ChuDongTu_HealTarget", new Vector3(5.5f, 1f, 0f), parent);
            var allyAttack = ally.AddComponent<ThanhGiong_Attack>();
            allyAttack.attackDamage = 1f;
            allyAttack.attackRange = 1f;
            allyAttack.attackSpeed = 0.25f;
            allyAttack.projectilePrefab = projectilePrefab;
            allyAttack.TakeDamage(20f);

            CreateEnemy("Dev3_TestEnemy_ChuDongTu_Target", new Vector3(4f, 0.5f, 3f), 35f, parent);
        }

        private static GameObject CreateHeroShell(string name, Vector3 position, Transform parent)
        {
            GameObject hero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hero.name = name;
            hero.transform.SetParent(parent);
            hero.transform.position = position;
            return hero;
        }

        private static EnemyStub CreateEnemy(string name, Vector3 position, float hp, Transform parent)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.name = name;
            enemy.transform.SetParent(parent);
            enemy.transform.position = position;
            enemy.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            var stub = enemy.AddComponent<EnemyStub>();
            stub.hp = hp;
            stub.speed = 0f;
            return stub;
        }

        private static void CreateOutOfRangeEnemy(Transform parent)
        {
            CreateEnemy("Dev3_OutOfRangeEnemy_DoesNotAttackUntilMoved", new Vector3(-4f, 0.5f, 8f), 50f, parent);
        }

        private void CreateDev2IntegrationLane(Transform parent)
        {
            if (dev2EnemyPrefab == null)
            {
                Debug.LogWarning("[Dev3 Combat] Dev2 enemy prefab is not assigned; integration probe skipped.", this);
                return;
            }

            GameObject hero = CreateHeroShell("Dev3_TestHero_Dev2Integration", new Vector3(8f, 1f, 0f), parent);
            var attack = hero.AddComponent<ThanhGiong_Attack>();
            attack.attackDamage = 10f;
            attack.attackRange = 5f;
            attack.attackSpeed = 2f;
            attack.projectilePrefab = projectilePrefab;

            GameObject enemy = Instantiate(dev2EnemyPrefab, new Vector3(8f, 0.5f, 3f), Quaternion.identity, parent);
            enemy.name = "Dev3_RealDev2Enemy_IntegrationProbe";
            dev2Target = enemy.GetComponent<Enemy>();
            if (dev2Target != null)
            {
                dev2TargetInitialHp = dev2Target.CurrentHealth;
            }
        }

        private void CreateProjectileTimeoutProbe(Transform parent)
        {
            GameObject probeObject = Instantiate(projectilePrefab, new Vector3(0f, 20f, 0f), Quaternion.identity, parent);
            probeObject.name = "Dev3_ProjectileTimeoutProbe";
            Projectile probe = probeObject.GetComponent<Projectile>();
            if (probe == null)
            {
                Debug.LogError("[Dev3 Combat] Stable projectile prefab is missing its Projectile component.", probeObject);
                Destroy(probeObject);
                return;
            }

            probe.Init(Vector3.right, 0f, false);
            probeObject.SetActive(true);
        }

        private static GameObject CreateObstaclePrefab(Transform parent)
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = "Dev3_RuntimeObstaclePrefab";
            obstacle.transform.SetParent(parent);
            obstacle.transform.position = new Vector3(2f, -50f, 0f);
            obstacle.transform.localScale = Vector3.one;
            obstacle.AddComponent<Obstacle>();
            obstacle.SetActive(false);
            return obstacle;
        }

        private IEnumerator ReportPreviewResult()
        {
            yield return new WaitForSeconds(3f);

            bool attackEventConfirmed = heroAttackEventCount > 0;
            bool damageConfirmed = thanhGiongTarget == null || thanhGiongTarget.CurrentHp < ThanhGiongTargetInitialHp;
            bool deathConfirmed = thanhGiongTarget == null;
            bool hitCleanupConfirmed = projectileHitCleanupCount > 0;
            bool timeoutCleanupConfirmed = projectileTimeoutCleanupCount > 0;
            bool dev2DamageConfirmed = dev2Target != null &&
                (!dev2Target.IsAlive() || dev2Target.CurrentHealth < dev2TargetInitialHp);

            Debug.Log(
                $"[Dev3 Combat] Preview check: attackEvent={(attackEventConfirmed ? "PASS" : "FAIL")}, " +
                $"damage={(damageConfirmed ? "PASS" : "FAIL")}, " +
                $"enemyDeath={(deathConfirmed ? "PASS" : "FAIL")}, " +
                $"projectileHitCleanup={(hitCleanupConfirmed ? "PASS" : "FAIL")}, " +
                $"projectileTimeoutCleanup={(timeoutCleanupConfirmed ? "PASS" : "FAIL")}, " +
                $"realDev2EnemyDamage={(dev2DamageConfirmed ? "PASS" : "FAIL")}.",
                this);
        }
    }
}

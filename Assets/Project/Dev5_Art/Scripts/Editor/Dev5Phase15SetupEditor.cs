#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor script tự động thiết lập Phase 15: Tạo đa dạng kẻ địch (Goblin, Orc, Enemy Archer) và thiết lập Wave Composition.
    /// Tự động chạy khi mở Unity / Domain Reload hoặc chạy thủ công qua thanh menu: Dev5 / Setup Phase 15 - Enemy Variety and Waves
    /// </summary>
    // [InitializeOnLoad]
    public static class Dev5Phase15SetupEditor
    {
        private const string SCENE_REQUIRED = "Scene_Dev5_Art";
        private const string BASE_ENEMY_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/Enemy_Test_Prefab.prefab";
        private const string GOBLIN_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/Goblin_Enemy_Prefab.prefab";
        private const string ORC_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/Orc_Enemy_Prefab.prefab";
        private const string ARCHER_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/EnemyArcher_Prefab.prefab";
        
        private const string ENEMY_PROJ_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Projectiles/EnemyProjectile_Prefab.prefab";
        private const string ENEMY_PROJ_MAT_PATH = "Assets/Project/Dev5_Art/Prefabs/Projectiles/EnemyProjectile_Material.mat";
        
        private const string GOBLIN_MAT_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/Goblin_Material.mat";
        private const string ORC_MAT_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/Orc_Material.mat";
        private const string ARCHER_MAT_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/EnemyArcher_Material.mat";

        static Dev5Phase15SetupEditor()
        {
            // EditorApplication.delayCall += RunSetupOnLoad;
        }

        private static void RunSetupOnLoad()
        {
            // Kiểm tra xem đã thiết lập prefab Goblin chưa, nếu chưa tự động chạy setup
            GameObject goblin = AssetDatabase.LoadAssetAtPath<GameObject>(GOBLIN_PREFAB_PATH);
            if (goblin == null)
            {
                Debug.Log("[Phase15Setup] Phát hiện chưa thiết lập Phase 15. Bắt đầu cấu hình tự động...");
                SetupPhase15();
            }
        }

        [MenuItem("Dev5/Setup Phase 15 - Enemy Variety and Waves")]
        public static void SetupPhase15()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.name.Equals(SCENE_REQUIRED, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[Phase15Setup] Đang chạy trên scene '{activeScene.name}' thay vì '{SCENE_REQUIRED}'.");
            }

            Undo.SetCurrentGroupName("Phase 15 Setup");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo Red Projectile cho Enemy Archer nếu chưa có
            GameObject enemyProjPrefab = CreateEnemyProjectilePrefab();
            if (enemyProjPrefab == null)
            {
                Debug.LogError("[Phase15Setup] Không thể tạo Enemy Projectile Prefab!");
                return;
            }

            // 2. Tạo Goblin Prefab
            GameObject goblinPrefab = CreateGoblinPrefab();

            // 3. Tạo Orc Prefab
            GameObject orcPrefab = CreateOrcPrefab();

            // 4. Tạo Enemy Archer Prefab
            GameObject archerPrefab = CreateEnemyArcherPrefab(enemyProjPrefab);

            // 5. Cấu hình WaveManager trong Scene
            ConfigureWaves(goblinPrefab, orcPrefab, archerPrefab);

            Undo.CollapseUndoOperations(undoGroup);

            // Lưu Scene và Assets
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool isSaved = EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Phase15Setup] ✅ Setup Phase 15 hoàn tất! Scene lưu: {(isSaved ? "Thành công" : "Thất bại")}");
        }

        private static Material GetOrCreateMaterial(string path, Color color)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLit != null)
                {
                    mat = new Material(urpLit);
                }
                else
                {
                    mat = new Material(Shader.Find("Standard"));
                }
                mat.color = color;
                mat.SetColor("_BaseColor", color);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static GameObject CreateEnemyProjectilePrefab()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Project/Dev5_Art/Prefabs/Projectiles"))
            {
                AssetDatabase.CreateFolder("Assets/Project/Dev5_Art/Prefabs", "Projectiles");
            }

            Material redMat = GetOrCreateMaterial(ENEMY_PROJ_MAT_PATH, Color.red);

            GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempSphere.name = "EnemyProjectile_Prefab";
            tempSphere.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

            SimpleProjectile proj = tempSphere.GetComponent<SimpleProjectile>();
            if (proj == null)
            {
                proj = tempSphere.AddComponent<SimpleProjectile>();
            }

            MeshRenderer mr = tempSphere.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = redMat;
            }

            SphereCollider col = tempSphere.GetComponent<SphereCollider>();
            if (col != null)
            {
                Object.DestroyImmediate(col);
            }

            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(tempSphere, ENEMY_PROJ_PREFAB_PATH);
            Object.DestroyImmediate(tempSphere);

            Debug.Log($"[Phase15Setup] Đã tạo Enemy Projectile Prefab tại: {ENEMY_PROJ_PREFAB_PATH}");
            return prefabAsset;
        }

        private static GameObject CreateGoblinPrefab()
        {
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BASE_ENEMY_PREFAB_PATH);
            if (basePrefab == null)
            {
                Debug.LogError($"[Phase15Setup] Không tìm thấy base prefab tại: {BASE_ENEMY_PREFAB_PATH}");
                return null;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
            if (instance == null) return null;

            instance.name = "Goblin_Enemy_Prefab";
            
            // Đổi kích thước nhỏ hơn
            instance.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            // Đổi màu
            Material goblinMat = GetOrCreateMaterial(GOBLIN_MAT_PATH, new Color(1.0f, 0.5f, 0.5f)); // Đỏ nhạt
            MeshRenderer mr = instance.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = goblinMat;
            }

            // Gắn EnemyRole
            EnemyRole role = instance.GetComponent<EnemyRole>();
            if (role == null)
            {
                role = instance.AddComponent<EnemyRole>();
            }
            role.role = EnemyClassRole.Goblin;
            role.attackType = EnemyAttackType.Melee;
            role.projectilePrefab = null;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, GOBLIN_PREFAB_PATH);
            Object.DestroyImmediate(instance);
            Debug.Log($"[Phase15Setup] Đã tạo Goblin Prefab tại: {GOBLIN_PREFAB_PATH}");
            return prefab;
        }

        private static GameObject CreateOrcPrefab()
        {
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BASE_ENEMY_PREFAB_PATH);
            if (basePrefab == null) return null;

            GameObject instance = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
            if (instance == null) return null;

            instance.name = "Orc_Enemy_Prefab";

            // Đổi kích thước lớn hơn
            instance.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);

            // Đổi màu
            Material orcMat = GetOrCreateMaterial(ORC_MAT_PATH, new Color(0.8f, 0.2f, 0.0f)); // Cam / Đỏ đậm
            MeshRenderer mr = instance.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = orcMat;
            }

            // Gắn EnemyRole
            EnemyRole role = instance.GetComponent<EnemyRole>();
            if (role == null)
            {
                role = instance.AddComponent<EnemyRole>();
            }
            role.role = EnemyClassRole.Orc;
            role.attackType = EnemyAttackType.Melee;
            role.projectilePrefab = null;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, ORC_PREFAB_PATH);
            Object.DestroyImmediate(instance);
            Debug.Log($"[Phase15Setup] Đã tạo Orc Prefab tại: {ORC_PREFAB_PATH}");
            return prefab;
        }

        private static GameObject CreateEnemyArcherPrefab(GameObject projPrefab)
        {
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BASE_ENEMY_PREFAB_PATH);
            if (basePrefab == null) return null;

            GameObject instance = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
            if (instance == null) return null;

            instance.name = "EnemyArcher_Prefab";

            // Đổi kích thước
            instance.transform.localScale = new Vector3(0.9f, 1.1f, 0.9f);

            // Đổi mesh thành Capsule
            MeshFilter mf = instance.GetComponent<MeshFilter>();
            if (mf != null)
            {
                GameObject capsuleTemp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                mf.sharedMesh = capsuleTemp.GetComponent<MeshFilter>().sharedMesh;
                Object.DestroyImmediate(capsuleTemp);
            }

            // Thay BoxCollider bằng CapsuleCollider
            BoxCollider boxCol = instance.GetComponent<BoxCollider>();
            if (boxCol != null)
            {
                Object.DestroyImmediate(boxCol);
                instance.AddComponent<CapsuleCollider>();
            }

            // Đổi màu
            Material archerMat = GetOrCreateMaterial(ARCHER_MAT_PATH, new Color(0.5f, 0.0f, 0.5f)); // Tím
            MeshRenderer mr = instance.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = archerMat;
            }

            // Gắn EnemyRole
            EnemyRole role = instance.GetComponent<EnemyRole>();
            if (role == null)
            {
                role = instance.AddComponent<EnemyRole>();
            }
            role.role = EnemyClassRole.Archer;
            role.attackType = EnemyAttackType.RangedProjectile;
            role.projectilePrefab = projPrefab;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, ARCHER_PREFAB_PATH);
            Object.DestroyImmediate(instance);
            Debug.Log($"[Phase15Setup] Đã tạo Enemy Archer Prefab tại: {ARCHER_PREFAB_PATH}");
            return prefab;
        }

        private static void ConfigureWaves(GameObject goblin, GameObject orc, GameObject archer)
        {
            WaveManager waveManager = Object.FindFirstObjectByType<WaveManager>();
            if (waveManager == null)
            {
                Debug.LogWarning("[Phase15Setup] Không tìm thấy WaveManager trong Scene!");
                return;
            }

            Undo.RecordObject(waveManager, "Configure Waves Composition");

            waveManager.waves.Clear();

            // Wave 1: 3 Goblins
            WaveData w1 = new WaveData();
            w1.waveName = "Wave 1";
            WaveEnemyEntry e1_1 = new WaveEnemyEntry();
            e1_1.enemyName = "Goblin";
            e1_1.enemyPrefab = goblin;
            e1_1.count = 3;
            e1_1.spawnInterval = 1.0f;
            e1_1.maxHealth = 30;
            e1_1.damage = 5;
            e1_1.attackRange = 1.3f;
            e1_1.attackCooldown = 1.5f;
            e1_1.moveSpeed = 3.0f;
            e1_1.killGoldReward = 2;
            w1.enemies.Add(e1_1);
            waveManager.waves.Add(w1);

            // Wave 2: 3 Goblin, 2 Orc
            WaveData w2 = new WaveData();
            w2.waveName = "Wave 2";
            WaveEnemyEntry e2_1 = new WaveEnemyEntry();
            e2_1.enemyName = "Goblin";
            e2_1.enemyPrefab = goblin;
            e2_1.count = 3;
            e2_1.spawnInterval = 1.0f;
            e2_1.maxHealth = 30;
            e2_1.damage = 5;
            e2_1.attackRange = 1.3f;
            e2_1.attackCooldown = 1.5f;
            e2_1.moveSpeed = 3.0f;
            e2_1.killGoldReward = 2;
            w2.enemies.Add(e2_1);

            WaveEnemyEntry e2_2 = new WaveEnemyEntry();
            e2_2.enemyName = "Orc";
            e2_2.enemyPrefab = orc;
            e2_2.count = 2;
            e2_2.spawnInterval = 1.5f;
            e2_2.maxHealth = 70;
            e2_2.damage = 10;
            e2_2.attackRange = 1.4f;
            e2_2.attackCooldown = 1.8f;
            e2_2.moveSpeed = 2.2f;
            e2_2.killGoldReward = 4;
            w2.enemies.Add(e2_2);
            waveManager.waves.Add(w2);

            // Wave 3: 4 Goblin, 2 Orc, 1 Archer
            WaveData w3 = new WaveData();
            w3.waveName = "Wave 3";
            WaveEnemyEntry e3_1 = new WaveEnemyEntry();
            e3_1.enemyName = "Goblin";
            e3_1.enemyPrefab = goblin;
            e3_1.count = 4;
            e3_1.spawnInterval = 1.0f;
            e3_1.maxHealth = 30;
            e3_1.damage = 5;
            e3_1.attackRange = 1.3f;
            e3_1.attackCooldown = 1.5f;
            e3_1.moveSpeed = 3.0f;
            e3_1.killGoldReward = 2;
            w3.enemies.Add(e3_1);

            WaveEnemyEntry e3_2 = new WaveEnemyEntry();
            e3_2.enemyName = "Orc";
            e3_2.enemyPrefab = orc;
            e3_2.count = 2;
            e3_2.spawnInterval = 1.5f;
            e3_2.maxHealth = 70;
            e3_2.damage = 10;
            e3_2.attackRange = 1.4f;
            e3_2.attackCooldown = 1.8f;
            e3_2.moveSpeed = 2.2f;
            e3_2.killGoldReward = 4;
            w3.enemies.Add(e3_2);

            WaveEnemyEntry e3_3 = new WaveEnemyEntry();
            e3_3.enemyName = "Enemy Archer";
            e3_3.enemyPrefab = archer;
            e3_3.count = 1;
            e3_3.spawnInterval = 1.5f;
            e3_3.maxHealth = 40;
            e3_3.damage = 7;
            e3_3.attackRange = 5.0f;
            e3_3.attackCooldown = 1.4f;
            e3_3.moveSpeed = 2.5f;
            e3_3.killGoldReward = 3;
            w3.enemies.Add(e3_3);
            waveManager.waves.Add(w3);

            // Wave 4: 4 Orc, 3 Archer
            WaveData w4 = new WaveData();
            w4.waveName = "Wave 4";
            WaveEnemyEntry e4_1 = new WaveEnemyEntry();
            e4_1.enemyName = "Orc";
            e4_1.enemyPrefab = orc;
            e4_1.count = 4;
            e4_1.spawnInterval = 1.5f;
            e4_1.maxHealth = 70;
            e4_1.damage = 10;
            e4_1.attackRange = 1.4f;
            e4_1.attackCooldown = 1.8f;
            e4_1.moveSpeed = 2.2f;
            e4_1.killGoldReward = 4;
            w4.enemies.Add(e4_1);

            WaveEnemyEntry e4_2 = new WaveEnemyEntry();
            e4_2.enemyName = "Enemy Archer";
            e4_2.enemyPrefab = archer;
            e4_2.count = 3;
            e4_2.spawnInterval = 1.5f;
            e4_2.maxHealth = 40;
            e4_2.damage = 7;
            e4_2.attackRange = 5.0f;
            e4_2.attackCooldown = 1.4f;
            e4_2.moveSpeed = 2.5f;
            e4_2.killGoldReward = 3;
            w4.enemies.Add(e4_2);
            waveManager.waves.Add(w4);

            EditorUtility.SetDirty(waveManager);
            Debug.Log("[Phase15Setup] Đã cấu hình Wave 1 - Wave 4 thành công cho WaveManager.");
        }
    }
}
#endif

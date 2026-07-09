#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor script tự động thiết lập Phase 9: PlayerUnitManager và nâng cấp các Prefab Tướng/Quái.
    /// Chạy qua thanh menu: Dev5 / Setup Phase 9 - Enemy AI Combat
    /// </summary>
    public static class Dev5Phase9SetupEditor
    {
        private const string SCENE_REQUIRED = "Scene_Dev5_Art";
        
        // Đường dẫn Prefab Tướng
        private const string KNIGHT_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Knight_Unit_Prefab.prefab";
        private const string ARCHER_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Archer_Unit_Prefab.prefab";
        private const string TANK_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Tank_Unit_Prefab.prefab";

        // Đường dẫn Prefab Quái
        private const string ENEMY_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/Enemy_Test_Prefab.prefab";

        [MenuItem("Dev5/Setup Phase 9 - Enemy AI Combat")]
        public static void SetupPhase9()
        {
            // 0. Xác nhận Scene hoạt động
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.name.Equals(SCENE_REQUIRED, System.StringComparison.OrdinalIgnoreCase))
            {
                bool proceed = EditorUtility.DisplayDialog(
                    "Sai Scene hoạt động",
                    $"Chức năng này được tối ưu cho scene '{SCENE_REQUIRED}'.\nScene hiện tại là '{activeScene.name}'.\n\nBạn có muốn tiếp tục?",
                    "Tiếp tục", "Hủy"
                );
                if (!proceed) return;
            }

            Undo.SetCurrentGroupName("Phase 9 Setup");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo/Cấu hình GameObject PlayerUnitManager trong Scene
            EnsurePlayerUnitManager();

            // 2. Nâng cấp 3 Prefab tướng (Cấu hình Health và maxHealth tương ứng)
            UpgradeHeroPrefab(KNIGHT_PREFAB_PATH, "Knight", 100);
            UpgradeHeroPrefab(ARCHER_PREFAB_PATH, "Archer", 70);
            UpgradeHeroPrefab(TANK_PREFAB_PATH, "Tank", 200);

            // 3. Nâng cấp Prefab quái (Thêm EnemyCombatStats và EnemyAutoAttack)
            UpgradeEnemyPrefab(ENEMY_PREFAB_PATH, "Enemy_Test", 5, 1.3f, 1.5f, 2.5f, 10f);

            Undo.CollapseUndoOperations(undoGroup);

            // Lưu scene lại
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool isSaved = EditorSceneManager.SaveScene(activeScene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Phase9Setup] ✅ Setup Phase 9 hoàn tất! Scene lưu: {(isSaved ? "Thành công" : "Thất bại")}");

            EditorUtility.DisplayDialog(
                "Phase 9 Setup Hoàn Tất",
                "Đã hoàn thành thiết lập các thành phần của Phase 9:\n" +
                "• GameObject: PlayerUnitManager (quản lý cờ trên board)\n" +
                "• Prefab Tướng (Máu mới): Knight (100 HP), Archer (70 HP), Tank (200 HP)\n" +
                "• Prefab Quái: Đã gắn EnemyCombatStats & EnemyAutoAttack (5 Dmg, 1.3 Range, 2.5 Speed)\n\n" +
                "Nhấn Play, mua cờ xếp lên board và nhấn Start Battle để xem cờ/quái chiến đấu lẫn nhau!",
                "OK"
            );
        }

        private static void EnsurePlayerUnitManager()
        {
            var existing = Object.FindAnyObjectByType<PlayerUnitManager>();
            if (existing != null)
            {
                Debug.Log("[Phase9Setup] PlayerUnitManager đã tồn tại trong Scene.");
                return;
            }

            GameObject go = new GameObject("PlayerUnitManager");
            Undo.RegisterCreatedObjectUndo(go, "Create PlayerUnitManager");
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            go.AddComponent<PlayerUnitManager>();
            Debug.Log("[Phase9Setup] Đã tạo GameObject 'PlayerUnitManager'.");
        }

        private static void UpgradeHeroPrefab(string path, string heroName, int maxHealth)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefabAsset == null)
            {
                Debug.LogWarning($"[Phase9Setup] Không tìm thấy prefab tướng tại đường dẫn: {path}");
                return;
            }

            // Tạo instance tạm trong Scene để chỉnh sửa
            GameObject instance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"[Phase9Setup] Không thể tạo instance của prefab tướng '{heroName}'!");
                return;
            }

            // Đảm bảo có component Health
            Health hp = instance.GetComponent<Health>();
            if (hp == null)
            {
                hp = instance.AddComponent<Health>();
            }

            // Set maxHealth qua SerializedObject để Unity lưu chuẩn xác
            var soHealth = new SerializedObject(hp);
            soHealth.FindProperty("maxHealth").intValue = maxHealth;
            soHealth.ApplyModifiedPropertiesWithoutUndo();

            // Đảm bảo có các component combat khác
            if (instance.GetComponent<PlaceableUnit>() == null) instance.AddComponent<PlaceableUnit>();
            if (instance.GetComponent<UnitCombatStats>() == null) instance.AddComponent<UnitCombatStats>();
            if (instance.GetComponent<UnitAutoAttack>() == null) instance.AddComponent<UnitAutoAttack>();

            // Lưu thay thế prefab cũ
            PrefabUtility.SaveAsPrefabAsset(instance, path);

            // Huỷ instance tạm
            Object.DestroyImmediate(instance);

            Debug.Log($"[Phase9Setup] Đã nâng cấp thành công prefab tướng '{heroName}' tại: {path}");
        }

        private static void UpgradeEnemyPrefab(string path, string enemyName, int damage, float range, float cooldown, float moveSpeed, float rotationSpeed)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefabAsset == null)
            {
                Debug.LogWarning($"[Phase9Setup] Không tìm thấy prefab quái tại đường dẫn: {path}");
                return;
            }

            // Tạo instance tạm trong Scene để chỉnh sửa
            GameObject instance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"[Phase9Setup] Không thể tạo instance của prefab quái '{enemyName}'!");
                return;
            }

            // Đảm bảo có component Health và EnemyController
            if (instance.GetComponent<Health>() == null) instance.AddComponent<Health>();
            if (instance.GetComponent<EnemyController>() == null) instance.AddComponent<EnemyController>();

            // Đảm bảo có EnemyCombatStats
            EnemyCombatStats stats = instance.GetComponent<EnemyCombatStats>();
            if (stats == null)
            {
                stats = instance.AddComponent<EnemyCombatStats>();
            }

            // Thiết lập chỉ số chiến đấu cho quái
            var soStats = new SerializedObject(stats);
            soStats.FindProperty("damage").intValue = damage;
            soStats.FindProperty("attackRange").floatValue = range;
            soStats.FindProperty("attackCooldown").floatValue = cooldown;
            soStats.FindProperty("moveSpeed").floatValue = moveSpeed;
            soStats.FindProperty("rotationSpeed").floatValue = rotationSpeed;
            soStats.ApplyModifiedPropertiesWithoutUndo();

            // Đảm bảo có EnemyAutoAttack
            EnemyAutoAttack attack = instance.GetComponent<EnemyAutoAttack>();
            if (attack == null)
            {
                attack = instance.AddComponent<EnemyAutoAttack>();
            }

            // Lưu thay thế prefab cũ
            PrefabUtility.SaveAsPrefabAsset(instance, path);

            // Huỷ instance tạm
            Object.DestroyImmediate(instance);

            Debug.Log($"[Phase9Setup] Đã nâng cấp thành công prefab quái '{enemyName}' tại: {path}");
        }
    }
}
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor script tự động thiết lập và cập nhật các chỉ số di chuyển mới của 3 Prefab tướng.
    /// Chạy qua menu: Dev5 / Setup Phase 7 - Unit Movement
    /// </summary>
    public static class Dev5Phase7SetupEditor
    {
        private const string KNIGHT_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Knight_Unit_Prefab.prefab";
        private const string ARCHER_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Archer_Unit_Prefab.prefab";
        private const string TANK_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Tank_Unit_Prefab.prefab";

        [MenuItem("Dev5/Setup Phase 7 - Unit Movement")]
        public static void SetupPhase7()
        {
            // Cập nhật các Prefab với chỉ số di chuyển và cự ly mới
            UpgradeHeroPrefab(KNIGHT_PREFAB_PATH, "Knight", 10, 1.5f, 1.0f, 1, 3f, 10f);
            UpgradeHeroPrefab(ARCHER_PREFAB_PATH, "Archer", 8, 5.0f, 1.2f, 1, 3f, 10f);
            UpgradeHeroPrefab(TANK_PREFAB_PATH, "Tank", 6, 1.3f, 1.5f, 1, 2f, 10f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Phase 7 Prefab Setup Hoàn Tất",
                "Đã cập nhật thành công 3 Prefab tướng với các thông số di chuyển của Phase 7:\n" +
                "• Knight: Tầm đánh 1.5, Tốc độ chạy 3\n" +
                "• Archer: Tầm đánh 5.0, Tốc độ chạy 3\n" +
                "• Tank: Tầm đánh 1.3, Tốc độ chạy 2\n\n" +
                "Nhấn Play, đặt tướng lên board và bắt đầu trận đấu để test di chuyển quây quái!",
                "OK"
            );
        }

        private static void UpgradeHeroPrefab(string path, string heroName, int damage, float range, float cooldown, int goldPerHit, float moveSpeed, float rotationSpeed)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefabAsset == null)
            {
                Debug.LogWarning($"[Phase7Setup] Không tìm thấy prefab tại đường dẫn: {path}");
                return;
            }

            // Tạo instance tạm trong Scene để chỉnh sửa
            GameObject instance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"[Phase7Setup] Không thể tạo instance của prefab '{heroName}'!");
                return;
            }

            // Thêm/cấu hình UnitCombatStats
            UnitCombatStats stats = instance.GetComponent<UnitCombatStats>();
            if (stats == null)
            {
                stats = instance.AddComponent<UnitCombatStats>();
            }

            var soStats = new SerializedObject(stats);
            soStats.FindProperty("damage").intValue = damage;
            soStats.FindProperty("attackRange").floatValue = range;
            soStats.FindProperty("attackCooldown").floatValue = cooldown;
            soStats.FindProperty("goldPerHit").intValue = goldPerHit;
            soStats.FindProperty("moveSpeed").floatValue = moveSpeed;
            soStats.FindProperty("rotationSpeed").floatValue = rotationSpeed;
            soStats.ApplyModifiedPropertiesWithoutUndo();

            // Đảm bảo có UnitAutoAttack
            UnitAutoAttack attack = instance.GetComponent<UnitAutoAttack>();
            if (attack == null)
            {
                instance.AddComponent<UnitAutoAttack>();
            }

            // Lưu thay đổi đè lên prefab cũ
            PrefabUtility.SaveAsPrefabAsset(instance, path);

            // Huỷ instance tạm
            Object.DestroyImmediate(instance);

            Debug.Log($"[Phase7Setup] Đã nâng cấp thành công prefab '{heroName}' tại: {path}");
        }
    }
}
#endif

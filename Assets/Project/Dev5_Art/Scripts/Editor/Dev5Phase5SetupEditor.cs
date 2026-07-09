#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor script tự động nâng cấp 3 Prefabs Tướng với component chiến đấu và tự đánh cho Phase 5.
    /// Chạy qua menu: Dev5 / Setup Phase 5 - Unit Auto Attack
    /// </summary>
    public static class Dev5Phase5SetupEditor
    {
        private const string KNIGHT_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Knight_Unit_Prefab.prefab";
        private const string ARCHER_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Archer_Unit_Prefab.prefab";
        private const string TANK_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Tank_Unit_Prefab.prefab";

        [MenuItem("Dev5/Setup Phase 5 - Unit Auto Attack")]
        public static void SetupPhase5()
        {
            // Nâng cấp từng Prefab
            UpgradeHeroPrefab(KNIGHT_PREFAB_PATH, "Knight", 10, 5f, 1.0f, 1);
            UpgradeHeroPrefab(ARCHER_PREFAB_PATH, "Archer", 8, 6f, 1.2f, 1);
            UpgradeHeroPrefab(TANK_PREFAB_PATH, "Tank", 6, 5f, 1.5f, 1);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Phase 5 Prefab Setup Hoàn Tất",
                "Đã nâng cấp thành công 3 Prefab tướng với:\n" +
                "• UnitCombatStats (Sát thương, tầm đánh, cooldown)\n" +
                "• UnitAutoAttack (Tự đánh khi đặt trên board)\n\n" +
                "Tướng mua từ Shop bây giờ sẽ tự động tấn công kẻ địch!",
                "OK"
            );
        }

        private static void UpgradeHeroPrefab(string path, string heroName, int damage, float range, float cooldown, int goldPerHit)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefabAsset == null)
            {
                Debug.LogWarning($"[Phase5Setup] Không tìm thấy prefab tại đường dẫn: {path}");
                return;
            }

            // Tạo instance tạm trong Scene để chỉnh sửa
            GameObject instance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"[Phase5Setup] Không thể tạo instance của prefab '{heroName}'!");
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
            soStats.ApplyModifiedPropertiesWithoutUndo();

            // Thêm UnitAutoAttack
            UnitAutoAttack attack = instance.GetComponent<UnitAutoAttack>();
            if (attack == null)
            {
                instance.AddComponent<UnitAutoAttack>();
            }

            // Lưu thay đổi đè lên prefab cũ
            PrefabUtility.SaveAsPrefabAsset(instance, path);

            // Huỷ instance tạm
            Object.DestroyImmediate(instance);

            Debug.Log($"[Phase5Setup] Đã nâng cấp thành công prefab '{heroName}' tại: {path}");
        }
    }
}
#endif

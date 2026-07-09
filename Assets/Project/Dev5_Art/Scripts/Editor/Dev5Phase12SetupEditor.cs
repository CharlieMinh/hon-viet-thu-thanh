#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor script tự động thiết lập Phase 12: Hệ thống nâng cấp Sao vô hạn.
    /// Chạy qua thanh menu: Dev5 / Setup Phase 12 - Infinite Star Upgrade
    /// </summary>
    public static class Dev5Phase12SetupEditor
    {
        private const string SCENE_REQUIRED = "Scene_Dev5_Art";
        private const string KNIGHT_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Knight_Unit_Prefab.prefab";
        private const string ARCHER_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Archer_Unit_Prefab.prefab";
        private const string TANK_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Tank_Unit_Prefab.prefab";

        [MenuItem("Dev5/Setup Phase 12 - Infinite Star Upgrade")]
        public static void SetupPhase12()
        {
            // 0. Xác nhận Scene hoạt động
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.name.Equals(SCENE_REQUIRED, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[Phase12Setup] Đang chạy trên scene '{activeScene.name}' thay vì '{SCENE_REQUIRED}'.");
            }

            Undo.SetCurrentGroupName("Phase 12 Star Setup");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo/Cấu hình GameObject UnitStarUpgradeManager trong Scene
            EnsureUnitStarUpgradeManager();

            // 2. Nâng cấp 3 Prefab tướng
            UpgradeHeroPrefab(KNIGHT_PREFAB_PATH, "Knight");
            UpgradeHeroPrefab(ARCHER_PREFAB_PATH, "Archer");
            UpgradeHeroPrefab(TANK_PREFAB_PATH, "Tank");

            Undo.CollapseUndoOperations(undoGroup);

            // Lưu scene lại
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool isSaved = EditorSceneManager.SaveScene(activeScene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Phase12Setup] ✅ Setup Phase 12 hoàn tất! Scene lưu: {(isSaved ? "Thành công" : "Thất bại")}");
        }

        private static void EnsureUnitStarUpgradeManager()
        {
            var existing = Object.FindAnyObjectByType<UnitStarUpgradeManager>();
            if (existing != null)
            {
                Debug.Log("[Phase12Setup] UnitStarUpgradeManager đã tồn tại trong Scene.");
                return;
            }

            GameObject go = new GameObject("UnitStarUpgradeManager");
            Undo.RegisterCreatedObjectUndo(go, "Create UnitStarUpgradeManager");
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            go.AddComponent<UnitStarUpgradeManager>();
            Debug.Log("[Phase12Setup] Đã tạo GameObject 'UnitStarUpgradeManager'.");
        }

        private static void UpgradeHeroPrefab(string path, string heroName)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefabAsset == null)
            {
                Debug.LogWarning($"[Phase12Setup] Không tìm thấy prefab tại đường dẫn: {path}");
                return;
            }

            // Tạo instance tạm trong Scene để chỉnh sửa
            GameObject instance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"[Phase12Setup] Không thể tạo instance của prefab '{heroName}'!");
                return;
            }

            // Thêm/cấu hình UnitStarData
            UnitStarData starData = instance.GetComponent<UnitStarData>();
            if (starData == null)
            {
                starData = instance.AddComponent<UnitStarData>();
            }

            var soStar = new SerializedObject(starData);
            soStar.FindProperty("unitId").stringValue = heroName;
            soStar.FindProperty("starLevel").intValue = 1;
            soStar.ApplyModifiedPropertiesWithoutUndo();

            // Thêm UnitStarVisual
            UnitStarVisual visual = instance.GetComponent<UnitStarVisual>();
            if (visual == null)
            {
                instance.AddComponent<UnitStarVisual>();
            }

            // Lưu thay đổi đè lên prefab cũ
            PrefabUtility.SaveAsPrefabAsset(instance, path);

            // Huỷ instance tạm
            Object.DestroyImmediate(instance);

            Debug.Log($"[Phase12Setup] Đã nâng cấp thành công prefab '{heroName}' tại: {path}");
        }
    }
}
#endif

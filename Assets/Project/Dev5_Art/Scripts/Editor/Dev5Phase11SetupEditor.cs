#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor script tự động thiết lập Phase 11: Cấu hình Vàng thưởng theo từng Wave trên WaveManager.
    /// Chạy qua thanh menu: Dev5 / Setup Phase 11 - Economy Rework
    /// </summary>
    public static class Dev5Phase11SetupEditor
    {
        private const string SCENE_REQUIRED = "Scene_Dev5_Art";
        private const string ENEMY_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/Enemy_Test_Prefab.prefab";

        [MenuItem("Dev5/Setup Phase 11 - Economy Rework")]
        public static void SetupPhase11()
        {
            // 0. Xác nhận Scene hoạt động
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.name.Equals(SCENE_REQUIRED, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[Phase11Setup] Đang chạy trên scene '{activeScene.name}' thay vì '{SCENE_REQUIRED}'.");
            }

            Undo.SetCurrentGroupName("Phase 11 Economy Setup");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Cấu hình lượng vàng thưởng tiêu diệt theo từng Wave trên WaveManager
            WaveManager waveManager = Object.FindAnyObjectByType<WaveManager>();
            if (waveManager != null)
            {
                var so = new SerializedObject(waveManager);
                SerializedProperty wavesProp = so.FindProperty("waves");

                if (wavesProp.arraySize >= 3)
                {
                    // Wave 1: 2 Gold
                    wavesProp.GetArrayElementAtIndex(0).FindPropertyRelative("enemyKillGoldReward").intValue = 2;
                    // Wave 2: 3 Gold
                    wavesProp.GetArrayElementAtIndex(1).FindPropertyRelative("enemyKillGoldReward").intValue = 3;
                    // Wave 3: 4 Gold
                    wavesProp.GetArrayElementAtIndex(2).FindPropertyRelative("enemyKillGoldReward").intValue = 4;
                    
                    Debug.Log("[Phase11Setup] Đã cấu hình enemyKillGoldReward cho 3 waves: Wave 1 (+2G), Wave 2 (+3G), Wave 3 (+4G).");
                }
                else
                {
                    Debug.LogWarning("[Phase11Setup] Số lượng wave cấu hình ít hơn 3, vui lòng kiểm tra lại WaveManager!");
                }

                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogError("[Phase11Setup] Không tìm thấy WaveManager trong Scene!");
            }

            // 2. Cấu hình prefab quái (Thiết lập killGoldReward mặc định là 2)
            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ENEMY_PREFAB_PATH);
            if (enemyPrefab != null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(enemyPrefab) as GameObject;
                if (instance != null)
                {
                    EnemyController enemyCtrl = instance.GetComponent<EnemyController>();
                    if (enemyCtrl == null)
                    {
                        enemyCtrl = instance.AddComponent<EnemyController>();
                    }

                    var soCtrl = new SerializedObject(enemyCtrl);
                    soCtrl.FindProperty("killGoldReward").intValue = 2;
                    soCtrl.ApplyModifiedPropertiesWithoutUndo();

                    PrefabUtility.SaveAsPrefabAsset(instance, ENEMY_PREFAB_PATH);
                    Object.DestroyImmediate(instance);
                    Debug.Log($"[Phase11Setup] Đã cập nhật thành công prefab quái tại: {ENEMY_PREFAB_PATH}");
                }
            }

            Undo.CollapseUndoOperations(undoGroup);

            // Lưu scene lại
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool isSaved = EditorSceneManager.SaveScene(activeScene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Phase11Setup] ✅ Setup Phase 11 hoàn tất! Scene lưu: {(isSaved ? "Thành công" : "Thất bại")}");

            Debug.Log("[Phase11Setup] Đã cấu hình thành công hệ thống Kinh tế mới:\n" +
                      "• WaveManager: Wave 1 (+2G/kill), Wave 2 (+3G/kill), Wave 3 (+4G/kill)\n" +
                      "• Enemy Prefab: killGoldReward mặc định = 2\n" +
                      "Nhấn Play, mua cờ và bắt đầu để kiểm tra: không cộng vàng mỗi hit, cộng vàng khi quái chết, và cộng lợi tức (Interest) sau wave thắng!");
        }
    }
}
#endif

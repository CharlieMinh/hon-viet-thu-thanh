#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor script tự động thiết lập Phase 10: BattleResetManager.
    /// Chạy qua thanh menu: Dev5 / Setup Phase 10 - Post-Combat Reset
    /// </summary>
    public static class Dev5Phase10SetupEditor
    {
        private const string SCENE_REQUIRED = "Scene_Dev5_Art";

        [MenuItem("Dev5/Setup Phase 10 - Post-Combat Reset")]
        public static void SetupPhase10()
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

            Undo.SetCurrentGroupName("Phase 10 Setup");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo/Cấu hình GameObject BattleResetManager trong Scene
            EnsureBattleResetManager();

            Undo.CollapseUndoOperations(undoGroup);

            // Lưu scene lại
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool isSaved = EditorSceneManager.SaveScene(activeScene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Phase10Setup] ✅ Setup Phase 10 hoàn tất! Scene lưu: {(isSaved ? "Thành công" : "Thất bại")}");

            EditorUtility.DisplayDialog(
                "Phase 10 Setup Hoàn Tất",
                "Đã cấu hình thành công hệ thống Reset sau trận đấu:\n" +
                "• GameObject: BattleResetManager (Lưu snapshot và khôi phục cờ sống)\n\n" +
                "Nhấn Play, đặt cờ chiến đấu, sau khi hết wave cờ sống tự động quay về ô cũ!",
                "OK"
            );
        }

        private static void EnsureBattleResetManager()
        {
            var existing = Object.FindAnyObjectByType<BattleResetManager>();
            if (existing != null)
            {
                Debug.Log("[Phase10Setup] BattleResetManager đã tồn tại trong Scene.");
                return;
            }

            GameObject go = new GameObject("BattleResetManager");
            Undo.RegisterCreatedObjectUndo(go, "Create BattleResetManager");
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            go.AddComponent<BattleResetManager>();
            Debug.Log("[Phase10Setup] Đã tạo GameObject 'BattleResetManager'.");
        }
    }
}
#endif

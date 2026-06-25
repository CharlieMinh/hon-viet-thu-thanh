// Dev5InputSystemFixer.cs — Editor-only utility
// Mục đích: Thay thế StandaloneInputModule → InputSystemUIInputModule
// trong tất cả EventSystem của scene đang mở, sau đó lưu scene.
// Chạy qua menu: Dev5 Tools / Fix EventSystem Input Module

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace HonVietThuThanh.Dev5.Editor
{
    public static class Dev5InputSystemFixer
    {
        private const string MENU_PATH = "Dev5 Tools/Fix EventSystem Input Module (New Input System)";

        [MenuItem(MENU_PATH)]
        public static void FixEventSystemInputModule()
        {
            int totalFixed = 0;

            // Tìm tất cả EventSystem trong scene (kể cả inactive)
            var allEventSystems = Object.FindObjectsByType<EventSystem>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            if (allEventSystems.Length == 0)
            {
                // Không có EventSystem → tạo mới đúng chuẩn
                Debug.Log("[Dev5InputSystemFixer] Không tìm thấy EventSystem, đang tạo mới...");
                CreateProperEventSystem();
                totalFixed++;
            }
            else
            {
                foreach (var es in allEventSystems)
                {
                    bool changed = SwapInputModule(es.gameObject);
                    if (changed)
                    {
                        EditorUtility.SetDirty(es.gameObject);
                        totalFixed++;
                    }
                }
            }

            if (totalFixed > 0)
            {
                EditorSceneManager.MarkAllScenesDirty();
                bool saved = EditorSceneManager.SaveOpenScenes();
                Debug.Log($"[Dev5InputSystemFixer] Đã sửa {totalFixed} EventSystem và lưu scene: {(saved ? "thành công" : "thất bại")}");
                EditorUtility.DisplayDialog(
                    "Dev5 Input System Fixer",
                    $"✅ Đã sửa {totalFixed} EventSystem.\n\nStandaloneInputModule → InputSystemUIInputModule\n\nScene đã được lưu.",
                    "OK"
                );
            }
            else
            {
                Debug.Log("[Dev5InputSystemFixer] Tất cả EventSystem đã dùng đúng InputSystemUIInputModule.");
                EditorUtility.DisplayDialog(
                    "Dev5 Input System Fixer",
                    "✅ Không cần sửa.\nTất cả EventSystem đã dùng InputSystemUIInputModule.",
                    "OK"
                );
            }
        }

        private static bool SwapInputModule(GameObject go)
        {
            bool changed = false;

            // Xóa StandaloneInputModule nếu có
            var standalone = go.GetComponent<StandaloneInputModule>();
            if (standalone != null)
            {
                Debug.Log($"[Dev5InputSystemFixer] Xóa StandaloneInputModule khỏi '{go.name}'");
                Object.DestroyImmediate(standalone);
                changed = true;
            }

            // Thêm InputSystemUIInputModule nếu chưa có
            var newModule = go.GetComponent<InputSystemUIInputModule>();
            if (newModule == null)
            {
                go.AddComponent<InputSystemUIInputModule>();
                Debug.Log($"[Dev5InputSystemFixer] Đã thêm InputSystemUIInputModule vào '{go.name}'");
                changed = true;
            }

            return changed;
        }

        private static void CreateProperEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[Dev5InputSystemFixer] Đã tạo EventSystem với InputSystemUIInputModule.");
        }

        [MenuItem(MENU_PATH, true)]
        private static bool ValidateFixEventSystem()
        {
            return !Application.isPlaying;
        }
    }
}
#endif

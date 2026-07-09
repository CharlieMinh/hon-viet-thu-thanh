#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor script tự động thiết lập Phase 8: WaveManager, EnemySpawnPoints và cấu hình Wave.
    /// Chạy qua thanh menu: Dev5 / Setup Phase 8 - Wave System
    /// </summary>
    public static class Dev5Phase8SetupEditor
    {
        private const string SCENE_REQUIRED = "Scene_Dev5_Art";
        private const string ENEMY_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/Enemy_Test_Prefab.prefab";

        // Tọa độ các Spawn Point
        private static readonly Vector3 PosSpawn0 = new Vector3(3.5f, 1.0f, -1.0f);
        private static readonly Vector3 PosSpawn1 = new Vector3(3.5f, 1.0f, 0.0f);
        private static readonly Vector3 PosSpawn2 = new Vector3(3.5f, 1.0f, 1.0f);

        [MenuItem("Dev5/Setup Phase 8 - Wave System")]
        public static void SetupPhase8()
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

            Undo.SetCurrentGroupName("Phase 8 Wave Setup");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo/Cấu hình GameObject WaveManager trong Scene
            WaveManager waveManager = EnsureWaveManager();

            // 2. Tìm hoặc tạo GameObject cha EnemySpawnPoints
            GameObject spawnPointsParent = EnsureSpawnPointsParent();

            // 3. Tạo/Cập nhật các Spawn Point con
            Transform sp0 = EnsureSpawnPoint("EnemySpawnPoint_0", PosSpawn0, spawnPointsParent.transform);
            Transform sp1 = EnsureSpawnPoint("EnemySpawnPoint_1", PosSpawn1, spawnPointsParent.transform);
            Transform sp2 = EnsureSpawnPoint("EnemySpawnPoint_2", PosSpawn2, spawnPointsParent.transform);

            // 4. Tìm parent Enemies (để chứa các quái spawn ra)
            GameObject enemiesParent = GameObject.Find("Enemies");
            if (enemiesParent == null)
            {
                enemiesParent = new GameObject("Enemies");
                Undo.RegisterCreatedObjectUndo(enemiesParent, "Create Enemies Parent");
                enemiesParent.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            }

            // 5. Tắt (Disable) 3 quái test cũ nếu có trong Scene để không gây nhiễu
            DisableStaticTestEnemies(enemiesParent.transform);

            // 6. Gán các trường cấu hình vào WaveManager
            if (waveManager != null)
            {
                var so = new SerializedObject(waveManager);

                // Gán enemyPrefab
                GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ENEMY_PREFAB_PATH);
                if (enemyPrefab != null)
                {
                    so.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab;
                }
                else
                {
                    Debug.LogWarning($"[Phase8Setup] Không tìm thấy Enemy Prefab tại: {ENEMY_PREFAB_PATH}");
                }

                // Gán spawnPoints
                SerializedProperty spProp = so.FindProperty("spawnPoints");
                spProp.ClearArray();
                spProp.InsertArrayElementAtIndex(0);
                spProp.GetArrayElementAtIndex(0).objectReferenceValue = sp0;
                spProp.InsertArrayElementAtIndex(1);
                spProp.GetArrayElementAtIndex(1).objectReferenceValue = sp1;
                spProp.InsertArrayElementAtIndex(2);
                spProp.GetArrayElementAtIndex(2).objectReferenceValue = sp2;

                // Gán enemiesParent
                so.FindProperty("enemiesParent").objectReferenceValue = enemiesParent.transform;

                // Gán Waves cấu hình
                SerializedProperty wavesProp = so.FindProperty("waves");
                wavesProp.ClearArray();

                // Wave 1
                InsertWaveData(wavesProp, 0, "Wave 1", 3, 0.3f, 30);
                // Wave 2
                InsertWaveData(wavesProp, 1, "Wave 2", 5, 0.3f, 40);
                // Wave 3
                InsertWaveData(wavesProp, 2, "Wave 3", 7, 0.25f, 50);

                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // 7. Đồng bộ UI text sang GamePhaseManager
            GamePhaseManager phaseManager = Object.FindAnyObjectByType<GamePhaseManager>();
            if (phaseManager != null)
            {
                // Cập nhật text ban đầu
                phaseManager.UpdateStateUI();
            }

            Undo.CollapseUndoOperations(undoGroup);

            // Lưu scene lại
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool isSaved = EditorSceneManager.SaveScene(activeScene);

            Debug.Log($"[Phase8Setup] ✅ Setup Phase 8 hoàn tất! Scene lưu: {(isSaved ? "Thành công" : "Thất bại")}");

            EditorUtility.DisplayDialog(
                "Phase 8 Setup Hoàn Tất",
                "Đã cấu hình thành công hệ thống Wave:\n" +
                "• GameObject: WaveManager (Sinh quái động khi bắt đầu chiến đấu)\n" +
                "• GameObject: EnemySpawnPoints (Chứa 3 spawn points tại Y=1.0f)\n" +
                "• Disable các enemy test tĩnh cũ trong scene.\n" +
                "• Thiết lập 3 Waves: Wave 1 (3 quái, HP 30), Wave 2 (5 quái, HP 40), Wave 3 (7 quái, HP 50).\n\n" +
                "Nhấn Play, mua cờ và nhấn Start Battle để chơi thử wave system!",
                "OK"
            );
        }

        private static WaveManager EnsureWaveManager()
        {
            var existing = Object.FindAnyObjectByType<WaveManager>();
            if (existing != null)
            {
                Debug.Log("[Phase8Setup] WaveManager đã tồn tại trong Scene.");
                return existing;
            }

            GameObject go = new GameObject("WaveManager");
            Undo.RegisterCreatedObjectUndo(go, "Create WaveManager");
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            WaveManager wm = go.AddComponent<WaveManager>();
            Debug.Log("[Phase8Setup] Đã tạo GameObject 'WaveManager'.");
            return wm;
        }

        private static GameObject EnsureSpawnPointsParent()
        {
            GameObject parent = GameObject.Find("EnemySpawnPoints");
            if (parent != null) return parent;

            parent = new GameObject("EnemySpawnPoints");
            Undo.RegisterCreatedObjectUndo(parent, "Create EnemySpawnPoints Parent");
            parent.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            return parent;
        }

        private static Transform EnsureSpawnPoint(string name, Vector3 position, Transform parent)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                existing.position = position;
                return existing;
            }

            GameObject go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(0, -90, 0); // Quay mặt về phía trái (hướng về phía người chơi)
            return go.transform;
        }

        private static void DisableStaticTestEnemies(Transform enemiesParent)
        {
            string[] testEnemyNames = { "Enemy_Test_0", "Enemy_Test_1", "Enemy_Test_2" };
            foreach (var name in testEnemyNames)
            {
                Transform child = enemiesParent.Find(name);
                if (child != null && child.gameObject.activeSelf)
                {
                    Undo.RegisterCompleteObjectUndo(child.gameObject, $"Disable {name}");
                    child.gameObject.SetActive(false);
                    Debug.Log($"[Phase8Setup] Đã tắt GameObject test enemy tĩnh: {name}");
                }
            }
        }

        private static void InsertWaveData(SerializedProperty wavesProp, int index, string name, int count, float interval, int health)
        {
            wavesProp.InsertArrayElementAtIndex(index);
            SerializedProperty element = wavesProp.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("waveName").stringValue = name;
            element.FindPropertyRelative("enemyCount").intValue = count;
            element.FindPropertyRelative("spawnInterval").floatValue = interval;
            element.FindPropertyRelative("enemyHealth").intValue = health;
        }
    }
}
#endif

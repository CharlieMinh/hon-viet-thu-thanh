#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor script tự động thiết lập Phase 4: Enemy Prefab, EnemyManager và Spawn 3 Enemy Test.
    /// Chạy qua thanh menu: Dev5 / Setup Phase 4 - Enemy & Health
    /// </summary>
    public static class Dev5Phase4SetupEditor
    {
        private const string SCENE_REQUIRED = "Scene_Dev5_Art";
        
        // Đường dẫn tài nguyên
        private const string MATERIAL_PATH = "Assets/Project/Dev5_Art/Materials/M_Enemy_Red.mat";
        private const string PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/Enemy_Test_Prefab.prefab";
        private const string MATERIALS_DIR = "Assets/Project/Dev5_Art/Materials";
        private const string PREFABS_DIR = "Assets/Project/Dev5_Art/Prefabs/Enemies";

        // Tọa độ 3 enemy test
        private static readonly Vector3 PosEnemy0 = new Vector3(3.5f, 1.0f, -1.0f);
        private static readonly Vector3 PosEnemy1 = new Vector3(3.5f, 1.0f, 0.0f);
        private static readonly Vector3 PosEnemy2 = new Vector3(3.5f, 1.0f, 1.0f);

        [MenuItem("Dev5/Setup Phase 4 - Enemy & Health")]
        public static void SetupPhase4()
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

            Undo.SetCurrentGroupName("Phase 4 Setup");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo Material màu đỏ cho Enemy
            Material redMaterial = EnsureEnemyMaterial();

            // 2. Tạo hoặc cập nhật Enemy Prefab dạng Cube màu đỏ
            GameObject enemyPrefab = EnsureEnemyPrefab(redMaterial);
            if (enemyPrefab == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không thể tạo hoặc tải Enemy Prefab!", "OK");
                return;
            }

            // 3. Tạo/Cấu hình GameObject EnemyManager trong Scene
            EnsureEnemyManager();

            // 4. Tạo/Tìm GameObject cha Enemies
            GameObject enemiesParent = EnsureEnemiesParent();

            // 5. Sinh 3 enemy test từ prefab
            SpawnTestEnemy("Enemy_Test_0", PosEnemy0, enemyPrefab, enemiesParent.transform);
            SpawnTestEnemy("Enemy_Test_1", PosEnemy1, enemyPrefab, enemiesParent.transform);
            SpawnTestEnemy("Enemy_Test_2", PosEnemy2, enemyPrefab, enemiesParent.transform);

            Undo.CollapseUndoOperations(undoGroup);

            // Lưu scene lại
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool isSaved = EditorSceneManager.SaveScene(activeScene);

            Debug.Log($"[Phase4Setup] ✅ Setup Phase 4 hoàn tất! Scene lưu: {(isSaved ? "Thành công" : "Thất bại")}");

            EditorUtility.DisplayDialog(
                "Phase 4 Setup Hoàn Tất",
                "Đã tạo thành công:\n" +
                "• Material: M_Enemy_Red.mat\n" +
                "• Prefab: Enemy_Test_Prefab.prefab (Cube màu đỏ, máu 30)\n" +
                "• GameObject: EnemyManager (có script quản lý)\n" +
                "• GameObject: Enemies (chứa 3 enemy test tại toạ độ Y=1.0f)\n\n" +
                "Nhấn Play, sau đó nhấn phím 'T' để test damage.",
                "OK"
            );
        }

        private static Material EnsureEnemyMaterial()
        {
            // Kiểm tra thư mục chứa materials
            if (!AssetDatabase.IsValidFolder(MATERIALS_DIR))
            {
                System.IO.Directory.CreateDirectory(MATERIALS_DIR);
                AssetDatabase.Refresh();
            }

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH);
            if (mat != null) return mat;

            // Tìm shader URP Lit hoặc Standard
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null)
            {
                Debug.LogError("[Phase4Setup] Không tìm thấy Shader tương thích!");
                return null;
            }

            mat = new Material(shader);
            Color redColor = new Color(0.9f, 0.1f, 0.1f, 1f); // Màu đỏ đậm đẹp mắt
            mat.SetColor("_BaseColor", redColor);
            mat.SetColor("_Color", redColor);

            AssetDatabase.CreateAsset(mat, MATERIAL_PATH);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Phase4Setup] Đã sinh chất liệu màu đỏ tại: {MATERIAL_PATH}");
            return mat;
        }

        private static GameObject EnsureEnemyPrefab(Material redMaterial)
        {
            // Kiểm tra thư mục chứa prefabs
            if (!AssetDatabase.IsValidFolder("Assets/Project/Dev5_Art/Prefabs"))
            {
                System.IO.Directory.CreateDirectory("Assets/Project/Dev5_Art/Prefabs");
            }
            if (!AssetDatabase.IsValidFolder(PREFABS_DIR))
            {
                System.IO.Directory.CreateDirectory(PREFABS_DIR);
                AssetDatabase.Refresh();
            }

            // Nếu prefab đã tồn tại, kiểm tra xem có hợp lệ không
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            if (existingPrefab != null)
            {
                return existingPrefab;
            }

            // Tạo GameObject tạm để dựng Prefab
            GameObject tempGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tempGO.name = "Enemy_Test_Prefab_Source";

            // Thiết lập MeshRenderer màu đỏ
            MeshRenderer renderer = tempGO.GetComponent<MeshRenderer>();
            if (renderer != null && redMaterial != null)
            {
                renderer.sharedMaterial = redMaterial;
            }

            // Gắn component Health
            Health health = tempGO.AddComponent<Health>();
            // Gán maxHealth = 30 qua SerializedObject
            var soHealth = new SerializedObject(health);
            soHealth.FindProperty("maxHealth").intValue = 30;
            soHealth.ApplyModifiedPropertiesWithoutUndo();

            // Gắn component EnemyController
            EnemyController controller = tempGO.AddComponent<EnemyController>();
            var soController = new SerializedObject(controller);
            soController.FindProperty("enemyName").stringValue = "Enemy_Test";
            soController.ApplyModifiedPropertiesWithoutUndo();

            // Lưu thành PrefabAsset
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAssetAndConnect(tempGO, PREFAB_PATH, InteractionMode.AutomatedAction);
            
            // Xóa object tạm trong Scene
            GameObject.DestroyImmediate(tempGO);

            AssetDatabase.SaveAssets();
            Debug.Log($"[Phase4Setup] Đã tạo thành công Enemy Prefab tại: {PREFAB_PATH}");
            return prefabAsset;
        }

        private static void EnsureEnemyManager()
        {
            var existing = Object.FindAnyObjectByType<EnemyManager>();
            if (existing != null)
            {
                Debug.Log("[Phase4Setup] EnemyManager đã tồn tại trong Scene.");
                return;
            }

            GameObject go = new GameObject("EnemyManager");
            Undo.RegisterCreatedObjectUndo(go, "Create EnemyManager");
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            go.AddComponent<EnemyManager>();
            Debug.Log("[Phase4Setup] Đã tạo GameObject 'EnemyManager'.");
        }

        private static GameObject EnsureEnemiesParent()
        {
            GameObject enemies = GameObject.Find("Enemies");
            if (enemies != null) return enemies;

            enemies = new GameObject("Enemies");
            Undo.RegisterCreatedObjectUndo(enemies, "Create Enemies Parent");
            enemies.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            return enemies;
        }

        private static void SpawnTestEnemy(string name, Vector3 position, GameObject prefab, Transform parent)
        {
            // Kiểm tra xem quái có tên này đã tồn tại chưa
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                // Cập nhật lại vị trí nếu đã có sẵn
                existing.position = position;
                Debug.Log($"[Phase4Setup] Quái '{name}' đã có sẵn, cập nhật vị trí về {position}.");
                return;
            }

            // Instantiate từ Prefab
            GameObject enemyGO = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            enemyGO.name = name;
            enemyGO.transform.position = position;
            enemyGO.transform.SetParent(parent);
            
            // Đăng ký undo
            Undo.RegisterCreatedObjectUndo(enemyGO, $"Spawn {name}");

            Debug.Log($"[Phase4Setup] Đã sinh quái '{name}' tại vị trí {position}.");
        }
    }
}
#endif

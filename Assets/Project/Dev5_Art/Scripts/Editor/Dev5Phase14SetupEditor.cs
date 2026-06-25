#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor script tự động thiết lập Phase 14: Cấu hình Vai trò (Role), Projectile Archer và Taunt Tank.
    /// Tự động chạy khi mở Unity / Domain Reload hoặc chạy thủ công qua thanh menu: Dev5 / Setup Phase 14 - Unit Roles and Skills
    /// </summary>
    // [InitializeOnLoad]
    public static class Dev5Phase14SetupEditor
    {
        private const string SCENE_REQUIRED = "Scene_Dev5_Art";
        private const string KNIGHT_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Knight_Unit_Prefab.prefab";
        private const string ARCHER_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Archer_Unit_Prefab.prefab";
        private const string TANK_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Tank_Unit_Prefab.prefab";
        private const string PROJ_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Projectiles/SimpleProjectile_Prefab.prefab";
        private const string PROJ_MAT_PATH = "Assets/Project/Dev5_Art/Prefabs/Projectiles/SimpleProjectile_Material.mat";

        static Dev5Phase14SetupEditor()
        {
            // Đăng ký tự động chạy thiết lập sau domain reload
            // EditorApplication.delayCall += RunSetupOnLoad;
        }

        private static void RunSetupOnLoad()
        {
            // Kiểm tra xem đã tạo projectile prefab chưa, nếu chưa thì tự động thiết lập
            GameObject proj = AssetDatabase.LoadAssetAtPath<GameObject>(PROJ_PREFAB_PATH);
            if (proj == null)
            {
                Debug.Log("[Phase14Setup] Phát hiện chưa cấu hình Phase 14. Bắt đầu tự động thiết lập...");
                SetupPhase14();
            }
        }

        [MenuItem("Dev5/Setup Phase 14 - Unit Roles and Skills")]
        public static void SetupPhase14()
        {
            // 0. Xác nhận Scene hoạt động
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.name.Equals(SCENE_REQUIRED, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[Phase14Setup] Đang chạy trên scene '{activeScene.name}' thay vì '{SCENE_REQUIRED}'.");
            }

            Undo.SetCurrentGroupName("Phase 14 UI Setup");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo Projectile Prefab nếu chưa có
            GameObject projectilePrefab = CreateProjectilePrefab();
            if (projectilePrefab == null)
            {
                Debug.LogError("[Phase14Setup] Không thể tạo projectile prefab!");
                return;
            }

            // 2. Cấu hình Knight
            ConfigureKnightPrefab();

            // 3. Cấu hình Archer
            ConfigureArcherPrefab(projectilePrefab);

            // 4. Cấu hình Tank
            ConfigureTankPrefab();

            Undo.CollapseUndoOperations(undoGroup);

            // Lưu scene
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool isSaved = EditorSceneManager.SaveScene(activeScene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Phase14Setup] ✅ Setup Phase 14 hoàn tất! Scene lưu: {(isSaved ? "Thành công" : "Thất bại")}");
        }

        private static GameObject CreateProjectilePrefab()
        {
            // Tạo thư mục nếu chưa có
            if (!AssetDatabase.IsValidFolder("Assets/Project/Dev5_Art/Prefabs/Projectiles"))
            {
                AssetDatabase.CreateFolder("Assets/Project/Dev5_Art/Prefabs", "Projectiles");
            }

            // Tạo Material màu vàng
            Material yellowMat = AssetDatabase.LoadAssetAtPath<Material>(PROJ_MAT_PATH);
            if (yellowMat == null)
            {
                Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLit != null)
                {
                    yellowMat = new Material(urpLit);
                }
                else
                {
                    yellowMat = new Material(Shader.Find("Standard"));
                }
                yellowMat.color = Color.yellow;
                yellowMat.SetColor("_BaseColor", Color.yellow);
                AssetDatabase.CreateAsset(yellowMat, PROJ_MAT_PATH);
            }

            // Tạo Sphere tạm thời
            GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempSphere.name = "SimpleProjectile_Prefab";
            tempSphere.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

            // Gắn SimpleProjectile script
            SimpleProjectile proj = tempSphere.GetComponent<SimpleProjectile>();
            if (proj == null)
            {
                proj = tempSphere.AddComponent<SimpleProjectile>();
            }

            // Gán material
            MeshRenderer mr = tempSphere.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = yellowMat;
            }

            // Xoá Collider để tránh kích hoạt vật lý ngoài mong muốn
            SphereCollider col = tempSphere.GetComponent<SphereCollider>();
            if (col != null)
            {
                Object.DestroyImmediate(col);
            }

            // Lưu thành prefab
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(tempSphere, PROJ_PREFAB_PATH);
            Object.DestroyImmediate(tempSphere);

            Debug.Log($"[Phase14Setup] Đã tạo projectile prefab thành công tại: {PROJ_PREFAB_PATH}");
            return prefabAsset;
        }

        private static void ConfigureKnightPrefab()
        {
            GameObject knightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(KNIGHT_PREFAB_PATH);
            if (knightPrefab == null)
            {
                Debug.LogWarning($"[Phase14Setup] Không tìm thấy Knight prefab tại: {KNIGHT_PREFAB_PATH}");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(knightPrefab) as GameObject;
            if (instance == null) return;

            // Gắn UnitRole.cs
            UnitRole role = instance.GetComponent<UnitRole>();
            if (role == null)
            {
                role = instance.AddComponent<UnitRole>();
            }
            role.role = UnitClassRole.Knight;
            role.attackType = AttackType.Melee;
            role.isTank = false;

            PrefabUtility.SaveAsPrefabAsset(instance, KNIGHT_PREFAB_PATH);
            Object.DestroyImmediate(instance);
            Debug.Log("[Phase14Setup] Đã cấu hình UnitRole cho Knight prefab.");
        }

        private static void ConfigureArcherPrefab(GameObject projPrefab)
        {
            GameObject archerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ARCHER_PREFAB_PATH);
            if (archerPrefab == null)
            {
                Debug.LogWarning($"[Phase14Setup] Không tìm thấy Archer prefab tại: {ARCHER_PREFAB_PATH}");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(archerPrefab) as GameObject;
            if (instance == null) return;

            // Gắn UnitRole.cs
            UnitRole role = instance.GetComponent<UnitRole>();
            if (role == null)
            {
                role = instance.AddComponent<UnitRole>();
            }
            role.role = UnitClassRole.Archer;
            role.attackType = AttackType.RangedProjectile;
            role.isTank = false;

            // Gán reference cho UnitAutoAttack
            UnitAutoAttack autoAttack = instance.GetComponent<UnitAutoAttack>();
            if (autoAttack == null)
            {
                autoAttack = instance.AddComponent<UnitAutoAttack>();
            }
            autoAttack.projectilePrefab = projPrefab;

            PrefabUtility.SaveAsPrefabAsset(instance, ARCHER_PREFAB_PATH);
            Object.DestroyImmediate(instance);
            Debug.Log("[Phase14Setup] Đã cấu hình UnitRole và gán Projectile Prefab cho Archer prefab.");
        }

        private static void ConfigureTankPrefab()
        {
            GameObject tankPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TANK_PREFAB_PATH);
            if (tankPrefab == null)
            {
                Debug.LogWarning($"[Phase14Setup] Không tìm thấy Tank prefab tại: {TANK_PREFAB_PATH}");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(tankPrefab) as GameObject;
            if (instance == null) return;

            // Gắn UnitRole.cs
            UnitRole role = instance.GetComponent<UnitRole>();
            if (role == null)
            {
                role = instance.AddComponent<UnitRole>();
            }
            role.role = UnitClassRole.Tank;
            role.attackType = AttackType.Melee;
            role.isTank = true;
            role.tauntRadius = 3f;

            PrefabUtility.SaveAsPrefabAsset(instance, TANK_PREFAB_PATH);
            Object.DestroyImmediate(instance);
            Debug.Log("[Phase14Setup] Đã cấu hình UnitRole (Tank, isTank = true, tauntRadius = 3) cho Tank prefab.");
        }
    }
}
#endif

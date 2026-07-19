using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5.Editor
{
    public static class CommanderAvatarSetupEditor
    {
        private static readonly Vector3 DefaultPosition = new Vector3(-6.25f, 1.05f, 2.8f);
        private static readonly Quaternion DefaultRotation = Quaternion.Euler(0f, 135f, 0f);
        private static readonly Quaternion CrimsonUprightRotation = Quaternion.AngleAxis(135f, Vector3.up) * Quaternion.AngleAxis(-90f, Vector3.right);
        private const string CrimsonMaterialPath = "Assets/Project/Dev5_Art/Meshy_AI_The_Crimson_Armored_E_0719064823_texture_fbx/Meshy_AI_The_Crimson_Armored_E_0719064823_texture_fbx/New Material.mat";
        private const string CrimsonFbxPath = "Assets/Project/Dev5_Art/Meshy_AI_The_Crimson_Armored_E_0719064823_texture_fbx/Meshy_AI_The_Crimson_Armored_E_0719064823_texture_fbx/Meshy_AI_The_Crimson_Armored_E_0719064823_texture.fbx";
        private const string CrimsonAlbedoPath = "Assets/Project/Dev5_Art/Meshy_AI_The_Crimson_Armored_E_0719064823_texture_fbx/Meshy_AI_The_Crimson_Armored_E_0719064823_texture_fbx/Meshy_AI_The_Crimson_Armored_E_0719064823_texture.png";
        private const string CrimsonNormalPath = "Assets/Project/Dev5_Art/Meshy_AI_The_Crimson_Armored_E_0719064823_texture_fbx/Meshy_AI_The_Crimson_Armored_E_0719064823_texture_fbx/Meshy_AI_The_Crimson_Armored_E_0719064823_texture_normal.png";
        private const string CrimsonMetallicPath = "Assets/Project/Dev5_Art/Meshy_AI_The_Crimson_Armored_E_0719064823_texture_fbx/Meshy_AI_The_Crimson_Armored_E_0719064823_texture_fbx/Meshy_AI_The_Crimson_Armored_E_0719064823_texture_metallic.png";

        [MenuItem("Hon Viet/Dev5/Commander/Create From Selected Model")]
        public static void CreateFromSelectedModel()
        {
            GameObject selectedPrefab = Selection.activeObject as GameObject;
            GameObject commander;

            if (selectedPrefab != null)
            {
                commander = PrefabUtility.InstantiatePrefab(selectedPrefab) as GameObject;
                if (commander == null)
                {
                    commander = Object.Instantiate(selectedPrefab);
                }
            }
            else
            {
                commander = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                Renderer renderer = commander.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                    renderer.sharedMaterial = shader != null ? new Material(shader) : renderer.sharedMaterial;
                    renderer.sharedMaterial.color = new Color(0.35f, 0.75f, 1f, 1f);
                }
            }

            commander.name = "PlayerCommander";
            commander.transform.SetPositionAndRotation(DefaultPosition, DefaultRotation);

            CommanderAvatar avatar = commander.GetComponent<CommanderAvatar>();
            if (avatar == null)
            {
                avatar = commander.AddComponent<CommanderAvatar>();
            }

            avatar.SetLockedPose(DefaultPosition, DefaultRotation);

            Selection.activeGameObject = commander;
            Undo.RegisterCreatedObjectUndo(commander, "Create Player Commander");
            EditorUtility.SetDirty(commander);
        }

        [MenuItem("Hon Viet/Dev5/Commander/Create Runtime Spawner")]
        public static void CreateRuntimeSpawner()
        {
            GameObject spawner = new GameObject("CommanderAvatarSpawner");
            spawner.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            spawner.AddComponent<CommanderAvatarSpawner>();

            Selection.activeGameObject = spawner;
            Undo.RegisterCreatedObjectUndo(spawner, "Create Commander Avatar Spawner");
            EditorUtility.SetDirty(spawner);
        }

        [MenuItem("Hon Viet/Dev5/Commander/Fix Crimson Commander Material Now")]
        public static void FixCrimsonCommanderMaterialNow()
        {
            CreateCrimsonCommanderInScene();
        }

        [MenuItem("Hon Viet/Dev5/Commander/Create Crimson Commander In Scene")]
        public static void CreateCrimsonCommanderInScene()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Setup Legacy Main Menu Avatar",
                "This manual command will create or update the legacy Crimson PlayerCommander, " +
                "modify its material, and save the active scene.",
                "Setup And Save",
                "Cancel");

            if (!confirmed)
            {
                return;
            }

            Material material = ConfigureCrimsonMaterial();
            if (material == null)
            {
                Debug.LogError($"[CommanderAvatarSetup] Missing material at {CrimsonMaterialPath}");
                return;
            }

            RemapCrimsonFbx(material);
            GameObject commander = EnsureCrimsonCommander(material);
            if (commander == null)
            {
                return;
            }

            int assigned = AssignMaterialToCommander(material);

            if (assigned > 0)
            {
                Scene activeScene = SceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
                Selection.activeGameObject = commander;
                Debug.Log($"[CommanderAvatarSetup] Created/fixed Crimson PlayerCommander, assigned material to {assigned} renderer(s), and saved scene.");
            }
            else
            {
                Debug.LogWarning("[CommanderAvatarSetup] Could not find PlayerCommander renderer(s). Select or create PlayerCommander, then run this menu again.");
            }
        }

        internal static Material ConfigureCrimsonMaterial()
        {
            ConfigureTextureImporters();
            AssetDatabase.ImportAsset(CrimsonMaterialPath, ImportAssetOptions.ForceUpdate);

            Material material = AssetDatabase.LoadAssetAtPath<Material>(CrimsonMaterialPath);
            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(CrimsonAlbedoPath);
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(CrimsonNormalPath);
            Texture2D metallic = AssetDatabase.LoadAssetAtPath<Texture2D>(CrimsonMetallicPath);

            if (material == null)
            {
                return null;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader != null)
            {
                material.shader = shader;
            }

            material.SetTexture("_BaseMap", albedo);
            material.SetTexture("_MainTex", albedo);
            material.SetColor("_BaseColor", Color.white);
            material.SetColor("_Color", Color.white);

            if (normal != null)
            {
                material.EnableKeyword("_NORMALMAP");
                material.SetTexture("_BumpMap", normal);
                material.SetFloat("_BumpScale", 1f);
            }

            if (metallic != null)
            {
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
                material.SetTexture("_MetallicGlossMap", metallic);
                material.SetFloat("_Metallic", 0.25f);
                material.SetFloat("_Smoothness", 0.45f);
            }

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        internal static GameObject EnsureCrimsonCommander(Material material)
        {
            GameObject existing = GameObject.Find("PlayerCommander");
            Vector3 position = existing != null ? existing.transform.position : DefaultPosition;
            Vector3 scale = existing != null ? existing.transform.localScale : Vector3.one;
            bool existingIsCrimson = existing != null && ContainsCrimsonMesh(existing);
            Quaternion rotation = existingIsCrimson ? existing.transform.rotation : CrimsonUprightRotation;

            if (existingIsCrimson)
            {
                if (LooksSideways(existing))
                {
                    rotation = CrimsonUprightRotation;
                }

                existing.transform.SetPositionAndRotation(position, rotation);
                existing.transform.localScale = scale;
                EnsureCommanderAvatar(existing, position, rotation);
                AssignMaterialToRenderers(existing, material);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(CrimsonFbxPath);
            if (modelAsset == null)
            {
                Debug.LogError($"[CommanderAvatarSetup] Missing Crimson model at {CrimsonFbxPath}");
                return existing;
            }

            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            GameObject commander = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (commander == null)
            {
                commander = Object.Instantiate(modelAsset);
            }

            commander.name = "PlayerCommander";
            commander.transform.SetPositionAndRotation(position, rotation);
            commander.transform.localScale = scale;

            EnsureCommanderAvatar(commander, position, rotation);
            AssignMaterialToRenderers(commander, material);

            Undo.RegisterCreatedObjectUndo(commander, "Create Crimson Player Commander");
            EditorUtility.SetDirty(commander);
            return commander;
        }

        internal static int AssignMaterialToCommander(Material material)
        {
            GameObject commander = GameObject.Find("PlayerCommander");
            if (commander == null)
            {
                return 0;
            }

            return AssignMaterialToRenderers(commander, material);
        }

        private static int AssignMaterialToRenderers(GameObject root, Material material)
        {
            if (root == null || material == null)
            {
                return 0;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                Undo.RecordObject(renderer, "Assign Commander Material");
                Material[] materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0)
                {
                    renderer.sharedMaterial = material;
                }
                else
                {
                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = material;
                    }

                    renderer.sharedMaterials = materials;
                }

                EditorUtility.SetDirty(renderer);
            }

            EditorUtility.SetDirty(root);
            return renderers.Length;
        }

        internal static void RemapCrimsonFbx(Material material)
        {
            ModelImporter importer = AssetImporter.GetAtPath(CrimsonFbxPath) as ModelImporter;
            if (importer == null || material == null)
            {
                return;
            }

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(CrimsonFbxPath);
            bool changed = false;
            foreach (Object asset in assets)
            {
                Material embeddedMaterial = asset as Material;
                if (embeddedMaterial == null)
                {
                    continue;
                }

                var identifier = new AssetImporter.SourceAssetIdentifier(embeddedMaterial);
                importer.AddRemap(identifier, material);
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static void ConfigureTextureImporters()
        {
            TextureImporter normalImporter = AssetImporter.GetAtPath(CrimsonNormalPath) as TextureImporter;
            if (normalImporter != null && normalImporter.textureType != TextureImporterType.NormalMap)
            {
                normalImporter.textureType = TextureImporterType.NormalMap;
                normalImporter.SaveAndReimport();
            }

            TextureImporter metallicImporter = AssetImporter.GetAtPath(CrimsonMetallicPath) as TextureImporter;
            if (metallicImporter != null && metallicImporter.sRGBTexture)
            {
                metallicImporter.sRGBTexture = false;
                metallicImporter.SaveAndReimport();
            }
        }

        private static void EnsureCommanderAvatar(GameObject commander, Vector3 position, Quaternion rotation)
        {
            CommanderAvatar avatar = commander.GetComponent<CommanderAvatar>();
            if (avatar == null)
            {
                avatar = commander.AddComponent<CommanderAvatar>();
            }

            avatar.SetLockedPose(position, rotation);
            EditorUtility.SetDirty(avatar);
        }

        private static bool ContainsCrimsonMesh(GameObject root)
        {
            Object source = PrefabUtility.GetCorrespondingObjectFromSource(root);
            if (source != null && AssetDatabase.GetAssetPath(source) == CrimsonFbxPath)
            {
                return true;
            }

            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter != null && IsCrimsonAsset(meshFilter.sharedMesh))
                {
                    return true;
                }
            }

            SkinnedMeshRenderer[] skinnedRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer skinnedRenderer in skinnedRenderers)
            {
                if (skinnedRenderer != null && IsCrimsonAsset(skinnedRenderer.sharedMesh))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool LooksSideways(GameObject root)
        {
            if (root == null)
            {
                return false;
            }

            Vector3 crimsonHeightAxis = root.transform.TransformDirection(Vector3.forward);
            return Mathf.Abs(Vector3.Dot(crimsonHeightAxis.normalized, Vector3.up)) < 0.65f;
        }

        private static bool IsCrimsonAsset(Object asset)
        {
            return asset != null && AssetDatabase.GetAssetPath(asset) == CrimsonFbxPath;
        }
    }

}

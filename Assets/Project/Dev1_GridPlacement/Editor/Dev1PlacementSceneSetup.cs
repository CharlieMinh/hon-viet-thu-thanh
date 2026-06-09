#if UNITY_EDITOR
using HonVietThuThanh.Dev1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor-only helper for setting up the Dev1 placement test scene.
/// This tool only prepares Dev1-owned objects and assets.
/// </summary>
public static class Dev1PlacementSceneSetup
{
    private const string Dev1SceneName = "Scene_Dev1_Placement";
    private const string MaterialsFolder = "Assets/Project/Dev1_GridPlacement/Materials";
    private const string PrefabsFolder = "Assets/Project/Dev1_GridPlacement/Prefabs";
    private const string CellMaterialPath = MaterialsFolder + "/MAT_Dev1_Cell.mat";
    private const string OccupiedMaterialPath = MaterialsFolder + "/MAT_Dev1_CellOccupied.mat";
    private const string HeroPrefabPath = PrefabsFolder + "/PF_Dev1_HeroPlaceholder.prefab";

    [MenuItem("Tools/Hon Viet Thu Thanh/Dev1/Setup Placement Scene")]
    public static void SetupPlacementScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name != Dev1SceneName)
        {
            bool continueSetup = EditorUtility.DisplayDialog(
                "Dev1 Placement Scene Setup",
                $"Current scene is '{currentScene.name}', not '{Dev1SceneName}'. Continue anyway?",
                "Continue",
                "Cancel");

            if (!continueSetup)
            {
                return;
            }
        }

        GameObject managerObject = GetOrCreateGameObject("Dev1_PlacementManager");
        HeroPlacementManager manager = GetOrAddComponent<HeroPlacementManager>(managerObject);

        GameObject loggerObject = GetOrCreateGameObject("Dev1_HeroPlacementDebugLogger");
        GetOrAddComponent<HeroPlacementDebugLogger>(loggerObject);

        GameObject gridRoot = GetOrCreateGameObject("Dev1_GridRoot");
        GameObject heroRoot = GetOrCreateGameObject("Dev1_HeroRoot");

        Material cellMaterial = GetOrCreateMaterial(CellMaterialPath, new Color(0.2f, 0.55f, 0.8f, 1f));
        Material occupiedMaterial = GetOrCreateMaterial(OccupiedMaterialPath, new Color(0.95f, 0.65f, 0.2f, 1f));
        GameObject heroPrefab = GetOrCreateHeroPlaceholderPrefab();

        SerializedObject serializedManager = new SerializedObject(manager);
        serializedManager.FindProperty("gridRoot").objectReferenceValue = gridRoot.transform;
        serializedManager.FindProperty("heroRoot").objectReferenceValue = heroRoot.transform;
        serializedManager.FindProperty("cellMaterial").objectReferenceValue = cellMaterial;
        serializedManager.FindProperty("occupiedMaterial").objectReferenceValue = occupiedMaterial;
        serializedManager.FindProperty("heroPlaceholderPrefab").objectReferenceValue = heroPrefab;
        serializedManager.FindProperty("rows").intValue = 5;
        serializedManager.FindProperty("columns").intValue = 8;
        serializedManager.FindProperty("cellSize").floatValue = 1f;
        serializedManager.FindProperty("gridOrigin").vector3Value = Vector3.zero;
        serializedManager.FindProperty("generateGridOnStart").boolValue = true;
        serializedManager.ApplyModifiedProperties();

        SetupCamera();
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(currentScene);
    }

    private static GameObject GetOrCreateGameObject(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);
        return existing != null ? existing : new GameObject(objectName);
    }

    private static T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : target.AddComponent<T>();
    }

    private static Material GetOrCreateMaterial(string path, Color color)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            return material;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        material = new Material(shader)
        {
            color = color
        };

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static GameObject GetOrCreateHeroPlaceholderPrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HeroPrefabPath);
        if (prefab != null)
        {
            return prefab;
        }

        GameObject temporaryCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        temporaryCube.name = "PF_Dev1_HeroPlaceholder";
        temporaryCube.transform.localScale = Vector3.one * 0.75f;
        prefab = PrefabUtility.SaveAsPrefabAsset(temporaryCube, HeroPrefabPath);
        Object.DestroyImmediate(temporaryCube);

        return prefab;
    }

    private static void SetupCamera()
    {
        GameObject cameraObject = GameObject.Find("Main Camera");
        if (cameraObject == null)
        {
            cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<AudioListener>();
        }

        Camera camera = GetOrAddComponent<Camera>(cameraObject);
        camera.transform.position = new Vector3(4f, 7f, -8f);
        camera.transform.rotation = Quaternion.Euler(50f, 0f, 0f);
        camera.fieldOfView = 45f;
    }
}
#endif

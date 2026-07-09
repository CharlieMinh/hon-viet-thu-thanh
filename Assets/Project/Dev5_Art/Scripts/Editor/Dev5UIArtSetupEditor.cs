#if UNITY_EDITOR
using HonVietThuThanh.Dev5;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace HonVietThuThanh.Dev5Editor
{
    public static class Dev5UIArtSetupEditor
    {
        private const string SceneName = "Scene_Dev5_Art";

        private const string KnightButtonSpritePath = "Assets/Project/Dev5_Art/UI/UI/Clean/Knight_clean.png";
        private const string ArcherButtonSpritePath = "Assets/Project/Dev5_Art/UI/UI/Clean/Archer_clean.png";
        private const string TankButtonSpritePath = "Assets/Project/Dev5_Art/UI/UI/Clean/Tank_clean.png";
        private const string StarSpritePath = "Assets/Project/Dev5_Art/UI/UI/Clean/Sao_clean.png";
        private const string HealthFillSpritePath = "Assets/Project/Dev5_Art/UI/UI/Clean/HealthFill_cropped.png";
        private const string HealthFrameSpritePath = "Assets/Project/Dev5_Art/UI/UI/Clean/HealthFrame_cutout.png";
        private const string InfoPanelSpritePath = "Assets/Project/Dev5_Art/UI/UI/Clean/Bang_Hien_TT_panel.png";
        private const string StartButtonSpritePath = "Assets/Project/Dev5_Art/UI/UI/Clean/start_button_ui.png";

        private static readonly string[] HeroPrefabPaths =
        {
            "Assets/Project/Dev5_Art/Prefabs/Heroes/Knight_Unit_Prefab.prefab",
            "Assets/Project/Dev5_Art/Prefabs/Heroes/Archer_Unit_Prefab.prefab",
            "Assets/Project/Dev5_Art/Prefabs/Heroes/Tank_Unit_Prefab.prefab"
        };

        private static readonly string[] EnemyPrefabPaths =
        {
            "Assets/Project/Dev5_Art/Prefabs/Enemies/Enemy_Test_Prefab.prefab",
            "Assets/Project/Dev5_Art/Prefabs/Enemies/Goblin_Enemy_Prefab.prefab",
            "Assets/Project/Dev5_Art/Prefabs/Enemies/Orc_Enemy_Prefab.prefab",
            "Assets/Project/Dev5_Art/Prefabs/Enemies/EnemyArcher_Prefab.prefab"
        };

        [MenuItem("Dev5/Setup UI Art Assets")]
        public static void SetupUIArtAssets()
        {
            Sprite knightSprite = ImportAsSprite(KnightButtonSpritePath);
            Sprite archerSprite = ImportAsSprite(ArcherButtonSpritePath);
            Sprite tankSprite = ImportAsSprite(TankButtonSpritePath);
            Sprite starSprite = ImportAsSprite(StarSpritePath);
            Sprite healthFillSprite = ImportAsSprite(HealthFillSpritePath);
            Sprite healthFrameSprite = ImportAsSprite(HealthFrameSpritePath);
            Sprite infoPanelSprite = ImportAsSprite(InfoPanelSpritePath);
            Sprite startButtonSprite = ImportAsSprite(StartButtonSpritePath);

            AssignShopButtons(knightSprite, archerSprite, tankSprite);
            AssignInfoPanels(infoPanelSprite);
            AssignStartBattleButton(startButtonSprite);
            ConfigureShopPanelLayout();
            AssignHeroPrefabArt(starSprite, healthFrameSprite, healthFillSprite);
            AssignEnemyPrefabArt(healthFrameSprite, healthFillSprite);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            bool saved = EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Dev5UIArtSetup] UI art setup complete. Scene saved: {saved}");
        }

        private static Sprite ImportAsSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[Dev5UIArtSetup] Missing texture importer at {path}");
                return null;
            }

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.alphaSource != TextureImporterAlphaSource.FromInput)
            {
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogError($"[Dev5UIArtSetup] Could not load sprite at {path}");
            }

            return sprite;
        }

        private static void AssignShopButtons(Sprite knightSprite, Sprite archerSprite, Sprite tankSprite)
        {
            ShopManager shopManager = Object.FindAnyObjectByType<ShopManager>();
            if (shopManager == null)
            {
                Debug.LogWarning("[Dev5UIArtSetup] ShopManager not found in the active scene.");
                return;
            }

            ConfigureButton(shopManager.buyKnightButton, knightSprite, "Knight");
            ConfigureButton(shopManager.buyArcherButton, archerSprite, "Archer");
            ConfigureButton(shopManager.buyTankButton, tankSprite, "Tank");
        }

        private static void ConfigureButton(Button button, Sprite sprite, string label, bool preserveAspect = true)
        {
            if (button == null || sprite == null)
            {
                Debug.LogWarning($"[Dev5UIArtSetup] Cannot configure {label} button.");
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image == null)
            {
                image = button.gameObject.AddComponent<Image>();
            }

            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.preserveAspect = preserveAspect;
            button.targetGraphic = image;

            EditorUtility.SetDirty(button);
            EditorUtility.SetDirty(image);
            Debug.Log($"[Dev5UIArtSetup] Assigned sprite to {label} button.");
        }

        private static void AssignInfoPanels(Sprite panelSprite)
        {
            ConfigurePanelBackground("UnitInfoPanel", panelSprite);
            ConfigurePanelBackground("InspectPanel", panelSprite);
            ConfigureInfoPanelLayout("UnitInfoPanel", new Vector2(320f, 310f), "UnitNameText", "UnitStatsText");
            ConfigureInfoPanelLayout("InspectPanel", new Vector2(330f, 320f), "InspectTitleText", "InspectStatsText");
        }

        private static void ConfigurePanelBackground(string objectName, Sprite sprite)
        {
            if (sprite == null)
            {
                return;
            }

            GameObject panel = GameObject.Find(objectName);
            if (panel == null)
            {
                Debug.LogWarning($"[Dev5UIArtSetup] Panel not found: {objectName}");
                return;
            }

            Image image = panel.GetComponent<Image>();
            if (image == null)
            {
                image = panel.AddComponent<Image>();
            }

            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.preserveAspect = false;
            EditorUtility.SetDirty(image);
            Debug.Log($"[Dev5UIArtSetup] Assigned info panel sprite to {objectName}.");
        }

        private static void ConfigureInfoPanelLayout(string panelName, Vector2 panelSize, string titleName, string statsName)
        {
            GameObject panel = GameObject.Find(panelName);
            if (panel == null)
            {
                return;
            }

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.sizeDelta = panelSize;
                EditorUtility.SetDirty(panelRect);
            }

            Transform title = panel.transform.Find(titleName);
            if (title != null && title.TryGetComponent(out RectTransform titleRect))
            {
                titleRect.anchorMin = new Vector2(0f, 1f);
                titleRect.anchorMax = new Vector2(1f, 1f);
                titleRect.anchoredPosition = new Vector2(0f, -32f);
                titleRect.sizeDelta = new Vector2(-100f, 32f);
                EditorUtility.SetDirty(titleRect);
            }

            Transform stats = panel.transform.Find(statsName);
            if (stats != null && stats.TryGetComponent(out RectTransform statsRect))
            {
                statsRect.anchorMin = Vector2.zero;
                statsRect.anchorMax = Vector2.one;
                statsRect.anchoredPosition = new Vector2(6f, -22f);
                statsRect.sizeDelta = new Vector2(-82f, -116f);
                EditorUtility.SetDirty(statsRect);
            }
        }

        private static void AssignStartBattleButton(Sprite startButtonSprite)
        {
            GameObject startButtonObject = GameObject.Find("StartBattleButton");
            if (startButtonObject == null)
            {
                Debug.LogWarning("[Dev5UIArtSetup] StartBattleButton not found.");
                return;
            }

            Button startButton = startButtonObject.GetComponent<Button>();
            if (startButton == null)
            {
                Debug.LogWarning("[Dev5UIArtSetup] StartBattleButton has no Button component.");
                return;
            }

            ConfigureButton(startButton, startButtonSprite, "Start Battle", false);

            RectTransform rect = startButtonObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(360f, 82f);
                rect.anchoredPosition = new Vector2(-24f, 24f);
                EditorUtility.SetDirty(rect);
            }

            TextMeshProUGUI label = startButtonObject.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = string.Empty;
                EditorUtility.SetDirty(label);
            }
        }

        private static void ConfigureShopPanelLayout()
        {
            ShopManager shopManager = Object.FindAnyObjectByType<ShopManager>();
            if (shopManager == null)
            {
                return;
            }

            Vector2 buttonSize = new Vector2(320f, 80f);
            ResizeButton(shopManager.buyKnightButton, buttonSize, -380f);
            ResizeButton(shopManager.buyArcherButton, buttonSize, 0f);
            ResizeButton(shopManager.buyTankButton, buttonSize, 380f);

            GameObject shopPanel = GameObject.Find("ShopPanel");
            if (shopPanel == null)
            {
                return;
            }

            Image panelImage = shopPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.enabled = false;
                EditorUtility.SetDirty(panelImage);
            }
        }

        private static void ResizeButton(Button button, Vector2 size, float anchoredX)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = size;
                rect.anchoredPosition = new Vector2(anchoredX, rect.anchoredPosition.y);
                EditorUtility.SetDirty(rect);
            }

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.45f);
            colors.colorMultiplier = 1f;
            button.colors = colors;
            EditorUtility.SetDirty(button);
        }

        private static void AssignHeroPrefabArt(Sprite starSprite, Sprite frameSprite, Sprite fillSprite)
        {
            foreach (string path in HeroPrefabPaths)
            {
                GameObject instance = InstantiatePrefab(path);
                if (instance == null)
                {
                    continue;
                }

                UnitStarVisual starVisual = instance.GetComponent<UnitStarVisual>();
                if (starVisual == null)
                {
                    starVisual = instance.AddComponent<UnitStarVisual>();
                }
                starVisual.starSprite = starSprite;

                HealthBar healthBar = instance.GetComponent<HealthBar>();
                if (healthBar == null)
                {
                    healthBar = instance.AddComponent<HealthBar>();
                }
                healthBar.health = instance.GetComponent<Health>();
                healthBar.frameSprite = frameSprite;
                healthBar.fillSprite = fillSprite;

                PrefabUtility.SaveAsPrefabAsset(instance, path);
                Object.DestroyImmediate(instance);
                Debug.Log($"[Dev5UIArtSetup] Assigned star and health bar art to {path}");
            }
        }

        private static void AssignEnemyPrefabArt(Sprite frameSprite, Sprite fillSprite)
        {
            foreach (string path in EnemyPrefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                GameObject instance = InstantiatePrefab(path);
                if (instance == null)
                {
                    continue;
                }

                HealthBar healthBar = instance.GetComponent<HealthBar>();
                if (healthBar == null)
                {
                    healthBar = instance.AddComponent<HealthBar>();
                }
                healthBar.health = instance.GetComponent<Health>();
                healthBar.frameSprite = frameSprite;
                healthBar.fillSprite = fillSprite;

                PrefabUtility.SaveAsPrefabAsset(instance, path);
                Object.DestroyImmediate(instance);
                Debug.Log($"[Dev5UIArtSetup] Assigned health bar art to {path}");
            }
        }

        private static GameObject InstantiatePrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[Dev5UIArtSetup] Prefab not found: {path}");
                return null;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"[Dev5UIArtSetup] Could not instantiate prefab: {path}");
            }

            return instance;
        }
    }
}
#endif

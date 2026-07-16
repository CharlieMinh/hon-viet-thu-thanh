using HonVietThuThanh.Dev5;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HonVietThuThanh.Dev5.EditorTools
{
    public static class Dev5OrcBossModelSetupEditor
    {
        private const string OrcPrefabPath = "Assets/Project/Dev5_Art/Prefabs/Enemies/Orc_Enemy_Prefab.prefab";
        private const string BossPrefabPath = "Assets/Project/Dev5_Art/Prefabs/Enemies/OrcBoss_Enemy_Prefab.prefab";
        private const string BossModelPath = "Assets/Project/Dev5_Art/Meshy_AI_Runeborn_Orc_Warlord_biped (1)/Meshy_AI_Runeborn_Orc_Warlord_biped/Meshy_AI_Runeborn_Orc_Warlord_biped_Character_output.fbx";
        private const string BossWalkAnimationPath = "Assets/Project/Dev5_Art/Meshy_AI_Runeborn_Orc_Warlord_biped (1)/Meshy_AI_Runeborn_Orc_Warlord_biped/Meshy_AI_Runeborn_Orc_Warlord_biped_Animation_Walking_frame_rate_60.fbx";
        private const string BossAttackAnimationPath = "Assets/Project/Dev5_Art/Meshy_AI_Runeborn_Orc_Warlord_biped (1)/Meshy_AI_Runeborn_Orc_Warlord_biped/Meshy_AI_Runeborn_Orc_Warlord_biped_Animation_Attack_withSkin.fbx";
        private const string BossTexturePath = "Assets/Project/Dev5_Art/Meshy_AI_Runeborn_Orc_Warlord_biped (1)/Meshy_AI_Runeborn_Orc_Warlord_biped/Meshy_AI_Runeborn_Orc_Warlord_biped_texture_0.png";
        private const string BossMaterialPath = "Assets/Project/Dev5_Art/Prefabs/Enemies/OrcBoss_Material.mat";
        private const string BossAnimatorFolder = "Assets/Project/Dev5_Art/Animations/Enemies/OrcBoss";
        private const string BossAnimatorControllerPath = BossAnimatorFolder + "/OrcBoss_AnimatorController.controller";

        [MenuItem("Tools/Hon Viet/Setup Orc Boss Model")]
        public static void SetupOrcBossModel()
        {
            AssetDatabase.Refresh();

            GameObject orcPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(OrcPrefabPath);
            GameObject bossModel = AssetDatabase.LoadAssetAtPath<GameObject>(BossModelPath);

            if (orcPrefab == null)
            {
                EditorUtility.DisplayDialog("Setup Orc Boss Model", $"Không tìm thấy Orc prefab:\n{OrcPrefabPath}", "OK");
                return;
            }

            if (bossModel == null)
            {
                EditorUtility.DisplayDialog("Setup Orc Boss Model", $"Không tìm thấy model boss:\n{BossModelPath}", "OK");
                return;
            }

            EnsureBossPrefabExists();
            ConfigureBossAnimationImports();
            AnimatorController animatorController = BuildBossAnimatorController();
            GameObject bossPrefab = BuildBossPrefab(bossModel);
            if (bossPrefab == null)
            {
                return;
            }

            int changedEntries = AssignBossPrefabToWave5(bossPrefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Setup Orc Boss Model",
                $"Đã tạo/cập nhật OrcBoss_Enemy_Prefab.\nĐã gán Animator: {(animatorController != null ? animatorController.name : "không có")}.\nĐã gán {changedEntries} entry Orc Chúa trong scene hiện tại.",
                "OK");
        }

        private static void EnsureBossPrefabExists()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath) != null)
            {
                return;
            }

            if (!AssetDatabase.CopyAsset(OrcPrefabPath, BossPrefabPath))
            {
                Debug.LogError($"[Dev5OrcBossModelSetup] Không copy được prefab từ {OrcPrefabPath} sang {BossPrefabPath}.");
            }
        }

        private static void ConfigureBossAnimationImports()
        {
            ConfigureModelImporter(BossModelPath, false);
            ConfigureModelImporter(BossWalkAnimationPath, true);
            ConfigureModelImporter(BossAttackAnimationPath, false);
        }

        private static void ConfigureModelImporter(string path, bool loopTime)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[Dev5OrcBossModelSetup] Không tìm thấy ModelImporter: {path}");
                return;
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true;

            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].loopTime = loopTime;
            }
            importer.clipAnimations = clips;

            importer.SaveAndReimport();
        }

        private static AnimatorController BuildBossAnimatorController()
        {
            AnimationClip walkClip = LoadAnimationClip(BossWalkAnimationPath);
            AnimationClip attackClip = LoadAnimationClip(BossAttackAnimationPath);

            if (walkClip == null && attackClip == null)
            {
                Debug.LogWarning("[Dev5OrcBossModelSetup] Không tìm thấy clip Walk/Attack để tạo Animator Controller.");
                return null;
            }

            EnsureFolder(BossAnimatorFolder);
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(BossAnimatorControllerPath) != null)
            {
                AssetDatabase.DeleteAsset(BossAnimatorControllerPath);
            }

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(BossAnimatorControllerPath);
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

            AnimatorState walkState = stateMachine.AddState(walkClip != null ? "Walk" : "AttackPreview");
            walkState.motion = walkClip != null ? walkClip : attackClip;
            stateMachine.defaultState = walkState;

            if (attackClip != null)
            {
                AnimatorState attackState = stateMachine.AddState("Attack", new Vector3(260f, 0f, 0f));
                attackState.motion = attackClip;

                AnimatorStateTransition anyToAttack = stateMachine.AddAnyStateTransition(attackState);
                anyToAttack.hasExitTime = false;
                anyToAttack.duration = 0.08f;
                anyToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

                AnimatorStateTransition attackToWalk = attackState.AddTransition(walkState);
                attackToWalk.hasExitTime = true;
                attackToWalk.exitTime = 0.9f;
                attackToWalk.duration = 0.12f;
            }

            AssetDatabase.SaveAssets();
            return controller;
        }

        private static AnimationClip LoadAnimationClip(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
            {
                AnimationClip clip = asset as AnimationClip;
                if (clip == null || clip.name.StartsWith("__preview__"))
                {
                    continue;
                }

                return clip;
            }

            return null;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private static GameObject BuildBossPrefab(GameObject bossModel)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(BossPrefabPath);
            if (root == null)
            {
                Debug.LogError($"[Dev5OrcBossModelSetup] Không mở được prefab: {BossPrefabPath}");
                return null;
            }

            try
            {
                root.name = "OrcBoss_Enemy_Prefab";

                Transform visual = FindChildRecursive(root.transform, "Visual");
                if (visual == null)
                {
                    visual = root.transform;
                }

                Transform modelSlot = FindChildRecursive(visual, "ModelSlot");
                if (modelSlot == null)
                {
                    GameObject modelSlotObject = new GameObject("ModelSlot");
                    modelSlot = modelSlotObject.transform;
                    modelSlot.SetParent(visual, false);
                }

                ClearBossVisualChildren(visual, modelSlot);

                GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(bossModel, root.scene);
                modelInstance.name = "OrcBoss_Model";
                modelInstance.transform.SetParent(modelSlot, false);
                modelInstance.transform.localPosition = Vector3.zero;
                modelInstance.transform.localRotation = Quaternion.identity;
                modelInstance.transform.localScale = Vector3.one;

                ApplyBossMaterial(modelInstance);
                ApplyBossAnimator(modelInstance);
                FitBossHeight(modelInstance, 2.8f);
                MoveHealthBar(root.transform, 3.2f);

                PrefabUtility.SaveAsPrefabAsset(root, BossPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath);
        }

        private static void ClearBossVisualChildren(Transform visual, Transform modelSlot)
        {
            for (int i = visual.childCount - 1; i >= 0; i--)
            {
                Transform child = visual.GetChild(i);
                if (child == modelSlot)
                {
                    continue;
                }

                Object.DestroyImmediate(child.gameObject);
            }

            for (int i = modelSlot.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(modelSlot.GetChild(i).gameObject);
            }
        }

        private static void ApplyBossAnimator(GameObject modelInstance)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(BossAnimatorControllerPath);
            if (controller == null)
            {
                return;
            }

            Animator animator = modelInstance.GetComponent<Animator>();
            if (animator == null)
            {
                animator = modelInstance.AddComponent<Animator>();
            }

            animator.runtimeAnimatorController = controller;

            Avatar avatar = LoadAvatar(BossModelPath);
            if (avatar != null)
            {
                animator.avatar = avatar;
            }

            EnemyAnimationController animationController = modelInstance.GetComponentInParent<EnemyAnimationController>();
            if (animationController != null)
            {
                animationController.FindAnimator();
            }
        }

        private static Avatar LoadAvatar(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
            {
                Avatar avatar = asset as Avatar;
                if (avatar != null)
                {
                    return avatar;
                }
            }

            return null;
        }

        private static void ApplyBossMaterial(GameObject modelInstance)
        {
            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(BossTexturePath);
            if (albedo == null)
            {
                return;
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(BossMaterialPath);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, BossMaterialPath);
            }

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", albedo);
            }
            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", albedo);
            }

            Renderer[] renderers = modelInstance.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = material;
                }
                renderer.sharedMaterials = materials;
            }
        }

        private static void FitBossHeight(GameObject modelInstance, float targetHeight)
        {
            Bounds bounds = CalculateRendererBounds(modelInstance);
            if (bounds.size.y <= 0.01f)
            {
                return;
            }

            float scale = targetHeight / bounds.size.y;
            modelInstance.transform.localScale = Vector3.one * scale;
        }

        private static Bounds CalculateRendererBounds(GameObject gameObject)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(gameObject.transform.position, Vector3.zero);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static void MoveHealthBar(Transform root, float y)
        {
            Transform healthBar = FindChildRecursive(root, "HealthBar");
            if (healthBar != null)
            {
                Vector3 localPosition = healthBar.localPosition;
                localPosition.y = y;
                healthBar.localPosition = localPosition;
            }
        }

        private static int AssignBossPrefabToWave5(GameObject bossPrefab)
        {
            WaveManager waveManager = Object.FindFirstObjectByType<WaveManager>();
            if (waveManager == null)
            {
                Debug.LogWarning("[Dev5OrcBossModelSetup] Không tìm thấy WaveManager trong scene đang mở. Prefab boss đã tạo, nhưng chưa gán vào Wave 5.");
                return 0;
            }

            SerializedObject serializedObject = new SerializedObject(waveManager);
            SerializedProperty waves = serializedObject.FindProperty("waves");
            int changedEntries = 0;

            if (waves != null)
            {
                for (int waveIndex = 0; waveIndex < waves.arraySize; waveIndex++)
                {
                    SerializedProperty wave = waves.GetArrayElementAtIndex(waveIndex);
                    SerializedProperty enemies = wave.FindPropertyRelative("enemies");
                    if (enemies == null)
                    {
                        continue;
                    }

                    for (int enemyIndex = 0; enemyIndex < enemies.arraySize; enemyIndex++)
                    {
                        SerializedProperty enemy = enemies.GetArrayElementAtIndex(enemyIndex);
                        SerializedProperty enemyName = enemy.FindPropertyRelative("enemyName");
                        SerializedProperty enemyPrefab = enemy.FindPropertyRelative("enemyPrefab");

                        if (enemyName != null && enemyPrefab != null && IsBossName(enemyName.stringValue))
                        {
                            enemyPrefab.objectReferenceValue = bossPrefab;
                            changedEntries++;
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(waveManager);
            Scene activeScene = waveManager.gameObject.scene;
            if (activeScene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }

            return changedEntries;
        }

        private static bool IsBossName(string value)
        {
            return string.Equals(value, "Orc Chúa", System.StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "Orc Chua", System.StringComparison.OrdinalIgnoreCase);
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildRecursive(root.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}

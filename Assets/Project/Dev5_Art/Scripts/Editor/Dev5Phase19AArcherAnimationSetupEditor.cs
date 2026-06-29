#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor script tự động thiết lập Phase 19A: Archer Animation Setup.
    /// Chạy qua thanh menu: Dev5 / Setup Phase 19A - Archer Animation
    /// </summary>
    public static class Dev5Phase19AArcherAnimationSetupEditor
    {
        private const string SCENE_REQUIRED = "Scene_Dev5_Art";
        private const string ARCHER_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Archer_Unit_Prefab.prefab";
        private const string ANIMATIONS_DIR = "Assets/Project/Dev5_Art/Animations/Archer";
        private const string CONTROLLER_PATH = "Assets/Project/Dev5_Art/Animations/Archer/Archer_AnimatorController.controller";

        [MenuItem("Dev5/Setup Phase 19A - Archer Animation")]
        public static void SetupPhase19AArcherAnimation()
        {
            var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (!activeScene.name.Equals(SCENE_REQUIRED, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[Phase19ASetup] Đang chạy trên scene '{activeScene.name}' thay vì '{SCENE_REQUIRED}'.");
            }

            Undo.SetCurrentGroupName("Phase 19A Archer Animation Setup");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo thư mục Animation nếu chưa có
            if (!System.IO.Directory.Exists(ANIMATIONS_DIR))
            {
                System.IO.Directory.CreateDirectory(ANIMATIONS_DIR);
                AssetDatabase.Refresh();
            }

            // 2. Quét và phân loại các file FBX từ thư mục Animation/
            string realAnimDir = "Assets/Project/Dev5_Art/Animations/Archer/Animation";
            string idleFbx = null;
            string moveFbx = null;
            string attackFbx = null;
            string deathFbx = null;

            if (System.IO.Directory.Exists(realAnimDir))
            {
                string[] fbxFiles = System.IO.Directory.GetFiles(realAnimDir, "*.fbx", System.IO.SearchOption.AllDirectories);
                foreach (string file in fbxFiles)
                {
                    string normalizedPath = file.Replace('\\', '/');
                    string fileNameLower = System.IO.Path.GetFileName(file).ToLower();
                    string dirNameLower = System.IO.Path.GetDirectoryName(file).ToLower().Replace('\\', '/');

                    if (dirNameLower.Contains("/idle"))
                    {
                        idleFbx = normalizedPath;
                    }
                    else if (dirNameLower.Contains("/t_pose") && fileNameLower.Contains("walking"))
                    {
                        moveFbx = normalizedPath;
                    }
                    else if (dirNameLower.Contains("/attack"))
                    {
                        attackFbx = normalizedPath;
                    }
                    else if (dirNameLower.Contains("/dead") || dirNameLower.Contains("/death"))
                    {
                        deathFbx = normalizedPath;
                    }
                }
            }

            // 3. Định cấu hình Rig = Humanoid và Loop settings cho từng FBX
            ConfigureModelImporter(idleFbx, true);
            ConfigureModelImporter(moveFbx, true);
            ConfigureModelImporter(attackFbx, false);
            ConfigureModelImporter(deathFbx, false);

            AssetDatabase.Refresh();

            // 4. Lấy các clip thật hoặc dùng placeholder làm fallback
            AnimationClip idleClip = GetClipFromFBX(idleFbx) ?? GetOrCreateClip(ANIMATIONS_DIR + "/Archer_Idle_Placeholder.anim");
            AnimationClip moveClip = GetClipFromFBX(moveFbx) ?? GetOrCreateClip(ANIMATIONS_DIR + "/Archer_Move_Placeholder.anim");
            AnimationClip attackClip = GetClipFromFBX(attackFbx) ?? GetOrCreateClip(ANIMATIONS_DIR + "/Archer_Attack_Placeholder.anim");
            AnimationClip deathClip = GetClipFromFBX(deathFbx) ?? GetOrCreateClip(ANIMATIONS_DIR + "/Archer_Death_Placeholder.anim");

            // Báo cáo kết quả map hoạt ảnh
            Debug.Log($"[Phase19ASetup] Mapping clips:\n" +
                      $"- Idle: {(idleFbx != null ? System.IO.Path.GetFileName(idleFbx) : "Fallback to placeholder")}\n" +
                      $"- Move: {(moveFbx != null ? System.IO.Path.GetFileName(moveFbx) : "Fallback to placeholder")}\n" +
                      $"- Attack: {(attackFbx != null ? System.IO.Path.GetFileName(attackFbx) : "Fallback to placeholder")}\n" +
                      $"- Death: {(deathFbx != null ? System.IO.Path.GetFileName(deathFbx) : "Fallback to placeholder")}");

            // 5. Tạo hoặc tải Animator Controller
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(CONTROLLER_PATH);
                Debug.Log($"[Phase19ASetup] Đã tạo Animator Controller mới tại: {CONTROLLER_PATH}");
            }

            // 6. Thiết lập Parameters
            EnsureParameter(controller, "IsMoving", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "Attack", AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "Death", AnimatorControllerParameterType.Trigger);

            // 7. Thiết lập States và Transitions
            SetupStatesAndTransitions(controller, idleClip, moveClip, attackClip, deathClip);

            // 8. Cập nhật Prefab Archer (sử dụng Model từ Alert/Idle FBX làm visual chính)
            string modelToInstantiate = idleFbx ?? moveFbx ?? attackFbx ?? deathFbx;
            RestructureArcherPrefab(ARCHER_PREFAB_PATH, controller, modelToInstantiate);

            Undo.CollapseUndoOperations(undoGroup);

            // Đánh dấu controller dirty để lưu thay đổi
            EditorUtility.SetDirty(controller);

            // Lưu thay đổi asset database
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Phase19ASetup] ✅ Setup Phase 19A - Archer Animation hoàn tất!");
        }

        private static void ConfigureModelImporter(string fbxPath, bool isLoop)
        {
            if (string.IsNullOrEmpty(fbxPath)) return;

            ModelImporter modelImporter = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (modelImporter != null)
            {
                bool changed = false;
                if (modelImporter.animationType != ModelImporterAnimationType.Human)
                {
                    modelImporter.animationType = ModelImporterAnimationType.Human;
                    changed = true;
                }

                ModelImporterClipAnimation[] clips = modelImporter.clipAnimations;
                if (clips == null || clips.Length == 0)
                {
                    clips = modelImporter.defaultClipAnimations;
                }

                if (clips != null && clips.Length > 0)
                {
                    for (int i = 0; i < clips.Length; i++)
                    {
                        if (clips[i].loopTime != isLoop)
                        {
                            clips[i].loopTime = isLoop;
                            changed = true;
                        }
                    }
                    if (changed)
                    {
                        modelImporter.clipAnimations = clips;
                    }
                }

                if (changed)
                {
                    modelImporter.SaveAndReimport();
                    Debug.Log($"[Phase19ASetup] Configured Rig=Humanoid and Loop={isLoop} on: {fbxPath}");
                }
            }
        }

        private static AnimationClip GetClipFromFBX(string fbxPath)
        {
            if (string.IsNullOrEmpty(fbxPath)) return null;

            var assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            foreach (var asset in assets)
            {
                if (asset is AnimationClip clip && !clip.name.StartsWith("__"))
                {
                    return clip;
                }
            }
            return null;
        }

        private static AnimationClip GetOrCreateClip(string path)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
                Debug.Log($"[Phase19ASetup] Đã tạo AnimationClip placeholder mới tại: {path}");
            }
            return clip;
        }

        private static void EnsureParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            foreach (var param in controller.parameters)
            {
                if (param.name == name && param.type == type)
                    return;
            }
            controller.AddParameter(name, type);
        }

        private static void SetupStatesAndTransitions(
            AnimatorController controller,
            AnimationClip idle,
            AnimationClip move,
            AnimationClip attack,
            AnimationClip death)
        {
            var rootStateMachine = controller.layers[0].stateMachine;

            // Lấy hoặc tạo states
            AnimatorState idleState = FindOrCreateState(rootStateMachine, "Idle", idle);
            AnimatorState moveState = FindOrCreateState(rootStateMachine, "Move", move);
            AnimatorState attackState = FindOrCreateState(rootStateMachine, "Attack", attack);
            AnimatorState deathState = FindOrCreateState(rootStateMachine, "Death", death);

            // Đặt default state
            rootStateMachine.defaultState = idleState;

            // Xóa toàn bộ transitions cũ để build mới không trùng lặp
            idleState.transitions = new AnimatorStateTransition[0];
            moveState.transitions = new AnimatorStateTransition[0];
            attackState.transitions = new AnimatorStateTransition[0];
            deathState.transitions = new AnimatorStateTransition[0];
            rootStateMachine.anyStateTransitions = new AnimatorStateTransition[0];

            // 1. Idle -> Move khi IsMoving == true
            var idleToMove = idleState.AddTransition(moveState);
            idleToMove.hasExitTime = false;
            idleToMove.duration = 0.1f;
            idleToMove.AddCondition(AnimatorConditionMode.If, 0f, "IsMoving");

            // 2. Move -> Idle khi IsMoving == false
            var moveToIdle = moveState.AddTransition(idleState);
            moveToIdle.hasExitTime = false;
            moveToIdle.duration = 0.1f;
            moveToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsMoving");

            // 3. Any State -> Attack khi trigger Attack
            var anyToAttack = rootStateMachine.AddAnyStateTransition(attackState);
            anyToAttack.hasExitTime = false;
            anyToAttack.duration = 0.1f;
            anyToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

            // 4. Attack -> Idle/Move sau khi attack kết thúc (Exit Time)
            var attackToIdle = attackState.AddTransition(idleState);
            attackToIdle.hasExitTime = true;
            attackToIdle.exitTime = 1.0f; // Chạy hết animation của clip
            attackToIdle.duration = 0.2f;
            attackToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsMoving");

            var attackToMove = attackState.AddTransition(moveState);
            attackToMove.hasExitTime = true;
            attackToMove.exitTime = 1.0f;
            attackToMove.duration = 0.2f;
            attackToMove.AddCondition(AnimatorConditionMode.If, 0f, "IsMoving");

            // 5. Any State -> Death khi trigger Death
            var anyToDeath = rootStateMachine.AddAnyStateTransition(deathState);
            anyToDeath.hasExitTime = false;
            anyToDeath.duration = 0.1f;
            anyToDeath.AddCondition(AnimatorConditionMode.If, 0f, "Death");
        }

        private static AnimatorState FindOrCreateState(AnimatorStateMachine stateMachine, string name, Motion motion)
        {
            foreach (var childState in stateMachine.states)
            {
                if (childState.state.name == name)
                {
                    childState.state.motion = motion;
                    return childState.state;
                }
            }
            AnimatorState state = stateMachine.AddState(name);
            state.motion = motion;
            return state;
        }

        private static void RestructureArcherPrefab(string path, AnimatorController controller, string modelFbxPath)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefabAsset == null)
            {
                Debug.LogWarning($"[Phase19ASetup] Không tìm thấy prefab Archer tại: {path}");
                return;
            }

            // Instantiate tạm prefab trong Scene
            GameObject instance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"[Phase19ASetup] Không thể instantiate prefab Archer: {prefabAsset.name}");
                return;
            }

            // 1. Thêm CharacterAnimationController vào root
            CharacterAnimationController animCtrl = instance.GetComponent<CharacterAnimationController>();
            if (animCtrl == null)
            {
                animCtrl = instance.AddComponent<CharacterAnimationController>();
                Debug.Log($"[Phase19ASetup] Đã thêm CharacterAnimationController vào root của Archer prefab.");
            }

            // Gán AudioSources và AudioClips
            AudioSource[] audioSources = instance.GetComponents<AudioSource>();
            AudioSource attackSource = null;
            AudioSource deathSource = null;

            if (audioSources.Length >= 2)
            {
                attackSource = audioSources[0];
                deathSource = audioSources[1];
            }
            else if (audioSources.Length == 1)
            {
                attackSource = audioSources[0];
                deathSource = instance.AddComponent<AudioSource>();
            }
            else
            {
                attackSource = instance.AddComponent<AudioSource>();
                deathSource = instance.AddComponent<AudioSource>();
            }

            // Cấu hình AudioSource
            attackSource.playOnAwake = false;
            attackSource.loop = false;
            deathSource.playOnAwake = false;
            deathSource.loop = false;

            // Load AudioClips
            string attackClipPath = "Assets/Project/Dev5_Art/Audio/Archer/Audio/Attack/freesound_community-punches-32563.mp3";
            string deathClipPath = "Assets/Project/Dev5_Art/Audio/Archer/Audio/Dead/freesound_community-male_grunts-100281.mp3";

            AudioClip attackClip = AssetDatabase.LoadAssetAtPath<AudioClip>(attackClipPath);
            AudioClip deathClip = AssetDatabase.LoadAssetAtPath<AudioClip>(deathClipPath);

            // Gán vào CharacterAnimationController thông qua SerializedObject
            SerializedObject so = new SerializedObject(animCtrl);
            so.FindProperty("attackAudioSource").objectReferenceValue = attackSource;
            so.FindProperty("deathAudioSource").objectReferenceValue = deathSource;
            so.FindProperty("attackClip").objectReferenceValue = attackClip;
            so.FindProperty("deathClip").objectReferenceValue = deathClip;
            so.ApplyModifiedProperties();

            Debug.Log($"[Phase19ASetup] Đã tự động cấu hình AudioSource và gán AudioClips cho Archer Prefab.");

            // 2. Cập nhật ModelSlot và Animator
            Transform visualChild = instance.transform.Find("Visual");
            if (visualChild != null)
            {
                Transform modelSlotChild = visualChild.Find("ModelSlot");
                if (modelSlotChild != null)
                {
                    GameObject modelFbx = AssetDatabase.LoadAssetAtPath<GameObject>(modelFbxPath);
                    if (modelFbx != null)
                    {
                        // Luôn dọn dẹp ModelSlot để đảm bảo chỉ dùng model Skinned FBX thật sự
                        for (int i = modelSlotChild.childCount - 1; i >= 0; i--)
                        {
                            UnityEngine.Object.DestroyImmediate(modelSlotChild.GetChild(i).gameObject);
                        }

                        GameObject modelInstance = PrefabUtility.InstantiatePrefab(modelFbx, modelSlotChild) as GameObject;
                        if (modelInstance != null)
                        {
                            modelInstance.name = "Meshy_AI_Dragonbow_General_biped";

                            // Fix: Dịch chuyển model xuống -1 để chân chạm đất
                            // (Root prefab đặt ở cellTop + 1.0 để CapsuleCollider center = 0, cao 2 đơn vị,
                            //  nên chân model phải tại localY = -1.0 để chạm mặt ô cờ)
                            modelInstance.transform.localPosition = new Vector3(0f, -1f, 0f);
                            modelInstance.transform.localRotation = Quaternion.identity;
                            modelInstance.transform.localScale = Vector3.one;

                            // Cấu hình Material và Texture cho SkinnedMeshRenderer của model
                            string fbxDir = System.IO.Path.GetDirectoryName(modelFbxPath).Replace('\\', '/');
                            string texPath = fbxDir + "/Meshy_AI_Dragonbow_General_biped_texture_0.png";
                            string metallicPath = fbxDir + "/Meshy_AI_Dragonbow_General_biped_texture_0_metallic.png";
                            string roughnessPath = fbxDir + "/Meshy_AI_Dragonbow_General_biped_texture_0_roughness.png";

                            Texture2D baseTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                            Texture2D metallicTex = AssetDatabase.LoadAssetAtPath<Texture2D>(metallicPath);
                            Texture2D roughnessTex = AssetDatabase.LoadAssetAtPath<Texture2D>(roughnessPath);

                            string matPath = "Assets/Project/Dev5_Art/Materials/M_Unit_Archer.mat";
                            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                            if (mat != null)
                            {
                                if (baseTex != null)
                                {
                                    mat.SetTexture("_BaseMap", baseTex);
                                    mat.SetTexture("_MainTex", baseTex);
                                }
                                if (metallicTex != null)
                                {
                                    mat.SetTexture("_MetallicGlossMap", metallicTex);
                                    mat.EnableKeyword("_METALLICGLOSSMAP");
                                }
                                EditorUtility.SetDirty(mat);

                                var smr = modelInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                                if (smr != null)
                                {
                                    smr.sharedMaterial = mat;
                                }
                            }

                            Animator animator = modelInstance.GetComponent<Animator>();
                            if (animator == null)
                            {
                                animator = modelInstance.AddComponent<Animator>();
                            }
                            animator.runtimeAnimatorController = controller;
                            animator.applyRootMotion = false;
                            animator.updateMode = AnimatorUpdateMode.Normal;

                            // Tìm Avatar từ tất cả assets bên trong FBX model (ưu tiên idle FBX có skinned mesh)
                            Avatar foundAvatar = null;

                            // 1. Thử lấy từ Animator component của FBX gốc
                            Animator fbxModelAnimator = modelFbx.GetComponent<Animator>();
                            if (fbxModelAnimator != null && fbxModelAnimator.avatar != null)
                            {
                                foundAvatar = fbxModelAnimator.avatar;
                            }

                            // 2. Nếu không thấy, load toàn bộ assets trong FBX
                            if (foundAvatar == null)
                            {
                                var assets = AssetDatabase.LoadAllAssetsAtPath(modelFbxPath);
                                foreach (var asset in assets)
                                {
                                    if (asset is Avatar av && av.isValid)
                                    {
                                        foundAvatar = av;
                                        break;
                                    }
                                }
                            }

                            if (foundAvatar != null)
                            {
                                animator.avatar = foundAvatar;
                                Debug.Log($"[Phase19ASetup] ✅ Gán Avatar '{foundAvatar.name}' cho Animator (isValid={foundAvatar.isValid}, isHuman={foundAvatar.isHuman})");
                            }
                            else
                            {
                                Debug.LogWarning($"[Phase19ASetup] ⚠️ Không tìm thấy Avatar trong {modelFbxPath}. Animation sẽ không chạy!");
                            }
                            string avatarName = animator.avatar != null ? animator.avatar.name : "(null)";
                            Debug.Log($"[Phase19ASetup] Khởi tạo model thật với localPosition.y=-1 (chân chạm đất), Animator với Avatar: {avatarName}");
                        }
                    }
                }
            }

            // Tự động detect để Animator reference của script được cập nhật
            animCtrl.FindAnimator();

            // 3. Lưu lại prefab và xóa instance tạm
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            UnityEngine.Object.DestroyImmediate(instance);

            Debug.Log($"[Phase19ASetup] Đã cập nhật thành công Archer Prefab tại: {path}");
        }
    }
}
#endif

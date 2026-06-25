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

            // 2. Tạo hoặc tải các clip placeholder (.anim)
            AnimationClip idleClip = GetOrCreateClip(ANIMATIONS_DIR + "/Archer_Idle_Placeholder.anim");
            AnimationClip moveClip = GetOrCreateClip(ANIMATIONS_DIR + "/Archer_Move_Placeholder.anim");
            AnimationClip attackClip = GetOrCreateClip(ANIMATIONS_DIR + "/Archer_Attack_Placeholder.anim");
            AnimationClip deathClip = GetOrCreateClip(ANIMATIONS_DIR + "/Archer_Death_Placeholder.anim");

            // 3. Tạo hoặc tải Animator Controller
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(CONTROLLER_PATH);
                Debug.Log($"[Phase19ASetup] Đã tạo Animator Controller mới tại: {CONTROLLER_PATH}");
            }

            // 4. Thiết lập Parameters
            EnsureParameter(controller, "IsMoving", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "Attack", AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "Death", AnimatorControllerParameterType.Trigger);

            // 5. Thiết lập States và Transitions
            SetupStatesAndTransitions(controller, idleClip, moveClip, attackClip, deathClip);

            // 6. Cập nhật Prefab Archer
            RestructureArcherPrefab(ARCHER_PREFAB_PATH, controller);

            Undo.CollapseUndoOperations(undoGroup);

            // Lưu thay đổi asset database
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Phase19ASetup] ✅ Setup Phase 19A - Archer Animation hoàn tất!");
        }

        private static AnimationClip GetOrCreateClip(string path)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                // Set default properties if needed
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

            // 4. Attack -> Idle sau khi attack kết thúc (Exit Time)
            var attackToIdle = attackState.AddTransition(idleState);
            attackToIdle.hasExitTime = true;
            attackToIdle.exitTime = 1.0f; // Chạy hết animation của clip
            attackToIdle.duration = 0.2f;

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

        private static void RestructureArcherPrefab(string path, AnimatorController controller)
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

            // 2. Cập nhật Animator nếu model con trong ModelSlot có sẵn Animator
            Transform visualChild = instance.transform.Find("Visual");
            if (visualChild != null)
            {
                Transform modelSlotChild = visualChild.Find("ModelSlot");
                if (modelSlotChild != null)
                {
                    Animator modelAnimator = modelSlotChild.GetComponentInChildren<Animator>();
                    if (modelAnimator != null)
                    {
                        modelAnimator.runtimeAnimatorController = controller;
                        modelAnimator.applyRootMotion = false;
                        Debug.Log($"[Phase19ASetup] Đã gán AnimatorController và đặt applyRootMotion = false cho model con của Archer.");
                    }
                }
            }

            // Tự động detect để Animator reference của script được cập nhật
            animCtrl.FindAnimator();

            // 3. Lưu lại prefab và xóa instance tạm
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);

            Debug.Log($"[Phase19ASetup] Đã cập nhật thành công Archer Prefab tại: {path}");
        }
    }
}
#endif

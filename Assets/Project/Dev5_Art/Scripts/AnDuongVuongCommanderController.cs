using System.Collections;
using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    [DisallowMultipleComponent]
    public sealed class AnDuongVuongCommanderController : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string commandTrigger = "Command";
        [SerializeField] private string victoryTrigger = "Victory";
        [SerializeField] private string defeatTrigger = "Defeat";
        [SerializeField] private string idleStateName = "Idle";
        [SerializeField] private string commandStateName = "Command";
        [SerializeField, Min(0f)] private float commandCooldown = 0.5f;

        [Header("Ambient Command Loop")]
        [SerializeField] private bool loopCommandAnimation = true;
        [SerializeField, Min(0f)] private float commandLoopDelay = 2f;

        [Header("Anchor Safety")]
        [SerializeField] private bool lockToAnchor = true;

        private GamePhaseManager subscribedPhaseManager;
        private Vector3 anchoredLocalPosition;
        private Quaternion anchoredLocalRotation;
        private float lastCommandTime = float.NegativeInfinity;
        private Coroutine commandLoopCoroutine;

        private void Awake()
        {
            CacheAnimator();
            CacheAnchorPose();
        }

        private void OnEnable()
        {
            CacheAnimator();
            CacheAnchorPose();
            TrySubscribeToGamePhase();
            StartCommandLoop();
        }

        private void Start()
        {
            TrySubscribeToGamePhase();
            StartCommandLoop();
        }

        private void Update()
        {
            if (subscribedPhaseManager == null)
            {
                TrySubscribeToGamePhase();
            }
        }

        private void LateUpdate()
        {
            if (!lockToAnchor)
            {
                return;
            }

            transform.localPosition = anchoredLocalPosition;
            transform.localRotation = anchoredLocalRotation;
        }

        private void OnDisable()
        {
            StopCommandLoop();
            UnsubscribeFromGamePhase();
        }

        public void PlayCommand()
        {
            TryPlayCommand();
        }

        public void PlayVictory()
        {
            PlayOptionalTrigger(victoryTrigger);
        }

        public void PlayDefeat()
        {
            PlayOptionalTrigger(defeatTrigger);
        }

        public void ReturnToIdle()
        {
            if (animator == null || string.IsNullOrWhiteSpace(idleStateName))
            {
                return;
            }

            int idleStateHash = Animator.StringToHash(idleStateName);
            if (animator.HasState(0, idleStateHash))
            {
                animator.Play(idleStateHash, 0, 0f);
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Combat:
                    if (!loopCommandAnimation)
                    {
                        PlayCommand();
                    }
                    break;
                case GameState.Win:
                    StopCommandLoop();
                    ReturnToIdle();
                    PlayVictory();
                    break;
                case GameState.Lose:
                    StopCommandLoop();
                    ReturnToIdle();
                    PlayDefeat();
                    break;
            }
        }

        private void StartCommandLoop()
        {
            if (!loopCommandAnimation || commandLoopCoroutine != null || !isActiveAndEnabled)
            {
                return;
            }

            commandLoopCoroutine = StartCoroutine(CommandLoop());
        }

        private void StopCommandLoop()
        {
            if (commandLoopCoroutine == null)
            {
                return;
            }

            StopCoroutine(commandLoopCoroutine);
            commandLoopCoroutine = null;
        }

        private IEnumerator CommandLoop()
        {
            yield return null;

            while (loopCommandAnimation && isActiveAndEnabled)
            {
                if (!TryPlayCommand())
                {
                    break;
                }

                yield return WaitForCommandCycle();

                if (commandLoopDelay > 0f)
                {
                    yield return new WaitForSeconds(commandLoopDelay);
                }
            }

            commandLoopCoroutine = null;
        }

        private IEnumerator WaitForCommandCycle()
        {
            if (animator == null || string.IsNullOrWhiteSpace(commandStateName))
            {
                yield break;
            }

            int entryFramesRemaining = 120;
            while (isActiveAndEnabled &&
                   !animator.GetCurrentAnimatorStateInfo(0).IsName(commandStateName) &&
                   entryFramesRemaining-- > 0)
            {
                yield return null;
            }

            while (isActiveAndEnabled)
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                if (!state.IsName(commandStateName) && !animator.IsInTransition(0))
                {
                    yield break;
                }

                if (state.IsName(commandStateName) &&
                    state.normalizedTime >= 1f &&
                    !animator.IsInTransition(0))
                {
                    ReturnToIdle();
                    yield break;
                }

                yield return null;
            }
        }

        private bool TryPlayCommand()
        {
            if (!CanUseTrigger(commandTrigger) || Time.unscaledTime < lastCommandTime + commandCooldown)
            {
                return false;
            }

            lastCommandTime = Time.unscaledTime;
            animator.ResetTrigger(commandTrigger);
            animator.SetTrigger(commandTrigger);
            return true;
        }

        private void CacheAnimator()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (animator != null)
            {
                animator.applyRootMotion = false;
            }
        }

        private void CacheAnchorPose()
        {
            anchoredLocalPosition = transform.localPosition;
            anchoredLocalRotation = transform.localRotation;
        }

        private void TrySubscribeToGamePhase()
        {
            GamePhaseManager phaseManager = GamePhaseManager.Instance;
            if (phaseManager == null || phaseManager == subscribedPhaseManager)
            {
                return;
            }

            UnsubscribeFromGamePhase();
            subscribedPhaseManager = phaseManager;
            subscribedPhaseManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void UnsubscribeFromGamePhase()
        {
            if (subscribedPhaseManager == null)
            {
                return;
            }

            subscribedPhaseManager.OnGameStateChanged -= HandleGameStateChanged;
            subscribedPhaseManager = null;
        }

        private void PlayOptionalTrigger(string triggerName)
        {
            if (!CanUseTrigger(triggerName))
            {
                return;
            }

            animator.ResetTrigger(triggerName);
            animator.SetTrigger(triggerName);
        }

        private bool CanUseTrigger(string triggerName)
        {
            if (animator == null || string.IsNullOrWhiteSpace(triggerName))
            {
                return false;
            }

            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].type == AnimatorControllerParameterType.Trigger &&
                    parameters[i].name == triggerName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

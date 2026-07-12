using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Dev5-only wrapper that exposes simple animation commands for enemy visuals.
    /// It only updates Animator parameters and does not own movement, combat, or HP logic.
    /// </summary>
    public class EnemyAnimationController : MonoBehaviour
    {
        private const string IsMovingParameter = "IsMoving";
        private const string IsRunningParameter = "IsRunning";
        private const string AttackParameter = "Attack";
        private const string DeathParameter = "Death";

        [SerializeField] private Animator animator;

        [Header("Audio Setup")]
        [SerializeField] private bool useAnimationEventsForAudio = true; // Use precise Animation Events
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip moveClip;   // Âm thanh di chuyển (loop/step)
        [SerializeField] private AudioClip attackClip; // Âm thanh tấn công (OneShot)
        [SerializeField] private AudioClip deathClip;  // Âm thanh khi chết (OneShot)

        private bool isDying = false;

        private void Awake()
        {
            FindAnimator();
            SetupAudioSource();
        }

        private void OnEnable()
        {
            if (GamePhaseManager.Instance != null)
            {
                GamePhaseManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (GamePhaseManager.Instance != null)
            {
                GamePhaseManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
            StopMoveSound();
        }

        private void Start()
        {
            // Re-subscribe in Start to ensure GamePhaseManager.Instance is ready
            if (GamePhaseManager.Instance != null)
            {
                GamePhaseManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
                GamePhaseManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GamePhaseManager.Instance != null)
            {
                GamePhaseManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
            StopMoveSound();
        }

        private void HandleGameStateChanged(GameState state)
        {
            // Force-stop looping movement audio when a wave is completed or is not in Combat state
            if (state == GameState.WaveCompleted || state == GameState.Win || state == GameState.Lose || state == GameState.Preparation)
            {
                StopMoveSound();
            }
        }

        private void SetupAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // Âm thanh 3D
            audioSource.minDistance = 2f;
            audioSource.maxDistance = 200f;
        }

        public void FindAnimator()
        {
            if (animator != null)
            {
                EnsureEventForwarder();
                return;
            }

            Transform visual = transform.Find("Visual");
            if (visual != null)
            {
                Transform modelSlot = visual.Find("ModelSlot");
                if (modelSlot != null)
                {
                    animator = modelSlot.GetComponentInChildren<Animator>(true);
                }
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (animator != null)
            {
                animator.applyRootMotion = false;
                Debug.Log($"[{gameObject.name}] FindAnimator: Animator found on child '{animator.gameObject.name}' with controller: {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "null")} and avatar: {(animator.avatar != null ? animator.avatar.name : "null")}");
                EnsureEventForwarder();
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] FindAnimator: Animator NOT found in children!");
            }
        }

        private void EnsureEventForwarder()
        {
            if (animator != null)
            {
                var forwarder = animator.GetComponent<EnemyAnimationEventForwarder>();
                if (forwarder == null)
                {
                    forwarder = animator.gameObject.AddComponent<EnemyAnimationEventForwarder>();
                    Debug.Log($"[{gameObject.name}] EnsureEventForwarder: Dynamically added EnemyAnimationEventForwarder to '{animator.gameObject.name}'");
                }
            }
        }

        public void RebindAnimator()
        {
            animator = null;
            FindAnimator();
            if (animator != null)
            {
                animator.Rebind();
            }
        }

        public void PlayIdle()
        {
            if (isDying) return;
            if (!TryGetAnimator())
            {
                return;
            }
            Debug.Log($"[{gameObject.name}] PlayIdle: Setting IsMoving = false, IsRunning = false");
            animator.SetBool(IsMovingParameter, false);
            animator.SetBool(IsRunningParameter, false);

            StopMoveSound();
        }

        public void PlayWalk()
        {
            if (isDying) return;
            if (!TryGetAnimator())
            {
                return;
            }
            Debug.Log($"[{gameObject.name}] PlayWalk: Setting IsMoving = true, IsRunning = false");
            animator.SetBool(IsMovingParameter, true);
            animator.SetBool(IsRunningParameter, false);

            if (!useAnimationEventsForAudio)
            {
                PlayMoveSound(1.0f);
            }
        }

        public void PlayRun()
        {
            if (isDying) return;
            if (!TryGetAnimator())
            {
                return;
            }
            Debug.Log($"[{gameObject.name}] PlayRun: Setting IsMoving = true, IsRunning = true");
            animator.SetBool(IsMovingParameter, true);
            animator.SetBool(IsRunningParameter, true);

            if (!useAnimationEventsForAudio)
            {
                PlayMoveSound(1.3f); // Tốc độ nhanh hơn, pitch cao hơn một chút
            }
        }

        public void PlayAttack()
        {
            if (isDying) return;
            if (!TryGetAnimator())
            {
                return;
            }
            Debug.Log($"[{gameObject.name}] PlayAttack: Triggering Attack");
            animator.SetTrigger(AttackParameter);

            if (!useAnimationEventsForAudio)
            {
                if (audioSource != null && attackClip != null)
                {
                    audioSource.PlayOneShot(attackClip);
                    Debug.Log($"[{gameObject.name}] PlayAttack (Legacy): Played attack sound '{attackClip.name}'");
                }
            }
        }

        // Precise audio event triggers called via EnemyAnimationEventForwarder
        public void TriggerFootstepSound()
        {
            if (isDying) return;
            if (useAnimationEventsForAudio && audioSource != null && moveClip != null)
            {
                audioSource.PlayOneShot(moveClip);
                Debug.Log($"[{gameObject.name}] TriggerFootstepSound: Played step sound '{moveClip.name}'");
            }
        }

        public void TriggerAttackSound()
        {
            if (isDying) return;
            if (useAnimationEventsForAudio && audioSource != null && attackClip != null)
            {
                audioSource.PlayOneShot(attackClip);
                Debug.Log($"[{gameObject.name}] TriggerAttackSound: Played attack sound '{attackClip.name}'");
            }
        }

        public void PlayDeath()
        {
            if (isDying) return;
            isDying = true;

            if (!TryGetAnimator())
            {
                Destroy(gameObject);
                return;
            }
            Debug.Log($"[{gameObject.name}] PlayDeath: Triggering Death");
            animator.SetTrigger(DeathParameter);

            StopMoveSound();
            if (audioSource != null && deathClip != null)
            {
                audioSource.PlayOneShot(deathClip);
                Debug.Log($"[{gameObject.name}] PlayDeath: Played death sound '{deathClip.name}'");
            }

            StartCoroutine(DestroyAfterDeathAnimation());
        }

        private System.Collections.IEnumerator DestroyAfterDeathAnimation()
        {
            float clipLength = 2.0f; // fallback

            if (animator != null)
            {
                // Wait for the animator state machine to start transitioning/transitioning to Death state
                yield return null;
                yield return null;

                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("Death") || stateInfo.IsTag("Death") || stateInfo.shortNameHash == Animator.StringToHash("Death"))
                {
                    float rawLength = stateInfo.length;
                    if (rawLength > 0.1f)
                    {
                        clipLength = rawLength;
                    }
                }
            }

            Debug.Log($"[{gameObject.name}] DestroyAfterDeathAnimation: Waiting {clipLength + 0.5f:F2}s before destroying gameObject.");
            yield return new WaitForSeconds(clipLength + 0.5f);
            Destroy(gameObject);
        }

        private void PlayMoveSound(float pitch)
        {
            if (audioSource != null && moveClip != null)
            {
                audioSource.pitch = pitch;
                if (!audioSource.isPlaying || audioSource.clip != moveClip)
                {
                    audioSource.clip = moveClip;
                    audioSource.loop = true;
                    audioSource.Play();
                    Debug.Log($"[{gameObject.name}] PlayMoveSound: Loop playing '{moveClip.name}'");
                }
            }
        }

        private void StopMoveSound()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log($"[{gameObject.name}] StopMoveSound: Stopped move sound");
            }
        }

        private bool TryGetAnimator()
        {
            if (animator == null)
            {
                FindAnimator();
            }

            return animator != null;
        }
    }
}

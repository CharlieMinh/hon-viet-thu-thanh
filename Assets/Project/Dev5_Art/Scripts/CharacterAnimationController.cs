using UnityEngine;
using System.Collections;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Phase 19A: Archer Animation Setup.
    /// Script gắn ở root Archer_Unit_Prefab để kết nối gameplay logic với Animator của model thật.
    /// </summary>
    public class CharacterAnimationController : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Bật để log debug ra Console")]
        public bool debugLog = false;

        [Tooltip("Tự động phát hiện di chuyển dựa trên thay đổi khoảng cách")]
        public bool autoDetectMovement = true;

        [Tooltip("Ngưỡng khoảng cách tối thiểu mỗi giây để coi là di chuyển")]
        public float movementThreshold = 0.01f;

        [Tooltip("Delay thêm (giây) sau khi animation Death kết thúc trước khi Destroy")]
        public float deathLingerSeconds = 0.5f;

        [Tooltip("Fallback delay (giây) nếu không đọc được clip length của animation Death")]
        public float deathFallbackDelay = 2f;

        [Header("Animator Reference (Auto Detected)")]
        [SerializeField] private Animator animator;

        [Header("Audio Setup")]
        [SerializeField] private bool muteForTesting = false;
        [SerializeField] private AudioSource attackAudioSource;
        [SerializeField] private AudioSource deathAudioSource;
        [SerializeField] private AudioClip attackClip;
        [SerializeField] private AudioClip deathClip;

        private Vector3 lastPosition;
        private Health health;
        private bool isMoving = false;
        private bool isDirectControl = false;
        private bool isDying = false;

        private void Awake()
        {
            FindAnimator();
            health = GetComponent<Health>();
            SetupAudioSources();
        }

        private void SetupAudioSources()
        {
            if (attackAudioSource == null)
            {
                AudioSource[] sources = GetComponents<AudioSource>();
                if (sources.Length > 0) attackAudioSource = sources[0];
                if (sources.Length > 1) deathAudioSource = sources[1];
            }

            if (attackAudioSource == null)
            {
                attackAudioSource = gameObject.AddComponent<AudioSource>();
                attackAudioSource.playOnAwake = false;
                attackAudioSource.loop = false;
            }
            if (deathAudioSource == null)
            {
                deathAudioSource = gameObject.AddComponent<AudioSource>();
                deathAudioSource.playOnAwake = false;
                deathAudioSource.loop = false;
            }
        }

        private void OnEnable()
        {
            if (GamePhaseManager.Instance != null)
            {
                GamePhaseManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }
            HonVietThuThanh.Dev4.SettingsMenuController.OnSFXVolumeChanged += HandleSFXVolumeChanged;
            UpdateSFXVolumes();
        }

        private void OnDisable()
        {
            if (GamePhaseManager.Instance != null)
            {
                GamePhaseManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
            HonVietThuThanh.Dev4.SettingsMenuController.OnSFXVolumeChanged -= HandleSFXVolumeChanged;
            StopAllAudio();
        }

        private void HandleSFXVolumeChanged(float newVolume)
        {
            UpdateSFXVolumes();
        }

        private void UpdateSFXVolumes()
        {
            if (attackAudioSource != null) attackAudioSource.volume = 1f;
            if (deathAudioSource != null) deathAudioSource.volume = 1f;
        }

        private void Start()
        {
            if (GamePhaseManager.Instance != null)
            {
                GamePhaseManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
                GamePhaseManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }

            if (health != null)
            {
                health.OnDeath += PlayDeath;
            }
            lastPosition = transform.position;
        }

        private void OnDestroy()
        {
            if (GamePhaseManager.Instance != null)
            {
                GamePhaseManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
            if (health != null)
            {
                health.OnDeath -= PlayDeath;
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.WaveCompleted || state == GameState.Win || state == GameState.Lose || state == GameState.Preparation)
            {
                StopAllAudio();
            }
        }

        private void StopAllAudio()
        {
            if (attackAudioSource != null && attackAudioSource.isPlaying)
            {
                attackAudioSource.Stop();
            }
            if (deathAudioSource != null && deathAudioSource.isPlaying)
            {
                deathAudioSource.Stop();
            }
        }

        private void Update()
        {
            // Không cập nhật animation khi đang dying
            if (isDying) return;

            // Tự động phát hiện di chuyển nếu không chịu điều khiển trực tiếp từ script ngoài
            if (autoDetectMovement && !isDirectControl)
            {
                Vector3 currentPosition = transform.position;
                // Đo khoảng cách trên mặt phẳng ngang XZ để tránh sai số nhảy cao độ
                float distanceXZ = Vector3.Distance(
                    new Vector3(currentPosition.x, 0f, currentPosition.z),
                    new Vector3(lastPosition.x, 0f, lastPosition.z)
                );

                // Tính toán vận tốc (khoảng cách di chuyển / delta time)
                float speed = Time.deltaTime > 0f ? (distanceXZ / Time.deltaTime) : 0f;
                bool currentlyMoving = speed > movementThreshold;

                if (currentlyMoving != isMoving)
                {
                    isMoving = currentlyMoving;
                    UpdateAnimatorBool("IsMoving", isMoving);
                }

                lastPosition = currentPosition;
            }
            else
            {
                lastPosition = transform.position;
            }
        }

        /// <summary>
        /// Tìm Animator component trong Visual/ModelSlot hoặc bất kỳ con nào.
        /// </summary>
        public void FindAnimator()
        {
            if (animator != null) return;

            // 1. Tìm trong Visual/ModelSlot
            Transform visualTrans = transform.Find("Visual");
            if (visualTrans != null)
            {
                Transform modelSlotTrans = visualTrans.Find("ModelSlot");
                if (modelSlotTrans != null)
                {
                    animator = modelSlotTrans.GetComponentInChildren<Animator>();
                }
            }

            // 2. Tìm trong các object con bất kỳ
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            // Thiết lập mặc định tắt Root Motion theo yêu cầu
            if (animator != null)
            {
                animator.applyRootMotion = false;
                if (debugLog)
                {
                    Debug.Log($"[{gameObject.name}] Đã tìm thấy Animator. Đặt applyRootMotion = false.");
                }
            }
        }

        /// <summary>
        /// Rebind lại Animator khi kéo model mới hoặc reset visual.
        /// </summary>
        public void RebindAnimator()
        {
            animator = null;
            FindAnimator();
            if (animator != null)
            {
                animator.Rebind();
            }
        }

        /// <summary>
        /// Trực tiếp thiết lập trạng thái di chuyển (vô hiệu hóa autoDetect tạm thời).
        /// </summary>
        public void SetMoving(bool moving)
        {
            if (isDying) return;
            isDirectControl = true; // Bật điều khiển trực tiếp
            if (isMoving != moving)
            {
                isMoving = moving;
                UpdateAnimatorBool("IsMoving", isMoving);
            }
        }

        /// <summary>
        /// Khôi phục lại autoDetect (gọi khi thoát khỏi Combat Phase).
        /// </summary>
        public void ResetToAutoDetect()
        {
            if (isDying) return;
            isDirectControl = false;
            isMoving = false;
            UpdateAnimatorBool("IsMoving", false);
            lastPosition = transform.position;
        }

        /// <summary>
        /// Kích hoạt trigger Attack.
        /// </summary>
        public void PlayAttack()
        {
            if (isDying) return;
            TriggerAnimator("Attack");

            if (muteForTesting) return;

            if (attackAudioSource != null && attackClip != null)
            {
                attackAudioSource.Stop();
                attackAudioSource.PlayOneShot(attackClip, HonVietThuThanh.Dev4.SettingsMenuController.SFXOutputVolume);
                Debug.Log($"[{gameObject.name}] PlayAttack: Playing attack sound '{attackClip.name}' on {attackAudioSource.name}");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] PlayAttack: Cannot play sound! attackAudioSource is {(attackAudioSource == null ? "null" : "not null")}, attackClip is {(attackClip == null ? "null" : "not null")}");
            }
        }

        /// <summary>
        /// Dừng phát âm thanh tấn công.
        /// </summary>
        public void StopAttackSound()
        {
            if (attackAudioSource != null && attackAudioSource.isPlaying)
            {
                attackAudioSource.Stop();
                Debug.Log($"[{gameObject.name}] StopAttackSound: Stopped attack sound");
            }
        }

        /// <summary>
        /// Kích hoạt trigger Death, rồi Destroy sau khi animation kết thúc.
        /// </summary>
        public void PlayDeath()
        {
            if (isDying) return;
            isDying = true;

            // Dừng di chuyển và đặt IsMoving = false
            isMoving = false;
            UpdateAnimatorBool("IsMoving", false);

            StopAttackSound();

            if (!muteForTesting && deathAudioSource != null && deathClip != null)
            {
                deathAudioSource.Stop();
                deathAudioSource.PlayOneShot(deathClip, HonVietThuThanh.Dev4.SettingsMenuController.SFXOutputVolume);
                Debug.Log($"[{gameObject.name}] PlayDeath: Playing death sound '{deathClip.name}' on {deathAudioSource.name}");
            }
            else if (muteForTesting)
            {
                Debug.Log($"[{gameObject.name}] PlayDeath: Sound muted for testing");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] PlayDeath: Cannot play sound! deathAudioSource is {(deathAudioSource == null ? "null" : "not null")}, deathClip is {(deathClip == null ? "null" : "not null")}");
            }

            TriggerAnimator("Death");

            // Bắt đầu coroutine chờ animation xong rồi Destroy
            StartCoroutine(DestroyAfterDeathAnimation());
        }

        /// <summary>
        /// Coroutine: đợi animation Death phát xong, rồi gọi Health.DestroyAfterDelay().
        /// </summary>
        private IEnumerator DestroyAfterDeathAnimation()
        {
            float clipLength = deathFallbackDelay;

            if (animator != null)
            {
                // Đợi 1 frame để Animator xử lý trigger và chuyển state
                yield return null;
                yield return null;

                // Lấy độ dài clip Death hiện tại đang phát
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("Death") || stateInfo.IsTag("Death"))
                {
                    float rawLength = stateInfo.length;
                    if (rawLength > 0.1f)
                    {
                        clipLength = rawLength;
                    }
                }

                if (debugLog)
                {
                    Debug.Log($"[{gameObject.name}] Death animation clip length = {clipLength:F2}s. Sẽ Destroy sau {clipLength + deathLingerSeconds:F2}s.");
                }
            }

            yield return new WaitForSeconds(clipLength + deathLingerSeconds);

            // Gọi Destroy qua Health nếu còn tồn tại
            if (health != null && gameObject != null)
            {
                health.DestroyAfterDelay(0f);
            }
            else if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        private void UpdateAnimatorBool(string parameterName, bool value)
        {
            if (animator == null)
            {
                FindAnimator();
            }

            if (animator != null)
            {
                try
                {
                    animator.SetBool(parameterName, value);
                    if (debugLog)
                    {
                        Debug.Log($"[{gameObject.name}] Animator.SetBool(\"{parameterName}\", {value})");
                    }
                }
                catch (System.Exception ex)
                {
                    if (debugLog)
                    {
                        Debug.LogWarning($"[{gameObject.name}] Không thể gọi SetBool \"{parameterName}\" trên Animator: {ex.Message}");
                    }
                }
            }
        }

        private void TriggerAnimator(string parameterName)
        {
            if (animator == null)
            {
                FindAnimator();
            }

            if (animator != null)
            {
                try
                {
                    animator.SetTrigger(parameterName);
                    if (debugLog)
                    {
                        Debug.Log($"[{gameObject.name}] Animator.SetTrigger(\"{parameterName}\")");
                    }
                }
                catch (System.Exception ex)
                {
                    if (debugLog)
                    {
                        Debug.LogWarning($"[{gameObject.name}] Không thể gọi SetTrigger \"{parameterName}\" trên Animator: {ex.Message}");
                    }
                }
            }
        }
    }
}

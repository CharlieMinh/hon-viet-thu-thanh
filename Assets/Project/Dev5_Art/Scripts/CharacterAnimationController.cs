using UnityEngine;

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

        [Header("Animator Reference (Auto Detected)")]
        [SerializeField] private Animator animator;

        private Vector3 lastPosition;
        private Health health;
        private bool isMoving = false;
        private bool isDirectControl = false;

        private void Awake()
        {
            FindAnimator();
            health = GetComponent<Health>();
        }

        private void Start()
        {
            if (health != null)
            {
                health.OnDeath += PlayDeath;
            }
            lastPosition = transform.position;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDeath -= PlayDeath;
            }
        }

        private void Update()
        {
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
            isDirectControl = true; // Bật điều khiển trực tiếp
            if (isMoving != moving)
            {
                isMoving = moving;
                UpdateAnimatorBool("IsMoving", isMoving);
            }
        }

        /// <summary>
        /// Kích hoạt trigger Attack.
        /// </summary>
        public void PlayAttack()
        {
            TriggerAnimator("Attack");
        }

        /// <summary>
        /// Kích hoạt trigger Death.
        /// </summary>
        public void PlayDeath()
        {
            TriggerAnimator("Death");
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

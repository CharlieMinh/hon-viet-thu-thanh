using UnityEngine;
using System;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Component quản lý máu dùng chung cho cả Unit và Enemy.
    /// Quản lý lượng máu hiện tại, tối đa và kích hoạt sự kiện khi chết.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Header("Cấu hình máu")]
        [SerializeField] private int maxHealth = 30;

        [Header("Base Stats (Lưu máu gốc để scale sao - Phase 12)")]
        [SerializeField] private int baseMaxHealth = -1;
        
        private int currentHealth;
        private bool isDead = false;

        /// <summary>
        /// Nhân chỉ số máu gốc với hệ số sao và hồi đầy máu.
        /// </summary>
        public void ApplyStarMultiplier(int starLevel)
        {
            if (baseMaxHealth <= 0)
            {
                baseMaxHealth = maxHealth;
            }

            float multiplier = Mathf.Pow(1.5f, starLevel - 1);
            int newMaxHealth = Mathf.RoundToInt(baseMaxHealth * multiplier);
            SetMaxHealth(newMaxHealth, true);
        }

        /// <summary>
        /// Gán lượng máu tối đa mới.
        /// </summary>
        public void SetMaxHealth(int newMaxHealth, bool healToFull)
        {
            maxHealth = Mathf.Max(1, newMaxHealth);
            if (healToFull)
            {
                currentHealth = maxHealth;
            }
            else
            {
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            }
            Debug.Log($"[Health] {gameObject.name} HP changed (SetMaxHealth): {currentHealth}/{maxHealth}");
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public bool IsDead => isDead;

        /// <summary>
        /// Kích hoạt khi lượng máu thay đổi. Tham số: (currentHealth, maxHealth).
        /// </summary>
        public event Action<int, int> OnHealthChanged;

        /// <summary>
        /// Kích hoạt khi đối tượng bị tiêu diệt (máu về 0).
        /// </summary>
        public event Action OnDeath;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Start()
        {
            // Bảo đảm khởi tạo lại nếu maxHealth thay đổi trong Inspector trước Start
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Khởi tạo lượng máu tối đa và hiện tại cho đối tượng.
        /// </summary>
        public void InitializeHealth(int healthValue)
        {
            maxHealth = healthValue;
            currentHealth = healthValue;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Nhận sát thương. Trừ máu và gọi Die() nếu máu <= 0.
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (isDead) return;
            if (amount <= 0) return;

            currentHealth = Mathf.Max(0, currentHealth - amount);
            Debug.Log($"[Health] {gameObject.name} HP changed (TakeDamage): {currentHealth}/{maxHealth}");
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Hồi máu cho đối tượng.
        /// </summary>
        public void Heal(int amount)
        {
            if (isDead) return;
            if (amount <= 0) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            Debug.Log($"[Health] {gameObject.name} HP changed (Heal): {currentHealth}/{maxHealth}");
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Thực hiện logic chết của đối tượng.
        /// Chỉ set flag và fire event — KHÔNG tự Destroy ngay.
        /// Destroy sẽ được xử lý bởi CharacterAnimationController (sau animation)
        /// hoặc DestroyAfterDelay() cho các đối tượng không có animation.
        /// </summary>
        private void Die()
        {
            isDead = true;
            Debug.Log($"[{gameObject.name}] Đã chết!");
            OnDeath?.Invoke();
            
            // Nếu có CharacterAnimationController, nó sẽ chịu trách nhiệm gọi DestroyAfterDelay sau khi animation kết thúc.
            // Ngược lại (các đối tượng khác không có animation), huỷ đối tượng ngay lập tức.
            if (GetComponent<CharacterAnimationController>() == null)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Gọi để huỷ GameObject sau một khoảng delay (giây).
        /// CharacterAnimationController hoặc EnemyController gọi khi animation Death kết thúc.
        /// </summary>
        public void DestroyAfterDelay(float delay = 0f)
        {
            Destroy(gameObject, delay);
        }
    }
}

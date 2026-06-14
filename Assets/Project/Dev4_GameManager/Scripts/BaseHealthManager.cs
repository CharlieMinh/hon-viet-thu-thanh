using System;
using UnityEngine;
using HonVietThuThanh.Shared;

namespace HonVietThuThanh.Dev4
{
    /// <summary>
    /// BaseHealthManager — quản lý máu của thành (Sinh Mệnh Đền).
    ///
    /// LẮNG NGHE:
    ///   GameEvents.OnEnemyReachedBase → trừ damagePerEnemy
    ///
    /// PHÁT RA:
    ///   OnBaseHPChanged(int current, int max) → UIManager cập nhật HUD
    ///   OnBaseDestroyed()                     → GameStateManager trigger LOSE
    ///
    /// SETUP trong Inspector:
    ///   - startBaseHP    = 100
    ///   - damagePerEnemy = 10 (Phase 1: mỗi enemy tới base trừ 10 HP)
    /// </summary>
    public class BaseHealthManager : MonoBehaviour
    {
        public static BaseHealthManager Instance { get; private set; }

        [Header("Cấu hình")]
        [SerializeField] private int startBaseHP = 100;
        [Tooltip("Số HP thành bị trừ mỗi khi 1 enemy tới base (Phase 1 hardcode)")]
        [SerializeField] private int damagePerEnemy = 10;

        public int CurrentBaseHP { get; private set; }
        public int MaxBaseHP     => startBaseHP;

        /// <summary>Phát khi HP thành thay đổi. Tham số: (current, max).</summary>
        public static event Action<int, int> OnBaseHPChanged;

        /// <summary>Phát 1 lần khi HP thành về 0.</summary>
        public static event Action OnBaseDestroyed;

        private bool _alreadyDestroyed;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            CurrentBaseHP    = startBaseHP;
            _alreadyDestroyed = false;

            Debug.Log($"[BaseHealthManager] Khởi tạo. HP thành: {CurrentBaseHP}/{MaxBaseHP}");
        }

        private void OnEnable()
        {
            GameEvents.OnEnemyReachedBase += HandleEnemyReachedBase;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyReachedBase -= HandleEnemyReachedBase;
        }

        // --- Event Handler ---

        private void HandleEnemyReachedBase(GameObject enemy)
        {
            TakeDamage(damagePerEnemy);
        }

        // --- Core Logic ---

        private void TakeDamage(int damage)
        {
            if (_alreadyDestroyed) return;

            CurrentBaseHP = Mathf.Max(0, CurrentBaseHP - damage);
            Debug.Log($"[BaseHealthManager] Thành bị tấn công -{damage}. HP: {CurrentBaseHP}/{MaxBaseHP}");
            OnBaseHPChanged?.Invoke(CurrentBaseHP, MaxBaseHP);

            if (CurrentBaseHP <= 0)
            {
                _alreadyDestroyed = true;
                Debug.Log("[BaseHealthManager] Thành đã thất thủ! Trigger LOSE.");
                OnBaseDestroyed?.Invoke();
            }
        }
    }
}

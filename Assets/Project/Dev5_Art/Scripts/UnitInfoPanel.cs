using UnityEngine;
using TMPro;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Component quản lý bảng hiển thị thông tin quân cờ được click chọn (Phase 13).
    /// </summary>
    public class UnitInfoPanel : MonoBehaviour
    {
        public static UnitInfoPanel Instance { get; private set; }

        [Header("UI References")]
        public GameObject panelParent;
        public TMP_Text nameText;
        public TMP_Text statsText;

        private PlaceableUnit targetUnit;
        private Health targetHealth;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[UnitInfoPanel] Phát hiện instance trùng lặp, tự huỷ.");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Ẩn panel khi mới khởi động game
            Hide();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            UnregisterHealthListener();
        }

        private void Update()
        {
            // Tránh NullReference khi cờ đang chọn bị huỷ (ví dụ do gộp sao hoặc chết trong combat)
            if (targetUnit == null && panelParent != null && panelParent.activeSelf)
            {
                Hide();
            }
        }

        /// <summary>
        /// Kích hoạt hiển thị thông tin quân cờ chỉ định.
        /// </summary>
        public void ShowUnitInfo(PlaceableUnit unit)
        {
            UnregisterHealthListener();

            targetUnit = unit;
            if (targetUnit == null)
            {
                Hide();
                return;
            }

            targetHealth = targetUnit.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged += HandleHealthChanged;
            }

            if (panelParent != null)
            {
                panelParent.SetActive(true);
            }

            RefreshCurrentUnit();
        }

        /// <summary>
        /// Cập nhật hiển thị chỉ số chi tiết của quân cờ.
        /// </summary>
        public void RefreshCurrentUnit()
        {
            if (targetUnit == null)
            {
                Hide();
                return;
            }

            UnitStarData starData = targetUnit.GetComponent<UnitStarData>();
            UnitCombatStats stats = targetUnit.GetComponent<UnitCombatStats>();
            Health hp = targetUnit.GetComponent<Health>();

            // 1. Render Tên + Sao tương ứng (Hỗ trợ định dạng sao vô hạn)
            string starString = "";
            if (starData != null)
            {
                int star = starData.starLevel;
                if (star == 1) starString = " ★";
                else if (star == 2) starString = " ★★";
                else if (star == 3) starString = " ★★★";
                else starString = $" ★ x{star}";
            }

            if (nameText != null)
            {
                nameText.text = $"{targetUnit.unitName}{starString}";
            }

            // 2. Render chỉ số chiến đấu hiện tại
            int curHP = hp != null ? hp.CurrentHealth : 0;
            int maxHP = hp != null ? hp.MaxHealth : 0;
            int dmg = stats != null ? stats.damage : 0;
            float range = stats != null ? stats.attackRange : 0f;
            float cooldown = stats != null ? stats.attackCooldown : 0f;
            float speed = stats != null ? stats.moveSpeed : 0f;

            string roleText = "";
            UnitRole roleComp = targetUnit.GetComponent<UnitRole>();
            if (roleComp != null)
            {
                roleText = $"Role: {roleComp.role}\nAttack: {roleComp.attackType}\n";
            }

            if (statsText != null)
            {
                statsText.text = roleText +
                                 $"HP: {curHP} / {maxHP}\n" +
                                 $"Damage: {dmg}\n" +
                                 $"Range: {range:F1}\n" +
                                 $"Cooldown: {cooldown:F1}s\n" +
                                 $"Move Speed: {speed:F1}";
            }
        }

        /// <summary>
        /// Ẩn bảng thông tin và dọn dẹp liên kết.
        /// </summary>
        public void Hide()
        {
            UnregisterHealthListener();
            targetUnit = null;
            targetHealth = null;

            if (panelParent != null)
            {
                panelParent.SetActive(false);
            }
        }

        private void HandleHealthChanged(int current, int max)
        {
            RefreshCurrentUnit();
        }

        private void UnregisterHealthListener()
        {
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged -= HandleHealthChanged;
            }
        }
    }
}

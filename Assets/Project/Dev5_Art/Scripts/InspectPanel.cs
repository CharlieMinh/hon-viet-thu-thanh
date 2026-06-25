using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Bảng hiển thị thông tin chi tiết (Inspect Panel) của Unit hoặc Enemy khi người chơi nhấn chuột phải (Phase 16).
    /// </summary>
    public class InspectPanel : MonoBehaviour
    {
        public static InspectPanel Instance { get; private set; }

        [Header("UI References")]
        public GameObject panelParent;
        public TMP_Text titleText;
        public TMP_Text statsText;
        public Button closeButton;

        // Đối tượng đang được inspect
        private PlaceableUnit targetUnit;
        private EnemyController targetEnemy;
        private Health targetHealth;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[InspectPanel] Phát hiện instance trùng lặp, tự huỷ.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Ẩn panel lúc khởi động
            Hide();
        }

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            UnsubscribeAll();
        }

        private void Update()
        {
            // Kiểm tra an toàn: nếu đối tượng đang inspect bị Destroy (Unity override == null để detect)
            // Dùng object.ReferenceEquals để phân biệt "chưa gán" vs "đã bị Destroy"
            if (panelParent != null && panelParent.activeSelf)
            {
                bool targetDestroyed = false;
                // Unity MonoBehaviour override == null → trả về true nếu native object đã bị Destroy
                if (!object.ReferenceEquals(targetUnit, null) && targetUnit == null) targetDestroyed = true;
                if (!object.ReferenceEquals(targetEnemy, null) && targetEnemy == null) targetDestroyed = true;
                if (!object.ReferenceEquals(targetHealth, null) && targetHealth == null) targetDestroyed = true;

                if (targetDestroyed)
                {
                    Debug.Log("[InspectPanel] Đối tượng đang inspect đã bị Destroy, tự động ẩn panel.");
                    Hide();
                }
            }
        }

        /// <summary>
        /// Mở bảng inspect cho cờ người chơi.
        /// </summary>
        public void ShowPlayerUnit(PlaceableUnit unit)
        {
            if (unit == null)
            {
                Hide();
                return;
            }

            targetUnit = unit;
            targetEnemy = null;

            SubscribeAll(unit.GetComponent<Health>());

            if (panelParent != null)
            {
                panelParent.SetActive(true);
            }

            Refresh();
        }

        /// <summary>
        /// Mở bảng inspect cho cờ kẻ địch.
        /// </summary>
        public void ShowEnemy(EnemyController enemy)
        {
            if (enemy == null)
            {
                Hide();
                return;
            }

            targetEnemy = enemy;
            targetUnit = null;

            SubscribeAll(enemy.GetComponent<Health>());

            if (panelParent != null)
            {
                panelParent.SetActive(true);
            }

            Refresh();
        }

        /// <summary>
        /// Ẩn bảng inspect.
        /// </summary>
        public void Hide()
        {
            UnsubscribeAll();
            targetUnit = null;
            targetEnemy = null;

            if (panelParent != null)
            {
                panelParent.SetActive(false);
            }
        }

        /// <summary>
        /// Cập nhật hiển thị thông số chi tiết.
        /// </summary>
        public void Refresh()
        {
            // Trường hợp 1: Đang inspect Player Unit
            if (targetUnit != null)
            {
                UnitStarData starData = targetUnit.GetComponent<UnitStarData>();
                UnitCombatStats stats = targetUnit.GetComponent<UnitCombatStats>();
                Health hp = targetUnit.GetComponent<Health>();
                UnitRole roleComp = targetUnit.GetComponent<UnitRole>();

                string starString = "";
                if (starData != null)
                {
                    int star = starData.starLevel;
                    if (star == 1) starString = " ★";
                    else if (star == 2) starString = " ★★";
                    else if (star == 3) starString = " ★★★";
                    else starString = $" ★ x{star}";
                }

                if (titleText != null)
                {
                    titleText.text = $"{targetUnit.unitName}{starString}";
                }

                int curHP = hp != null ? hp.CurrentHealth : 0;
                int maxHP = hp != null ? hp.MaxHealth : 0;
                int dmg = stats != null ? stats.damage : 0;
                float range = stats != null ? stats.attackRange : 0f;
                float cooldown = stats != null ? stats.attackCooldown : 0f;
                float speed = stats != null ? stats.moveSpeed : 0f;

                string roleStr = roleComp != null ? roleComp.role.ToString() : "N/A";
                string attackTypeStr = roleComp != null ? roleComp.attackType.ToString() : "N/A";
                
                string tauntStr = "";
                if (roleComp != null && roleComp.isTank)
                {
                    tauntStr = $"\nTaunt Radius: {roleComp.tauntRadius:F1}";
                }

                if (statsText != null)
                {
                    statsText.text = $"Type: Player Unit\n" +
                                     $"Role: {roleStr}\n" +
                                     $"Attack: {attackTypeStr}\n" +
                                     $"HP: {curHP} / {maxHP}\n" +
                                     $"Damage: {dmg}\n" +
                                     $"Range: {range:F1}\n" +
                                     $"Cooldown: {cooldown:F1}s\n" +
                                     $"Move Speed: {speed:F1}" +
                                     tauntStr;
                }
            }
            // Trường hợp 2: Đang inspect Enemy
            else if (targetEnemy != null)
            {
                EnemyCombatStats stats = targetEnemy.GetComponent<EnemyCombatStats>();
                Health hp = targetEnemy.GetComponent<Health>();
                EnemyRole roleComp = targetEnemy.GetComponent<EnemyRole>();

                if (titleText != null)
                {
                    titleText.text = targetEnemy.enemyName;
                }

                int curHP = hp != null ? hp.CurrentHealth : 0;
                int maxHP = hp != null ? hp.MaxHealth : 0;
                int dmg = stats != null ? stats.damage : 0;
                float range = stats != null ? stats.attackRange : 0f;
                float cooldown = stats != null ? stats.attackCooldown : 0f;
                float speed = stats != null ? stats.moveSpeed : 0f;

                string roleStr = roleComp != null ? roleComp.role.ToString() : "N/A";
                string attackTypeStr = roleComp != null ? roleComp.attackType.ToString() : "N/A";

                if (statsText != null)
                {
                    statsText.text = $"Type: Enemy\n" +
                                     $"Role: {roleStr}\n" +
                                     $"Attack: {attackTypeStr}\n" +
                                     $"HP: {curHP} / {maxHP}\n" +
                                     $"Damage: {dmg}\n" +
                                     $"Range: {range:F1}\n" +
                                     $"Cooldown: {cooldown:F1}s\n" +
                                     $"Move Speed: {speed:F1}\n" +
                                     $"Kill Reward: +{targetEnemy.killGoldReward}G";
                }
            }
        }

        private void SubscribeAll(Health health)
        {
            UnsubscribeAll();
            targetHealth = health;
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged += HandleHealthChanged;
                targetHealth.OnDeath         += HandleTargetDeath;
            }
        }

        private void UnsubscribeAll()
        {
            if (!object.ReferenceEquals(targetHealth, null) && targetHealth != null)
            {
                targetHealth.OnHealthChanged -= HandleHealthChanged;
                targetHealth.OnDeath         -= HandleTargetDeath;
            }
            targetHealth = null;
        }

        private void HandleHealthChanged(int current, int max)
        {
            Refresh();
        }

        /// <summary>
        /// Tự động ẩn panel ngay khi target chết (tránh hiện thông tin đối tượng đã bị Destroy).
        /// </summary>
        private void HandleTargetDeath()
        {
            Debug.Log("[InspectPanel] Target đã chết → tự động ẩn panel.");
            Hide();
        }
    }
}

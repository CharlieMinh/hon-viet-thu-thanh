using UnityEngine;
using TMPro;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
            ApplyArtLayout();
            ShowDefaultInfo();
            
            // Ẩn panel lúc khởi động
            ShowDefaultInfo();
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
                    ShowDefaultInfo();
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
                ShowDefaultInfo();
                return;
            }

            targetUnit = unit;
            targetEnemy = null;

            SubscribeAll(unit.GetComponent<Health>());

            if (panelParent != null)
            {
                ApplyArtLayout();
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
                ShowDefaultInfo();
                return;
            }

            targetEnemy = enemy;
            targetUnit = null;

            SubscribeAll(enemy.GetComponent<Health>());

            if (panelParent != null)
            {
                ApplyArtLayout();
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
        public void ShowDefaultInfo()
        {
            UnsubscribeAll();
            targetUnit = null;
            targetEnemy = null;

            ApplyArtLayout();

            if (titleText != null)
            {
                titleText.text = "Thông tin tướng";
            }

            if (statsText != null)
            {
                statsText.text = "Cung thủ : 4 tiền\n" +
                                 "Hiệp sĩ : 3 tiền\n" +
                                 "Đỡ đòn : 5 tiền";
            }

            if (panelParent != null)
            {
                panelParent.SetActive(true);
            }
        }

        public void Refresh()
        {
            // Trường hợp 1: Đang inspect Player Unit
            if (targetUnit != null)
            {
                UnitStarData starData = targetUnit.GetComponent<UnitStarData>();
                UnitCombatStats stats = targetUnit.GetComponent<UnitCombatStats>();
                Health hp = targetUnit.GetComponent<Health>();
                UnitRole roleComp = targetUnit.GetComponent<UnitRole>();

                if (titleText != null)
                {
                    titleText.text = BuildUnitTitle(targetUnit.unitName, starData);
                }

                int curHP = hp != null ? hp.CurrentHealth : 0;
                int maxHP = hp != null ? hp.MaxHealth : 0;
                int dmg = stats != null ? stats.damage : 0;
                float range = stats != null ? stats.attackRange : 0f;
                float cooldown = stats != null ? stats.attackCooldown : 0f;
                float speed = stats != null ? stats.moveSpeed : 0f;

                string roleStr = roleComp != null ? GetUnitRoleText(roleComp.role) : "Không xác định";
                string attackTypeStr = roleComp != null ? GetAttackTypeText(roleComp.attackType) : "Không xác định";
                
                string tauntStr = "";
                if (roleComp != null && roleComp.isTank)
                {
                    tauntStr = $"\nBán kính khiêu khích: {roleComp.tauntRadius:F1}";
                }

                if (statsText != null)
                {
                    statsText.text = $"Loại: Tướng người chơi\n" +
                                     $"Vai trò: {roleStr}\n" +
                                     $"Kiểu đánh: {attackTypeStr}\n" +
                                     $"Sinh lực: {curHP} / {maxHP}\n" +
                                     $"Sát thương: {dmg}\n" +
                                     $"Tầm đánh: {range:F1}\n" +
                                     $"Hồi chiêu: {cooldown:F1} giây\n" +
                                     $"Tốc độ di chuyển: {speed:F1}" +
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

                string roleStr = roleComp != null ? GetEnemyRoleText(roleComp.role) : "Không xác định";
                string attackTypeStr = roleComp != null ? GetEnemyAttackTypeText(roleComp.attackType) : "Không xác định";

                if (statsText != null)
                {
                    statsText.text = $"Loại: Kẻ địch\n" +
                                     $"Vai trò: {roleStr}\n" +
                                     $"Kiểu đánh: {attackTypeStr}\n" +
                                     $"Sinh lực: {curHP} / {maxHP}\n" +
                                     $"Sát thương: {dmg}\n" +
                                     $"Tầm đánh: {range:F1}\n" +
                                     $"Hồi chiêu: {cooldown:F1} giây\n" +
                                     $"Tốc độ di chuyển: {speed:F1}\n" +
                                     $"Thưởng hạ gục: +{targetEnemy.killGoldReward} Vàng";
                }
            }
        }

        private static string BuildUnitTitle(string unitName, UnitStarData starData)
        {
            int starLevel = starData != null ? Mathf.Max(1, starData.starLevel) : 1;
            return $"{unitName} - {starLevel} Sao";
        }

        private static string GetUnitRoleText(UnitClassRole role)
        {
            switch (role)
            {
                case UnitClassRole.Knight: return "Kỵ sĩ";
                case UnitClassRole.Archer: return "Xạ thủ";
                case UnitClassRole.Tank: return "Đỡ đòn";
                default: return "Không xác định";
            }
        }

        private static string GetAttackTypeText(AttackType attackType)
        {
            switch (attackType)
            {
                case AttackType.Melee: return "Cận chiến";
                case AttackType.RangedProjectile: return "Đánh xa";
                default: return "Không xác định";
            }
        }

        private static string GetEnemyRoleText(EnemyClassRole role)
        {
            switch (role)
            {
                case EnemyClassRole.Goblin: return "Yêu tinh";
                case EnemyClassRole.Orc: return "Quái nhân";
                case EnemyClassRole.Archer: return "Xạ thủ";
                default: return "Không xác định";
            }
        }

        private static string GetEnemyAttackTypeText(EnemyAttackType attackType)
        {
            switch (attackType)
            {
                case EnemyAttackType.Melee: return "Cận chiến";
                case EnemyAttackType.RangedProjectile: return "Đánh xa";
                default: return "Không xác định";
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
            ShowDefaultInfo();
        }

        private void ApplyArtLayout()
        {
            GameObject root = panelParent != null ? panelParent : gameObject;

            RectTransform panelRect = root.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.sizeDelta = new Vector2(330f, 320f);
            }

            Image panelImage = root.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = Color.white;
                panelImage.type = Image.Type.Simple;
                panelImage.preserveAspect = false;
#if UNITY_EDITOR
                Sprite panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Project/Dev5_Art/UI/UI/Clean/Bang_Hien_TT_panel.png");
                if (panelSprite != null)
                {
                    panelImage.sprite = panelSprite;
                }
#endif
            }

            ApplyTitleLayout(titleText);
            ApplyStatsLayout(statsText);
        }

        private static void ApplyTitleLayout(TMP_Text text)
        {
            if (text == null)
            {
                return;
            }

            RectTransform rect = text.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -34f);
                rect.sizeDelta = new Vector2(-110f, 30f);
            }

            text.alignment = TextAlignmentOptions.Left;
            text.fontSize = 16f;
            text.enableWordWrapping = false;
        }

        private static void ApplyStatsLayout(TMP_Text text)
        {
            if (text == null)
            {
                return;
            }

            RectTransform rect = text.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(8f, -20f);
                rect.sizeDelta = new Vector2(-96f, -126f);
            }

            text.alignment = TextAlignmentOptions.TopLeft;
            text.fontSize = 13f;
            text.enableWordWrapping = false;
        }
    }
}

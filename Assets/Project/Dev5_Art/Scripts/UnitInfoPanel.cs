using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
            ApplyArtLayout();

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
                ApplyArtLayout();
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

            if (nameText != null)
            {
                nameText.text = BuildUnitTitle(targetUnit.unitName, starData);
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
                roleText = $"Vai trò: {GetUnitRoleText(roleComp.role)}\n" +
                           $"Kiểu đánh: {GetAttackTypeText(roleComp.attackType)}\n";
            }

            if (statsText != null)
            {
                statsText.text = roleText +
                                 $"Sinh lực: {curHP} / {maxHP}\n" +
                                 $"Sát thương: {dmg}\n" +
                                 $"Tầm đánh: {range:F1}\n" +
                                 $"Hồi chiêu: {cooldown:F1} giây\n" +
                                 $"Tốc độ di chuyển: {speed:F1}";
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

        private void ApplyArtLayout()
        {
            GameObject root = panelParent != null ? panelParent : gameObject;

            RectTransform panelRect = root.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.sizeDelta = new Vector2(320f, 310f);
            }

            Image panelImage = root.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = Color.white;
                panelImage.type = Image.Type.Simple;
                panelImage.preserveAspect = false;
                Sprite panelSprite = Dev5RuntimeUIArt.LoadSprite(Dev5RuntimeUIArt.InfoPanel);
                if (panelSprite != null)
                {
                    panelImage.sprite = panelSprite;
                }
            }

            ApplyTitleLayout(nameText);
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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HonVietThuThanh.Shared;

namespace HonVietThuThanh.Dev4
{
    /// <summary>
    /// HeroSelectionUI — panel chọn hero ở cuối màn hình.
    ///
    /// Hiển thị danh sách hero với:
    ///   - Tên + icon
    ///   - Cost (Linh Khí)
    ///   - Grey out nếu không đủ tiền
    ///   - Highlight hero đang chọn
    ///
    /// Khi người chơi click hero → phát GameEvents.OnHeroSelected(HeroType)
    /// Dev1 (HeroPlacementManager) lắng nghe event này để biết hero nào sẽ đặt.
    ///
    /// SETUP trong Inspector:
    ///   1. Tạo GameObject "HeroSelectionUI", gắn script này
    ///   2. Tạo Panel ngang cuối màn hình với HorizontalLayoutGroup
    ///   3. Tạo HeroButton prefab: Button + Image(icon) + Text(name) + Text(cost)
    ///      → kéo vào heroButtonPrefab
    ///   4. Kéo container của buttons vào heroButtonContainer
    ///   5. Kéo tất cả HeroData assets vào heroDatas list
    /// </summary>
    public class HeroSelectionUI : MonoBehaviour
    {
        [Header("Prefab button cho từng hero")]
        [SerializeField] private GameObject heroButtonPrefab;

        [Header("Container chứa các buttons (HorizontalLayoutGroup)")]
        [SerializeField] private Transform heroButtonContainer;

        [Header("HeroData assets (kéo từ Shared/Data)")]
        [SerializeField] private List<HeroData> heroDatas = new List<HeroData>();

        [Header("Màu khi đủ/không đủ tiền")]
        [SerializeField] private Color affordableColor    = Color.white;
        [SerializeField] private Color notAffordableColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        [SerializeField] private Color selectedColor      = new Color(1f, 0.85f, 0.2f, 1f);

        private HeroType _selectedHero;
        private readonly List<HeroButtonEntry> _buttons = new List<HeroButtonEntry>();

        private struct HeroButtonEntry
        {
            public HeroType  HeroType;
            public Button    Button;
            public Image     Background;
            public TextMeshProUGUI CostText;
        }

        private void OnEnable()
        {
            EconomyManager.OnGoldChanged += RefreshButtonStates;
        }

        private void OnDisable()
        {
            EconomyManager.OnGoldChanged -= RefreshButtonStates;
        }

        private void Start()
        {
            BuildButtons();
            // Chọn hero đầu tiên mặc định
            HeroData firstHero = heroDatas.Find(data => data != null);
            if (firstHero != null)
                SelectHero(firstHero.heroType);
        }

        // --- Build UI ---

        private void BuildButtons()
        {
            if (heroButtonContainer == null)
            {
                Debug.LogError("[HeroSelectionUI] Chưa gán heroButtonContainer.", this);
                return;
            }

            if (heroButtonPrefab == null)
            {
                Debug.LogError("[HeroSelectionUI] Chưa gán heroButtonPrefab.", this);
                return;
            }

            foreach (Transform child in heroButtonContainer)
            {
                Destroy(child.gameObject);
            }

            _buttons.Clear();

            foreach (var data in heroDatas)
            {
                if (data == null)
                {
                    Debug.LogWarning("[HeroSelectionUI] HeroData bị null, bỏ qua.");
                    continue;
                }

                GameObject go = Instantiate(heroButtonPrefab, heroButtonContainer);
                go.name = $"HeroBtn_{data.heroType}";

                // Gán tên hero
                var nameText = go.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();

                if (nameText == null)
                {
                    nameText = go.GetComponentInChildren<TextMeshProUGUI>();
                }

                if (nameText)
                {
                    nameText.text = data.heroName;
                }
                else
                {
                    Debug.LogWarning($"[HeroSelectionUI] Không tìm thấy text để hiện tên hero trên {go.name}", go);
                }

                // Gán icon nếu prefab có Icon
                var icon = go.transform.Find("Icon")?.GetComponent<Image>();
                if (icon && data.heroIcon)
                {
                    icon.sprite = data.heroIcon;
                }

                // Gán cost text nếu prefab có CostText
                var costText = go.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
                if (costText)
                {
                    costText.text = $"⚡ {data.cost}";
                }

                var btn = go.GetComponent<Button>();
                var background = go.GetComponent<Image>();

                if (btn == null)
                {
                    Debug.LogError($"[HeroSelectionUI] Prefab {go.name} thiếu Button component.", go);
                    continue;
                }

                HeroType capturedType = data.heroType;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => SelectHero(capturedType));

                _buttons.Add(new HeroButtonEntry
                {
                    HeroType = data.heroType,
                    Button = btn,
                    Background = background,
                    CostText = costText
                });
            }

            RefreshButtonStates(EconomyManager.Instance ? EconomyManager.Instance.CurrentGold : 150);
        }

        // --- Selection ---

        private void SelectHero(HeroType heroType)
        {
            if (EconomyManager.Instance && !EconomyManager.Instance.CanAfford(heroType))
            {
                Debug.Log($"[HeroSelectionUI] Không đủ Linh Khí để chọn {heroType}");
                return;
            }

            _selectedHero = heroType;
            Debug.Log($"[HeroSelectionUI] Chọn hero: {heroType}");
            GameEvents.RaiseHeroSelected(heroType);
            RefreshButtonStates(EconomyManager.Instance ? EconomyManager.Instance.CurrentGold : 0);
        }

        // --- Refresh ---

        private void RefreshButtonStates(int currentGold)
        {
            foreach (var entry in _buttons)
            {
                if (entry.Button == null) continue;

                int cost       = EconomyManager.Instance ? EconomyManager.Instance.GetCost(entry.HeroType) : 50;
                bool canAfford = currentGold >= cost;
                bool isSelected = entry.HeroType == _selectedHero;

                // Interactable
                entry.Button.interactable = canAfford;

                // Màu background
                if (entry.Background)
                {
                    entry.Background.color = isSelected
                        ? selectedColor
                        : canAfford ? affordableColor : notAffordableColor;
                }

                // Cost text màu đỏ nếu không đủ tiền
                if (entry.CostText)
                    entry.CostText.color = canAfford ? Color.white : Color.red;
            }
        }
    }
}

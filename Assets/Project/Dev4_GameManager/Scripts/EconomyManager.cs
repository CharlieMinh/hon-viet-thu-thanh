using System;
using System.Collections.Generic;
using UnityEngine;
using HonVietThuThanh.Shared;

namespace HonVietThuThanh.Dev4
{
    /// <summary>
    /// EconomyManager — quản lý Linh Khí (gold) của người chơi.
    ///
    /// LẮNG NGHE:
    ///   GameEvents.OnHeroPlaced  → trừ cost của hero đó
    ///   GameEvents.OnEnemyDied   → cộng goldReward
    ///
    /// PHÁT RA:
    ///   OnGoldChanged(int currentGold) → UIManager cập nhật HUD
    ///
    /// SETUP trong Inspector:
    ///   - startGold   = 150 (Phase 1 default)
    ///   - heroDatas   = kéo tất cả HeroData assets vào đây
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [Header("Cấu hình")]
        [SerializeField] private int startGold = 150;

        [Header("HeroData (kéo assets từ Shared/Data vào đây)")]
        [SerializeField] private List<HeroData> heroDatas = new List<HeroData>();

        public int CurrentGold { get; private set; }

        /// <summary>Phát khi Linh Khí thay đổi. Tham số: số Linh Khí hiện tại.</summary>
        public static event Action<int> OnGoldChanged;

        private readonly Dictionary<HeroType, int> _costLookup = new Dictionary<HeroType, int>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            CurrentGold = startGold;
            BuildCostLookup();

            Debug.Log($"[EconomyManager] Khởi tạo. Linh Khí: {CurrentGold}");
        }

        private void OnEnable()
        {
            GameEvents.OnHeroPlaced += HandleHeroPlaced;
            GameEvents.OnEnemyDied  += HandleEnemyDied;
        }

        private void OnDisable()
        {
            GameEvents.OnHeroPlaced -= HandleHeroPlaced;
            GameEvents.OnEnemyDied  -= HandleEnemyDied;
        }

        // --- Event Handlers ---

        private void HandleHeroPlaced(HeroType heroType, Vector2Int gridPosition)
        {
            int cost = GetCost(heroType);
            if (CurrentGold < cost)
            {
                // Phase 1: chỉ log — UI validation sẽ ngăn trước khi đặt
                Debug.LogWarning($"[EconomyManager] Không đủ Linh Khí! Cần {cost}, có {CurrentGold}");
                return;
            }

            CurrentGold -= cost;
            Debug.Log($"[EconomyManager] Đặt {heroType} (-{cost}). Linh Khí: {CurrentGold}");
            OnGoldChanged?.Invoke(CurrentGold);
        }

        private void HandleEnemyDied(GameObject enemy, int goldReward)
        {
            if (goldReward <= 0) return;

            CurrentGold += goldReward;
            Debug.Log($"[EconomyManager] Địch chết (+{goldReward}). Linh Khí: {CurrentGold}");
            OnGoldChanged?.Invoke(CurrentGold);
        }

        // --- Public API ---

        /// <summary>Kiểm tra người chơi có đủ tiền để đặt hero này không.</summary>
        public bool CanAfford(HeroType heroType) => CurrentGold >= GetCost(heroType);

        /// <summary>Lấy cost của hero type. Trả về 50 nếu chưa config.</summary>
        public int GetCost(HeroType heroType)
        {
            return _costLookup.TryGetValue(heroType, out int cost) ? cost : 50;
        }

        // --- Private Helpers ---

        private void BuildCostLookup()
        {
            foreach (var data in heroDatas)
            {
                if (data == null) continue;
                _costLookup[data.heroType] = data.cost;
            }

            // Fallback defaults nếu chưa có HeroData asset
            if (!_costLookup.ContainsKey(HeroType.ThanhGiong)) _costLookup[HeroType.ThanhGiong] = 100;
            if (!_costLookup.ContainsKey(HeroType.SonTinh))    _costLookup[HeroType.SonTinh]    = 125;
            if (!_costLookup.ContainsKey(HeroType.ChuDongTu))  _costLookup[HeroType.ChuDongTu]  = 75;
        }
    }
}

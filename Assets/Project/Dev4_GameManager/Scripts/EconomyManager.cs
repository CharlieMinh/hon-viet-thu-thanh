using System;
using System.Collections.Generic;
using UnityEngine;
using HonVietThuThanh.Shared;

namespace HonVietThuThanh.Dev4
{
    /// <summary>
    /// EconomyManager — quản lý Linh Khí (gold) của người chơi.
    ///
    /// LUỒNG ĐÚNG:
    ///   Dev1 gọi TrySpendForPlacement(heroType) TRƯỚC khi spawn hero.
    ///   Nếu đủ tiền -> trừ tiền -> Dev1 mới được đặt hero.
    ///   Nếu không đủ tiền -> không đặt hero.
    ///
    /// LẮNG NGHE:
    ///   GameEvents.OnEnemyDied -> cộng goldReward.
    ///
    /// KHÔNG trừ tiền trong OnHeroPlaced để tránh double-spend hoặc free placement.
    /// </summary>
    public class EconomyManager : MonoBehaviour, IPlacementEconomyService
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
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            CurrentGold = startGold;
            BuildCostLookup();

            Debug.Log($"[EconomyManager] Khởi tạo. Linh Khí: {CurrentGold}");
        }

        private void Start()
        {
            // Đẩy giá trị ban đầu lên UI sau khi các UI listener đã OnEnable.
            OnGoldChanged?.Invoke(CurrentGold);
        }

        private void OnEnable()
        {
            GameEvents.OnEnemyDied += HandleEnemyDied;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyDied -= HandleEnemyDied;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // --- IPlacementEconomyService ---

        /// <summary>
        /// Dev1 gọi hàm này trước khi đặt hero.
        /// Đủ tiền thì trừ luôn và trả true.
        /// Không đủ tiền thì không trừ và trả false.
        /// </summary>
        public bool TrySpendForPlacement(HeroType heroType, int cost)
        {
            if (cost < 0)
            {
                Debug.LogWarning($"[EconomyManager] Cost không hợp lệ cho {heroType}: {cost}");
                return false;
            }

            if (CurrentGold < cost)
            {
                Debug.LogWarning($"[EconomyManager] Không đủ Linh Khí để đặt {heroType}. Cần {cost}, có {CurrentGold}");
                return false;
            }

            CurrentGold -= cost;
            Debug.Log($"[EconomyManager] Đặt {heroType} (-{cost}). Linh Khí: {CurrentGold}");
            OnGoldChanged?.Invoke(CurrentGold);

            return true;
        }

        // --- Event Handlers ---

        private void HandleEnemyDied(GameObject enemy, int goldReward)
        {
            if (goldReward <= 0) return;

            CurrentGold += goldReward;
            Debug.Log($"[EconomyManager] Địch chết (+{goldReward}). Linh Khí: {CurrentGold}");
            OnGoldChanged?.Invoke(CurrentGold);
        }

        // --- Public API ---

        /// <summary>Kiểm tra người chơi có đủ tiền để đặt hero này không.</summary>
        public bool CanAfford(HeroType heroType)
        {
            return CurrentGold >= GetCost(heroType);
        }

        /// <summary>Lấy cost của hero type. Trả về 50 nếu chưa config.</summary>
        public int GetCost(HeroType heroType)
        {
            return _costLookup.TryGetValue(heroType, out int cost) ? cost : 50;
        }

        // --- Private Helpers ---

        private void BuildCostLookup()
        {
            _costLookup.Clear();

            foreach (var data in heroDatas)
            {
                if (data == null) continue;
                _costLookup[data.heroType] = data.cost;
            }

            // Fallback defaults nếu chưa có HeroData asset
            if (!_costLookup.ContainsKey(HeroType.ThanhGiong)) _costLookup[HeroType.ThanhGiong] = 100;
            if (!_costLookup.ContainsKey(HeroType.SonTinh)) _costLookup[HeroType.SonTinh] = 125;
            if (!_costLookup.ContainsKey(HeroType.ChuDongTu)) _costLookup[HeroType.ChuDongTu] = 75;
        }
    }
}
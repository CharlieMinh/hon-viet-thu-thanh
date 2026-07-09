using UnityEngine;
using UnityEngine.InputSystem;
using HonVietThuThanh.Shared;

namespace HonVietThuThanh.Dev4
{
    /// <summary>
    /// Dev4Test — test harness độc lập cho Dev4 module.
    ///
    /// Giả lập events từ Dev1/2/3 bằng phím số để verify toàn bộ Dev4
    /// (EconomyManager, BaseHealthManager, GameStateManager, UIManager)
    /// mà không cần module thật.
    ///
    /// PHÍM TEST:
    ///   T → TrySpendForPlacement trực tiếp (test IPlacementEconomyService)
    ///   1 → Raise OnHeroPlaced ThanhGiong  (chỉ log, không trừ tiền)
    ///   2 → Raise OnHeroPlaced SonTinh
    ///   3 → Raise OnHeroPlaced ChuDongTu
    ///   4 → Enemy chết (+25 gold)
    ///   5 → Enemy tới base (-10 HP)
    ///   6 → Bắt đầu wave
    ///   7 → Hoàn thành wave (3 lần → WIN)
    ///   R → Reset wave index
    ///   H → In lại hướng dẫn
    ///
    /// ⚠️ Disable hoặc xóa trước khi merge vào Integration.
    /// </summary>
    public class Dev4Test : MonoBehaviour
    {
        [Header("Debug Info (read-only in Play Mode)")]
        [SerializeField] private int    debugGold;
        [SerializeField] private int    debugBaseHP;
        [SerializeField] private string debugGameState;

        private int _waveIndex;

        // ---------------------------------------------------------------
        // Named handlers — fix blocker 4: lambda -= không remove được,
        // phải dùng named method để unsubscribe đúng cách.
        // ---------------------------------------------------------------
        private void OnGoldChanged(int gold)         => debugGold      = gold;
        private void OnBaseHPChanged(int cur, int _) => debugBaseHP    = cur;
        private void OnStateChanged(GameState state) => debugGameState = state.ToString();

        private void OnEnable()
        {
            EconomyManager.OnGoldChanged        += OnGoldChanged;
            BaseHealthManager.OnBaseHPChanged   += OnBaseHPChanged;
            GameStateManager.OnGameStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            EconomyManager.OnGoldChanged        -= OnGoldChanged;
            BaseHealthManager.OnBaseHPChanged   -= OnBaseHPChanged;
            GameStateManager.OnGameStateChanged -= OnStateChanged;
        }

        private void Start()
        {
            if (EconomyManager.Instance)    debugGold      = EconomyManager.Instance.CurrentGold;
            if (BaseHealthManager.Instance) debugBaseHP    = BaseHealthManager.Instance.CurrentBaseHP;
            if (GameStateManager.Instance)  debugGameState = GameStateManager.Instance.CurrentState.ToString();

            RunAutoValidation();
            PrintHelp();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            // T: gọi thẳng TrySpendForPlacement (test blocker 1 & 2)
            if (kb.tKey.wasPressedThisFrame)
                TestTrySpendForPlacement();

            // 1-3: Raise OnHeroPlaced — verify chỉ log, không trừ tiền
            if (kb.digit1Key.wasPressedThisFrame)
            {
                Debug.Log("--- [TEST] Raise OnHeroPlaced ThanhGiong (chỉ log, không trừ tiền) ---");
                GameEvents.RaiseHeroPlaced(HeroType.ThanhGiong, Vector2Int.zero);
            }
            if (kb.digit2Key.wasPressedThisFrame)
            {
                Debug.Log("--- [TEST] Raise OnHeroPlaced SonTinh (chỉ log, không trừ tiền) ---");
                GameEvents.RaiseHeroPlaced(HeroType.SonTinh, new Vector2Int(1, 0));
            }
            if (kb.digit3Key.wasPressedThisFrame)
            {
                Debug.Log("--- [TEST] Raise OnHeroPlaced ChuDongTu (chỉ log, không trừ tiền) ---");
                GameEvents.RaiseHeroPlaced(HeroType.ChuDongTu, new Vector2Int(2, 0));
            }

            // 4: enemy chết → gold tăng
            if (kb.digit4Key.wasPressedThisFrame)
            {
                Debug.Log("--- [TEST] Enemy chết → +25 Linh Khí ---");
                GameEvents.RaiseEnemyDied(null, 25);
            }

            // 5: enemy tới base → HP thành giảm
            if (kb.digit5Key.wasPressedThisFrame)
            {
                Debug.Log("--- [TEST] Enemy tới base → -10 HP thành ---");
                GameEvents.RaiseEnemyReachedBase(null);
            }

            // 6: bắt đầu wave
            if (kb.digit6Key.wasPressedThisFrame)
            {
                Debug.Log($"--- [TEST] Wave {_waveIndex + 1} bắt đầu ---");
                GameEvents.RaiseWaveStarted(_waveIndex);
            }

            // 7: hoàn thành wave — 3 lần → WIN
            if (kb.digit7Key.wasPressedThisFrame)
            {
                Debug.Log($"--- [TEST] Wave {_waveIndex + 1} hoàn thành ---");
                GameEvents.RaiseWaveCompleted(_waveIndex);
                _waveIndex++;
            }

            // R: reset wave index
            if (kb.rKey.wasPressedThisFrame)
            {
                _waveIndex = 0;
                Debug.Log("--- [TEST] Wave index reset về 0 ---");
            }

            // H: in hướng dẫn
            if (kb.hKey.wasPressedThisFrame)
                PrintHelp();
        }

        // ---------------------------------------------------------------
        // Test TrySpendForPlacement trực tiếp
        // ---------------------------------------------------------------

        private void TestTrySpendForPlacement()
        {
            if (EconomyManager.Instance == null)
            {
                Debug.LogError("[Dev4Test T] EconomyManager.Instance null!");
                return;
            }

            int before = EconomyManager.Instance.CurrentGold;
            bool result = EconomyManager.Instance.TrySpendForPlacement(HeroType.ThanhGiong, 100);
            int after  = EconomyManager.Instance.CurrentGold;

            if (result)
                Debug.Log($"[Dev4Test T] ✅ TrySpend ThanhGiong -100. Gold: {before} → {after}");
            else
                Debug.Log($"[Dev4Test T] ❌ Không đủ tiền (cần 100, có {before}). Gold không đổi: {after}");
        }

        // ---------------------------------------------------------------
        // Auto-validation chạy khi Start
        // ---------------------------------------------------------------

        private void RunAutoValidation()
        {
            Debug.Log("===== [Dev4Test] Auto-Validation =====");
            bool pass = true;

            if (EconomyManager.Instance == null)
            { Debug.LogError("[Dev4Test] ❌ EconomyManager.Instance null"); pass = false; }
            else
                Debug.Log($"[Dev4Test] ✅ EconomyManager OK — Gold: {EconomyManager.Instance.CurrentGold}");

            if (BaseHealthManager.Instance == null)
            { Debug.LogError("[Dev4Test] ❌ BaseHealthManager.Instance null"); pass = false; }
            else
                Debug.Log($"[Dev4Test] ✅ BaseHealthManager OK — HP: {BaseHealthManager.Instance.CurrentBaseHP}/{BaseHealthManager.Instance.MaxBaseHP}");

            if (GameStateManager.Instance == null)
            { Debug.LogError("[Dev4Test] ❌ GameStateManager.Instance null"); pass = false; }
            else
                Debug.Log($"[Dev4Test] ✅ GameStateManager OK — State: {GameStateManager.Instance.CurrentState}");

            if (EconomyManager.Instance is IPlacementEconomyService)
                Debug.Log("[Dev4Test] ✅ EconomyManager implements IPlacementEconomyService");
            else
            { Debug.LogError("[Dev4Test] ❌ EconomyManager KHÔNG implement IPlacementEconomyService!"); pass = false; }

            Debug.Log(pass
                ? "===== [Dev4Test] PASSED ====="
                : "===== [Dev4Test] FAILED — xem lỗi ở trên =====");
        }

        private static void PrintHelp()
        {
            Debug.Log(
                "===== DEV4 TEST KEYS =====\n" +
                "T → TrySpendForPlacement ThanhGiong -100 (test IPlacementEconomyService)\n" +
                "1 → OnHeroPlaced ThanhGiong  (chỉ log, KHÔNG trừ tiền)\n" +
                "2 → OnHeroPlaced SonTinh     (chỉ log, KHÔNG trừ tiền)\n" +
                "3 → OnHeroPlaced ChuDongTu   (chỉ log, KHÔNG trừ tiền)\n" +
                "4 → Enemy chết → +25 Linh Khí\n" +
                "5 → Enemy tới base → -10 HP thành\n" +
                "6 → Bắt đầu wave tiếp theo\n" +
                "7 → Hoàn thành wave (3 lần → WIN)\n" +
                "R → Reset wave index về 0\n" +
                "H → Hiện lại hướng dẫn này\n" +
                "=========================="
            );
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using HonVietThuThanh.Shared;

namespace HonVietThuThanh.Dev4
{
    /// <summary>
    /// Dev4Test — script test tạm để giả lập events từ Dev1/2/3.
    /// Dùng TRONG EDITOR để kiểm tra EconomyManager, BaseHealthManager,
    /// GameStateManager, UIManager hoạt động đúng mà không cần module thật.
    ///
    /// ⚠️ XÓA hoặc disable script này trước khi ghép Integration với Dev1/2/3.
    ///
    /// CÁCH DÙNG:
    ///   1. Gắn script này vào bất kỳ GameObject nào trong Scene_Dev4_UI
    ///   2. Nhấn Play
    ///   3. Dùng phím số 1-7 để giả lập từng event
    ///   4. Quan sát Console + HUD cập nhật
    ///
    /// NOTE: Project dùng New Input System → dùng Keyboard.current thay vì
    /// UnityEngine.Input.GetKeyDown (legacy).
    /// </summary>
    public class Dev4Test : MonoBehaviour
    {
        [Header("Thông tin test (chỉ đọc, cập nhật trong Play Mode)")]
        [SerializeField, Tooltip("Linh Khí hiện tại")]
        private int debugGold;

        [SerializeField, Tooltip("HP thành hiện tại")]
        private int debugBaseHP;

        [SerializeField, Tooltip("Trạng thái game hiện tại")]
        private string debugGameState;

        private int _waveIndex = 0;

        private void OnEnable()
        {
            EconomyManager.OnGoldChanged        += g      => debugGold      = g;
            BaseHealthManager.OnBaseHPChanged   += (c, _) => debugBaseHP    = c;
            GameStateManager.OnGameStateChanged += s      => debugGameState = s.ToString();
        }

        private void OnDisable()
        {
            EconomyManager.OnGoldChanged        -= g      => debugGold      = g;
            BaseHealthManager.OnBaseHPChanged   -= (c, _) => debugBaseHP    = c;
            GameStateManager.OnGameStateChanged -= s      => debugGameState = s.ToString();
        }

        private void Start()
        {
            // Sync giá trị ban đầu
            if (EconomyManager.Instance)    debugGold      = EconomyManager.Instance.CurrentGold;
            if (BaseHealthManager.Instance) debugBaseHP    = BaseHealthManager.Instance.CurrentBaseHP;
            if (GameStateManager.Instance)  debugGameState = GameStateManager.Instance.CurrentState.ToString();

            PrintHelp();
        }

        private void Update()
        {
            // New Input System: dùng Keyboard.current thay cho Input.GetKeyDown
            var kb = Keyboard.current;
            if (kb == null) return; // không có keyboard (build mobile, etc.)

            // Phím 1: Giả lập đặt Thánh Gióng (trừ 100 Linh Khí)
            if (kb.digit1Key.wasPressedThisFrame)
            {
                Debug.Log("--- [TEST] Đặt Thánh Gióng (cost 100) ---");
                GameEvents.RaiseHeroPlaced(HeroType.ThanhGiong, new Vector2Int(0, 0));
            }

            // Phím 2: Giả lập đặt Sơn Tinh (trừ 125 Linh Khí)
            if (kb.digit2Key.wasPressedThisFrame)
            {
                Debug.Log("--- [TEST] Đặt Sơn Tinh (cost 125) ---");
                GameEvents.RaiseHeroPlaced(HeroType.SonTinh, new Vector2Int(1, 0));
            }

            // Phím 3: Giả lập đặt Chử Đồng Tử (trừ 75 Linh Khí)
            if (kb.digit3Key.wasPressedThisFrame)
            {
                Debug.Log("--- [TEST] Đặt Chử Đồng Tử (cost 75) ---");
                GameEvents.RaiseHeroPlaced(HeroType.ChuDongTu, new Vector2Int(2, 0));
            }

            // Phím 4: Giả lập 1 enemy chết (+25 Linh Khí)
            if (kb.digit4Key.wasPressedThisFrame)
            {
                Debug.Log("--- [TEST] Enemy chết → +25 Linh Khí ---");
                GameEvents.RaiseEnemyDied(null, 25);
            }

            // Phím 5: Giả lập enemy tới base (-10 HP thành)
            if (kb.digit5Key.wasPressedThisFrame)
            {
                Debug.Log("--- [TEST] Enemy tới base → -10 HP thành ---");
                GameEvents.RaiseEnemyReachedBase(null);
            }

            // Phím 6: Giả lập bắt đầu wave tiếp theo
            if (kb.digit6Key.wasPressedThisFrame)
            {
                Debug.Log($"--- [TEST] Bắt đầu Wave {_waveIndex + 1} ---");
                GameEvents.RaiseWaveStarted(_waveIndex);
            }

            // Phím 7: Giả lập hoàn thành wave hiện tại
            if (kb.digit7Key.wasPressedThisFrame)
            {
                Debug.Log($"--- [TEST] Hoàn thành Wave {_waveIndex + 1} ---");
                GameEvents.RaiseWaveCompleted(_waveIndex);
                _waveIndex++;
            }

            // Phím R: Reset wave index về 0
            if (kb.rKey.wasPressedThisFrame)
            {
                _waveIndex = 0;
                Debug.Log("--- [TEST] Reset wave index về 0 ---");
            }

            // Phím H: In lại hướng dẫn phím
            if (kb.hKey.wasPressedThisFrame)
            {
                PrintHelp();
            }
        }

        private static void PrintHelp()
        {
            Debug.Log(
                "===== DEV4 TEST KEYS =====\n" +
                "1 → Đặt Thánh Gióng (-100 Linh Khí)\n" +
                "2 → Đặt Sơn Tinh    (-125 Linh Khí)\n" +
                "3 → Đặt Chử Đồng Tử (-75  Linh Khí)\n" +
                "4 → Enemy chết      (+25  Linh Khí)\n" +
                "5 → Enemy tới base  (-10  HP Thành)\n" +
                "6 → Bắt đầu wave tiếp theo\n" +
                "7 → Hoàn thành wave hiện tại\n" +
                "R → Reset wave index về 0\n" +
                "H → Hiện lại hướng dẫn này\n" +
                "=========================="
            );
        }
    }
}

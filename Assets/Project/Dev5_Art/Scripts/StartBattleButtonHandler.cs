using UnityEngine;
using UnityEngine.UI;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Gắn lên nút Start Battle UI để gọi khởi động Combat Phase.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class StartBattleButtonHandler : MonoBehaviour
    {
        private void Start()
        {
            Button btn = GetComponent<Button>();
            btn.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (GamePhaseManager.Instance != null)
            {
                GamePhaseManager.Instance.StartCombat();
            }
            else
            {
                Debug.LogWarning("[StartBattleButtonHandler] Không tìm thấy GamePhaseManager.Instance!");
            }
        }
    }
}

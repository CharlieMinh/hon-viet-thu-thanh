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
            ApplyArtLayout();
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

        private void ApplyArtLayout()
        {
            RectTransform rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(360f, 82f);
                rect.anchoredPosition = new Vector2(-24f, 24f);
            }

            Image image = GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.white;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
                Sprite startSprite = Dev5RuntimeUIArt.LoadSprite(Dev5RuntimeUIArt.StartButton);
                if (startSprite != null)
                {
                    image.sprite = startSprite;
                }
            }
        }
    }
}

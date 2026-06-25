using UnityEngine;
using TMPro;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Component quản lý hiển thị sao trên đầu cờ và scale kích thước cờ (Phase 12).
    /// </summary>
    [RequireComponent(typeof(UnitStarData))]
    public class UnitStarVisual : MonoBehaviour
    {
        private TMP_Text starText;
        private UnitStarData starData;
        private Vector3 baseScale;
        private bool baseScaleCaptured = false;

        private void Awake()
        {
            starData = GetComponent<UnitStarData>();
            CaptureBaseScale();
            SetupTextComponent();
        }

        private void Start()
        {
            RefreshVisual();
        }

        private void CaptureBaseScale()
        {
            if (!baseScaleCaptured)
            {
                baseScale = transform.localScale;
                baseScaleCaptured = true;
            }
        }

        private void SetupTextComponent()
        {
            // Tìm trong các gameobject con trước
            starText = GetComponentInChildren<TMP_Text>();
            if (starText == null)
            {
                // Tìm Transform cha thích hợp (tìm child "UI")
                Transform uiParent = transform.Find("UI");
                Transform parentToUse = uiParent != null ? uiParent : transform;

                // Tìm hoặc tạo child "StarText" dưới parentToUse
                Transform existingStarText = parentToUse.Find("StarText");
                if (existingStarText != null)
                {
                    starText = existingStarText.GetComponent<TMP_Text>();
                    if (starText == null)
                    {
                        starText = existingStarText.gameObject.AddComponent<TextMeshPro>();
                    }
                }
                else
                {
                    // Nếu chưa có, sinh động GameObject mới cho Text
                    GameObject textGo = new GameObject("StarText");
                    textGo.transform.SetParent(parentToUse, false);
                    // Đặt ở tọa độ Y = 2.5f (trên đỉnh Capsule cao 2)
                    textGo.transform.localPosition = new Vector3(0f, 2.5f, 0f);
                    textGo.transform.localRotation = Quaternion.identity;

                    starText = textGo.AddComponent<TextMeshPro>();
                }
                
                starText.alignment = TextAlignmentOptions.Center;
                starText.fontSize = 4;
                starText.color = Color.yellow;
            }
        }

        /// <summary>
        /// Làm mới văn bản sao và tỉ lệ phóng to của quân cờ.
        /// </summary>
        public void RefreshVisual()
        {
            if (starData == null) starData = GetComponent<UnitStarData>();
            if (starData == null) return;

            CaptureBaseScale();

            int star = starData.starLevel;

            // 1. Cập nhật Text hiển thị theo định dạng phù hợp với sao vô hạn
            if (starText != null)
            {
                if (star == 1)
                {
                    starText.text = "★";
                }
                else if (star == 2)
                {
                    starText.text = "★★";
                }
                else if (star == 3)
                {
                    starText.text = "★★★";
                }
                else
                {
                    starText.text = $"★ x{star}";
                }
            }

            // 2. Tăng nhẹ kích thước cờ theo cấp sao: tăng 10% mỗi cấp sao từ sao thứ 2
            float scaleMultiplier = 1f + 0.1f * (star - 1);
            transform.localScale = baseScale * scaleMultiplier;
        }

        private void LateUpdate()
        {
            // Billboard effect: Xoay text hướng thẳng về Camera chính để người chơi luôn nhìn thấy
            if (starText != null && Camera.main != null)
            {
                starText.transform.rotation = Quaternion.LookRotation(starText.transform.position - Camera.main.transform.position);
            }
        }
    }
}

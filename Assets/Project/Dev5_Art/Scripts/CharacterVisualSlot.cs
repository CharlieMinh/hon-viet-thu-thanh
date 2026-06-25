using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Phase 18 Setup: Character Model Visual Slot.
    /// Script này gắn ở root prefab unit/enemy để chuẩn bị slot cho model thật và quản lý placeholder.
    /// </summary>
    public class CharacterVisualSlot : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Transform root chứa visual")]
        public Transform visualRoot;

        [Tooltip("Transform slot chứa model thật")]
        public Transform modelSlot;

        [Tooltip("GameObject chứa visual placeholder")]
        public GameObject placeholder;

        private void Start()
        {
            AutoFitModelIfNeeded();
        }

        /// <summary>
        /// Ẩn hoặc hiển thị placeholder visual.
        /// </summary>
        public void SetPlaceholderVisible(bool visible)
        {
            if (placeholder != null)
            {
                placeholder.SetActive(visible);
            }
        }

        /// <summary>
        /// Xóa toàn bộ model con đang có trong ModelSlot.
        /// </summary>
        public void ClearModelSlot()
        {
            if (modelSlot == null) return;

            for (int i = modelSlot.childCount - 1; i >= 0; i--)
            {
                Transform child = modelSlot.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            // Hiện lại placeholder sau khi dọn dẹp ModelSlot
            SetPlaceholderVisible(true);
        }

        /// <summary>
        /// Kiểm tra xem ModelSlot có chứa model con hay không.
        /// Nếu có thì ẩn placeholder, nếu không thì hiển thị placeholder.
        /// </summary>
        public void AutoFitModelIfNeeded()
        {
            if (modelSlot == null) return;

            bool hasRealModel = modelSlot.childCount > 0;
            SetPlaceholderVisible(!hasRealModel);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Tránh chạy trực tiếp thay đổi ActiveState trong OnValidate của prefab template
            // Bằng cách sử dụng EditorApplication.delayCall
            if (placeholder != null && modelSlot != null)
            {
                UnityEditor.EditorApplication.delayCall -= SafeValidatePlaceholder;
                UnityEditor.EditorApplication.delayCall += SafeValidatePlaceholder;
            }
        }

        private void SafeValidatePlaceholder()
        {
            if (this == null || placeholder == null || modelSlot == null) return;
            bool hasRealModel = modelSlot.childCount > 0;
            if (placeholder.activeSelf == hasRealModel)
            {
                placeholder.SetActive(!hasRealModel);
            }
        }
#endif
    }
}

using TMPro;
using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    public static class Dev5RuntimeUIArt
    {
        public const string RoundCompleteBackground = "Dev5UI/RoundComplete_Background";
        public const string InfoPanel = "Dev5UI/Bang_Hien_TT_panel";
        public const string StartButton = "Dev5UI/start_button_ui";
        public const string GameLogo = "Dev5UI/logo_game_transparent";

        private const string MenuFontResourcePath = "Fonts & Materials/LiberationSans SDF - Fallback";

        private static TMP_FontAsset cachedMenuFont;

        public static Sprite LoadSprite(string resourcePath)
        {
            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null)
            {
                Debug.LogWarning($"[Dev5RuntimeUIArt] Missing UI sprite resource: {resourcePath}");
            }

            return sprite;
        }

        public static TMP_FontAsset LoadMenuFont()
        {
            if (cachedMenuFont == null)
            {
                cachedMenuFont = Resources.Load<TMP_FontAsset>(MenuFontResourcePath);
            }

            return cachedMenuFont;
        }
    }
}

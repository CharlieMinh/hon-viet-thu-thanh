namespace HonVietThuThanh.Dev4
{
    /// <summary>
    /// Stores the next scene request while the loading scene is active.
    /// This is intentionally simple and does not touch gameplay systems.
    /// </summary>
    public static class SceneLoadRequest
    {
        public const string DefaultTargetSceneName = "Scene_Dev5_Art";
        public const string DefaultLoadingTitle = "";
        public const string DefaultLoadingDescription =
            "L\u00e0ng Ph\u00f9 \u0110\u1ed5ng ch\u00ecm trong kh\u00f3i l\u1eeda. Ng\u01b0\u1eddi Gi\u1eef \u0110\u1ec1n ph\u1ea3i tri\u1ec7u h\u1ed3i c\u00e1c anh h\u00f9ng \u0111\u1ec3 b\u1ea3o v\u1ec7 D\u00f2ng Ch\u1ea3y Linh Kh\u00ed.";
        public const string DefaultLoadingTip = "M\u1eb9o: S\u01a1n Tinh c\u00f3 th\u1ec3 ch\u1eb7n b\u01b0\u1edbc ti\u1ebfn c\u1ee7a k\u1ebb \u0111\u1ecbch.";

        public static string TargetSceneName { get; private set; } = DefaultTargetSceneName;
        public static string LoadingTitle { get; private set; } = DefaultLoadingTitle;
        public static string LoadingDescription { get; private set; } = DefaultLoadingDescription;
        public static string LoadingTip { get; private set; } = DefaultLoadingTip;

        public static void Configure(string targetSceneName, string title, string description, string tip)
        {
            TargetSceneName = string.IsNullOrWhiteSpace(targetSceneName) ? DefaultTargetSceneName : targetSceneName;
            LoadingTitle = string.IsNullOrWhiteSpace(title) ? DefaultLoadingTitle : title;
            LoadingDescription = string.IsNullOrWhiteSpace(description) ? DefaultLoadingDescription : description;
            LoadingTip = string.IsNullOrWhiteSpace(tip) ? DefaultLoadingTip : tip;
        }
    }
}

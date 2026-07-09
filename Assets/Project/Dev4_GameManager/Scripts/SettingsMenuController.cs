using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HonVietThuThanh.Dev4
{
    /// <summary>
    /// Handles the main menu settings panel only.
    /// Keeps settings local to PlayerPrefs and does not touch gameplay systems.
    /// </summary>
    public class SettingsMenuController : MonoBehaviour
    {
        private const string MasterVolumeKey = "MasterVolume";
        private const string FullscreenKey = "Fullscreen";
        private const string ResolutionWidthKey = "ResolutionWidth";
        private const string ResolutionHeightKey = "ResolutionHeight";

        private static readonly ResolutionOption[] ResolutionOptions =
        {
            new ResolutionOption(1280, 720, "1280 x 720"),
            new ResolutionOption(1366, 768, "1366 x 768"),
            new ResolutionOption(1600, 900, "1600 x 900"),
            new ResolutionOption(1920, 1080, "1920 x 1080 (Khuyến nghị)")
        };

        [Header("Panel")]
        [SerializeField] private GameObject settingsPanel;

        [Header("Audio")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TMP_Text masterVolumePercentText;

        [Header("Display")]
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown resolutionDropdown;

        private bool initialized;

        private void Awake()
        {
            InitializeDropdown();
            RegisterListeners();
            RefreshFromSavedSettings();
        }

        private void OnEnable()
        {
            if (initialized)
            {
                RefreshFromSavedSettings();
            }
        }

        private void OnDestroy()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.RemoveListener(PreviewMasterVolume);
            }
        }

        public void OpenSettings()
        {
            RefreshFromSavedSettings();
            SetPanelActive(true);
        }

        public void CloseWithoutSaving()
        {
            RefreshFromSavedSettings();
            SetPanelActive(false);
        }

        public void SaveSettings()
        {
            float volume = masterVolumeSlider != null ? masterVolumeSlider.value : LoadSavedVolume();
            bool fullscreen = fullscreenToggle != null ? fullscreenToggle.isOn : LoadSavedFullscreen();
            ResolutionOption selectedResolution = GetSelectedResolution();

            AudioListener.volume = volume;
            Screen.SetResolution(selectedResolution.Width, selectedResolution.Height, fullscreen);

            PlayerPrefs.SetFloat(MasterVolumeKey, volume);
            PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(ResolutionWidthKey, selectedResolution.Width);
            PlayerPrefs.SetInt(ResolutionHeightKey, selectedResolution.Height);
            PlayerPrefs.Save();

            Debug.Log("[SettingsMenuController] Settings saved.");
            SetPanelActive(false);
        }

        public void RefreshFromSavedSettings()
        {
            initialized = true;

            float savedVolume = LoadSavedVolume();
            bool savedFullscreen = LoadSavedFullscreen();
            int savedResolutionIndex = LoadSavedResolutionIndex();

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.SetValueWithoutNotify(savedVolume);
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.SetIsOnWithoutNotify(savedFullscreen);
            }

            if (resolutionDropdown != null)
            {
                resolutionDropdown.SetValueWithoutNotify(savedResolutionIndex);
                resolutionDropdown.RefreshShownValue();
            }

            PreviewMasterVolume(savedVolume);
        }

        private void RegisterListeners()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.minValue = 0f;
                masterVolumeSlider.maxValue = 1f;
                masterVolumeSlider.wholeNumbers = false;
                masterVolumeSlider.onValueChanged.RemoveListener(PreviewMasterVolume);
                masterVolumeSlider.onValueChanged.AddListener(PreviewMasterVolume);
            }
        }

        private void InitializeDropdown()
        {
            if (resolutionDropdown == null)
            {
                return;
            }

            resolutionDropdown.ClearOptions();

            foreach (ResolutionOption option in ResolutionOptions)
            {
                resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(option.Label));
            }

            resolutionDropdown.RefreshShownValue();
        }

        private void PreviewMasterVolume(float value)
        {
            AudioListener.volume = Mathf.Clamp01(value);
            UpdateVolumePercentText(value);
        }

        private void UpdateVolumePercentText(float value)
        {
            if (masterVolumePercentText == null)
            {
                return;
            }

            int percent = Mathf.RoundToInt(Mathf.Clamp01(value) * 100f);
            masterVolumePercentText.text = $"{percent}%";
        }

        private ResolutionOption GetSelectedResolution()
        {
            int index = resolutionDropdown != null ? resolutionDropdown.value : LoadSavedResolutionIndex();
            index = Mathf.Clamp(index, 0, ResolutionOptions.Length - 1);
            return ResolutionOptions[index];
        }

        private static float LoadSavedVolume()
        {
            return Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 0.75f));
        }

        private static bool LoadSavedFullscreen()
        {
            return PlayerPrefs.HasKey(FullscreenKey)
                ? PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1
                : Screen.fullScreen;
        }

        private static int LoadSavedResolutionIndex()
        {
            if (!PlayerPrefs.HasKey(ResolutionWidthKey) || !PlayerPrefs.HasKey(ResolutionHeightKey))
            {
                return FindResolutionIndex(1920, 1080);
            }

            int width = PlayerPrefs.GetInt(ResolutionWidthKey);
            int height = PlayerPrefs.GetInt(ResolutionHeightKey);
            return FindResolutionIndex(width, height);
        }

        private static int FindResolutionIndex(int width, int height)
        {
            for (int i = 0; i < ResolutionOptions.Length; i++)
            {
                ResolutionOption option = ResolutionOptions[i];
                if (option.Width == width && option.Height == height)
                {
                    return i;
                }
            }

            return ResolutionOptions.Length - 1;
        }

        private void SetPanelActive(bool active)
        {
            GameObject panel = settingsPanel != null ? settingsPanel : gameObject;
            panel.SetActive(active);
        }

        [Serializable]
        private readonly struct ResolutionOption
        {
            public ResolutionOption(int width, int height, string label)
            {
                Width = width;
                Height = height;
                Label = label;
            }

            public int Width { get; }
            public int Height { get; }
            public string Label { get; }
        }
    }
}

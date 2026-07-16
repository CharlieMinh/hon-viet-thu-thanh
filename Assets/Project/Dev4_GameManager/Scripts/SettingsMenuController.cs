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
        private const string MusicVolumeKey = "MusicVolume";
        private const string SFXVolumeKey = "SFXVolume";
        private const string FullscreenKey = "Fullscreen";
        private const string ResolutionWidthKey = "ResolutionWidth";
        private const string ResolutionHeightKey = "ResolutionHeight";
        private const float SFXOutputBoost = 1.35f;

        public static event Action<float> OnMusicVolumeChanged;
        public static event Action<float> OnSFXVolumeChanged;

        private static float sfxVolume = -1f;
        public static float SFXVolume
        {
            get
            {
                if (sfxVolume < 0f)
                {
                    sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SFXVolumeKey, 0.75f));
                }
                return sfxVolume;
            }
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                OnSFXVolumeChanged?.Invoke(sfxVolume);
            }
        }

        public static float SFXOutputVolume => Mathf.Clamp(SFXVolume * SFXOutputBoost, 0f, 1.5f);

        private static float musicVolume = -1f;
        public static float MusicVolume
        {
            get
            {
                if (musicVolume < 0f)
                {
                    musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 0.75f));
                }
                return musicVolume;
            }
            set
            {
                musicVolume = Mathf.Clamp01(value);
                OnMusicVolumeChanged?.Invoke(musicVolume);
            }
        }

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
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private TMP_Text musicVolumePercentText;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TMP_Text sfxVolumePercentText;

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
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.RemoveListener(PreviewMusicVolume);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.RemoveListener(PreviewSFXVolume);
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
            float musicVol = musicVolumeSlider != null ? musicVolumeSlider.value : MusicVolume;
            float sfxVol = sfxVolumeSlider != null ? sfxVolumeSlider.value : SFXVolume;
            bool fullscreen = fullscreenToggle != null ? fullscreenToggle.isOn : LoadSavedFullscreen();
            ResolutionOption selectedResolution = GetSelectedResolution();

            MusicVolume = musicVol;
            SFXVolume = sfxVol;
            Screen.SetResolution(selectedResolution.Width, selectedResolution.Height, fullscreen);

            PlayerPrefs.SetFloat(MusicVolumeKey, musicVol);
            PlayerPrefs.SetFloat(SFXVolumeKey, sfxVol);
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

            // Invalidate static cache to force reload from PlayerPrefs
            sfxVolume = -1f;
            musicVolume = -1f;

            float savedMusicVolume = MusicVolume;
            float savedSFXVolume = SFXVolume;
            bool savedFullscreen = LoadSavedFullscreen();
            int savedResolutionIndex = LoadSavedResolutionIndex();

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.SetValueWithoutNotify(savedMusicVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.SetValueWithoutNotify(savedSFXVolume);
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

            PreviewMusicVolume(savedMusicVolume);
            PreviewSFXVolume(savedSFXVolume);
        }

        private void RegisterListeners()
        {
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.minValue = 0f;
                musicVolumeSlider.maxValue = 1f;
                musicVolumeSlider.wholeNumbers = false;
                musicVolumeSlider.onValueChanged.RemoveListener(PreviewMusicVolume);
                musicVolumeSlider.onValueChanged.AddListener(PreviewMusicVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.minValue = 0f;
                sfxVolumeSlider.maxValue = 1f;
                sfxVolumeSlider.wholeNumbers = false;
                sfxVolumeSlider.onValueChanged.RemoveListener(PreviewSFXVolume);
                sfxVolumeSlider.onValueChanged.AddListener(PreviewSFXVolume);
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

        private void PreviewMusicVolume(float value)
        {
            MusicVolume = value;
            UpdateMusicVolumePercentText(value);
        }

        private void PreviewSFXVolume(float value)
        {
            SFXVolume = value;
            UpdateSFXVolumePercentText(value);
        }

        private void UpdateMusicVolumePercentText(float value)
        {
            if (musicVolumePercentText == null)
            {
                return;
            }

            int percent = Mathf.RoundToInt(Mathf.Clamp01(value) * 100f);
            musicVolumePercentText.text = $"{percent}%";
        }

        private void UpdateSFXVolumePercentText(float value)
        {
            if (sfxVolumePercentText == null)
            {
                return;
            }

            int percent = Mathf.RoundToInt(Mathf.Clamp01(value) * 100f);
            sfxVolumePercentText.text = $"{percent}%";
        }

        private ResolutionOption GetSelectedResolution()
        {
            int index = resolutionDropdown != null ? resolutionDropdown.value : LoadSavedResolutionIndex();
            index = Mathf.Clamp(index, 0, ResolutionOptions.Length - 1);
            return ResolutionOptions[index];
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

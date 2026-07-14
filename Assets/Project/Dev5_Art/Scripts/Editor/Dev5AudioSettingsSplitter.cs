using System.Linq;
using HonVietThuThanh.Dev4;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace HonVietThuThanh.Dev5.Editor
{
    public static class Dev5AudioSettingsSplitter
    {
        private const string ScenePath = "Assets/Project/Dev5_Art/Scenes/Scene_Dev5_Art.unity";

        [MenuItem("Hon Viet Thu Thanh/Dev5/Split Audio Settings Sliders")]
        public static void SplitAudioSettingsSliders()
        {
            if (EditorSceneManager.GetActiveScene().path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath);
            }

            SettingsMenuController settings = Object.FindObjectsByType<SettingsMenuController>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
            if (settings == null)
            {
                Debug.LogError("[Dev5AudioSettingsSplitter] SettingsMenuController not found.");
                return;
            }

            Slider musicSlider = FindOrRename<Slider>("Slider_MusicVolume", "Slider_MasterVolume");
            TMP_Text musicLabel = FindOrRename<TMP_Text>("MusicVolume_Label", "MasterVolume_Label");
            TMP_Text musicPercent = FindOrRename<TMP_Text>("MusicVolume_Percent_Text", "MasterVolume_Percent_Text");

            Slider sfxSlider = FindOrDuplicate(musicSlider, "Slider_SFXVolume");
            TMP_Text sfxLabel = FindOrDuplicate(musicLabel, "SFXVolume_Label");
            TMP_Text sfxPercent = FindOrDuplicate(musicPercent, "SFXVolume_Percent_Text");

            ConfigureRow(musicSlider, musicLabel, musicPercent, "Nhac nen", 95f);
            ConfigureRow(sfxSlider, sfxLabel, sfxPercent, "Hieu ung", 45f);

            SerializedObject serializedSettings = new SerializedObject(settings);
            serializedSettings.FindProperty("musicVolumeSlider").objectReferenceValue = musicSlider;
            serializedSettings.FindProperty("musicVolumePercentText").objectReferenceValue = musicPercent;
            serializedSettings.FindProperty("sfxVolumeSlider").objectReferenceValue = sfxSlider;
            serializedSettings.FindProperty("sfxVolumePercentText").objectReferenceValue = sfxPercent;
            serializedSettings.ApplyModifiedPropertiesWithoutUndo();

            AudioSource bgmSource = Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(source => source.gameObject.name == "Audio Source");
            if (bgmSource != null && bgmSource.GetComponent<BgmVolumeController>() == null)
            {
                bgmSource.gameObject.AddComponent<BgmVolumeController>();
                bgmSource.volume = SettingsMenuController.MusicVolume;
            }

            EditorUtility.SetDirty(settings);
            if (bgmSource != null)
            {
                EditorUtility.SetDirty(bgmSource.gameObject);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("[Dev5AudioSettingsSplitter] Split audio settings into Music and SFX sliders.");
        }

        private static T FindOrRename<T>(string desiredName, string fallbackName) where T : Component
        {
            T component = FindByName<T>(desiredName);
            if (component != null)
            {
                return component;
            }

            component = FindByName<T>(fallbackName);
            if (component != null)
            {
                component.gameObject.name = desiredName;
            }

            return component;
        }

        private static T FindOrDuplicate<T>(T source, string name) where T : Component
        {
            T existing = FindByName<T>(name);
            if (existing != null)
            {
                return existing;
            }

            GameObject copy = Object.Instantiate(source.gameObject, source.transform.parent);
            copy.name = name;
            return copy.GetComponent<T>();
        }

        private static T FindByName<T>(string name) where T : Component
        {
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(component => component.gameObject.name == name);
        }

        private static void ConfigureRow(Slider slider, TMP_Text label, TMP_Text percent, string labelText, float y)
        {
            if (slider != null)
            {
                RectTransform rect = slider.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(125f, y);
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.wholeNumbers = false;
                slider.SetValueWithoutNotify(0.75f);
            }

            if (label != null)
            {
                RectTransform rect = label.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(-235f, y);
                label.text = labelText;
            }

            if (percent != null)
            {
                RectTransform rect = percent.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(370f, y);
                percent.text = "75%";
            }
        }
    }
}

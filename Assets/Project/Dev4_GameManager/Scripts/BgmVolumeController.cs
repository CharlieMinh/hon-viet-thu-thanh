using UnityEngine;

namespace HonVietThuThanh.Dev4
{
    [RequireComponent(typeof(AudioSource))]
    public class BgmVolumeController : MonoBehaviour
    {
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            SettingsMenuController.OnMusicVolumeChanged += UpdateVolume;
            UpdateVolume(SettingsMenuController.MusicVolume);
        }

        private void OnDisable()
        {
            SettingsMenuController.OnMusicVolumeChanged -= UpdateVolume;
        }

        private void UpdateVolume(float volume)
        {
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }
    }
}

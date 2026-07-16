using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Visual/audio-only feedback for the Tank unit. It never changes placement,
    /// combat, health, economy, movement, or collider behavior.
    /// </summary>
    public class TankVoiceFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AudioSource audioSource;

        [Header("Voice Clips")]
        [SerializeField] private AudioClip[] placedClips;
        [SerializeField] private AudioClip[] attackClips;
        [SerializeField] private AudioClip[] hitClips;
        [SerializeField] private AudioClip[] deathClips;

        [Header("Playback")]
        [SerializeField] private bool muteForTesting = false;
        [SerializeField, Range(0f, 1f)] private float masterVolume = 0.85f;
        [SerializeField] private Vector2 pitchRange = new Vector2(0.96f, 1.04f);
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float hitCooldown = 0.35f;

        private PlaceableUnit placeableUnit;
        private Health health;

        private int previousHealth = -1;
        private bool wasPlaced;
        private bool placedVoicePlayed;
        private bool deathVoicePlayed;

        private float nextAttackTime;
        private float nextHitTime;
        private GameObject runtimeAudioObject;
        private float runtimeAudioDestroyTime;

        private int lastPlacedIndex = -1;
        private int lastAttackIndex = -1;
        private int lastHitIndex = -1;
        private int lastDeathIndex = -1;

        private void Reset()
        {
            ConfigureAudioSource();
        }

        private void Awake()
        {
            ConfigureAudioSource();

            placeableUnit = GetComponent<PlaceableUnit>();
            health = GetComponent<Health>();
            wasPlaced = placeableUnit != null && placeableUnit.IsPlacedOnBoard;
        }

        private void OnEnable()
        {
            if (health != null)
            {
                previousHealth = health.CurrentHealth;
                health.OnHealthChanged += HandleHealthChanged;
                health.OnDeath += HandleDeath;
            }
            HonVietThuThanh.Dev4.SettingsMenuController.OnSFXVolumeChanged += HandleSFXVolumeChanged;
            ConfigureAudioSource();
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnHealthChanged -= HandleHealthChanged;
                health.OnDeath -= HandleDeath;
            }
            HonVietThuThanh.Dev4.SettingsMenuController.OnSFXVolumeChanged -= HandleSFXVolumeChanged;
        }

        private void HandleSFXVolumeChanged(float newVolume)
        {
            ConfigureAudioSource();
        }

        private void OnValidate()
        {
            if (pitchRange.y < pitchRange.x)
            {
                pitchRange.y = pitchRange.x;
            }

            masterVolume = Mathf.Clamp01(masterVolume);
            attackCooldown = Mathf.Max(0f, attackCooldown);
            hitCooldown = Mathf.Max(0f, hitCooldown);

            ConfigureAudioSource();
        }

        private void OnDestroy()
        {
            if (runtimeAudioObject != null)
            {
                float destroyDelay = Mathf.Max(0f, runtimeAudioDestroyTime - Time.time);
                Destroy(runtimeAudioObject, destroyDelay);
                runtimeAudioObject = null;
            }
        }

        private void Update()
        {
            if (placedVoicePlayed || placeableUnit == null)
            {
                return;
            }

            bool isPlaced = placeableUnit.IsPlacedOnBoard;
            if (!wasPlaced && isPlaced)
            {
                placedVoicePlayed = true;
                PlayRandomClip(placedClips, ref lastPlacedIndex);
            }

            wasPlaced = isPlaced;
        }

        public void PlayAttackVoice()
        {
            if (deathVoicePlayed || Time.time < nextAttackTime)
            {
                return;
            }

            nextAttackTime = Time.time + attackCooldown;
            PlayRandomClip(attackClips, ref lastAttackIndex);
        }

        private void HandleHealthChanged(int current, int max)
        {
            if (previousHealth >= 0 && current < previousHealth && Time.time >= nextHitTime)
            {
                nextHitTime = Time.time + hitCooldown;
                PlayRandomClip(hitClips, ref lastHitIndex);
            }

            previousHealth = current;
        }

        private void HandleDeath()
        {
            if (deathVoicePlayed)
            {
                return;
            }

            deathVoicePlayed = true;
            PlayRandomClip(deathClips, ref lastDeathIndex);
        }

        private AudioSource EnsureAudioSource()
        {
            if (audioSource != null)
            {
                return audioSource;
            }

            if (!Application.isPlaying)
            {
                return null;
            }

            runtimeAudioObject = new GameObject("TankVoiceAudioRuntime");
            runtimeAudioObject.hideFlags = HideFlags.HideInHierarchy;
            runtimeAudioObject.transform.position = transform.position;

            audioSource = runtimeAudioObject.AddComponent<AudioSource>();
            ConfigureAudioSource();
            return audioSource;
        }

        private void ConfigureAudioSource()
        {
            if (audioSource == null)
            {
                return;
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1f;
        }

        private void PlayRandomClip(AudioClip[] clips, ref int lastIndex)
        {
            if (muteForTesting)
            {
                return;
            }
            if (clips == null || clips.Length == 0)
            {
                return;
            }

            int index = PickClipIndex(clips, lastIndex);
            AudioClip clip = clips[index];
            if (clip == null)
            {
                return;
            }

            AudioSource source = EnsureAudioSource();
            if (source == null)
            {
                return;
            }

            lastIndex = index;
            source.transform.position = transform.position;
            runtimeAudioDestroyTime = Mathf.Max(runtimeAudioDestroyTime, Time.time + clip.length);
            source.pitch = Random.Range(pitchRange.x, pitchRange.y);
            source.PlayOneShot(clip, masterVolume * HonVietThuThanh.Dev4.SettingsMenuController.SFXOutputVolume);
        }

        private int PickClipIndex(AudioClip[] clips, int lastIndex)
        {
            if (clips.Length <= 1)
            {
                return 0;
            }

            int index = Random.Range(0, clips.Length);
            if (index == lastIndex)
            {
                index = (index + 1) % clips.Length;
            }

            return index;
        }
    }
}

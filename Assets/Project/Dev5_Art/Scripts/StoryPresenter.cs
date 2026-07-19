using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HonVietThuThanh.Dev5
{
    public class StoryPresenter : MonoBehaviour
    {
        [Header("Manual Test")]
        [SerializeField] private StorySequence testSequence;
        [SerializeField] private bool playOnStart;

        [Header("Roots")]
        [SerializeField] private GameObject blackScreenPanel;
        [SerializeField] private GameObject dialogueOverlay;
        [SerializeField] private GameObject unlockPanel;

        [Header("Black Screen")]
        [SerializeField] private TMP_Text storyText;
        [SerializeField] private Button blackContinueButton;
        [SerializeField] private Button blackSkipButton;

        [Header("Dialogue")]
        [SerializeField] private Image portraitLeft;
        [SerializeField] private Image portraitRight;
        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Button dialogueContinueButton;
        [SerializeField] private Button dialogueSkipButton;

        [Header("Unlock")]
        [SerializeField] private Image heroPortrait;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text heroNameText;
        [SerializeField] private TMP_Text roleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Button unlockCloseButton;

        [Header("Audio")]
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource sfxAudioSource;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField] private bool createAudioSourcesIfMissing = true;

        private StorySequence currentSequence;
        private int currentLineIndex;

        public bool IsPlaying => currentSequence != null;

        public event Action<StorySequence> SequenceCompleted;

        private void Awake()
        {
            AutoBindMissingReferences();
            ResolveAudioSources();
            HideAll();
        }

        private void OnEnable()
        {
            AddButtonListeners();
        }

        private void Start()
        {
            if (playOnStart && testSequence != null)
            {
                Play(testSequence);
            }
        }

        private void OnDisable()
        {
            StopLineAudio();
            RemoveButtonListeners();
        }

        [ContextMenu("Auto Bind From Children")]
        public void AutoBindMissingReferences()
        {
            Transform searchRoot = ResolveSearchRoot();

            blackScreenPanel ??= FindChildGameObject(searchRoot, "BlackScreenPanel");
            dialogueOverlay ??= FindChildGameObject(searchRoot, "DialogueOverlay");
            unlockPanel ??= FindChildGameObject(searchRoot, "UnlockPanel");

            storyText ??= FindChildComponent<TMP_Text>(searchRoot, "StoryText");
            blackContinueButton ??= FindButton(searchRoot, "BlackScreenPanel", "ContinueButton");
            blackSkipButton ??= FindButton(searchRoot, "BlackScreenPanel", "SkipButton");

            portraitLeft ??= FindChildComponent<Image>(searchRoot, "PortraitLeft");
            portraitRight ??= FindChildComponent<Image>(searchRoot, "PortraitRight");
            speakerNameText ??= FindChildComponent<TMP_Text>(searchRoot, "SpeakerNameText");
            dialogueText ??= FindChildComponent<TMP_Text>(searchRoot, "DialogueText");
            dialogueContinueButton ??= FindButton(searchRoot, "DialogueOverlay", "ContinueButton");
            dialogueSkipButton ??= FindButton(searchRoot, "DialogueOverlay", "SkipButton");

            heroPortrait ??= FindChildComponent<Image>(searchRoot, "HeroPortrait");
            titleText ??= FindChildComponent<TMP_Text>(searchRoot, "TitleText");
            heroNameText ??= FindChildComponent<TMP_Text>(searchRoot, "HeroNameText");
            roleText ??= FindChildComponent<TMP_Text>(searchRoot, "RoleText");
            descriptionText ??= FindChildComponent<TMP_Text>(searchRoot, "DescriptionText");
            unlockCloseButton ??= FindButton(searchRoot, "UnlockPanel", "CloseButton");
        }

        [ContextMenu("Play Test Sequence")]
        public void PlayTestSequence()
        {
            if (testSequence == null)
            {
                Debug.LogWarning("[StoryPresenter] No test sequence assigned.", this);
                return;
            }

            Play(testSequence);
        }

        public void Play(StorySequence sequence)
        {
            AutoBindMissingReferences();

            if (sequence == null)
            {
                Debug.LogWarning("[StoryPresenter] Cannot play a null story sequence.", this);
                return;
            }

            StopLineAudio();
            currentSequence = sequence;
            currentLineIndex = 0;

            switch (sequence.mode)
            {
                case StorySequenceMode.BlackScreen:
                    ShowCurrentBlackScreenLine();
                    break;
                case StorySequenceMode.Dialogue:
                    ShowCurrentDialogueLine();
                    break;
                case StorySequenceMode.Unlock:
                    ShowUnlock(sequence);
                    break;
            }
        }

        public void Continue()
        {
            if (currentSequence == null)
            {
                StopLineAudio();
                HideAll();
                return;
            }

            if (currentSequence.mode == StorySequenceMode.Unlock)
            {
                CompleteCurrentSequence();
                return;
            }

            StopLineAudio();
            currentLineIndex++;

            if (currentLineIndex >= currentSequence.lines.Count)
            {
                CompleteCurrentSequence();
                return;
            }

            if (currentSequence.mode == StorySequenceMode.BlackScreen)
            {
                ShowCurrentBlackScreenLine();
            }
            else
            {
                ShowCurrentDialogueLine();
            }
        }

        public void Skip()
        {
            CompleteCurrentSequence();
        }

        public void HideAll()
        {
            SetActive(blackScreenPanel, false);
            SetActive(dialogueOverlay, false);
            SetActive(unlockPanel, false);
        }

        private void ShowCurrentBlackScreenLine()
        {
            if (blackScreenPanel == null)
            {
                Debug.LogWarning("[StoryPresenter] BlackScreenPanel is missing. Make sure StoryRoot was created by the Story UI Shell tool.", this);
                return;
            }

            HideAll();
            SetActive(blackScreenPanel, true);

            StoryLine line = GetCurrentLine();
            ApplyLineAudio(line);
            if (storyText != null)
            {
                storyText.text = line != null ? line.text : string.Empty;
            }
        }

        private void ShowCurrentDialogueLine()
        {
            if (dialogueOverlay == null)
            {
                Debug.LogWarning("[StoryPresenter] DialogueOverlay is missing. Make sure StoryRoot was created by the Story UI Shell tool.", this);
                return;
            }

            HideAll();
            SetActive(dialogueOverlay, true);

            StoryLine line = GetCurrentLine();
            ApplyLineAudio(line);
            if (speakerNameText != null)
            {
                speakerNameText.text = line != null ? line.speakerName : string.Empty;
            }

            if (dialogueText != null)
            {
                dialogueText.text = line != null ? line.text : string.Empty;
            }

            ApplyPortrait(line);
        }

        private void ApplyLineAudio(StoryLine line)
        {
            if (line == null)
            {
                return;
            }

            ResolveAudioSources();

            if (line.changeMusic && line.musicClip != null && musicAudioSource != null)
            {
                if (musicAudioSource.clip != line.musicClip)
                {
                    musicAudioSource.Stop();
                    musicAudioSource.clip = line.musicClip;
                }

                musicAudioSource.loop = true;
                musicAudioSource.spatialBlend = 0f;
                musicAudioSource.volume = musicVolume;

                if (!musicAudioSource.isPlaying)
                {
                    musicAudioSource.Play();
                }
            }

            if (line.audioClip != null && sfxAudioSource != null)
            {
                StopLineAudio();
                sfxAudioSource.PlayOneShot(line.audioClip, sfxVolume);
            }
        }

        private void StopLineAudio()
        {
            if (sfxAudioSource != null && sfxAudioSource.isPlaying)
            {
                sfxAudioSource.Stop();
            }
        }

        private void ResolveAudioSources()
        {
            if (musicAudioSource == null)
            {
                GameObject existingMusicSource = GameObject.Find("Audio Source");
                if (existingMusicSource != null)
                {
                    musicAudioSource = existingMusicSource.GetComponent<AudioSource>();
                }
            }

            if (!createAudioSourcesIfMissing)
            {
                return;
            }

            if (musicAudioSource == null)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
                musicAudioSource.playOnAwake = false;
                musicAudioSource.loop = true;
                musicAudioSource.spatialBlend = 0f;
                musicAudioSource.volume = musicVolume;
            }

            if (sfxAudioSource == null)
            {
                sfxAudioSource = gameObject.AddComponent<AudioSource>();
                sfxAudioSource.playOnAwake = false;
                sfxAudioSource.loop = false;
                sfxAudioSource.spatialBlend = 0f;
                sfxAudioSource.volume = sfxVolume;
            }
        }

        private void ShowUnlock(StorySequence sequence)
        {
            if (unlockPanel == null)
            {
                Debug.LogWarning("[StoryPresenter] UnlockPanel is missing. Make sure StoryRoot was created by the Story UI Shell tool.", this);
                return;
            }

            HideAll();
            SetActive(unlockPanel, true);

            SetImage(heroPortrait, sequence.unlockPortrait);
            SetText(titleText, sequence.unlockTitle);
            SetText(heroNameText, sequence.unlockHeroName);
            SetText(roleText, sequence.unlockRole);
            SetText(descriptionText, sequence.unlockDescription);
        }

        private StoryLine GetCurrentLine()
        {
            if (currentSequence == null || currentSequence.lines == null)
            {
                return null;
            }

            if (currentLineIndex < 0 || currentLineIndex >= currentSequence.lines.Count)
            {
                return null;
            }

            return currentSequence.lines[currentLineIndex];
        }

        private void ApplyPortrait(StoryLine line)
        {
            SetImage(portraitLeft, null);
            SetImage(portraitRight, null);

            if (line == null || line.portrait == null)
            {
                return;
            }

            if (line.portraitSide == StoryPortraitSide.Left)
            {
                SetImage(portraitLeft, line.portrait);
            }
            else if (line.portraitSide == StoryPortraitSide.Right)
            {
                SetImage(portraitRight, line.portrait);
            }
        }

        private void CompleteCurrentSequence()
        {
            StorySequence completedSequence = currentSequence;
            StopLineAudio();
            currentSequence = null;
            currentLineIndex = 0;
            HideAll();
            SequenceCompleted?.Invoke(completedSequence);
        }

        private void AddButtonListeners()
        {
            blackContinueButton?.onClick.AddListener(Continue);
            blackSkipButton?.onClick.AddListener(Skip);
            dialogueContinueButton?.onClick.AddListener(Continue);
            dialogueSkipButton?.onClick.AddListener(Skip);
            unlockCloseButton?.onClick.AddListener(Continue);
        }

        private void RemoveButtonListeners()
        {
            blackContinueButton?.onClick.RemoveListener(Continue);
            blackSkipButton?.onClick.RemoveListener(Skip);
            dialogueContinueButton?.onClick.RemoveListener(Continue);
            dialogueSkipButton?.onClick.RemoveListener(Skip);
            unlockCloseButton?.onClick.RemoveListener(Continue);
        }

        private Transform ResolveSearchRoot()
        {
            if (transform.name == "StoryRoot")
            {
                return transform;
            }

            Transform current = transform.parent;
            while (current != null)
            {
                if (current.name == "StoryRoot")
                {
                    return current;
                }

                current = current.parent;
            }

            GameObject storyRoot = GameObject.Find("StoryRoot");
            return storyRoot != null ? storyRoot.transform : transform;
        }

        private GameObject FindChildGameObject(Transform searchRoot, string childName)
        {
            Transform child = FindChildRecursive(searchRoot, childName);
            return child != null ? child.gameObject : null;
        }

        private T FindChildComponent<T>(Transform searchRoot, string childName) where T : Component
        {
            Transform child = FindChildRecursive(searchRoot, childName);
            return child != null ? child.GetComponent<T>() : null;
        }

        private Button FindButton(Transform searchRoot, string rootName, string buttonName)
        {
            Transform root = FindChildRecursive(searchRoot, rootName);
            if (root == null)
            {
                return null;
            }

            Transform button = FindChildRecursive(root, buttonName);
            return button != null ? button.GetComponent<Button>() : null;
        }

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }

                Transform found = FindChildRecursive(child, childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }

        private static void SetImage(Image target, Sprite sprite)
        {
            if (target == null)
            {
                return;
            }

            target.sprite = sprite;
            Color color = target.color;
            color.a = sprite != null ? 1f : 0.18f;
            target.color = color;
            target.preserveAspect = true;
        }
    }
}

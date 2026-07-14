using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    public class StoryStartupController : MonoBehaviour
    {
        private enum StartupStoryStep
        {
            None,
            Intro,
            Round1PreDialogue,
            Round1Unlock
        }

        [Header("References")]
        [SerializeField] private StoryPresenter presenter;
        [SerializeField] private StorySequence introSequence;
        [SerializeField] private StorySequence round1PreDialogueSequence;
        [SerializeField] private StorySequence round1UnlockSequence;

        [Header("Startup")]
        [SerializeField] private bool playIntroOnStart = true;
        [SerializeField] private bool enterPreparationAfterIntro = true;

        private StartupStoryStep currentStep = StartupStoryStep.None;

        private void Awake()
        {
            ResolvePresenter();
        }

        private void Start()
        {
            if (!playIntroOnStart)
            {
                return;
            }

            PlayIntro();
        }

        private void OnDestroy()
        {
            if (presenter != null)
            {
                presenter.SequenceCompleted -= HandleSequenceCompleted;
            }
        }

        [ContextMenu("Play Intro")]
        public void PlayIntro()
        {
            ResolvePresenter();

            if (presenter == null)
            {
                Debug.LogWarning("[StoryStartupController] Cannot play intro: StoryPresenter is missing.", this);
                return;
            }

            if (introSequence == null)
            {
                Debug.LogWarning("[StoryStartupController] Cannot play intro: intro sequence is not assigned.", this);
                return;
            }

            presenter.SequenceCompleted -= HandleSequenceCompleted;
            presenter.SequenceCompleted += HandleSequenceCompleted;
            currentStep = StartupStoryStep.Intro;
            Debug.Log($"[StoryStartupController] Playing intro sequence '{introSequence.name}'.", this);
            presenter.Play(introSequence);
        }

        private void HandleSequenceCompleted(StorySequence completedSequence)
        {
            if (currentStep == StartupStoryStep.Intro && completedSequence == introSequence)
            {
                if (round1PreDialogueSequence != null)
                {
                    currentStep = StartupStoryStep.Round1PreDialogue;
                    Debug.Log($"[StoryStartupController] Playing round 1 pre-dialogue sequence '{round1PreDialogueSequence.name}'.", this);
                    presenter.Play(round1PreDialogueSequence);
                    return;
                }

                PlayRound1UnlockOrComplete();
                return;
            }

            if (currentStep == StartupStoryStep.Round1PreDialogue && completedSequence == round1PreDialogueSequence)
            {
                PlayRound1UnlockOrComplete();
                return;
            }

            if (currentStep == StartupStoryStep.Round1Unlock && completedSequence == round1UnlockSequence)
            {
                CompleteStartupStoryFlow();
            }
        }

        private void PlayRound1UnlockOrComplete()
        {
            if (round1UnlockSequence != null)
            {
                currentStep = StartupStoryStep.Round1Unlock;
                Debug.Log($"[StoryStartupController] Playing round 1 unlock sequence '{round1UnlockSequence.name}'.", this);
                presenter.Play(round1UnlockSequence);
                return;
            }

            CompleteStartupStoryFlow();
        }

        private void CompleteStartupStoryFlow()
        {
            currentStep = StartupStoryStep.None;
            presenter.SequenceCompleted -= HandleSequenceCompleted;

            if (enterPreparationAfterIntro && GamePhaseManager.Instance != null)
            {
                GamePhaseManager.Instance.StartPreparation();
            }
        }

        private void ResolvePresenter()
        {
            if (presenter != null)
            {
                return;
            }

            presenter = GetComponent<StoryPresenter>();
            if (presenter != null)
            {
                return;
            }

            presenter = GetComponentInChildren<StoryPresenter>(true);
            if (presenter != null)
            {
                return;
            }

            presenter = FindAnyObjectByType<StoryPresenter>(FindObjectsInactive.Include);
        }
    }
}

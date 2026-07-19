using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    public class StoryWaveDialogueController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StoryPresenter presenter;

        [Header("Round 2")]
        [SerializeField] private StorySequence round2PreDialogueSequence;
        [SerializeField] private StorySequence round2UnlockSequence;
        [SerializeField] private bool playRound2PreDialogue = true;
        [SerializeField] private bool playRound2Unlock = true;

        [Header("Round 3")]
        [SerializeField] private StorySequence round3PreDialogueSequence;
        [SerializeField] private StorySequence round3UnlockSequence;
        [SerializeField] private bool playRound3PreDialogue = true;
        [SerializeField] private bool playRound3Unlock = true;

        [Header("Round 4")]
        [SerializeField] private StorySequence round4PreDialogueSequence;
        [SerializeField] private bool playRound4PreDialogue = true;

        [Header("Round 5")]
        [SerializeField] private StorySequence round5PreDialogueSequence;
        [SerializeField] private bool playRound5PreDialogue = true;

        private bool round2PreDialoguePlayed;
        private bool round2UnlockPlayed;
        private bool round3PreDialoguePlayed;
        private bool round3UnlockPlayed;
        private bool round4PreDialoguePlayed;
        private bool round5PreDialoguePlayed;

        private void Awake()
        {
            ResolvePresenter();
        }

        private void Update()
        {
            TryPlayRound2PreDialogue();
            TryPlayRound2UnlockWithoutDialogue();
            TryPlayRound3PreDialogue();
            TryPlayRound3UnlockWithoutDialogue();
            TryPlayRound4PreDialogue();
            TryPlayRound5PreDialogue();
        }

        private void OnDisable()
        {
            if (presenter != null)
            {
                presenter.SequenceCompleted -= HandleRound2PreDialogueCompleted;
                presenter.SequenceCompleted -= HandleRound3PreDialogueCompleted;
            }
        }

        private void TryPlayRound2PreDialogue()
        {
            if (!playRound2PreDialogue || round2PreDialoguePlayed || round2PreDialogueSequence == null)
            {
                return;
            }

            ResolvePresenter();
            if (presenter == null || presenter.IsPlaying)
            {
                return;
            }

            if (!IsPreparationOpenForStory())
            {
                return;
            }

            if (WaveManager.Instance.currentWaveIndex != 1)
            {
                return;
            }

            round2PreDialoguePlayed = true;
            if (playRound2Unlock && round2UnlockSequence != null)
            {
                presenter.SequenceCompleted -= HandleRound2PreDialogueCompleted;
                presenter.SequenceCompleted += HandleRound2PreDialogueCompleted;
            }

            Debug.Log($"[StoryWaveDialogueController] Playing round 2 pre-dialogue sequence '{round2PreDialogueSequence.name}'.", this);
            presenter.Play(round2PreDialogueSequence);
        }

        private void TryPlayRound2UnlockWithoutDialogue()
        {
            if (round2PreDialogueSequence != null || !CanPlayRound2Unlock())
            {
                return;
            }

            PlayRound2Unlock();
        }

        private void HandleRound2PreDialogueCompleted(StorySequence completedSequence)
        {
            if (completedSequence != round2PreDialogueSequence)
            {
                return;
            }

            presenter.SequenceCompleted -= HandleRound2PreDialogueCompleted;

            if (CanPlayRound2Unlock())
            {
                PlayRound2Unlock();
            }
        }

        private bool CanPlayRound2Unlock()
        {
            if (!playRound2Unlock || round2UnlockPlayed || round2UnlockSequence == null)
            {
                return false;
            }

            ResolvePresenter();
            if (presenter == null || presenter.IsPlaying)
            {
                return false;
            }

            if (!IsPreparationOpenForStory())
            {
                return false;
            }

            return WaveManager.Instance.currentWaveIndex == 1;
        }

        private void PlayRound2Unlock()
        {
            round2UnlockPlayed = true;
            Debug.Log($"[StoryWaveDialogueController] Playing round 2 unlock sequence '{round2UnlockSequence.name}'.", this);
            presenter.Play(round2UnlockSequence);
        }

        private void TryPlayRound3PreDialogue()
        {
            if (!playRound3PreDialogue || round3PreDialoguePlayed || round3PreDialogueSequence == null)
            {
                return;
            }

            ResolvePresenter();
            if (presenter == null || presenter.IsPlaying)
            {
                return;
            }

            if (!IsPreparationOpenForStory())
            {
                return;
            }

            if (WaveManager.Instance.currentWaveIndex != 2)
            {
                return;
            }

            round3PreDialoguePlayed = true;
            if (playRound3Unlock && round3UnlockSequence != null)
            {
                presenter.SequenceCompleted -= HandleRound3PreDialogueCompleted;
                presenter.SequenceCompleted += HandleRound3PreDialogueCompleted;
            }

            Debug.Log($"[StoryWaveDialogueController] Playing round 3 pre-dialogue sequence '{round3PreDialogueSequence.name}'.", this);
            presenter.Play(round3PreDialogueSequence);
        }

        private void TryPlayRound3UnlockWithoutDialogue()
        {
            if (round3PreDialogueSequence != null || !CanPlayRound3Unlock())
            {
                return;
            }

            PlayRound3Unlock();
        }

        private void HandleRound3PreDialogueCompleted(StorySequence completedSequence)
        {
            if (completedSequence != round3PreDialogueSequence)
            {
                return;
            }

            presenter.SequenceCompleted -= HandleRound3PreDialogueCompleted;

            if (CanPlayRound3Unlock())
            {
                PlayRound3Unlock();
            }
        }

        private bool CanPlayRound3Unlock()
        {
            if (!playRound3Unlock || round3UnlockPlayed || round3UnlockSequence == null)
            {
                return false;
            }

            ResolvePresenter();
            if (presenter == null || presenter.IsPlaying)
            {
                return false;
            }

            if (!IsPreparationOpenForStory())
            {
                return false;
            }

            return WaveManager.Instance.currentWaveIndex == 2;
        }

        private void PlayRound3Unlock()
        {
            round3UnlockPlayed = true;
            Debug.Log($"[StoryWaveDialogueController] Playing round 3 unlock sequence '{round3UnlockSequence.name}'.", this);
            presenter.Play(round3UnlockSequence);
        }

        private void TryPlayRound4PreDialogue()
        {
            if (!playRound4PreDialogue || round4PreDialoguePlayed || round4PreDialogueSequence == null)
            {
                return;
            }

            ResolvePresenter();
            if (presenter == null || presenter.IsPlaying)
            {
                return;
            }

            if (!IsPreparationOpenForStory())
            {
                return;
            }

            if (WaveManager.Instance.currentWaveIndex != 3)
            {
                return;
            }

            round4PreDialoguePlayed = true;
            Debug.Log($"[StoryWaveDialogueController] Playing round 4 pre-dialogue sequence '{round4PreDialogueSequence.name}'.", this);
            presenter.Play(round4PreDialogueSequence);
        }

        private void TryPlayRound5PreDialogue()
        {
            if (!playRound5PreDialogue || round5PreDialoguePlayed || round5PreDialogueSequence == null)
            {
                return;
            }

            ResolvePresenter();
            if (presenter == null || presenter.IsPlaying)
            {
                return;
            }

            if (!IsPreparationOpenForStory())
            {
                return;
            }

            if (WaveManager.Instance.currentWaveIndex != 4)
            {
                return;
            }

            round5PreDialoguePlayed = true;
            Debug.Log($"[StoryWaveDialogueController] Playing round 5 pre-dialogue sequence '{round5PreDialogueSequence.name}'.", this);
            presenter.Play(round5PreDialogueSequence);
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

        private static bool IsPreparationOpenForStory()
        {
            return GamePhaseManager.Instance != null
                && WaveManager.Instance != null
                && GamePhaseManager.Instance.CurrentState == GameState.Preparation;
        }
    }
}

using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Dev5-only wrapper that exposes simple animation commands for enemy visuals.
    /// It only updates Animator parameters and does not own movement, combat, or HP logic.
    /// </summary>
    public class EnemyAnimationController : MonoBehaviour
    {
        private const string IsMovingParameter = "IsMoving";
        private const string IsRunningParameter = "IsRunning";
        private const string AttackParameter = "Attack";
        private const string DeathParameter = "Death";

        [SerializeField] private Animator animator;

        private void Awake()
        {
            FindAnimator();
        }

        public void FindAnimator()
        {
            if (animator != null)
            {
                return;
            }

            Transform visual = transform.Find("Visual");
            if (visual != null)
            {
                Transform modelSlot = visual.Find("ModelSlot");
                if (modelSlot != null)
                {
                    animator = modelSlot.GetComponentInChildren<Animator>();
                }
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator != null)
            {
                animator.applyRootMotion = false;
            }
        }

        public void RebindAnimator()
        {
            animator = null;
            FindAnimator();
            if (animator != null)
            {
                animator.Rebind();
            }
        }

        public void PlayIdle()
        {
            if (!TryGetAnimator())
            {
                return;
            }

            animator.SetBool(IsMovingParameter, false);
            animator.SetBool(IsRunningParameter, false);
        }

        public void PlayWalk()
        {
            if (!TryGetAnimator())
            {
                return;
            }

            animator.SetBool(IsMovingParameter, true);
            animator.SetBool(IsRunningParameter, false);
        }

        public void PlayRun()
        {
            if (!TryGetAnimator())
            {
                return;
            }

            animator.SetBool(IsMovingParameter, true);
            animator.SetBool(IsRunningParameter, true);
        }

        public void PlayAttack()
        {
            if (!TryGetAnimator())
            {
                return;
            }

            animator.SetTrigger(AttackParameter);
        }

        public void PlayDeath()
        {
            if (!TryGetAnimator())
            {
                return;
            }

            animator.SetTrigger(DeathParameter);
        }

        private bool TryGetAnimator()
        {
            if (animator == null)
            {
                FindAnimator();
            }

            return animator != null;
        }
    }
}

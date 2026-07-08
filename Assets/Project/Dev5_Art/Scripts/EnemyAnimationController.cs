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
                    animator = modelSlot.GetComponentInChildren<Animator>(true);
                }
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (animator != null)
            {
                animator.applyRootMotion = false;
                Debug.Log($"[{gameObject.name}] FindAnimator: Animator found on child '{animator.gameObject.name}' with controller: {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "null")} and avatar: {(animator.avatar != null ? animator.avatar.name : "null")}");
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] FindAnimator: Animator NOT found in children!");
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
            Debug.Log($"[{gameObject.name}] PlayIdle: Setting IsMoving = false, IsRunning = false");
            animator.SetBool(IsMovingParameter, false);
            animator.SetBool(IsRunningParameter, false);
        }

        public void PlayWalk()
        {
            if (!TryGetAnimator())
            {
                return;
            }
            Debug.Log($"[{gameObject.name}] PlayWalk: Setting IsMoving = true, IsRunning = false");
            animator.SetBool(IsMovingParameter, true);
            animator.SetBool(IsRunningParameter, false);
        }

        public void PlayRun()
        {
            if (!TryGetAnimator())
            {
                return;
            }
            Debug.Log($"[{gameObject.name}] PlayRun: Setting IsMoving = true, IsRunning = true");
            animator.SetBool(IsMovingParameter, true);
            animator.SetBool(IsRunningParameter, true);
        }

        public void PlayAttack()
        {
            if (!TryGetAnimator())
            {
                return;
            }
            Debug.Log($"[{gameObject.name}] PlayAttack: Triggering Attack");
            animator.SetTrigger(AttackParameter);
        }

        public void PlayDeath()
        {
            if (!TryGetAnimator())
            {
                return;
            }
            Debug.Log($"[{gameObject.name}] PlayDeath: Triggering Death");
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

using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Forwarder component attached to the GameObject with the Animator (visual model)
    /// to forward Animation Events (footsteps, attack impacts) to the parent EnemyAnimationController.
    /// </summary>
    public class EnemyAnimationEventForwarder : MonoBehaviour
    {
        private EnemyAnimationController parentController;

        private void Awake()
        {
            parentController = GetComponentInParent<EnemyAnimationController>();
            if (parentController == null)
            {
                Debug.LogWarning($"[EnemyAnimationEventForwarder] EnemyAnimationController not found in parent of '{gameObject.name}'");
            }
        }

        // Animation Event: Footstep
        public void OnFootstep()
        {
            if (parentController != null)
            {
                parentController.TriggerFootstepSound();
            }
        }

        // Animation Event: Attack Impact
        public void OnAttackImpact()
        {
            if (parentController != null)
            {
                parentController.TriggerAttackSound();
            }
        }
    }
}

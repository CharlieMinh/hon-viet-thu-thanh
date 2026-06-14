using UnityEngine;

namespace HonVietThuThanh.Dev2_EnemyWave
{
    /// <summary>
    /// Moves a placeholder enemy from the lane start to the lane end.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyMover : MonoBehaviour
    {
        [SerializeField] private float arrivalThreshold = 0.05f;

        private Enemy owner;
        private Vector3 targetPosition;
        private float moveSpeed;
        private bool isMoving;

        public void Initialize(LanePath lanePath, Enemy enemy, float speed)
        {
            owner = enemy;
            moveSpeed = Mathf.Max(0.1f, speed);
            isMoving = lanePath != null && lanePath.HasValidPath;

            if (!isMoving)
            {
                return;
            }

            transform.position = lanePath.GetSpawnPosition();
            targetPosition = lanePath.GetEndPosition();

            Vector3 forward = lanePath.GetForwardDirection();
            if (forward.sqrMagnitude > 0.0001f)
            {
                transform.forward = forward;
            }
        }

        public void StopMovement()
        {
            isMoving = false;
        }

        private void Update()
        {
            if (!isMoving || owner == null || !owner.IsAlive())
            {
                return;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) <= arrivalThreshold)
            {
                isMoving = false;
                owner.MarkReachedBase();
            }
        }
    }
}

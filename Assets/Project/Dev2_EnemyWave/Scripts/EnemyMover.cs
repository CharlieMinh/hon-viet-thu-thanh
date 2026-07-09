using UnityEngine;

namespace HonVietThuThanh.Dev2_EnemyWave
{
    /// <summary>
    /// Moves a placeholder enemy from the lane start to the lane end.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyMover : MonoBehaviour
    {
        [SerializeField] private float arrivalThreshold = 0.1f;
        [SerializeField] private float rotationSpeed = 10f;

        private Enemy owner;
        private LanePath currentPath;
        private int currentWaypointIndex = -1;
        private float moveSpeed;
        private bool isMoving;

        public void Initialize(LanePath lanePath, Enemy enemy, float speed)
        {
            owner = enemy;
            currentPath = lanePath;
            moveSpeed = Mathf.Max(0.1f, speed);
            
            isMoving = lanePath != null && lanePath.HasValidPath;

            if (!isMoving)
            {
                return;
            }

            transform.position = lanePath.GetWaypointPosition(0);
            currentWaypointIndex = 1;
            UpdateRotation();
        }

        public void StopMovement()
        {
            isMoving = false;
        }

        private void Update()
        {
            if (!isMoving || owner == null || !owner.IsAlive() || currentPath == null)
            {
                return;
            }

            Vector3 targetPosition = currentPath.GetWaypointPosition(currentWaypointIndex);
            
            // Move toward current waypoint
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime);

            // Smooth rotation toward target
            Vector3 direction = targetPosition - transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Check if reached current waypoint
            if (Vector3.Distance(transform.position, targetPosition) <= arrivalThreshold)
            {
                currentWaypointIndex++;

                // Check if reached the end of path
                if (currentWaypointIndex >= currentPath.WaypointCount)
                {
                    isMoving = false;
                    owner.MarkReachedBase();
                }
            }
        }

        private void UpdateRotation()
        {
            if (currentPath == null || currentWaypointIndex < 0 || currentWaypointIndex >= currentPath.WaypointCount)
                return;

            Vector3 targetPosition = currentPath.GetWaypointPosition(currentWaypointIndex);
            Vector3 direction = targetPosition - transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.forward = direction.normalized;
            }
        }
    }
}

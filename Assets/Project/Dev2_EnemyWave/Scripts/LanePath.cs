using UnityEngine;

namespace HonVietThuThanh.Dev2_EnemyWave
{
    /// <summary>
    /// Simple lane definition for the Dev2 prototype.
    /// </summary>
    [DisallowMultipleComponent]
    public class LanePath : MonoBehaviour
    {
        [SerializeField] private Transform laneStart;
        [SerializeField] private Transform laneEnd;
        [SerializeField] private Color gizmoColor = new(0.95f, 0.6f, 0.15f, 1f);

        public Transform LaneStart => laneStart;
        public Transform LaneEnd => laneEnd;
        public bool HasValidPath => laneStart != null && laneEnd != null && laneStart != laneEnd;

        public Vector3 GetSpawnPosition()
        {
            return laneStart != null ? laneStart.position : transform.position;
        }

        public Vector3 GetEndPosition()
        {
            return laneEnd != null ? laneEnd.position : transform.position;
        }

        public Vector3 GetForwardDirection()
        {
            Vector3 direction = GetEndPosition() - GetSpawnPosition();
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }

        private void Reset()
        {
            AutoAssignChildren();
        }

        private void OnValidate()
        {
            AutoAssignChildren();
        }

        private void OnDrawGizmos()
        {
            if (!HasValidPath)
            {
                return;
            }

            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(laneStart.position, 0.25f);
            Gizmos.DrawSphere(laneEnd.position, 0.25f);
            Gizmos.DrawLine(laneStart.position, laneEnd.position);
        }

        private void AutoAssignChildren()
        {
            if (laneStart == null)
            {
                Transform start = transform.Find("LaneStart");
                if (start != null)
                {
                    laneStart = start;
                }
            }

            if (laneEnd == null)
            {
                Transform end = transform.Find("LaneEnd");
                if (end != null)
                {
                    laneEnd = end;
                }
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace HonVietThuThanh.Dev2_EnemyWave
{
    /// <summary>
    /// Defines a path for enemies using a start point and a sequence of waypoints.
    /// </summary>
    [DisallowMultipleComponent]
    public class LanePath : MonoBehaviour
    {
        [SerializeField] private Transform laneStart;
        [SerializeField] private List<Transform> waypoints = new();
        [SerializeField] private Color gizmoColor = new(0.95f, 0.6f, 0.15f, 1f);

        public Transform LaneStart => laneStart;
        public IReadOnlyList<Transform> Waypoints => waypoints;
        public bool HasValidPath => laneStart != null && waypoints != null && waypoints.Count > 0;

        public Vector3 GetSpawnPosition()
        {
            return laneStart != null ? laneStart.position : transform.position;
        }

        /// <summary>
        /// Returns the world position for a unified path index.
        /// Index 0 is laneStart (spawn point); index 1+ maps to waypoints[index - 1].
        /// </summary>
        public Vector3 GetWaypointPosition(int index)
        {
            if (index == 0)
            {
                return GetSpawnPosition();
            }

            int waypointIndex = index - 1;
            if (waypoints == null || waypointIndex < 0 || waypointIndex >= waypoints.Count)
            {
                return GetSpawnPosition();
            }

            return waypoints[waypointIndex].position;
        }

        /// <summary>
        /// Total unified path length: laneStart (index 0) + all waypoints.
        /// </summary>
        public int WaypointCount => (laneStart != null ? 1 : 0) + (waypoints?.Count ?? 0);

        public Vector3 GetEndPosition()
        {
            if (waypoints == null || waypoints.Count == 0)
            {
                return GetSpawnPosition();
            }
            return waypoints[waypoints.Count - 1].position;
        }

        private void OnDrawGizmos()
        {
            if (laneStart == null) return;

            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(laneStart.position, 0.25f);

            Vector3 previousPos = laneStart.position;

            if (waypoints != null)
            {
                for (int i = 0; i < waypoints.Count; i++)
                {
                    if (waypoints[i] == null) continue;

                    Vector3 currentPos = waypoints[i].position;
                    Gizmos.DrawSphere(currentPos, 0.2f);
                    Gizmos.DrawLine(previousPos, currentPos);
                    previousPos = currentPos;
                }
            }
        }
    }
}

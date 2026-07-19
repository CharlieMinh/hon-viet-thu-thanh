using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Non-combat player commander/avatar. It is intentionally kept outside
    /// board, shop, health, and combat systems.
    /// </summary>
    [DisallowMultipleComponent]
    public class CommanderAvatar : MonoBehaviour
    {
        [Header("Idle Pose")]
        [SerializeField] private bool lockPosition = true;
        [SerializeField] private bool lockRotation = true;
        [SerializeField] private bool faceMainCamera = false;
        [SerializeField] private bool yawOnly = true;

        [Header("Safety")]
        [SerializeField] private bool disableGameplayComponents = true;
        [SerializeField] private bool disableColliders = true;
        [SerializeField] private bool makeRigidbodiesKinematic = true;

        private Vector3 lockedPosition;
        private Quaternion lockedRotation;

        private void Awake()
        {
            CacheLockedPose();

            if (disableGameplayComponents)
            {
                DisableGameplayComponents();
            }

            if (disableColliders)
            {
                DisableColliders();
            }

            if (makeRigidbodiesKinematic)
            {
                MakeRigidbodiesKinematic();
            }
        }

        private void OnEnable()
        {
            CacheLockedPose();
        }

        private void LateUpdate()
        {
            if (lockPosition)
            {
                transform.position = lockedPosition;
            }

            if (faceMainCamera && Camera.main != null)
            {
                FaceCamera();
                return;
            }

            if (lockRotation)
            {
                transform.rotation = lockedRotation;
            }
        }

        public void SetLockedPose(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            lockedPosition = position;
            lockedRotation = rotation;
        }

        private void CacheLockedPose()
        {
            lockedPosition = transform.position;
            lockedRotation = transform.rotation;
        }

        private void FaceCamera()
        {
            Vector3 direction = Camera.main.transform.position - transform.position;
            if (yawOnly)
            {
                direction.y = 0f;
            }

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(-direction.normalized, Vector3.up);
            lockedRotation = transform.rotation;
        }

        private void DisableGameplayComponents()
        {
            DisableComponent<PlaceableUnit>();
            DisableComponent<UnitAutoAttack>();
            DisableComponent<UnitCombatStats>();
            DisableComponent<UnitStarData>();
            DisableComponent<UnitStarVisual>();
            DisableComponent<UnitRole>();
            DisableComponent<Health>();
            DisableComponent<HealthBar>();
            DisableComponent<TankVoiceFeedback>();
            DisableComponent<TankVisualFeedback>();
            DisableComponent<SelectableUnitVisual>();
        }

        private void DisableComponent<T>() where T : Behaviour
        {
            T[] components = GetComponentsInChildren<T>(true);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null)
                {
                    components[i].enabled = false;
                }
            }
        }

        private void DisableColliders()
        {
            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
        }

        private void MakeRigidbodiesKinematic()
        {
            Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < rigidbodies.Length; i++)
            {
                rigidbodies[i].isKinematic = true;
                rigidbodies[i].useGravity = false;
            }
        }
    }
}

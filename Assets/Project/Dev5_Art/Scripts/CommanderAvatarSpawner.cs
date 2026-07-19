using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Spawns one stationary player commander/avatar in the scene.
    /// Assign an FBX or prefab to commanderPrefab in the Inspector.
    /// </summary>
    public class CommanderAvatarSpawner : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject commanderPrefab;
        [SerializeField] private string commanderName = "PlayerCommander";

        [Header("Spawn")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Vector3 fallbackPosition = new Vector3(-6.25f, 1.05f, 2.8f);
        [SerializeField] private Vector3 fallbackEulerAngles = new Vector3(0f, 135f, 0f);
        [SerializeField] private Vector3 localScale = Vector3.one;
        [SerializeField] private bool spawnOnlyIfMissing = true;

        private CommanderAvatar spawnedCommander;

        private void Start()
        {
            SpawnCommanderIfNeeded();
        }

        [ContextMenu("Spawn Commander Now")]
        public void SpawnCommanderIfNeeded()
        {
            if (spawnOnlyIfMissing)
            {
                CommanderAvatar existing = FindAnyObjectByType<CommanderAvatar>(FindObjectsInactive.Include);
                if (existing != null)
                {
                    spawnedCommander = existing;
                    return;
                }
            }

            Vector3 position = spawnPoint != null ? spawnPoint.position : fallbackPosition;
            Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.Euler(fallbackEulerAngles);

            GameObject commander = commanderPrefab != null
                ? Instantiate(commanderPrefab, position, rotation)
                : CreateFallbackCommander(position, rotation);

            commander.name = commanderName;
            commander.transform.localScale = localScale;

            spawnedCommander = commander.GetComponent<CommanderAvatar>();
            if (spawnedCommander == null)
            {
                spawnedCommander = commander.AddComponent<CommanderAvatar>();
            }

            spawnedCommander.SetLockedPose(position, rotation);
        }

        private GameObject CreateFallbackCommander(Vector3 position, Quaternion rotation)
        {
            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            fallback.transform.SetPositionAndRotation(position, rotation);

            Renderer renderer = fallback.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.35f, 0.75f, 1f, 1f);
            }

            return fallback;
        }
    }
}

using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace HonVietThuThanh.Dev5
{
    [InitializeOnLoad]
    public static class Dev5TempInspector
    {
        static Dev5TempInspector()
        {
            // Delay call to ensure Unity scene is fully loaded
            EditorApplication.delayCall += DeleteTargetCube;
        }

        private static void DeleteTargetCube()
        {
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            bool deletedAny = false;

            foreach (GameObject go in allObjects)
            {
                if (go != null && go.name == "Cube")
                {
                    Vector3 pos = go.transform.position;
                    // Check if the position matches the target position (0.63, 1.02, -0.79) with some tolerance
                    if (Mathf.Abs(pos.x - 0.63f) < 0.05f &&
                        Mathf.Abs(pos.y - 1.02f) < 0.05f &&
                        Mathf.Abs(pos.z - (-0.79f)) < 0.05f)
                    {
                        Debug.Log($"[Dev5TempInspector] Found target Cube at {pos}, deleting it now...");
                        Undo.DestroyObjectImmediate(go);
                        deletedAny = true;
                    }
                }
            }

            if (deletedAny)
            {
                // Mark the current active scene as dirty so the changes are saved
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("[Dev5TempInspector] Successfully deleted the target Cube and marked the scene as dirty.");
            }
        }
    }
}

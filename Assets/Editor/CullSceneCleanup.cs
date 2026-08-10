using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// One-shot Cull helper (M3): removes dormant components/objects from scenes via the Unity API
/// so serialization is written by Unity itself rather than hand-edited YAML.
/// Delete this file together with the scripts it cleans up.
/// </summary>
public static class CullSceneCleanup
{
    private static readonly string[] LevelScenes =
    {
        "Assets/Level1.unity",
        "Assets/Level2.unity",
        "Assets/Level3.unity",
    };

    public static void Run()
    {
        int removedVectorTest = 0;
        int removedPmr = 0;

        foreach (var scenePath in LevelScenes)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool dirty = false;

            // vectorTest lives on a dedicated inactive "coordnate spawner test" object -> remove the object
            foreach (var vt in Object.FindObjectsOfType<vectorTest>(true))
            {
                Debug.Log($"[Cull] {scenePath}: destroying GameObject '{vt.gameObject.name}' (vectorTest)");
                Object.DestroyImmediate(vt.gameObject);
                removedVectorTest++;
                dirty = true;
            }

            // PlayerMovementRefactored is attached-but-disabled on the Level1 Player -> remove component only
            foreach (var pmr in Object.FindObjectsOfType<PlayerMovementRefactored>(true))
            {
                Debug.Log($"[Cull] {scenePath}: destroying component PlayerMovementRefactored on '{pmr.gameObject.name}'");
                Object.DestroyImmediate(pmr);
                removedPmr++;
                dirty = true;
            }

            if (dirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Cull] saved {scenePath}");
            }
            else
            {
                Debug.Log($"[Cull] {scenePath}: nothing to remove");
            }
        }

        Debug.Log($"[Cull] DONE. vectorTest objects removed: {removedVectorTest}; PlayerMovementRefactored components removed: {removedPmr}");
    }
}

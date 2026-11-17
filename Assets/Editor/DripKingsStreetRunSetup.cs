using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using DripKings.Prototype;

public static class DripKingsStreetRunSetup
{
    [MenuItem("Drip Kings/Open Street Run Prototype")]
    public static void Open()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/DripKings/Prototype/StreetRun.unity", OpenSceneMode.Single);
        Selection.activeGameObject = Object.FindFirstObjectByType<StreetRunPrototype>().gameObject;
        Debug.Log("Street Run is ready. Press Play. This standalone offline scene does not change Build Settings.");
    }
}

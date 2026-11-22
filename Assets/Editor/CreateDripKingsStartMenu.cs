#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using DripKings.MVP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DripKings.EditorTools
{
    /// <summary>Installs an offline boot scene without replacing an existing track or menu scene.</summary>
    public static class CreateDripKingsStartMenu
    {
        private const string MenuPath = "Assets/DripKings_StartMenu.unity";
        private const string RacingPath = "Assets/DripKings_MVP_Test.unity";

        [MenuItem("Drip Kings/MVP/Install Branded Start Menu")]
        public static void Install()
        {
            if (!File.Exists(RacingPath))
                throw new FileNotFoundException("MVP racing scene is missing.", RacingPath);

            if (!File.Exists(MenuPath))
            {
                if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    return;

                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                var cameraObject = new GameObject("Menu Camera");
                var camera = cameraObject.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color32(10, 15, 28, 255);
                cameraObject.AddComponent<AudioListener>();

                var menuObject = new GameObject("Drip Kings Menu");
                menuObject.AddComponent<DripKingsMenuController>();

                EditorSceneManager.SaveScene(scene, MenuPath);
                AssetDatabase.SaveAssets();
                Debug.Log("DRIP_KINGS_MENU_SCENE_CREATED: " + MenuPath);
            }
            else
            {
                Debug.Log("DRIP_KINGS_MENU_SCENE_PRESERVED: " + MenuPath);
            }

            // Keep the original Unity demo scenes available in Build Settings; only our menu
            // and racing scene are included in the separate MVP Windows build.
            var allScenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(MenuPath, true),
                new EditorBuildSettingsScene(RacingPath, true)
            };
            string[] priorScenes =
            {
                "Assets/Scenes/NetcodeBootstrap.unity",
                "Assets/Scenes/Terminal/TerminalScene.unity",
                "Assets/Scenes/Oasis/OasisScene.unity",
                "Assets/Scenes/Garden/GardenScene.unity",
                "Assets/Scenes/Cockpit/CockpitScene.unity"
            };
            foreach (string path in priorScenes)
                if (File.Exists(path))
                    allScenes.Add(new EditorBuildSettingsScene(path, true));

            foreach (var oldScene in EditorBuildSettings.scenes)
            {
                if (allScenes.Exists(s => s.path == oldScene.path)) continue;
                allScenes.Add(oldScene);
            }

            EditorBuildSettings.scenes = allScenes.ToArray();
            Debug.Log("DRIP_KINGS_MENU_INSTALL_PASS: Menu first, Race second, original scenes preserved.");
        }
    }
}
#endif

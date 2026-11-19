#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using DripKings.MVP;

namespace DripKings.EditorTools
{
    /// <summary>Generates a disposable offline MVP test scene without modifying existing scenes.</summary>
    public static class CreateMvpTestScene
    {
        [MenuItem("Drip Kings/MVP/Create Test Scene")]
        public static void Create()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Test Ground";
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(100f, 1f, 100f);

            var car = GameObject.CreatePrimitive(PrimitiveType.Cube);
            car.name = "MVP Car";
            car.transform.position = new Vector3(0f, 1f, -30f);
            car.transform.localScale = new Vector3(1.8f, 1f, 3.6f);
            car.AddComponent<Rigidbody>().mass = 1200f;
            car.AddComponent<ArcadeCarController>();

            var cameraObject = new GameObject("MVP Chase Camera");
            cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = car.transform.position + new Vector3(0f, 4f, -8f);
            cameraObject.AddComponent<ChaseCamera>().SetTarget(car.transform);

            var sun = new GameObject("Sun");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            sun.transform.rotation = Quaternion.Euler(45f, -25f, 0f);

            var race = new GameObject("MVP Race").AddComponent<CheckpointRace>();
            car.AddComponent<MvpRaceRestart>().Configure(race);
            Vector3[] gates = {
                new Vector3(0f, 2f, -10f),
                new Vector3(0f, 2f, 15f),
                new Vector3(25f, 2f, 15f),
                new Vector3(25f, 2f, -10f)
            };
            for (int i = 0; i < gates.Length; i++)
            {
                var gate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                gate.name = "Checkpoint " + i;
                gate.transform.position = gates[i];
                gate.transform.localScale = new Vector3(10f, 4f, 0.4f);
                gate.GetComponent<MeshRenderer>().enabled = false;
                gate.AddComponent<RaceCheckpoint>().Configure(race, i);
            }

            const string path = "Assets/DripKings_MVP_Test.unity";
            EditorSceneManager.SaveScene(scene, path);
            AssetDatabase.SaveAssets();
            Debug.Log("Drip Kings test scene saved to " + path + ". Open it and press Play.");
        }
    }
}
#endif

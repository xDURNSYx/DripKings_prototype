using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SnapshotExporter
{
    [MenuItem("Tools/Snapshot/Export Project + Scene Snapshot")]
    public static void Export()
    {
        var snapshot = new Snapshot();
        snapshot.generatedUtc = DateTime.UtcNow.ToString("o");
        snapshot.unityVersion = Application.unityVersion;
        snapshot.productName = Application.productName;
        snapshot.companyName = Application.companyName;
        snapshot.projectPath = Directory.GetParent(Application.dataPath).FullName;
        snapshot.activeBuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
        snapshot.activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget).ToString();
        snapshot.colorSpace = QualitySettings.activeColorSpace.ToString();
        var rp = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
        snapshot.renderPipelineAsset = rp ? rp.name : "";

        var manifestPath = Path.Combine(snapshot.projectPath, "Packages", "manifest.json");
        snapshot.packages = ReadPackagesFromManifest(manifestPath);
        snapshot.buildScenes = ReadBuildScenes();
        snapshot.openScenes = ReadOpenScenes();
        ReadTagsAndLayers(out snapshot.tags, out snapshot.layers, out snapshot.sortingLayers);
        snapshot.qualityLevels = ReadQualityLevels();

        var json = JsonUtility.ToJson(snapshot, true);
        var outputPath = Path.Combine(snapshot.projectPath, "UnitySnapshot.json");
        File.WriteAllText(outputPath, json);
        Debug.Log("Unity snapshot exported to: " + outputPath);
    }

    private static List<PackageInfo> ReadPackagesFromManifest(string manifestPath)
    {
        var result = new List<PackageInfo>();
        if (!File.Exists(manifestPath))
        {
            return result;
        }

        var text = File.ReadAllText(manifestPath);
        var depsIndex = text.IndexOf("\"dependencies\"");
        if (depsIndex < 0)
        {
            return result;
        }

        var startBrace = text.IndexOf("{", depsIndex);
        if (startBrace < 0)
        {
            return result;
        }

        var endBrace = FindMatchingBrace(text, startBrace);
        if (endBrace < 0)
        {
            return result;
        }

        var depsBlock = text.Substring(startBrace + 1, endBrace - startBrace - 1);
        var matches = Regex.Matches(depsBlock, "\"(?<name>[^\"]+)\"\\s*:\\s*\"(?<version>[^\"]+)\"");
        foreach (Match match in matches)
        {
            result.Add(new PackageInfo
            {
                name = match.Groups["name"].Value,
                version = match.Groups["version"].Value
            });
        }

        return result;
    }

    private static int FindMatchingBrace(string text, int openIndex)
    {
        var depth = 0;
        for (var i = openIndex; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '{')
            {
                depth++;
            }
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private static List<BuildSceneInfo> ReadBuildScenes()
    {
        var result = new List<BuildSceneInfo>();
        var scenes = EditorBuildSettings.scenes;
        for (var i = 0; i < scenes.Length; i++)
        {
            result.Add(new BuildSceneInfo
            {
                path = scenes[i].path,
                enabled = scenes[i].enabled
            });
        }

        return result;
    }

    private static List<SceneInfo> ReadOpenScenes()
    {
        var result = new List<SceneInfo>();
        var count = EditorSceneManager.sceneCount;
        for (var i = 0; i < count; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);
            var sceneInfo = new SceneInfo
            {
                path = scene.path,
                name = scene.name,
                isLoaded = scene.isLoaded,
                isDirty = scene.isDirty,
                rootObjects = new List<GameObjectInfo>()
            };

            if (scene.isLoaded)
            {
                var roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    sceneInfo.rootObjects.Add(BuildGameObjectInfo(root));
                }
            }

            result.Add(sceneInfo);
        }

        return result;
    }

    private static GameObjectInfo BuildGameObjectInfo(GameObject go)
    {
        var info = new GameObjectInfo
        {
            name = go.name,
            active = go.activeSelf,
            tag = go.tag,
            layer = go.layer,
            transform = new TransformInfo
            {
                localPosition = go.transform.localPosition,
                localRotationEuler = go.transform.localEulerAngles,
                localScale = go.transform.localScale
            },
            components = new List<string>(),
            children = new List<GameObjectInfo>()
        };

        var comps = go.GetComponents<Component>();
        foreach (var comp in comps)
        {
            if (comp == null)
            {
                info.components.Add("(Missing Component)");
            }
            else
            {
                info.components.Add(comp.GetType().FullName);
            }
        }

        for (var i = 0; i < go.transform.childCount; i++)
        {
            info.children.Add(BuildGameObjectInfo(go.transform.GetChild(i).gameObject));
        }

        return info;
    }

    private static void ReadTagsAndLayers(out List<string> tags, out List<string> layers, out List<string> sortingLayers)
    {
        tags = new List<string>();
        layers = new List<string>();
        sortingLayers = new List<string>();

        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets == null || assets.Length == 0)
        {
            return;
        }

        var so = new SerializedObject(assets[0]);
        var tagsProp = so.FindProperty("tags");
        if (tagsProp != null && tagsProp.isArray)
        {
            for (var i = 0; i < tagsProp.arraySize; i++)
            {
                tags.Add(tagsProp.GetArrayElementAtIndex(i).stringValue);
            }
        }

        var layersProp = so.FindProperty("layers");
        if (layersProp != null && layersProp.isArray)
        {
            for (var i = 0; i < layersProp.arraySize; i++)
            {
                layers.Add(layersProp.GetArrayElementAtIndex(i).stringValue);
            }
        }

        var sortingProp = so.FindProperty("m_SortingLayers");
        if (sortingProp != null && sortingProp.isArray)
        {
            for (var i = 0; i < sortingProp.arraySize; i++)
            {
                var item = sortingProp.GetArrayElementAtIndex(i);
                var nameProp = item.FindPropertyRelative("name");
                if (nameProp != null)
                {
                    sortingLayers.Add(nameProp.stringValue);
                }
            }
        }
    }

    private static List<QualityLevelInfo> ReadQualityLevels()
    {
        var result = new List<QualityLevelInfo>();
        var names = QualitySettings.names;
        var activeIndex = QualitySettings.GetQualityLevel();
        for (var i = 0; i < names.Length; i++)
        {
            result.Add(new QualityLevelInfo
            {
                name = names[i],
                index = i,
                isActive = i == activeIndex
            });
        }

        return result;
    }

    [Serializable]
    private class Snapshot
    {
        public string generatedUtc;
        public string unityVersion;
        public string productName;
        public string companyName;
        public string projectPath;
        public string activeBuildTarget;
        public string activeBuildTargetGroup;
        public string colorSpace;
        public string renderPipelineAsset;
        public List<PackageInfo> packages;
        public List<BuildSceneInfo> buildScenes;
        public List<SceneInfo> openScenes;
        public List<string> tags;
        public List<string> layers;
        public List<string> sortingLayers;
        public List<QualityLevelInfo> qualityLevels;
    }

    [Serializable]
    private class PackageInfo
    {
        public string name;
        public string version;
    }

    [Serializable]
    private class BuildSceneInfo
    {
        public string path;
        public bool enabled;
    }

    [Serializable]
    private class SceneInfo
    {
        public string path;
        public string name;
        public bool isLoaded;
        public bool isDirty;
        public List<GameObjectInfo> rootObjects;
    }

    [Serializable]
    private class GameObjectInfo
    {
        public string name;
        public bool active;
        public string tag;
        public int layer;
        public TransformInfo transform;
        public List<string> components;
        public List<GameObjectInfo> children;
    }

    [Serializable]
    private class TransformInfo
    {
        public Vector3 localPosition;
        public Vector3 localRotationEuler;
        public Vector3 localScale;
    }

    [Serializable]
    private class QualityLevelInfo
    {
        public string name;
        public int index;
        public bool isActive;
    }
}

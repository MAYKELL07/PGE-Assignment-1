using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

namespace Emberfall
{
    public static class EmberfallProjectSetup
    {
        const string Root = "Assets/Emberfall/";
        const string AdventureScene = Root + "Scenes/Emberfall.unity";
        const string PhysicsScene = Root + "Scenes/PhysicsLab.unity";

        [MenuItem("Tools/Project/Prepare Scenes")]
        public static void PrepareSubmission()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before project setup.");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            ConfigureAudioImports();
            CreatePhysicsScene();
            OrganizeAdventureScene();
            EmberfallDelivery.Configure();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(AdventureScene, true),
                new EditorBuildSettingsScene(PhysicsScene, true)
            };

            EditorSceneManager.OpenScene(AdventureScene, OpenSceneMode.Single);
            AssetDatabase.SaveAssets();
            ValidateSubmission();
            Debug.Log("EMBERFALL: Assignment submission structure prepared.");
        }

        static void ConfigureAudioImports()
        {
            foreach (string assetPath in AssetDatabase.FindAssets("t:AudioClip", new[] { Root + "Audio" })
                         .Select(AssetDatabase.GUIDToAssetPath))
            {
                if (!(AssetImporter.GetAtPath(assetPath) is AudioImporter importer)) continue;
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                bool isMusic = assetPath.Contains("/Music/");
                settings.loadType = isMusic ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = isMusic ? 0.72f : 0.82f;
                settings.preloadAudioData = !isMusic;
                importer.defaultSampleSettings = settings;
                importer.forceToMono = !isMusic;
                importer.SaveAndReimport();
            }
        }

        static void CreatePhysicsScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var systems = new GameObject("__SYSTEMS");
            var lab = new GameObject("Physics Lab Controller");
            lab.transform.SetParent(systems.transform);
            lab.AddComponent<Emberfall.Physics.PhysicsShowcase>();

            var cameras = new GameObject("__CAMERAS");
            Camera camera = new GameObject("Physics Lab Camera").AddComponent<Camera>();
            camera.transform.SetParent(cameras.transform);
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 6.2f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.07f, 0.11f);
            camera.gameObject.AddComponent<AudioListener>();
            if (!camera.GetComponent<UniversalAdditionalCameraData>())
                camera.gameObject.AddComponent<UniversalAdditionalCameraData>();

            new GameObject("__RUNTIME_CONTENT");
            EditorSceneManager.SaveScene(scene, PhysicsScene);
        }

        static void OrganizeAdventureScene()
        {
            Scene scene = EditorSceneManager.OpenScene(AdventureScene, OpenSceneMode.Single);
            GameObject systems = FindOrCreateRoot(scene, "__SYSTEMS");
            GameObject cameras = FindOrCreateRoot(scene, "__CAMERAS");
            FindOrCreateRoot(scene, "__RUNTIME_CONTENT");

            EmberfallGame game = UnityEngine.Object.FindFirstObjectByType<EmberfallGame>();
            if (game)
            {
                game.gameObject.name = "Adventure";
                game.transform.SetParent(systems.transform);
                if (game.transform.childCount == 0) game.CreateEditableLevel();
                SaveSceneSprites(game);
            }

            Camera camera = Camera.main;
            if (camera)
            {
                camera.gameObject.name = "Adventure Camera";
                camera.transform.SetParent(cameras.transform);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = game ? game.gameObject : systems;
            if (SceneView.lastActiveSceneView)
            {
                SceneView.lastActiveSceneView.in2DMode = true;
                SceneView.lastActiveSceneView.LookAt(Vector3.zero, Quaternion.identity, 9f);
            }
        }

        static void SaveSceneSprites(EmberfallGame game)
        {
            const string folder = Root + "Art/Sprites";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(Root + "Art", "Sprites");
            foreach (var renderer in game.GetComponentsInChildren<SpriteRenderer>(true))
            {
                Sprite sprite = renderer.sprite;
                if (!sprite || AssetDatabase.Contains(sprite)) continue;
                string path = folder + "/" + sprite.texture.name + ".asset";
                Sprite saved = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (!saved) { AssetDatabase.CreateAsset(sprite, path); saved = sprite; }
                renderer.sprite = saved;
            }
            EditorUtility.SetDirty(game);
        }

        static GameObject FindOrCreateRoot(Scene scene, string rootName)
        {
            GameObject existing = scene.GetRootGameObjects().FirstOrDefault(item => item.name == rootName);
            return existing ? existing : new GameObject(rootName);
        }

        [MenuItem("Tools/Project/Validate Scenes")]
        public static void ValidateSubmission()
        {
            var results = new List<string>();
            void Check(bool condition, string description)
            {
                results.Add((condition ? "PASS: " : "FAIL: ") + description);
                if (!condition) throw new InvalidOperationException(description);
            }

            Check(EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D, "Project defaults to 2D mode");
            Check(File.Exists(AdventureScene), "Adventure scene exists");
            Check(File.Exists(PhysicsScene), "Physics Lab scene exists");
            Check(EditorBuildSettings.scenes.Length == 2 && EditorBuildSettings.scenes.All(item => item.enabled), "Both assessment scenes are enabled in Build Settings");
            Check(AssetDatabase.FindAssets("t:AudioClip", new[] { Root + "Audio" }).Length >= 10, "Music and sound-effect library is imported");
            Check(File.Exists(Root + "Scripts/Physics/EulerBody2D.cs"), "Euler integration implementation is present");
            Check(File.Exists(Root + "Scripts/Physics/CollisionAlgorithms2D.cs"), "Circle, AABB and SAT collision algorithms are present");
            Check(File.Exists(Root + "Documentation/TechnicalReportDraft.md"), "Technical report draft is included");
            Check(File.Exists(Root + "Documentation/AssessmentEvidence.md"), "Assessment evidence matrix is included");

            string report = string.Join(Environment.NewLine, results) + Environment.NewLine + "ALL STRUCTURE CHECKS PASSED";
            File.WriteAllText("Temp/Emberfall-structure-results.txt", report);
            Debug.Log(report);
        }
    }
}

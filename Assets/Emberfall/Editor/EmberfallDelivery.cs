using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.IO;
namespace Emberfall {
public static class EmberfallDelivery {
 const string Root="Assets/Emberfall/";
 const string AdventureScene=Root+"Scenes/Emberfall.unity";
 const string PhysicsScene=Root+"Scenes/PhysicsLab.unity";
 [MenuItem("Tools/Project/Configure 2D Rendering")] public static void Configure(){
 if(EditorApplication.isPlaying)throw new System.Exception("Stop Play Mode before configuring");
 EditorSettings.defaultBehaviorMode=EditorBehaviorMode.Mode2D;
 foreach(SceneView view in Resources.FindObjectsOfTypeAll<SceneView>()){view.in2DMode=true;view.orthographic=true;}
 AssetDatabase.StartAssetEditing();try{foreach(var path in Directory.GetFiles(Root+"Art/Resources/Interface","*.png")){var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(!importer)continue;importer.textureType=TextureImporterType.Default;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.SaveAndReimport();}}finally{AssetDatabase.StopAssetEditing();}
 var renderer=AssetDatabase.LoadAssetAtPath<Renderer2DData>(Root+"Settings/EmberfallRenderer2D.asset");if(!renderer){renderer=ScriptableObject.CreateInstance<Renderer2DData>();AssetDatabase.CreateAsset(renderer,Root+"Settings/EmberfallRenderer2D.asset");}
 var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Root+"Settings/EmberfallPipeline2D.asset");if(!pipeline){pipeline=UniversalRenderPipelineAsset.Create(renderer);pipeline.supportsHDR=false;pipeline.msaaSampleCount=1;AssetDatabase.CreateAsset(pipeline,Root+"Settings/EmberfallPipeline2D.asset");}
 GraphicsSettings.defaultRenderPipeline=pipeline;int original=QualitySettings.GetQualityLevel();for(int i=0;i<QualitySettings.names.Length;i++){QualitySettings.SetQualityLevel(i,false);QualitySettings.renderPipeline=pipeline;}QualitySettings.SetQualityLevel(original,false);
 GraphicsSettings.transparencySortMode=TransparencySortMode.Orthographic;
 EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(AdventureScene,true),new EditorBuildSettingsScene(PhysicsScene,true)};
 PlayerSettings.productName="Emberfall";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
 var target=UnityEditor.Build.NamedBuildTarget.Standalone;PlayerSettings.SetScriptingBackend(target,ScriptingImplementation.Mono2x);PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{GraphicsDeviceType.Direct3D11});
 var scene=EditorSceneManager.GetActiveScene();if(scene.path==AdventureScene){var camera=Camera.main;if(camera){camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.045f,.07f);camera.GetUniversalAdditionalCameraData().SetRenderer(0);}RenderSettings.skybox=null;EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}
 AssetDatabase.SaveAssets();File.WriteAllText("Temp/Emberfall-2D.txt","Default behavior: 2D\nScene view: 2D orthographic\nRender pipeline: dedicated URP Renderer2D across all quality levels\nBuild scenes: Emberfall and PhysicsLab\nWindows x64 / Mono / Direct3D 11\n");Debug.Log("EMBERFALL: Converted to 2D.");
 }
 [MenuItem("Tools/Build/Windows Game")] public static void Build(){
 if(EditorApplication.isPlaying)throw new System.Exception("Stop Play Mode before building");
 string path="Builds/Windows/Emberfall.exe";Directory.CreateDirectory(Path.GetDirectoryName(path));
 var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{AdventureScene,PhysicsScene},locationPathName=path,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
 File.WriteAllText("Temp/Emberfall-build-result.txt",report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings+"\nSize: "+report.summary.totalSize+"\nTime: "+report.summary.totalTime+"\nOutput: "+path);Debug.Log("EMBERFALL BUILD: "+report.summary.result);
 }
}
}

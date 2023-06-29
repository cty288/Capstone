using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Mikrocosmos;
using MikroFramework.ResKit;
using MikroFramework.SceneEntranceKit;
using MikroFramework.Serializer;
using MikroFramework.Utilities;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;
using EditorUtility = UnityEditor.EditorUtility;


public class BuildGame : EditorWindow
{
    protected static string m_BuildPath = "";
    private static string version = "";
    public static BuildTarget buildTarget = BuildTarget.StandaloneWindows;
    public static bool debugMode = true;
    public static bool exportAppidFile = false;
    
    [MenuItem("DPunk/Build Game")] 
    public static void BuildGameWorkflow () {
        
        version = Application.version;
        buildTarget = EditorUserBuildSettings.activeBuildTarget;
        //set buildpath to be located in the parent folder of the Assets folder
        m_BuildPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Dpunk Released/" + version + "/"));
        GetWindow<BuildGame>().Show();
    }
    
    [MenuItem("DPunk/Build AB Only")] 
    public static void BuildABMenuItem () {
        BuildAB();
        GameObject gameObject = GameObject.Find("[ResKit]");
        if (gameObject) {
            DestroyImmediate(gameObject);
        }
    }

    public static void Build(string path, BuildTarget target, string version) {
        PlayerSettings.bundleVersion = version;
        
        //if the path doesn't exist, create it
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
        //check if the editor's build target is the same as the target platform. If not, switch it.
       // BuildTarget previousBuildTarget = EditorUserBuildSettings.activeBuildTarget;
        if (EditorUserBuildSettings.activeBuildTarget != target) {
            string prevABLocation = HotUpdateConfig.LocalAssetBundleFolder;
            //delete the previous asset bundle folder
            if (Directory.Exists(prevABLocation)) {
                Directory.Delete(prevABLocation, true);
            }
            EditorUserBuildSettings.SwitchActiveBuildTarget(target);
        }
        

        BuildAB();
        GameObject gameObject = GameObject.Find("[ResKit]");
        if (gameObject) {
            DestroyImmediate(gameObject);
        }
        string[] levels = new string[] {"Assets/01. Scenes/GameEntrance.unity"};
        
        
        // Build player.
        BuildPipeline.BuildPlayer(levels, path + "/Mikrocosmos.exe", target, BuildOptions.None);

      
        //open the folder
        MikroFramework.Utilities.EditorUtility.OpenInFolder(path);
    }
    private void OnGUI() {
        //choose location: an input field and a button, when clicked, open a file browser.
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Build Location");
        m_BuildPath = EditorGUILayout.TextField(m_BuildPath);
        if (GUILayout.Button("Browse")) {
            m_BuildPath = EditorUtility.OpenFolderPanel("Choose Location of Built Game", "", "");
        }
        EditorGUILayout.EndHorizontal();
        
        //define version, with default value equals to Application.version
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Version");
        version = EditorGUILayout.TextField(version);
        EditorGUILayout.EndHorizontal();
        
        //build target
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Build Target");
        buildTarget = (BuildTarget) EditorGUILayout.EnumPopup(buildTarget);
        EditorGUILayout.EndHorizontal();
        
       
        
        //build button
        if (GUILayout.Button("Build & Switch Build Target!")) {
            Build(m_BuildPath, buildTarget, version);
        }
    }
    
    
     public static void BuildAB(bool isHotUpdate=false) {
            string outputPath = "";
            if (!isHotUpdate) {
                 outputPath= HotUpdateConfig.LocalAssetBundleFolder;
            }
            else {
                outputPath = HotUpdateConfig.AssetBundleAssetDataBuildPath;
            }
            

            if (!Directory.Exists(outputPath)) {
                Directory.CreateDirectory(outputPath);
            }

            BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression,
                EditorUserBuildSettings.activeBuildTarget);

            AssetDatabase.Refresh();

            MikroFramework.Utilities.EditorUtility.OpenInFolder(outputPath);

            
            WriteVersionConfig(outputPath);
            
        }

        private static void WriteVersionConfig(string outPutPath) {
            UnityEditor.EditorApplication.isPlaying = true;
            bool latestSimulationMode = ResManager.SimulationMode;
            ResManager.SimulationMode = true;
            ResData.Singleton.Init(null,(e)=>{});
            
            string versionConfigFilePath = outPutPath + "ResVersion.json";

            if (File.Exists(versionConfigFilePath))
            {
                File.Delete(versionConfigFilePath);
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(outPutPath);

            FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            
            ResVersion resVersion = new ResVersion();
            resVersion.ABMD5List = new List<ABMD5Base>();

            List<AssetBundleData> assetBundleDatas = ResData.Singleton.AssetBundleDatas;
            assetBundleDatas.Add(new AssetBundleData() {
                Name = ResKitUtility.CurrentPlatformName
            });
            

            for (int i = 0; i < files.Length; i++) {
                if (!files[i].Name.EndsWith(".meta") && !files[i].Name.EndsWith(".manifest")) {
                    string fileCompletePath = files[i].FullName;
                    fileCompletePath = fileCompletePath.Replace('\\', '/');
                    string fileAssetPath= fileCompletePath.Substring(outPutPath.Length);

                    foreach (AssetBundleData assetBundleData in assetBundleDatas) {
                        if (assetBundleData.Name == fileAssetPath) {

                            ABMD5Base abmd5Base = new ABMD5Base()
                            {
                                AssetName = fileAssetPath,
                                MD5 = ResKitUtility.BuildFileMd5(files[i].FullName),
                                FileSize = files[i].Length / 1024.0f,
                                assetDatas = assetBundleData.AssetDataList
                            };

                            resVersion.ABMD5List.Add(abmd5Base);
                            break;
                        }
                    }
                    
                    
                }
            }

            resVersion.Version = Application.version;
            string resVersionJson = AdvancedJsonSerializer.Singleton.Serialize(resVersion);

            
            File.WriteAllText(versionConfigFilePath, resVersionJson);
            UnityEditor.EditorApplication.isPlaying = false;
            ResManager.SimulationMode = latestSimulationMode;
            
        }

        /*
        public int callbackOrder { get; } = 0;
        public void OnPreprocessBuild(BuildReport report) {
            BuildABMenuItem();
            Debug.Log("Build AB Finished!");
        }*/
}



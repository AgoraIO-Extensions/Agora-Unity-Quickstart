using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
#if UNITY_2018_4_OR_NEWER
using UnityEditor.Build.Reporting;
#endif 


#if UNITY_IOS
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
#endif

using UnityEditor.Callbacks;
using System.IO;

namespace Agora_RTC_Plugin.API_Example
{
    public class CommandBuild : MonoBehaviour
    {
        private static string[] GetAllScenes()
        {
            List<string> scenesList = new List<string>();
            string[] allScenes = AssetDatabase.FindAssets("t:Scene", new string[] { "Assets" });
            string folder = "";
            for (int i = 0; i < allScenes.Length; i++)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(allScenes[i]);
                if (scenePath.EndsWith("/HomeScene.unity")) {
                    folder = scenePath.Substring(0, scenePath.Length - 16);
                    scenesList.Add(scenePath);
                    break;
                }
            }

            if (scenesList.Count == 0) {
                throw new System.Exception("Can not find demo HomeScene.unity.");
            }

            allScenes = AssetDatabase.FindAssets("t:Scene", new string[] { folder });
            for (int i = 0; i < allScenes.Length; i++)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(allScenes[i]);
              
                if (!scenePath.EndsWith("/HomeScene.unity"))
                {
                    scenesList.Add(scenePath);
                }
            }


            foreach (var scene in scenesList) {
                Debug.Log(scene);
            }

            return scenesList.ToArray();
        }

        [MenuItem("Build Agora Demo/Android")]
        public static void BuildAndroid()
        {
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetAllScenes();
            buildPlayerOptions.locationPathName = "../Build/android_studio";
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;

#if UNITY_2018_4_OR_NEWER
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build Android succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build Android failed");
        }
#else
            string message = BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Build Android: " + message);
#endif
        }


        [MenuItem("Build Agora Demo/IPhone")]
        public static void BuildIPhone()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetAllScenes();
            buildPlayerOptions.locationPathName = "../Build/IPhone";
            buildPlayerOptions.target = BuildTarget.iOS;
            buildPlayerOptions.options = BuildOptions.None;

#if UNITY_2018_4_OR_NEWER
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build IPhone succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build IPhone failed");
        }
#else
            string message = BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Build IPhone: " + message);
#endif
        }


        [PostProcessBuild(2)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget == BuildTarget.iOS)
            {
#if UNITY_IOS
                // linked library
                string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
                PBXProject proj = new PBXProject();
                proj.ReadFromFile(projPath);

                string target = GetTargetGuid(proj);
                proj.SetBuildProperty(target, "VALIDATE_WORKSPACE", "YES");
                proj.SetBuildProperty(target, "PRODUCT_BUNDLE_IDENTIFIER", "io.agora.Unitydemo");

#if UNITY_2019_3_OR_NEWER
                string unityFrameWorkTarget = proj.GetUnityFrameworkTargetGuid();
#else
                string unityFrameWorkTarget = proj.TargetGuidByName("Unity-Framwork");
#endif
                proj.SetBuildProperty(unityFrameWorkTarget, "ENABLE_BITCODE", "NO");
                proj.SetBuildProperty(unityFrameWorkTarget, "CODE_SIGN_STYLE", "Manual");
                proj.SetBuildProperty(unityFrameWorkTarget, "SUPPORTS_MACCATALYST", "NO");

                // done, write to the project file
                File.WriteAllText(projPath, proj.WriteToString());


                //Set Application supports iTunes file sharing to true
                string pListPath = path + "/Info.plist";
                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(pListPath));
                PlistElementDict rootDic = plist.root;
                //Set Application supports iTunes file sharing to true
                rootDic.SetBoolean("UIFileSharingEnabled", true);
                rootDic.SetString("NSLocalNetworkUsageDescription", "for wayang");
                PlistElementArray plistElementArray = rootDic.CreateArray("NSBonjourServices");
                plistElementArray.AddString("_tictactoe._tcp");


                File.WriteAllText(pListPath, plist.WriteToString());
#endif
            }
        }


        [MenuItem("Build Agora Demo/Mac")]
        public static void BuildMac()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetAllScenes();
            buildPlayerOptions.locationPathName = "../Build/Mac.app";
            buildPlayerOptions.target = BuildTarget.StandaloneOSX;
            buildPlayerOptions.options = BuildOptions.None;

#if UNITY_2018_4_OR_NEWER
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build Mac succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build Mac failed");
        }
#else
            string message = BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Build Mac: " + message);
#endif
        }


        [MenuItem("Build Agora Demo/x86")]
        public static void BuildWin32()
        {

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetAllScenes();
            buildPlayerOptions.locationPathName = "../Build/x86/x86.exe";
            buildPlayerOptions.target = BuildTarget.StandaloneWindows;
            buildPlayerOptions.options = BuildOptions.None;

#if UNITY_2018_4_OR_NEWER
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build x86 succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build x86 failed");
        }
#else
            string message = BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Build Win32: " + message);
#endif

        }

        [MenuItem("Build Agora Demo/x86_64")]
        public static void BuildWin64()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetAllScenes();
            buildPlayerOptions.locationPathName = "../Build/x86_64/x86_64.exe";
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.None;

#if UNITY_2018_4_OR_NEWER
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build x86_64 succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build x86_64 failed");
        }
#else
            string message = BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Build x86_64: " + message);
#endif

        }

        [MenuItem("Build Agora Demo/All")]
        public static void BuildAll()
        {
            BuildAndroid();
            BuildIPhone();
            BuildMac();
            BuildWin32();
            BuildWin64();
        }

#if UNITY_IOS
        static string GetTargetGuid(PBXProject proj)
        {
#if UNITY_2019_3_OR_NEWER
            return proj.GetUnityMainTargetGuid();
#else
            return proj.TargetGuidByName("Unity-iPhone");
#endif
        }
#endif

    }
}

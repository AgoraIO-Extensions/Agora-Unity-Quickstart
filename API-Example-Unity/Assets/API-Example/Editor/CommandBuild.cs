using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
#if UNITY_2018_4_OR_NEWER
using UnityEditor.Build.Reporting;
#endif 
using UnityEngine;

public class CommandBuild : MonoBehaviour
{


    private static string[] GetAllScenes()
    {
        string[] scenes = new string[] {
            "Assets/API-Example/HomeScene.unity",
            "Assets/API-Example/Examples/Basic/JoinChannelVideo/BasicVideoCallScene.unity",
            "Assets/API-Example/Examples/Basic/JoinChannelAudio/BasicAudioCallScene.unity",

            "Assets/API-Example/Examples/Advanced/AudioMixing/AudioMixingScene.unity",
            "Assets/API-Example/Examples/Advanced/AudioSpectrum/AudioSpectrumScene.unity",
            "Assets/API-Example/Examples/Advanced/ChannelMediaRelay/ChannelMediaRelayScene.unity",
            "Assets/API-Example/Examples/Advanced/ContentInspect/ContentInspectScene.unity",
            "Assets/API-Example/Examples/Advanced/CustomCaptureAudio/CustomCaptureAudioScene.unity",
            "Assets/API-Example/Examples/Advanced/CustomCaptureVideo/CustomCaptureVideoScene.unity",
            "Assets/API-Example/Examples/Advanced/CustomRenderAudio/CustomRenderAudioScene.unity",
            "Assets/API-Example/Examples/Advanced/DeviceManager/DeviceManagerScene.unity",
            "Assets/API-Example/Examples/Advanced/DualCamera/DualCameraScene.unity",
            "Assets/API-Example/Examples/Advanced/JoinChannelVideoToken/JoinChannelVideoTokenScene.unity",
            "Assets/API-Example/Examples/Advanced/JoinChannelWithUserAccount/JoinChannelWithUserAccountScene.unity",
            "Assets/API-Example/Examples/Advanced/MediaPlayer/MediaPlayerScene.unity",
            "Assets/API-Example/Examples/Advanced/MediaRecorder/MediaRecorderScene.unity",
            "Assets/API-Example/Examples/Advanced/Metadata/MetadataScene.unity",
            "Assets/API-Example/Examples/Advanced/ProcessAudioRawData/ProcessAudioRawDataScene.unity",
            "Assets/API-Example/Examples/Advanced/ProcessVideoRawData/ProcessVideoRawDataScene.unity",
            "Assets/API-Example/Examples/Advanced/PushEncodedVideoImage/PushEncodedVideoImageScene.unity",
            "Assets/API-Example/Examples/Advanced/RtmpStreaming/RtmpStreamingScene.unity",
            "Assets/API-Example/Examples/Advanced/ScreenShare/ScreenShareScene.unity",
            "Assets/API-Example/Examples/Advanced/ScreenShareWhileVideoCall/ScreenShareWhileVideoCallScene.unity",
            "Assets/API-Example/Examples/Advanced/SetBeautyEffectOptions/SetBeautyEffectOptionsScene.unity",
            "Assets/API-Example/Examples/Advanced/SetEncryption/SetEncryptionScene.unity",
            "Assets/API-Example/Examples/Advanced/SetVideoEncodeConfiguration/SetVideoEncodeConfigurationScene.unity",
            "Assets/API-Example/Examples/Advanced/SpatialAudioWithMediaPlayer/SpatialAudioWithMediaPlayerScene.unity",
            "Assets/API-Example/Examples/Advanced/StartDirectCdnStreaming/StartDirectCdnStreamingScene.unity",
            "Assets/API-Example/Examples/Advanced/StartLocalVideoTranscoder/StartLocalVideoTranscoderScene.unity",
            "Assets/API-Example/Examples/Advanced/StartRhythmPlayer/StartRhythmPlayerScene.unity",
            "Assets/API-Example/Examples/Advanced/StartRtmpStreamWithTranscoding/StartRtmpStreamWithTranscodingScene.unity",
            "Assets/API-Example/Examples/Advanced/StreamMessage/StreamMessageScene.unity",
            "Assets/API-Example/Examples/Advanced/TakeSnapshot/TakeSnapshotScene.unity",
            "Assets/API-Example/Examples/Advanced/VirtualBackground/VirtualBackgroundScene.unity",
            "Assets/API-Example/Examples/Advanced/VoiceChanger/VoiceChangerScene.unity"
        };
        return scenes;
    }

    [MenuItem("Build/Android")]
    public static void BuildAndrod()
    {

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetAllScenes();
        buildPlayerOptions.locationPathName = "../Build/Android.apk";
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


    [MenuItem("Build/IPhone")]
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

    [MenuItem("Build/Mac")]
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


    [MenuItem("Build/x86")]
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

    [MenuItem("Build/x86_64")]
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

    [MenuItem("Build/All")]
    public static void BuildAll()
    {
        BuildAndrod();
        BuildIPhone();
        BuildMac();
        BuildWin32();
        BuildWin64();
    }


}

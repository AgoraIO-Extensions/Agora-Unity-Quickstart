﻿//#define USE_PLUGIN

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
 
 
using System.Runtime.InteropServices;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.PluginSceneSample
{
    using PluginSamplePtr = IntPtr;
    using RtcEnginePtr = IntPtr;
    public class PluginSceneSample : MonoBehaviour
    {

        #region DllImport

#if USE_PLUGIN

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private const string PluginLibName = "AgoraRawDataPlugin";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        private const string PluginLibName = "AgoraRawDataPluginUnity";
#elif UNITY_IPHONE
		private const string PluginLibName = "__Internal";
#else
        private const string PluginLibName = "AgoraRawDataPlugin";
#endif

        [System.Runtime.InteropServices.DllImport(PluginLibName, CharSet = System.Runtime.InteropServices.CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern PluginSamplePtr CreateSamplePlugin(RtcEnginePtr engine);

        [System.Runtime.InteropServices.DllImport(PluginLibName, CharSet = System.Runtime.InteropServices.CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void DestroySamplePlugin(PluginSamplePtr engine);

        [System.Runtime.InteropServices.DllImport(PluginLibName, CharSet = System.Runtime.InteropServices.CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool EnablePlugin(PluginSamplePtr engine);

        [System.Runtime.InteropServices.DllImport(PluginLibName, CharSet = System.Runtime.InteropServices.CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool DisablePlugin(PluginSamplePtr engine);
#endif
        #endregion


        [FormerlySerializedAs("appIdInput")]
        [SerializeField]
        private AppIdInput _appIdInput;

        [Header("_____________Basic Configuration_____________")]
        [FormerlySerializedAs("APP_ID")]
        [SerializeField]
        private string _appID = "";

        [FormerlySerializedAs("TOKEN")]
        [SerializeField]
        private string _token = "";

        [FormerlySerializedAs("CHANNEL_NAME")]
        [SerializeField]
        private string _channelName = "";

        public Text LogText;
        internal Logger Log;
        internal IRtcEngine RtcEngine = null;

#if USE_PLUGIN
        PluginSamplePtr pluginSamplePtr = IntPtr.Zero;
#endif

        // Use this for initialization
        void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
                JoinChannel();
            }
        }

        // Update is called once per frame
        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        //Show data in AgoraBasicProfile
        [ContextMenu("ShowAgoraBasicProfileData")]
        private void LoadAssetData()
        {
            if (_appIdInput == null) return;
            _appID = _appIdInput.appID;
            _token = _appIdInput.token;
            _channelName = _appIdInput.channelName;
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        private void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT, AREA_CODE.AREA_CODE_GLOB, new LogConfig("./log.txt"));
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
        }


        public void JoinChannel()
        {
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.JoinChannel(_token, _channelName, "", 0);
        }


        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }

        internal string GetChannelName()
        {
            return _channelName;
        }



        internal static void MakeVideoView(uint uid, string channelId = "")
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            // create a GameObject and assign to this new user
            var videoSurface = MakeImageSurface(uid.ToString());
            if (ReferenceEquals(videoSurface, null)) return;
            // configure videoSurface
            if (uid == 0)
            {
                videoSurface.SetForUser(uid, channelId);
            }
            else
            {
                videoSurface.SetForUser(uid, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            }

            videoSurface.OnTextureSizeModify += (int width, int height) =>
            {
                float scale = (float)height / (float)width;
                videoSurface.transform.localScale = new Vector3(-5, 5 * scale, 1);
                Debug.Log("OnTextureSizeModify: " + width + "  " + height);
            };

            videoSurface.SetEnable(true);
        }

        // VIDEO TYPE 1: 3D Object
        private static VideoSurface MakePlaneSurface(string goName)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            var mesh = go.GetComponent<MeshRenderer>();
            if (mesh != null)
            {
                Debug.LogWarning("VideoSureface update shader");
                mesh.material = new Material(Shader.Find("Unlit/Texture"));
            }
            // set up transform
            go.transform.Rotate(-90.0f, 0.0f, 0.0f);
            go.transform.position = Vector3.zero;
            go.transform.localScale = new Vector3(0.25f, 0.5f, 0.5f);

            // configure videoSurface
            var videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

        // Video TYPE 2: RawImage
        private static VideoSurface MakeImageSurface(string goName)
        {
            GameObject go = new GameObject();

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // to be renderered onto
            go.AddComponent<RawImage>();
            // make the object draggable
            go.AddComponent<UIElementDrag>();
            var canvas = GameObject.Find("VideoCanvas");
            if (canvas != null)
            {
                go.transform.parent = canvas.transform;
                Debug.Log("add video view");
            }
            else
            {
                Debug.Log("Canvas is null video view");
            }

            // set up transform
            go.transform.Rotate(0f, 0.0f, 180.0f);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = new Vector3(2f, 3f, 1f);

            // configure videoSurface
            var videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

        internal static void DestroyVideoView(uint uid)
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                Destroy(go);
            }
        }

        public void OnEnablePluginButtonClick()
        {
#if USE_PLUGIN
            if (this.pluginSamplePtr != IntPtr.Zero)
            {
                this.Log.UpdateLog("plugine already enabled");
                return;
            }

            RtcEnginePtr rtcEnginePtr = IntPtr.Zero;
            this.RtcEngine.GetNativeHandler(ref rtcEnginePtr);
            this.pluginSamplePtr = CreateSamplePlugin(rtcEnginePtr);

            bool result = EnablePlugin(this.pluginSamplePtr);
            this.Log.UpdateLog("Enable Plugin :" + result);

            int nRet = RtcEngine.SetRecordingAudioFrameParameters(48000, 2, RAW_AUDIO_FRAME_OP_MODE_TYPE.RAW_AUDIO_FRAME_OP_MODE_READ_WRITE, 960);
            this.Log.UpdateLog("SetRecordingAudioFrameParameters: " + nRet);
#else
            this.Log.UpdateLog("if you want use plugin, you need uncomment the first line in PluginSceneSample.cs and import VideoObserverPlugin into this project");
#endif
        }

        public void onDisablePluginButtonClick()
        {
#if USE_PLUGIN
            if (this.pluginSamplePtr == IntPtr.Zero)
            {
                this.Log.UpdateLog("plugin not actived");
                return;
            }

            bool result = DisablePlugin(this.pluginSamplePtr);
            this.Log.UpdateLog("Disable Plugin :" + result);
            DestroySamplePlugin(this.pluginSamplePtr);
            this.pluginSamplePtr = IntPtr.Zero;

            int nRet = RtcEngine.SetRecordingAudioFrameParameters(48000, 2, RAW_AUDIO_FRAME_OP_MODE_TYPE.RAW_AUDIO_FRAME_OP_MODE_READ_ONLY, 960);
            this.Log.UpdateLog("SetRecordingAudioFrameParameters: " + nRet);
#else
            this.Log.UpdateLog("if you want use plugin, you need uncomment the first line in PluginSceneSample.cs and import VideoObserverPlugin into this project");
#endif
        }
    }


    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly PluginSceneSample _sample;

        internal UserEventHandler(PluginSceneSample videoSample)
        {
            _sample = videoSample;
        }

        public override void OnError(int err, string msg)
        {
            _sample.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _sample.Log.UpdateLog(string.Format("sdk version: ${0}",
                _sample.RtcEngine.GetVersion(ref build)));
            _sample.Log.UpdateLog(string.Format("sdk build: ${0}",
              build));
            _sample.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));

            PluginSceneSample.MakeVideoView(0);
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _sample.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _sample.Log.UpdateLog("OnLeaveChannel");
            PluginSceneSample.DestroyVideoView(0);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _sample.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _sample.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            PluginSceneSample.MakeVideoView(uid, _sample.GetChannelName());
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _sample.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            PluginSceneSample.DestroyVideoView(uid);
        }

        public override void OnUplinkNetworkInfoUpdated(UplinkNetworkInfo info)
        {
            _sample.Log.UpdateLog("OnUplinkNetworkInfoUpdated");
        }

        public override void OnDownlinkNetworkInfoUpdated(DownlinkNetworkInfo info)
        {
            _sample.Log.UpdateLog("OnDownlinkNetworkInfoUpdated");
        }
    }

}

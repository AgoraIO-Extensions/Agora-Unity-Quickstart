﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using Agora.Util;
using Logger = Agora.Util.Logger;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.DualCamera
{
    public class DualCamera : MonoBehaviour
    {
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
        internal IRtcEngineEx RtcEngine = null;

        internal bool IsChannelJoined = false;

        private IVideoDeviceManager _videoDeviceManager;
        private DeviceInfo[] _videoDeviceInfos;
        private CameraCapturerConfiguration _config1;
        private CameraCapturerConfiguration _config2;

        public uint UID1 = 123;
        public uint UID2 = 456;

        // Use this for initialization
        private void Start()
        {
#if UNITY_IPHONE || UNITY_ANDROID
            this.LogText.text = "iOS/Android is not supported, but you could see how it works on the Editor for Windows/MacOS";

#else
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
                GetVideoDeviceManager();
            }
#endif
        }

        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
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

        private void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngineEx();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
        }

        public void MainCameraJoinChannel()
        {
            RtcEngine.StartPreview();
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            
            var ret = RtcEngine.StartPrimaryCameraCapture(_config1);
            Log.UpdateLog(
                string.Format("StartPrimaryCameraCapture returns: {0}", ret));
            ChannelMediaOptions options1 = new ChannelMediaOptions();
            options1.publishCameraTrack.SetValue(true);
            options1.autoSubscribeAudio.SetValue(true);
            options1.autoSubscribeVideo.SetValue(true);
            options1.publishScreenTrack.SetValue(false);
            options1.enableAudioRecordingOrPlayout.SetValue(true);
            options1.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            ret = RtcEngine.JoinChannel(_token, _channelName, UID1, options1);
            Debug.Log("MainCameraJoinChannel returns: " + ret);
        }

        public void MainCameraLeaveChannel()
        {
            RtcEngine.StopPrimaryCameraCapture();
            var ret = RtcEngine.LeaveChannel();
            Debug.Log("MainCameraLeaveChannel returns: " + ret);
        }

        public void SecondCameraJoinChannel()
        {
            var ret = RtcEngine.StartSecondaryCameraCapture(_config2);
            Log.UpdateLog(
                string.Format("StartSecondaryCameraCapture returns: {0}", ret));
            ChannelMediaOptions options2 = new ChannelMediaOptions();
            options2.autoSubscribeAudio.SetValue(false);
            options2.autoSubscribeVideo.SetValue(false);
            options2.publishCustomAudioTrack.SetValue(false);
            options2.publishCameraTrack.SetValue(false);
            options2.publishSecondaryCameraTrack.SetValue(true);
            options2.enableAudioRecordingOrPlayout.SetValue(false);
            options2.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            ret = RtcEngine.JoinChannelEx(_token, new RtcConnection(_channelName, UID2), options2);
            Debug.Log("JoinChannelEx returns: " + ret);
        }

        public void SecondCameraLeaveChannel()
        {
            RtcEngine.StopSecondaryCameraCapture();
            var ret = RtcEngine.LeaveChannelEx(new RtcConnection(_channelName, 456));
            Debug.Log("SecondCameraLeaveChannel returns: " + ret);
        }

        private void GetVideoDeviceManager()
        {
            _videoDeviceManager = RtcEngine.GetVideoDeviceManager();
            _videoDeviceInfos = _videoDeviceManager.EnumerateVideoDevices();
            Log.UpdateLog(string.Format("VideoDeviceManager count: {0}", _videoDeviceInfos.Length));
            for (var i = 0; i < _videoDeviceInfos.Length; i++)
            {
                Log.UpdateLog(string.Format("VideoDeviceManager device index: {0}, name: {1}, id: {2}", i,
                    _videoDeviceInfos[i].deviceName, _videoDeviceInfos[i].deviceId));
            }

            _config1 = new CameraCapturerConfiguration();
            _config1.deviceId = _videoDeviceInfos[0].deviceId;
            Debug.Log("PrimaryCamera: " + _config1.deviceId);
            _config1.format = new VideoFormat();

            if (_videoDeviceInfos.Length > 1)
            {
                _config2 = new CameraCapturerConfiguration();
                _config2.deviceId = _videoDeviceInfos[1].deviceId;
                Debug.Log("SecondaryCamera: " + _config2.deviceId);
                _config2.format = new VideoFormat();
            }
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.StopSecondaryCameraCapture();
            RtcEngine.StopPrimaryCameraCapture();
            RtcEngine.LeaveChannelEx(new RtcConnection(_channelName, 456));
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }

        internal string GetChannelName()
        {
            return _channelName;
        }

        #region -- Video Render UI Logic ---

        internal static void MakeVideoView(uint uid, string channelId = "", VIDEO_SOURCE_TYPE videoSourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA)
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            // create a GameObject and assign to this new user
            VideoSurface videoSurface = null;

            if (videoSourceType == VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA)
            {
                videoSurface = MakeImageSurface("MainCameraView");
            }
            else if (videoSourceType == VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA_SECONDARY)
            {
                videoSurface = MakeImageSurface("SecondCameraView");
            }
            else
            {
                videoSurface = MakeImageSurface(uid.ToString());
            }

            if (ReferenceEquals(videoSurface, null)) return;
            // configure videoSurface
            videoSurface.SetForUser(uid, channelId, videoSourceType);

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
            var go = GameObject.Find(goName);
            if (!ReferenceEquals(go, null))
            {
                return null; // reuse
            }

            go = GameObject.CreatePrimitive(PrimitiveType.Plane);

            if (go == null)
            {
                return null;
            }

            go.name = goName;
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

        internal static void DestroyVideoView(string name)
        {
            var go = GameObject.Find(name);
            if (!ReferenceEquals(go, null))
            {
                Destroy(go);
            }
        }

        #endregion
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly DualCamera _videoSample;

        internal UserEventHandler(DualCamera videoSample)
        {
            _videoSample = videoSample;
        }

        public override void OnError(int err, string msg)
        {
            _videoSample.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _videoSample.IsChannelJoined = true;
            _videoSample.Log.UpdateLog(string.Format("sdk version: ${0}",
                _videoSample.RtcEngine.GetVersion(ref build)));
            _videoSample.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                    connection.channelId, connection.localUid, elapsed));

            if (connection.localUid == _videoSample.UID1)
            {
                DualCamera.MakeVideoView(0);
            }

            if (connection.localUid == _videoSample.UID2)
            {
                DualCamera.MakeVideoView(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA_SECONDARY);
            }
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _videoSample.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _videoSample.IsChannelJoined = false;
            _videoSample.Log.UpdateLog("OnLeaveChannel");
            if (connection.localUid == _videoSample.UID1)
            {
                DualCamera.DestroyVideoView("MainCameraView");
            }

            if (connection.localUid == _videoSample.UID2)
            {
                DualCamera.DestroyVideoView("SecondCameraView");
            }
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
            CLIENT_ROLE_TYPE newRole)
        {
            _videoSample.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _videoSample.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            if (uid != _videoSample.UID1 && uid != _videoSample.UID2)
            {
                DualCamera.MakeVideoView(uid, _videoSample.GetChannelName());
            }
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _videoSample.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            if (uid != _videoSample.UID1 && uid != _videoSample.UID2)
            {
                DualCamera.DestroyVideoView(uid.ToString());
            }
        }
    }

    #endregion
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using agora.rtc;
using agora.util;
using Logger = agora.util.Logger;

namespace Agora_Plugin.API_Example.examples.advanced.DualCamera
{
    public class DualCamera : MonoBehaviour
    {
        [FormerlySerializedAs("appIdInput")]
        [SerializeField]
        private AppIdInput appIdInput;

        [Header("_____________Basic Configuration_____________")]
        [FormerlySerializedAs("APP_ID")]
        [SerializeField]
        private string appID = "";

        [FormerlySerializedAs("TOKEN")]
        [SerializeField]
        private string token = "";

        [FormerlySerializedAs("CHANNEL_NAME")]
        [SerializeField]
        private string channelName = "";

        public Text logText;
        internal Logger Logger;
        internal IRtcEngine mRtcEngine = null;
        private const float Offset = 100;

        private Button _JoinChannelbutton;
        private Button _LeaveChannelbutton;
        private Button _StartButton;

        internal bool isJoinChannel = false;

        private IVideoDeviceManager _videoDeviceManager;
        internal DeviceInfo[] _videoDeviceInfos;
        internal CameraCapturerConfiguration config1;
        internal CameraCapturerConfiguration config2;

        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            CheckAppId();
            InitEngine();
            GetVideoDeviceManager();
        }

        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        private void CheckAppId()
        {
            Logger = new Logger(logText);
            Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        //Show data in AgoraBasicProfile
        [ContextMenu("ShowAgoraBasicProfileData")]
        public void LoadAssetData()
        {
            if (appIdInput == null) return;
            appID = appIdInput.appID;
            token = appIdInput.token;
            channelName = appIdInput.channelName;
        }

        private void InitEngine()
        {
            mRtcEngine = RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(appID, 0, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            mRtcEngine.Initialize(context);
            mRtcEngine.InitEventHandler(handler);
            mRtcEngine.SetLogFile("./log.txt");
            mRtcEngine.StartPreview();
            mRtcEngine.EnableAudio();
            mRtcEngine.EnableVideo();
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        }

        public void MainCameraJoinChannel()
        {
            var ret = mRtcEngine.StartPrimaryCameraCapture(config1);
            Logger.UpdateLog(
                string.Format("StartPrimaryCameraCapture returns: {0}", ret));
            ChannelMediaOptions options1 = new ChannelMediaOptions();
            options1.publishCameraTrack.SetValue(true);
            options1.publishAudioTrack.SetValue(true);
            options1.autoSubscribeAudio.SetValue(true);
            options1.autoSubscribeVideo.SetValue(true);
            options1.publishScreenTrack.SetValue(false);
            options1.enableAudioRecordingOrPlayout.SetValue(true);
            options1.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            ret = mRtcEngine.JoinChannel(token, channelName, 123, options1);
            Debug.Log("MainCameraJoinChannel returns: " + ret);
        }

        public void SecondCameraJoinChannel()
        {
            var ret = mRtcEngine.StartSecondaryCameraCapture(config2);
            Logger.UpdateLog(
                string.Format("StartSecondaryCameraCapture returns: {0}", ret));
            ChannelMediaOptions options2 = new ChannelMediaOptions();
            options2.autoSubscribeAudio.SetValue(false);
            options2.autoSubscribeVideo.SetValue(false);
            options2.publishAudioTrack.SetValue(false);
            options2.publishCameraTrack.SetValue(false);
            options2.publishSecondaryCameraTrack.SetValue(true);
            options2.enableAudioRecordingOrPlayout.SetValue(false);
            options2.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            ret = mRtcEngine.JoinChannelEx(token, new RtcConnection(channelName, 456), options2);
            Debug.Log("JoinChannelEx returns: " + ret);
        }

        private void GetVideoDeviceManager()
        {
            _videoDeviceManager = mRtcEngine.GetVideoDeviceManager();
            _videoDeviceInfos = _videoDeviceManager.EnumerateVideoDevices();
            Logger.UpdateLog(string.Format("VideoDeviceManager count: {0}", _videoDeviceInfos.Length));
            for (var i = 0; i < _videoDeviceInfos.Length; i++)
            {
                Logger.UpdateLog(string.Format("VideoDeviceManager device index: {0}, name: {1}, id: {2}", i,
                    _videoDeviceInfos[i].deviceName, _videoDeviceInfos[i].deviceId));
            }

            config1 = new CameraCapturerConfiguration();
            config1.deviceId = _videoDeviceInfos[0].deviceId;
            Debug.Log("PrimaryCamera: " + config1.deviceId);
            config1.format = new VideoFormat();

            if (_videoDeviceInfos.Length > 1)
            {
                config2 = new CameraCapturerConfiguration();
                config2.deviceId = _videoDeviceInfos[1].deviceId;
                Debug.Log("SecondaryCamera: " + config2.deviceId);
                config2.format = new VideoFormat();
            }
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (mRtcEngine == null) return;
            mRtcEngine.InitEventHandler(null);
            mRtcEngine.LeaveChannel();
            mRtcEngine.StopSecondaryCameraCapture();
            mRtcEngine.StopPrimaryCameraCapture();
            mRtcEngine.LeaveChannelEx(new RtcConnection(channelName, 456));
            mRtcEngine.Dispose();
        }

        //private void OnApplicationQuit()
        //{
        //    Debug.Log("OnApplicationQuit");
        //    if (mRtcEngine == null) return;
        //    mRtcEngine.StopSecondaryCameraCapture();
        //    mRtcEngine.StopPrimaryCameraCapture();
        //    mRtcEngine.LeaveChannelEx(new RtcConnection(channelName, 456));
        //    mRtcEngine.InitEventHandler(null);
        //    mRtcEngine.LeaveChannel();
        //    mRtcEngine.Dispose();
        //}

        internal string GetChannelName()
        {
            return channelName;
        }

        internal static void MakeVideoView(uint uid, string channelId = "", VIDEO_SOURCE_TYPE videoSourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA)
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            // create a GameObject and assign to this new user
            VideoSurface videoSurface = new VideoSurface();

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
            videoSurface.SetEnable(true);
        }

        // VIDEO TYPE 1: 3D Object
        private VideoSurface MakePlaneSurface(string goName)
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
    }

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly DualCamera _videoSample;

        internal UserEventHandler(DualCamera videoSample)
        {
            _videoSample = videoSample;
        }

        public override void OnWarning(int warn, string msg)
        {
            _videoSample.Logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _videoSample.Logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _videoSample.isJoinChannel = true;
            _videoSample.Logger.UpdateLog(string.Format("sdk version: ${0}",
                _videoSample.mRtcEngine.GetVersion()));
            _videoSample.Logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                    connection.channelId, connection.localUid, elapsed));

            if (connection.localUid == 123)
            {
                DualCamera.MakeVideoView(0);
            }

            if (connection.localUid == 456)
            {
                DualCamera.MakeVideoView(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA_SECONDARY);
            }
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _videoSample.Logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _videoSample.isJoinChannel = false;
            _videoSample.Logger.UpdateLog("OnLeaveChannel");
            if (connection.localUid == 123)
            {
                DualCamera.DestroyVideoView("MainCameraView");
            }

            if (connection.localUid == 456)
            {
                DualCamera.DestroyVideoView("SecondCameraView");
            }
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
            CLIENT_ROLE_TYPE newRole)
        {
            _videoSample.Logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _videoSample.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            if (uid != 123 && uid != 456)
            {
                DualCamera.MakeVideoView(uid, _videoSample.GetChannelName());
            }
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _videoSample.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            if (uid != 123 && uid != 456)
            {
                DualCamera.DestroyVideoView(uid.ToString());
            }
        }
    }
}
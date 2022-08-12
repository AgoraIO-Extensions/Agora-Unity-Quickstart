using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using Agora.Util;
using Logger = Agora.Util.Logger;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.ProcessVideoRawData
{
    public class ProcessVideoRawData : MonoBehaviour
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
        private Logger Log;
        private IRtcEngine RtcEngine;

        internal int Count;

        internal int WriteCount;
        internal int ReadCount;

        internal byte[] VideoBuffer = new byte[0];

        private int _videoFrameWidth = 1080;
        public int VideoFrameWidth
        {
            set
            {
                if (value != _videoFrameWidth)
                {
                    _videoFrameWidth = value;
                    _needResize = true;
                }

            }
        
            get
            {
                return _videoFrameWidth;
            }
        }

        private int _videoFrameHeight = 720;
        public int VideoFrameHeight
        {
            set
            {
                if (value != _videoFrameHeight)
                {
                    _videoFrameHeight = value;
                    _needResize = true;
                }

            }

            get
            {
                return _videoFrameHeight;

            }
        }

        private bool _needResize = false;
        public GameObject VideoView;
        private Texture2D _texture;
        private bool _isTextureAttach = false;


        void Start()
        {
#if UNITY_WEBGL
            this.LogText.text = "Not Support in this platform!";
#else
            LoadAssetData();
            if (CheckAppId())
            {
                SetUpUI();
                InitEngine();
                JoinChannel();
            }
#endif
        }

        // Update is called once per frame
        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();

            if (!_isTextureAttach)
            {
                var rd = VideoView.GetComponent<RawImage>();
                rd.texture = _texture;
                _isTextureAttach = true;
            }
            else if (VideoBuffer != null && VideoBuffer.Length != 0 && !_needResize)
            {
                lock (VideoBuffer)
                {
                    _texture.LoadRawTextureData(VideoBuffer);
                    _texture.Apply();
                }
            }
            else if(_needResize)
            {
                _texture.Resize(_videoFrameWidth, _videoFrameHeight);
                _texture.Apply();
                _needResize = false;
            }
        }

        bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        //Show data in AgoraBasicProfile
        [ContextMenu("ShowAgoraBasicProfileData")]
        public void LoadAssetData()
        {
            if (_appIdInput == null) return;
            _appID = _appIdInput.appID;
            _token = _appIdInput.token;
            _channelName = _appIdInput.channelName;
        }

        void SetUpUI()
        {
            var bufferLength = _videoFrameHeight * _videoFrameWidth * 4;
            _texture = new Texture2D(_videoFrameWidth, _videoFrameHeight, TextureFormat.RGBA32, false);
            _texture.Apply();
        }

        void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
            RtcEngine.RegisterVideoFrameObserver(new VideoFrameObserver(this), OBSERVER_MODE.RAW_DATA);
        }

        void JoinChannel()
        {
            VideoEncoderConfiguration config = new VideoEncoderConfiguration();
            config.dimensions = new VideoDimensions(_videoFrameWidth, _videoFrameHeight);
            RtcEngine.SetVideoEncoderConfiguration(config);

            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.JoinChannel(_token, _channelName);
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine != null)
            {
                if (_texture != null)
                {
                    GameObject.Destroy(_texture);
                    _texture = null;
                }

                RtcEngine.UnRegisterVideoFrameObserver();
                RtcEngine.InitEventHandler(null);
                RtcEngine.LeaveChannel();
                RtcEngine.Dispose();
            }
        }

        internal class UserEventHandler : IRtcEngineEventHandler
        {
            private readonly ProcessVideoRawData _agoraVideoRawData;

            internal UserEventHandler(ProcessVideoRawData agoraVideoRawData)
            {
                _agoraVideoRawData = agoraVideoRawData;
            }

            public override void OnWarning(int warn, string msg)
            {
                _agoraVideoRawData.Log.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
            }

            public override void OnError(int err, string msg)
            {
                _agoraVideoRawData.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
            }

            public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _agoraVideoRawData.Log.UpdateLog(string.Format("sdk version: ${0}",
                    _agoraVideoRawData.RtcEngine.GetVersion()));
                _agoraVideoRawData.Log.UpdateLog(
                    string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                        connection.channelId, connection.localUid, elapsed));
            }

            public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _agoraVideoRawData.Log.UpdateLog("OnRejoinChannelSuccess");
            }

            public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
            {
                _agoraVideoRawData.Log.UpdateLog("OnLeaveChannel");
            }

            public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
                CLIENT_ROLE_TYPE newRole)
            {
                _agoraVideoRawData.Log.UpdateLog("OnClientRoleChanged");
            }

            public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
            {
                _agoraVideoRawData.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid,
                    elapsed));
            }

            public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
            {
                _agoraVideoRawData.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                    (int)reason));
            }
        }

        internal class VideoFrameObserver : IVideoFrameObserver
        {
            private readonly ProcessVideoRawData _agoraVideoRawData;

            internal VideoFrameObserver(ProcessVideoRawData agoraVideoRawData)
            {
                _agoraVideoRawData = agoraVideoRawData;
            }

            public override bool OnCaptureVideoFrame(VideoFrame videoFrame, VideoFrameBufferConfig config)
            {
                Debug.Log("OnCaptureVideoFrame-----------" + " width:" + videoFrame.width + " height:" +
                          videoFrame.height);
                _agoraVideoRawData.VideoFrameWidth = videoFrame.width;
                _agoraVideoRawData.VideoFrameHeight = videoFrame.height;
                lock (_agoraVideoRawData.VideoBuffer)
                {
                    _agoraVideoRawData.VideoBuffer = videoFrame.yBuffer;
                }
                return true;
            }

            public override bool OnRenderVideoFrame(string channelId, uint uid, VideoFrame videoFrame)
            {
                Debug.Log("OnRenderVideoFrameHandler-----------" + " uid:" + uid + " width:" + videoFrame.width +
                          " height:" + videoFrame.height);
                return true;
            }

            public override VIDEO_OBSERVER_FRAME_TYPE GetVideoFormatPreference()
            {
                return VIDEO_OBSERVER_FRAME_TYPE.FRAME_TYPE_RGBA;
            }
        }
    }
}

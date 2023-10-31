using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using io.agora.rtc.demo;


namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.ProcessVideoRawDataS
{
    public class ProcessVideoRawDataS : MonoBehaviour
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
        internal IRtcEngineS RtcEngine;

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
            LoadAssetData();
            if (CheckAppId())
            {
                SetUpUI();
                InitEngine();
                JoinChannel();
            }
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
            else if (_needResize)
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
            _texture = new Texture2D(_videoFrameWidth, _videoFrameHeight, TextureFormat.RGBA32, false);
            _texture.Apply();
        }

        void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngineS.CreateAgoraRtcEngine();
            UserEventHandlerS handler = new UserEventHandlerS(this);
            RtcEngineContextS context = new RtcEngineContextS();
            context.appId = _appID;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
            context.areaCode = AREA_CODE.AREA_CODE_GLOB;
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
            RtcEngine.RegisterVideoFrameObserver(new VideoFrameObserverS(this),
                VIDEO_OBSERVER_FRAME_TYPE.FRAME_TYPE_RGBA,
                VIDEO_MODULE_POSITION.POSITION_POST_CAPTURER |
                VIDEO_MODULE_POSITION.POSITION_PRE_RENDERER |
                VIDEO_MODULE_POSITION.POSITION_PRE_ENCODER,
                OBSERVER_MODE.RAW_DATA);
        }

        void JoinChannel()
        {
            VideoEncoderConfiguration config = new VideoEncoderConfiguration();
            config.dimensions = new VideoDimensions(_videoFrameWidth, _videoFrameHeight);
            RtcEngine.SetVideoEncoderConfiguration(config);

            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.JoinChannel(_token, _channelName, "", "123");
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

        private static float[] ConvertByteToFloat16(byte[] byteArray)
        {
            var floatArray = new float[byteArray.Length / 2];
            for (var i = 0; i < floatArray.Length; i++)
            {
                floatArray[i] = System.BitConverter.ToInt16(byteArray, i * 2) / 32768f; // -Int16.MinValue
            }

            return floatArray;
        }
    }

    #region -- Agora Event ---

    internal class UserEventHandlerS : IRtcEngineEventHandlerS
    {
        private readonly ProcessVideoRawDataS _agoraVideoRawData;

        internal UserEventHandlerS(ProcessVideoRawDataS agoraVideoRawData)
        {
            _agoraVideoRawData = agoraVideoRawData;
        }

        public override void OnError(int err, string msg)
        {
            _agoraVideoRawData.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnectionS connection, int elapsed)
        {
            int build = 0;
            _agoraVideoRawData.Log.UpdateLog(string.Format("sdk version: ${0}",
                _agoraVideoRawData.RtcEngine.GetVersion(ref build)));
            _agoraVideoRawData.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                    connection.channelId, connection.localUserAccount, elapsed));
        }

        public override void OnRejoinChannelSuccess(RtcConnectionS connection, int elapsed)
        {
            _agoraVideoRawData.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnectionS connection, RtcStats stats)
        {
            _agoraVideoRawData.Log.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(RtcConnectionS connection, CLIENT_ROLE_TYPE oldRole,
            CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _agoraVideoRawData.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnectionS connection, string userAccount, int elapsed)
        {
            _agoraVideoRawData.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", userAccount,
                elapsed));
        }

        public override void OnUserOffline(RtcConnectionS connection, string userAccount, USER_OFFLINE_REASON_TYPE reason)
        {
            _agoraVideoRawData.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", userAccount,
                (int)reason));
        }
    }

    internal class VideoFrameObserverS : IVideoFrameObserverS
    {
        private readonly ProcessVideoRawDataS _agoraVideoRawData;

        internal VideoFrameObserverS(ProcessVideoRawDataS agoraVideoRawData)
        {
            _agoraVideoRawData = agoraVideoRawData;
        }

        public override bool OnCaptureVideoFrame(VIDEO_SOURCE_TYPE type, VideoFrame videoFrame)
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

        public override bool OnRenderVideoFrame(string channelId, string remoteUserAccount, VideoFrame videoFrame)
        {
            Debug.Log("OnRenderVideoFrameHandler-----------" + " uid:" + remoteUserAccount + " width:" + videoFrame.width +
                        " height:" + videoFrame.height);
            return true;
        }
    }

    #endregion
}

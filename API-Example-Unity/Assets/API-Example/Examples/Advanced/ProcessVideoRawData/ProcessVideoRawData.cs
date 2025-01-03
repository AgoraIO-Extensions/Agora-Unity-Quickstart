using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using io.agora.rtc.demo;


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
        internal Logger Log;
        internal IRtcEngine RtcEngine;

        internal int Count;

        internal int WriteCount;
        internal int ReadCount;

        public GameObject VideoView;
        public Texture2D _texture;
        public int _videoSourceWidth = 960;
        public int _videoSourceHeight = 540;
        public bool _isNewFrame = false;
        public byte[] _videoBuffer = new byte[960 * 540];

        private void Awake()
        {
            _texture = new Texture2D(_videoSourceWidth, _videoSourceHeight, TextureFormat.RGBA32, false);
            _texture.Apply();
            var rd = VideoView.GetComponent<RawImage>();
            rd.texture = _texture;
        }


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

            lock (_videoBuffer)
            {
                if (_isNewFrame)
                {
                    if (_videoSourceWidth != _texture.width || _videoSourceHeight != _texture.height)
                    {
                        _texture.Reinitialize(_videoSourceWidth, _videoSourceHeight);
                       
                    }
                    Debug.Log("_texture: " + _texture.width + " : " + _texture.height);
                    _texture.LoadRawTextureData(_videoBuffer);
                    _texture.Apply();
                    _isNewFrame = false;
                }
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
            //_texture = new Texture2D(_videoFrameWidth, _videoFrameHeight, TextureFormat.RGBA32, false);
            //_texture.Apply();
        }

        void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext();
            context.appId = _appID;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
            context.areaCode = AREA_CODE.AREA_CODE_GLOB;
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
            RtcEngine.RegisterVideoFrameObserver(new VideoFrameObserver(this),
                VIDEO_OBSERVER_FRAME_TYPE.FRAME_TYPE_RGBA,
                VIDEO_MODULE_POSITION.POSITION_POST_CAPTURER |
                VIDEO_MODULE_POSITION.POSITION_PRE_RENDERER |
                VIDEO_MODULE_POSITION.POSITION_PRE_ENCODER,
                OBSERVER_MODE.RAW_DATA);
        }

        void JoinChannel()
        {
            VideoEncoderConfiguration config = new VideoEncoderConfiguration();
            config.dimensions = new VideoDimensions(_videoSourceHeight, _videoSourceHeight);
            RtcEngine.SetVideoEncoderConfiguration(config);

            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.JoinChannel(_token, _channelName, "", 0);
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

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly ProcessVideoRawData _agoraVideoRawData;

        internal UserEventHandler(ProcessVideoRawData agoraVideoRawData)
        {
            _agoraVideoRawData = agoraVideoRawData;
        }

        public override void OnError(int err, string msg)
        {
            _agoraVideoRawData.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _agoraVideoRawData.Log.UpdateLog(string.Format("sdk version: ${0}",
                _agoraVideoRawData.RtcEngine.GetVersion(ref build)));
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
            CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
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

        public override bool OnCaptureVideoFrame(VIDEO_SOURCE_TYPE type, VideoFrame videoFrame)
        {
            lock (_agoraVideoRawData._videoBuffer)
            {
                Debug.Log("OnCaptureVideoFrame----------- width:" + videoFrame.width +
                      " height:" + videoFrame.height);
                _agoraVideoRawData._videoSourceWidth = videoFrame.width;
                _agoraVideoRawData._videoSourceHeight = videoFrame.height;
                _agoraVideoRawData._videoBuffer = videoFrame.yBuffer;
                Debug.Log("_videoBuffer length: " + _agoraVideoRawData._videoBuffer.Length);
                _agoraVideoRawData._isNewFrame = true;
            }
            return true;
        }

        public override bool OnRenderVideoFrame(string channelId, uint remoteUid, VideoFrame videoFrame)
        {
            Debug.Log("OnRenderVideoFrameHandler-----------" + " uid:" + remoteUid + " width:" + videoFrame.width +
                        " height:" + videoFrame.height);
            return true;
        }
    }

    #endregion
}

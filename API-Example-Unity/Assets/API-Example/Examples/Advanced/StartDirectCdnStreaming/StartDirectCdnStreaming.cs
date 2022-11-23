using Agora.Rtc;
using Agora.Util;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Logger = Agora.Util.Logger;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.StartDirectCdnStreaming
{
    public class StartDirectCdnStreaming : MonoBehaviour
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


        private const string PUBLISH_URL = "rtmp://push.alexmk.name/live/agora_rtc_unity";
        public Text LogText;
        internal Logger Log;
        internal IRtcEngine RtcEngine = null;

        // Use this for initialization
        void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
                SetProfile();
                StartDirectCdnStreamingCamera();
            }
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
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(new UserEventHandler(this));
        }

        private void SetProfile()
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        }

        private void StartDirectCdnStreamingCamera()
        {
            DirectCdnStreamingMediaOptions options = new DirectCdnStreamingMediaOptions();
            options.publishMicrophoneTrack.SetValue(true);
            options.publishCameraTrack.SetValue(true);
           
            RtcEngine.SetDirectCdnStreamingVideoConfiguration(new VideoEncoderConfiguration
            {
                dimensions = new VideoDimensions { width = 1280, height = 720 },
                frameRate = 15,
                bitrate = 2260,
                minBitrate = -1,
                degradationPreference = DEGRADATION_PREFERENCE.MAINTAIN_QUALITY,
                codecType = VIDEO_CODEC_TYPE.VIDEO_CODEC_H264,
                mirrorMode = VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_DISABLED
            });
            RtcEngine.StartDirectCdnStreaming(PUBLISH_URL, options);
            RtcEngine.StartPreview();
            MakeVideoView(0);
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            DestroyVideoView(0);
            RtcEngine.StopPreview();
            RtcEngine.StopDirectCdnStreaming();
            RtcEngine.Dispose();
        }

        internal string GetChannelName()
        {
            return _channelName;
        }

        #region -- Video Render UI Logic ---

        internal static void MakeVideoView(uint uid, string channelId = "")
        {
            GameObject go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            // create a GameObject and assign to this new user
            VideoSurface videoSurface = MakeImageSurface(uid.ToString());
            if (!ReferenceEquals(videoSurface, null))
            {
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
        }

        // VIDEO TYPE 1: 3D Object
        private static VideoSurface MakePlaneSurface(string goName)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // set up transform
            go.transform.Rotate(-90.0f, 0.0f, 0.0f);
            float yPos = Random.Range(3.0f, 5.0f);
            float xPos = Random.Range(-2.0f, 2.0f);
            go.transform.position = new Vector3(xPos, yPos, 0f);
            go.transform.localScale = new Vector3(0.25f, 0.5f, .5f);

            // configure videoSurface
            VideoSurface videoSurface = go.AddComponent<VideoSurface>();
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
            GameObject canvas = GameObject.Find("VideoCanvas");
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
            go.transform.localScale = new Vector3(3f, 4f, 1f);

            // configure videoSurface
            VideoSurface videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

        internal static void DestroyVideoView(uint uid)
        {
            GameObject go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                Object.Destroy(go);
            }
        }

        #endregion
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly StartDirectCdnStreaming _startDirectCdnStreaming;

        internal UserEventHandler(StartDirectCdnStreaming startDirectCdnStreaming)
        {
            _startDirectCdnStreaming = startDirectCdnStreaming;
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _startDirectCdnStreaming.Log.UpdateLog(string.Format("sdk version: ${0}",
                _startDirectCdnStreaming.RtcEngine.GetVersion(ref build)));
            _startDirectCdnStreaming.Log.UpdateLog(string.Format(
                "onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", connection.channelId,
                connection.localUid, elapsed));
            StartDirectCdnStreaming.MakeVideoView(0);
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _startDirectCdnStreaming.Log.UpdateLog("OnLeaveChannelSuccess");
            StartDirectCdnStreaming.DestroyVideoView(0);
        }

        public override void OnUserJoined(RtcConnection connection, uint remoteUid, int elapsed)
        {
            _startDirectCdnStreaming.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}",
                connection.localUid, elapsed));
            StartDirectCdnStreaming.MakeVideoView(remoteUid, _startDirectCdnStreaming.GetChannelName());
        }

        public override void OnUserOffline(RtcConnection connection, uint remoteUid,
            USER_OFFLINE_REASON_TYPE reason)
        {
            _startDirectCdnStreaming.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", remoteUid,
                (int)reason));
            StartDirectCdnStreaming.DestroyVideoView(remoteUid);
        }

        public override void OnError(int error, string msg)
        {
            _startDirectCdnStreaming.Log.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
        }

        public override void OnConnectionLost(RtcConnection connection)
        {
            _startDirectCdnStreaming.Log.UpdateLog("OnConnectionLost ");
        }

        public override void OnDirectCdnStreamingStateChanged(DIRECT_CDN_STREAMING_STATE state, DIRECT_CDN_STREAMING_ERROR error, string message)
        {
            _startDirectCdnStreaming.Log.UpdateLog(string.Format("OnDirectCdnStreamingStateChanged state: {0}, error: {1}", state, error));
        }

        public override void OnDirectCdnStreamingStats(DirectCdnStreamingStats stats)
        {
            _startDirectCdnStreaming.Log.UpdateLog("OnDirectCdnStreamingStats videoHeight:" + stats.videoHeight + " videoWidth:" + stats.videoWidth);
            _startDirectCdnStreaming.Log.UpdateLog("OnDirectCdnStreamingStats fps:" + stats.fps);
        }
    }

    #endregion
}

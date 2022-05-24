using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using UnityEngine.Serialization;
using Logger = agora.util.Logger;

namespace Agora_Plugin.API_Example.examples.advanced.SetVideoEncodeConfiguration
{
    public class SetVideoEncodeConfiguration : MonoBehaviour
    {
        [FormerlySerializedAs("appIdInput")] [SerializeField]
        private AppIdInput appIdInput;
        
        [Header("_____________Basic Configuration_____________")]
        [FormerlySerializedAs("APP_ID")] [SerializeField]
        private string appID = "";

        [FormerlySerializedAs("TOKEN")] [SerializeField]
        private string token = "";

        [FormerlySerializedAs("CHANNEL_NAME")] [SerializeField]
        private string channelName = "";
        
        public Text logText;
        private Logger logger;
        internal IRtcEngine mRtcEngine = null;
        private const float Offset = 100;

        // A list of dimensions for swithching
        VideoDimensions[] dimensions = new VideoDimensions[]
        {
            new VideoDimensions {width = 640, height = 480},
            new VideoDimensions {width = 480, height = 480},
            new VideoDimensions {width = 480, height = 240}
        };

        // Start is called before the first frame update
        void Start()
        {
            LoadAssetData();
            CheckAppId();
            InitEngine();
            JoinChannel();
            SetVideoEncoderConfiguration();
        }

        // Update is called once per frame
        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        internal string GetChannelName()
        {
            return channelName;
        }

        void CheckAppId()
        {
            logger = new Logger(logText);
            logger.DebugAssert(appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
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

        void InitEngine()
        {
            mRtcEngine = RtcEngineImpl.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(appID, 0, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            mRtcEngine.Initialize(context);
            mRtcEngine.InitEventHandler(handler);
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            mRtcEngine.EnableAudio();
            mRtcEngine.EnableVideo();
        }

        void JoinChannel()
        {
            mRtcEngine.JoinChannel(token, channelName, "");
        }

        public void SetVideoEncoderConfiguration(int dim = 0)
        {
            if (dim >= dimensions.Length)
            {
                Debug.LogError("Invalid dimension choice!");
                return;
            }

            VideoEncoderConfiguration config = new VideoEncoderConfiguration
            {
                dimensions = dimensions[dim],
                frameRate = 15,
                codecType = VIDEO_CODEC_TYPE.VIDEO_CODEC_GENERIC_H264,
                bitrate = 0,
                minBitrate = 1,
                orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_ADAPTIVE,
                degradationPreference = DEGRADATION_PREFERENCE.MAINTAIN_FRAMERATE,
                mirrorMode = VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_AUTO
            };
            mRtcEngine.SetVideoEncoderConfiguration(config);
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (mRtcEngine == null) return;
            mRtcEngine.InitEventHandler(null);
            mRtcEngine.LeaveChannel();
        }

        void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            if (mRtcEngine != null)
            {
                mRtcEngine.LeaveChannel();
                mRtcEngine.Dispose();
            }
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

            videoSurface.SetEnable(true);
        }

        // VIDEO TYPE 1: 3D Object
        private VideoSurface MakePlaneSurface(string goName)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);

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

        internal static void DestroyVideoView(uint uid)
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                Destroy(go);
            }
        }


        internal class UserEventHandler : IRtcEngineEventHandler
        {
            private readonly SetVideoEncodeConfiguration _videoEncoderConfiguration;

            internal UserEventHandler(SetVideoEncodeConfiguration videoEncoderConfiguration)
            {
                _videoEncoderConfiguration = videoEncoderConfiguration;
            }

            public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _videoEncoderConfiguration.logger.UpdateLog(string.Format("sdk version: ${0}",
                    _videoEncoderConfiguration.mRtcEngine.GetVersion()));
                _videoEncoderConfiguration.logger.UpdateLog(string.Format(
                    "onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", connection.channelId,
                    connection.localUid, elapsed));
                MakeVideoView(0);
            }

            public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
            {
                _videoEncoderConfiguration.logger.UpdateLog("OnLeaveChannelSuccess");
                DestroyVideoView(0);
            }

            public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
            {
                _videoEncoderConfiguration.logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid,
                    elapsed));
                MakeVideoView(uid, _videoEncoderConfiguration.GetChannelName());
            }

            public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
            {
                _videoEncoderConfiguration.logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                    (int) reason));
                DestroyVideoView(uid);
            }

            public override void OnWarning(int warn, string msg)
            {
                _videoEncoderConfiguration.logger.UpdateLog(
                    string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
            }

            public override void OnError(int error, string msg)
            {
                _videoEncoderConfiguration.logger.UpdateLog(
                    string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
            }

            public override void OnConnectionLost(RtcConnection connection)
            {
                _videoEncoderConfiguration.logger.UpdateLog(string.Format("OnConnectionLost "));
            }
        }
    }
}

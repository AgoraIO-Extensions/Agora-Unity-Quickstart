using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using Logger = agora.util.Logger;

namespace Agora_Plugin.API_Example.examples.advanced.SetVideoEncodeConfiguration
{
    public class SetVideoEncodeConfiguration : MonoBehaviour
    {
        [SerializeField] private string APP_ID = "";

        [SerializeField] private string TOKEN = "";

        [SerializeField] private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";
        public Text logText;
        private Logger logger;
        internal IAgoraRtcEngine mRtcEngine = null;
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
            return CHANNEL_NAME;
        }

        void CheckAppId()
        {
            logger = new Logger(logText);
            logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
        }

        void InitEngine()
        {
            mRtcEngine = agora.rtc.AgoraRtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(null, APP_ID, null, true,
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
            mRtcEngine.JoinChannel(TOKEN, CHANNEL_NAME, "");
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
                frameRate = FRAME_RATE.FRAME_RATE_FPS_15,
                codecType = VIDEO_CODEC_TYPE.VIDEO_CODEC_GENERIC_H264,
                bitrate = 0,
                minBitrate = 1,
                orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_ADAPTIVE,
                degradationPreference = DEGRADATION_PREFERENCE.MAINTAIN_FRAMERATE,
                mirrorMode = VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_AUTO
            };
            mRtcEngine.SetVideoEncoderConfiguration(config);
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
        private AgoraVideoSurface MakePlaneSurface(string goName)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // set up transform
            go.transform.Rotate(-90.0f, 0.0f, 0.0f);
            var yPos = Random.Range(3.0f, 5.0f);
            var xPos = Random.Range(-2.0f, 2.0f);
            go.transform.position = new Vector3(xPos, yPos, 0f);
            go.transform.localScale = new Vector3(0.25f, 0.5f, 0.5f);

            // configure videoSurface
            var videoSurface = go.AddComponent<AgoraVideoSurface>();
            return videoSurface;
        }

        // Video TYPE 2: RawImage
        private static AgoraVideoSurface MakeImageSurface(string goName)
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
            //var xPos = Random.Range(Offset - Screen.width / 2f, Screen.width / 2f - Offset);
            //var yPos = Random.Range(Offset, Screen.height / 2f - Offset);
            //Debug.Log("position x " + xPos + " y: " + yPos);
            go.transform.localPosition = new Vector3(Screen.width / 2f - Offset, Screen.height / 2f - Offset, 0f);
            go.transform.localScale = new Vector3(2f, 3f, 1f);

            // configure videoSurface
            var videoSurface = go.AddComponent<AgoraVideoSurface>();
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


        internal class UserEventHandler : IAgoraRtcEngineEventHandler
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

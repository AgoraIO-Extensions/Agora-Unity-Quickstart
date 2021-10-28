using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using Logger = agora.util.Logger;

public class SetVideoEncodeConfiguration : MonoBehaviour
{
    [SerializeField]
    private string APP_ID = "";

    [SerializeField]
    private string TOKEN = "";

    [SerializeField]
    private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";
    public Text logText;
    private Logger logger;
    internal IAgoraRtcEngine mRtcEngine = null;
    private const float Offset = 100;
    //private static string channelName = "Agora_Channel";
    // A list of dimensions for swithching
    VideoDimensions[] dimensions = new VideoDimensions[]{
        new VideoDimensions { width = 640, height = 480 },
        new VideoDimensions { width = 480, height = 480 },
        new VideoDimensions { width = 480, height = 240 }
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
        mRtcEngine.Initialize(new RtcEngineContext(APP_ID, AREA_CODE.AREA_CODE_GLOB, new LogConfig("log.txt")));
        mRtcEngine.InitEventHandler(new UserEventHandler(this));
        mRtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
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
        if (dim >= dimensions.Length) {
            Debug.LogError("Invalid dimension choice!");
            return;
        }
        VideoEncoderConfiguration config = new VideoEncoderConfiguration
        {
            dimensions = dimensions[dim],
            frameRate = FRAME_RATE.FRAME_RATE_FPS_15,
            minFrameRate = -1,
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

    private void DestroyVideoView(uint uid)
    {
        GameObject go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            Object.Destroy(go);
        }
    }

    private void makeVideoView(uint uid, string channelId = "")
    {
        GameObject go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            return; // reuse
        }

        // create a GameObject and assign to this new user
        VideoSurface videoSurface = makeImageSurface(uid.ToString());
        if (!ReferenceEquals(videoSurface, null))
        {
            // configure videoSurface
            videoSurface.SetForUser(uid, channelId);
            videoSurface.SetEnable(true);
        }
    }

    // VIDEO TYPE 1: 3D Object
    private VideoSurface makePlaneSurface(string goName)
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
    private static VideoSurface makeImageSurface(string goName)
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
        float xPos = Random.Range(Offset - Screen.width / 2f, Screen.width / 2f - Offset);
        float yPos = Random.Range(Offset, Screen.height / 2f - Offset);
        Debug.Log("position x " + xPos + " y: " + yPos);
        go.transform.localPosition = new Vector3(xPos, yPos, 0f);
        go.transform.localScale = new Vector3(3f, 4f, 1f);

        // configure videoSurface
        VideoSurface videoSurface = go.AddComponent<VideoSurface>();
        return videoSurface;
    }

    internal class UserEventHandler : IAgoraRtcEngineEventHandler
    {
        private readonly SetVideoEncodeConfiguration _videoEncoderConfiguration;

        internal UserEventHandler(SetVideoEncodeConfiguration videoEncoderConfiguration)
        {
            _videoEncoderConfiguration = videoEncoderConfiguration;
        }
        
        public override void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
        {
            _videoEncoderConfiguration.logger.UpdateLog(string.Format("sdk version: ${0}", _videoEncoderConfiguration.mRtcEngine.GetVersion()));
            _videoEncoderConfiguration.logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
            _videoEncoderConfiguration.makeVideoView(0);
        }

        public override void OnLeaveChannel(RtcStats stats)
        {
            _videoEncoderConfiguration.logger.UpdateLog("OnLeaveChannelSuccess");
            _videoEncoderConfiguration.DestroyVideoView(0);
        }

        public override void OnUserJoined(uint uid, int elapsed)
        {
            _videoEncoderConfiguration.logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            _videoEncoderConfiguration.makeVideoView(uid, _videoEncoderConfiguration.GetChannelName());
        }

        public override void OnUserOffline(uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _videoEncoderConfiguration.logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid, (int)reason));
            _videoEncoderConfiguration.DestroyVideoView(uid);
        }

        public override void OnWarning(int warn, string msg)
        {
            _videoEncoderConfiguration.logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int error, string msg)
        {
            _videoEncoderConfiguration.logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
        }

        public override void OnConnectionLost()
        {
            _videoEncoderConfiguration.logger.UpdateLog(string.Format("OnConnectionLost "));
        }
    }
}

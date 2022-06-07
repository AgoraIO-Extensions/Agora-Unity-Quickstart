using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using agora_utilities;

public class video_encoder_configuration : MonoBehaviour
{
    [SerializeField]
    public string APP_ID = "";

    [SerializeField]
    public string TOKEN = "";

    [SerializeField]
    public string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

    public Text LogText;
    private Logger _logger;
    private IRtcEngine _rtcEngine = null;
    private const float _offset = 100;

    // A list of dimensions for swithching
    VideoDimensions[] _dimensions = new VideoDimensions[]{
        new VideoDimensions { width = 640, height = 480 },
        new VideoDimensions { width = 480, height = 480 },
        new VideoDimensions { width = 480, height = 240 }
    };

    // Start is called before the first frame update
    void Start()
    {
        if (CheckAppId())
        {
            InitEngine();
            JoinChannel();
            SetVideoEncoderConfiguration(); // use default one
        }
    }

    // Update is called once per frame
    void Update()
    {
        PermissionHelper.RequestMicrophontPermission();
        PermissionHelper.RequestCameraPermission();
    }

    bool CheckAppId()
    {
        _logger = new Logger(LogText);
        return  _logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
    }

    void InitEngine()
    {
        _rtcEngine = IRtcEngine.GetEngine(APP_ID);
        _rtcEngine.SetLogFile("log.txt");
        _rtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        _rtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        _rtcEngine.EnableAudio();
        _rtcEngine.EnableVideo();
        _rtcEngine.EnableVideoObserver();
        _rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
        _rtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        _rtcEngine.OnWarning += OnSDKWarningHandler;
        _rtcEngine.OnError += OnSDKErrorHandler;
        _rtcEngine.OnConnectionLost += OnConnectionLostHandler;
        _rtcEngine.OnUserJoined += OnUserJoinedHandler;
        _rtcEngine.OnUserOffline += OnUserOfflineHandler;
    }

    void JoinChannel()
    {
        _rtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
    }


    /// <summary>
    ///   Setting the Encoder Configuration.  Possbily called from UI button
    /// </summary>
    /// <param name="dim"></param>
    public void SetVideoEncoderConfiguration(int dim = 0)
    {
        if (dim >= _dimensions.Length) {
            Debug.LogError("Invalid dimension choice!");
            return;
	    }

        VideoEncoderConfiguration config = new VideoEncoderConfiguration
        {
            dimensions = _dimensions[dim],
            frameRate = FRAME_RATE.FRAME_RATE_FPS_15,
            minFrameRate = -1,
            bitrate = 0,
            minBitrate = 1,
            orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_ADAPTIVE,
            degradationPreference = DEGRADATION_PREFERENCE.MAINTAIN_FRAMERATE,
            mirrorMode = VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_AUTO
        };
        _rtcEngine.SetVideoEncoderConfiguration(config);
    }

    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("sdk version: ${0}", IRtcEngine.GetSdkVersion()));
        _logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
        makeVideoView(0);
    }

    void OnLeaveChannelHandler(RtcStats stats)
    {
        _logger.UpdateLog("OnLeaveChannelSuccess");
        DestroyVideoView(0);
    }

    void OnUserJoinedHandler(uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        makeVideoView(uid);
    }

    void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        _logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid, (int)reason));
        DestroyVideoView(uid);
    }

    void OnSDKWarningHandler(int warn, string msg)
    {
        _logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
    }

    void OnSDKErrorHandler(int error, string msg)
    {
        _logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
    }

    void OnConnectionLostHandler()
    {
        _logger.UpdateLog(string.Format("OnConnectionLost "));
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        if (_rtcEngine != null)
        {
            _rtcEngine.LeaveChannel();
            _rtcEngine.DisableVideoObserver();
            IRtcEngine.Destroy();
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

    private void makeVideoView(uint uid)
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
            videoSurface.SetForUser(uid);
            videoSurface.SetEnable(true);
            videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
            videoSurface.SetGameFps(30);
        }
    }

    // VIDEO TYPE 1: 3D Object
    public VideoSurface makePlaneSurface(string goName)
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
    public VideoSurface makeImageSurface(string goName)
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
        float xPos = Random.Range(_offset - Screen.width / 2f, Screen.width / 2f - _offset);
        float yPos = Random.Range(_offset, Screen.height / 2f - _offset);
        Debug.Log("position x " + xPos + " y: " + yPos);
        go.transform.localPosition = new Vector3(xPos, yPos, 0f);
        go.transform.localScale = new Vector3(3f, 4f, 1f);

        // configure videoSurface
        VideoSurface videoSurface = go.AddComponent<VideoSurface>();
        return videoSurface;
    }
}

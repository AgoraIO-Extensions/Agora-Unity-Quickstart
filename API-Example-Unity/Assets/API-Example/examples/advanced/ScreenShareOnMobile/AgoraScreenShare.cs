using System.Collections;
using UnityEngine;
using agora_gaming_rtc;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using agora_utilities;

public class AgoraScreenShare : MonoBehaviour
{

    [SerializeField]
    public string APP_ID = "YOUR_APPID";

    [SerializeField]
    public string TOKEN = "";

    [SerializeField]
    public string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

    public Text LogText;

    public int BitRate = 2080;
    public FRAME_RATE FrameRate = FRAME_RATE.FRAME_RATE_FPS_15;

    private Logger _logger;
    private IRtcEngine _rtcEngine = null;
    private const float _offset = 100;
    private Texture2D _texture;
    private Rect _rect;
    private WebCamTexture _webCameraTexture;
    public RawImage _rawImage;
    private Vector2 _cameraSize = new Vector2(640, 480);
    private int _cameraFPS = 15;

    // Use this for initialization
    void Start()
    {
        if (CheckAppId())
        {
            InitCameraDevice();
            InitTexture();
            InitEngine();
            JoinChannel();
        }
    }

    void Update()
    {
        PermissionHelper.RequestMicrophontPermission();
        PermissionHelper.RequestCameraPermission();
        StartCoroutine(shareScreen());
    }

    IEnumerator shareScreen()
    {
        yield return new WaitForEndOfFrame();
        IRtcEngine rtc = IRtcEngine.QueryEngine();
        if (rtc != null)
        {
            _texture.ReadPixels(_rect, 0, 0);
            _texture.Apply();
            byte[] bytes = _texture.GetRawTextureData();
            int size = Marshal.SizeOf(bytes[0]) * bytes.Length;
            ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
            externalVideoFrame.type = ExternalVideoFrame.VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
            externalVideoFrame.format = ExternalVideoFrame.VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;
            externalVideoFrame.buffer = bytes;
            externalVideoFrame.stride = (int)_rect.width;
            externalVideoFrame.height = (int)_rect.height;
            //externalVideoFrame.cropLeft = 10;
            //externalVideoFrame.cropTop = 10;
            //externalVideoFrame.cropRight = 10;
            //externalVideoFrame.cropBottom = 10;
            externalVideoFrame.rotation = 180;
            externalVideoFrame.timestamp = System.DateTime.Now.Ticks / 10000;
            int a = rtc.PushVideoFrame(externalVideoFrame);
            Debug.Log("PushVideoFrame ret = " + a);
        }
    }

    void InitEngine()
    {
        _rtcEngine = IRtcEngine.GetEngine(APP_ID);
        _rtcEngine.SetLogFile("log.txt");
        _rtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        _rtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        // Set up video encoder so good resolution can be shown, otherwise it is default to 480p
        _rtcEngine.SetVideoEncoderConfiguration(new VideoEncoderConfiguration
        {
            bitrate = BitRate,
            frameRate = FrameRate,
            dimensions = new VideoDimensions { width = (int)_rect.width, height = (int)_rect.height }
        });
        // Note Agora SDK v3.x only support max resolution 720p encode by default.

        _rtcEngine.SetExternalVideoSource(true, false);
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
        _rtcEngine.EnableAudio();
        _rtcEngine.EnableVideo();
        _rtcEngine.EnableVideoObserver();
        int ret = _rtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
        Debug.Log(string.Format("JoinChannel ret: {0}", ret));
    }

    bool CheckAppId()
    {
        _logger = new Logger(LogText);
        return _logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in Canvas!!!!");
    }

    void InitTexture()
    {
        _rect = new Rect(0, 0, Screen.width, Screen.height);
        _texture = new Texture2D((int)_rect.width, (int)_rect.height, TextureFormat.RGBA32, false);

        _logger.UpdateLog("_rect = " + _rect);
    }

    public void InitCameraDevice()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        _webCameraTexture = new WebCamTexture(devices[0].name, (int)_cameraSize.x, (int)_cameraSize.y, _cameraFPS);
        _rawImage.texture = _webCameraTexture;
        _webCameraTexture.Play();
    }

    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("sdk version: {0}", IRtcEngine.GetSdkVersion()));
        _logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));

    }

    void OnLeaveChannelHandler(RtcStats stats)
    {
        _logger.UpdateLog("OnLeaveChannelSuccess");
    }

    void OnUserJoinedHandler(uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("OnUserJoined uid: {0} elapsed: {1}", uid, elapsed));
        makeVideoView(uid);
    }

    void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        _logger.UpdateLog(string.Format("OnUserOffLine uid: {0}, reason: {1}", uid, (int)reason));
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
        if (_webCameraTexture)
        {
            _webCameraTexture.Stop();
        }

        if (_rtcEngine != null)
        {
            _rtcEngine.LeaveChannel();
            _rtcEngine.DisableVideoObserver();
            IRtcEngine.Destroy();
            _rtcEngine = null;
        }
    }

    private void DestroyVideoView(uint uid)
    {
        GameObject go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            Destroy(go);
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

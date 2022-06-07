using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using agora_utilities;

/// <summary>
///   This example shows how to run video chat with token APIs.
///   You will need a token server to supply the RTC token.  And your AppID
/// must be created with certficate.
///   The server URL format used in this example is based on Agora Token Service
/// example repo:
///   https://github.com/AgoraIO-Community/agora-token-service
/// </summary>
public class HelloVideoTokenAgora : MonoBehaviour
{

    [SerializeField]
    private string APP_ID = "YOUR_APPID";

    public Text LogText;
    private Logger _logger;
    private IRtcEngine _rtcEngine = null;
    private const float _offset = 100;
    private static string _channelName = "Agora_Channel";
    private static string _channelToken = "";
    private static string _tokenBase = "http://localhost:8080";
    private CONNECTION_STATE_TYPE _state = CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED;

    // Use this for initialization
    void Start()
    {
        if (CheckAppId())
        {
            InitEngine();
            JoinChannel();
        }
    }

    void RenewOrJoinToken(string newToken)
    {
        HelloVideoTokenAgora._channelToken = newToken;
        if (_state == CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED
            || _state == CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED
            || _state == CONNECTION_STATE_TYPE.CONNECTION_STATE_FAILED
        )
        {
            // If we are not connected yet, connect to the channel as normal
            JoinChannel();
        }
        else
        {
            // If we are already connected, we should just update the token
            UpdateToken();
        }
    }

    // Update is called once per frame
    void Update()
    {
        PermissionHelper.RequestMicrophontPermission();
        PermissionHelper.RequestCameraPermission();
    }

    void UpdateToken()
    {
        _rtcEngine.RenewToken(HelloVideoTokenAgora._channelToken);
    }

    bool CheckAppId()
    {
        _logger = new Logger(LogText);
        return _logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
    }

    void InitEngine()
    {
        _rtcEngine = IRtcEngine.GetEngine(APP_ID);
        _rtcEngine.SetLogFile("log.txt");
        _rtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        _rtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        _rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
        _rtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        _rtcEngine.OnWarning += OnSDKWarningHandler;
        _rtcEngine.OnError += OnSDKErrorHandler;
        _rtcEngine.OnConnectionLost += OnConnectionLostHandler;
        _rtcEngine.OnUserJoined += OnUserJoinedHandler;
        _rtcEngine.OnUserOffline += OnUserOfflineHandler;
        _rtcEngine.OnTokenPrivilegeWillExpire += OnTokenPrivilegeWillExpireHandler;
        _rtcEngine.OnConnectionStateChanged += OnConnectionStateChangedHandler;

    }

    void JoinChannel()
    {
        if (string.IsNullOrEmpty(_channelToken))
        {
            StartCoroutine(HelperClass.FetchToken(_tokenBase, _channelName, 0, this.RenewOrJoinToken));
            return;
        }
        _rtcEngine.EnableAudio();
        _rtcEngine.EnableVideo();
        _rtcEngine.EnableVideoObserver();
        _rtcEngine.JoinChannelByKey(_channelToken, _channelName, "", 0);
    }

    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("sdk version: ${0}", IRtcEngine.GetSdkVersion()));
        _logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
        _logger.UpdateLog(string.Format("New Token: {0}", HelloVideoTokenAgora._channelToken));
        // HelperClass.FetchToken(tokenBase, channelName, 0, this.RenewOrJoinToken);
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

    void OnTokenPrivilegeWillExpireHandler(string token)
    {
        StartCoroutine(HelperClass.FetchToken(_tokenBase, _channelName, 0, this.RenewOrJoinToken));
    }

    void OnConnectionStateChangedHandler(CONNECTION_STATE_TYPE state, CONNECTION_CHANGED_REASON_TYPE reason)
    {
        this._state = state;
        _logger.UpdateLog(string.Format("ConnectionState changed {0}, reason: ${1}", state, reason));
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
        }
    }

    // Create Video Surface on RawImage
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
            go.transform.SetParent( canvas.transform );
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

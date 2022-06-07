using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using agora_utilities;

public class AgoraMultiChannel : MonoBehaviour
{
    [SerializeField]
    public string APP_ID = "YOUR_APPID";

    [SerializeField]
    public string TOKEN_1 = "";

    [SerializeField]
    public string CHANNEL_NAME_1 = "YOUR_CHANNEL_NAME_1";

    [SerializeField]
    public string TOKEN_2 = "";

    [SerializeField]
    public string CHANNEL_NAME_2 = "YOUR_CHANNEL_NAME_2";


    public Text LogText;
    private Logger _logger;
    private IRtcEngine _rtcEngine = null;
    private AgoraChannel _channel1 = null;
    private AgoraChannel _channel2 = null;
    private const float _offset = 100;

    // Use this for initialization
    void Start()
    {
        if (CheckAppId())
        {
            InitEngine();
            JoinChannel();
        }
    }

    void Update()
    {
        PermissionHelper.RequestMicrophontPermission();
        PermissionHelper.RequestCameraPermission();
    }

    bool CheckAppId()
    {
        _logger = new Logger(LogText);
        return _logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
    }

    void InitEngine()
    {
        _rtcEngine = IRtcEngine.GetEngine(APP_ID);
        _rtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        // If you want to user Multi Channel Video, please call "SetMultiChannleWant to true"
        _rtcEngine.SetMultiChannelWant(true);
      

        _channel1 = _rtcEngine.CreateChannel(CHANNEL_NAME_1);
        _channel2 = _rtcEngine.CreateChannel(CHANNEL_NAME_2);
        _channel1.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        _channel2.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE);

        _channel1.ChannelOnJoinChannelSuccess = Channel1OnJoinChannelSuccessHandler;
        _channel1.ChannelOnLeaveChannel = Channel1OnLeaveChannelHandler;
        _channel1.ChannelOnUserJoined = Channel1OnUserJoinedHandler;
        _channel1.ChannelOnError = Channel1OnErrorHandler;
        _channel2.ChannelOnJoinChannelSuccess = Channel2OnJoinChannelSuccessHandler;
        _channel2.ChannelOnLeaveChannel = Channel2OnLeaveChannelHandler;
        _channel2.ChannelOnUserJoined = Channel2OnUserJoinedHandler;
        _channel2.ChannelOnError = Channel2OnErrorHandler;
    }

    void JoinChannel()
    {
        _rtcEngine.EnableAudio();
        _rtcEngine.EnableVideo();
        _rtcEngine.EnableVideoObserver();
        _channel1.JoinChannel(TOKEN_1, "", 0, new ChannelMediaOptions(true, true));
        _channel2.JoinChannel(TOKEN_2, "", 0, new ChannelMediaOptions(true, true, false, false));
    }

    public void Leave(int channel)
    {
        if (channel == 1)
        {
            _channel1.LeaveChannel();
        }
        if (channel == 2)
        {
            _channel2.LeaveChannel();
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        if (_rtcEngine != null)
        {
            _channel1.LeaveChannel();
            _channel2.LeaveChannel();
            _channel1.ReleaseChannel();
            _channel2.ReleaseChannel();

            _rtcEngine.DisableVideoObserver();
            IRtcEngine.Destroy();
        }
    }

    void Channel1OnJoinChannelSuccessHandler(string channelId, uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("sdk version: ${0}", IRtcEngine.GetSdkVersion()));
        _logger.UpdateLog(string.Format("onJoinChannelSuccess channelId: {0}, uid: {1}, elapsed: {2}", channelId, uid,
            elapsed));
        makeVideoView(channelId, 0);
    }

    void Channel2OnJoinChannelSuccessHandler(string channelId, uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("sdk version: ${0}", IRtcEngine.GetSdkVersion()));
        _logger.UpdateLog(string.Format("onJoinChannelSuccess channelId: {0}, uid: {1}, elapsed: {2}", channelId, uid,
            elapsed));
        makeVideoView(channelId, 0);
    }

    void Channel1OnLeaveChannelHandler(string channelId, RtcStats rtcStats)
    {
        _logger.UpdateLog(string.Format("Channel1OnLeaveChannelHandler channelId: {0}", channelId));
    }

    void Channel2OnLeaveChannelHandler(string channelId, RtcStats rtcStats)
    {
        _logger.UpdateLog(string.Format("Channel1OnLeaveChannelHandler channelId: {0}", channelId));
    }

    void Channel1OnErrorHandler(string channelId, int err, string message)
    {
        _logger.UpdateLog(string.Format("Channel1OnErrorHandler channelId: {0}, err: {1}, message: {2}", channelId, err,
            message));
    }

    void Channel2OnErrorHandler(string channelId, int err, string message)
    {
        _logger.UpdateLog(string.Format("Channel2OnErrorHandler channelId: {0}, err: {1}, message: {2}", channelId, err,
            message));
    }

    void Channel1OnUserJoinedHandler(string channelId, uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("Channel1OnUserJoinedHandler channelId: {0} uid: ${1} elapsed: ${2}", channelId,
            uid, elapsed));
        makeVideoView(channelId, uid);
    }

    void Channel2OnUserJoinedHandler(string channelId, uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("Channel2OnUserJoinedHandler channelId: {0} uid: ${1} elapsed: ${2}", channelId,
            uid, elapsed));
        makeVideoView(channelId, uid);
    }

    void Channel2OnUserOfflineHandler(string channelId, uint uid, USER_OFFLINE_REASON reason)
    {
        _logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid, (int)reason));
        DestroyVideoView(uid);
    }


    private void makeVideoView(string channelId, uint uid)
    {
        string objName = channelId + "_" + uid.ToString();
        GameObject go = GameObject.Find(objName);
        if (!ReferenceEquals(go, null))
        {
            return; // reuse
        }

        // create a GameObject and assign to this new user
        VideoSurface videoSurface = makeImageSurface(objName);
        if (!ReferenceEquals(videoSurface, null))
        {
            // configure videoSurface
            videoSurface.SetForMultiChannelUser(channelId, uid);
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
        // make the object draggable
        go.AddComponent<UIElementDrag>();
        // to be renderered onto
        go.AddComponent<RawImage>();

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

    private void DestroyVideoView(uint uid)
    {
        GameObject go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            Object.Destroy(go);
        }
    }
}
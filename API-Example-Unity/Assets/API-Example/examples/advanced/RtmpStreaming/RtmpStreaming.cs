using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using agora_utilities;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class RtmpStreaming : MonoBehaviour
{
    [SerializeField]
    public string APP_ID = "";

    [SerializeField]
    public string TOKEN = "";

    [SerializeField]
    public string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

    [SerializeField]
    public string RTMP_URL = "";

    public Text LogText;
    private Logger _logger;
    private IRtcEngine _rtcEngine = null;
    private const float _offset = 100;
    private uint _remoteUid = 0;
    private bool _isStreaming = false;

    // Use this for initialization
    void Start()
    {
        if (CheckAppId())
        {
            InitEngine();
            JoinChannel();
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
        return _logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
    }

    void InitEngine()
    {
        _rtcEngine = IRtcEngine.GetEngine(APP_ID);
        _rtcEngine.SetLogFile("log.txt");
        _rtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        _rtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        _rtcEngine.SetVideoEncoderConfiguration(new VideoEncoderConfiguration
        {
            dimensions = new VideoDimensions {width = 720, height = 640},
            frameRate = FRAME_RATE.FRAME_RATE_FPS_24
        });
     
        _rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
        _rtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        _rtcEngine.OnWarning += OnSDKWarningHandler;
        _rtcEngine.OnError += OnSDKErrorHandler;
        _rtcEngine.OnConnectionLost += OnConnectionLostHandler;
        _rtcEngine.OnUserJoined += OnUserJoinedHandler;
        _rtcEngine.OnUserOffline += OnUserOfflineHandler;
        _rtcEngine.OnStreamPublished += OnStreamPublishedHandler;
        _rtcEngine.OnRtmpStreamingStateChanged += OnRtmpStreamingStateChangedHandler;
        _rtcEngine.OnRtmpStreamingEvent += OnRtmpStreamingEventHandler;
    }

    void StartTranscoding(bool ifRemoteUser = false)
    {
        if (_isStreaming && !ifRemoteUser) return;
        if (_isStreaming && ifRemoteUser)
        {
            _rtcEngine.RemovePublishStreamUrl(RTMP_URL);
        }
        
        var lt = new LiveTranscoding();
        lt.videoBitrate = 400;
        lt.videoCodecProfile = VIDEO_CODEC_PROFILE_TYPE.VIDEO_CODEC_PROFILE_HIGH;
        lt.videoGop = 30;
        lt.videoFramerate = 24;
        lt.lowLatency = false;
        lt.audioSampleRate = AUDIO_SAMPLE_RATE_TYPE.AUDIO_SAMPLE_RATE_44100;
        lt.audioBitrate = 48;
        lt.audioChannels = 1;
        lt.audioCodecProfile = AUDIO_CODEC_PROFILE_TYPE.AUDIO_CODEC_PROFILE_LC_AAC;
        lt.liveStreamAdvancedFeatures = new LiveStreamAdvancedFeature[0];
        
        var localUesr = new TranscodingUser()
        {
            uid = 0,
            x = 0,
            y = 0,
            width = 360,
            height = 640,
            audioChannel = 0,
            alpha = 1.0,
        };
        
        if (ifRemoteUser)
        {
            var remoteUser = new TranscodingUser()
            {
                uid = _remoteUid,
                x = 360,
                y = 0,
                width = 360,
                height = 640,
                audioChannel = 0,
                alpha = 1.0,
            };
            lt.userCount = 2;
            lt.width = 720;
            lt.height = 640;
            lt.transcodingUsers = new[] {localUesr, remoteUser};
        }
        else
        {
            lt.userCount = 1;
            lt.width = 360;
            lt.height = 640;
            lt.transcodingUsers = new[] {localUesr};
        }
        
        _rtcEngine.SetLiveTranscoding(lt);

        var rc = _rtcEngine.AddPublishStreamUrl(RTMP_URL, true);
         _logger.UpdateLog(string.Format("AddPublishStreamUrl: {0}", rc));
    }

    void JoinChannel()
    {
        _rtcEngine.EnableAudio();
        _rtcEngine.EnableVideo();
        _rtcEngine.EnableVideoObserver();
        _rtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
    }

    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("sdk version: ${0}", IRtcEngine.GetSdkVersion()));
        _logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName,
            uid, elapsed));
        makeVideoView(0);
        StartTranscoding();
    }

    void OnLeaveChannelHandler(RtcStats stats)
    {
        _logger.UpdateLog("OnLeaveChannelSuccess");
        DestroyVideoView(0);
    }

    void OnUserJoinedHandler(uint uid, int elapsed)
    {
        if (_remoteUid == 0) _remoteUid = uid;
        StartTranscoding(true);
        _logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        makeVideoView(uid);
    }

    void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        _remoteUid = 0;
        _logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid, (int) reason));
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
        _logger.UpdateLog("OnConnectionLost ");
    }

    void OnStreamPublishedHandler(string url, int error)
    {
        _logger.UpdateLog(string.Format("OnStreamPublished url: {0}, error : {1}", url, error));
    }

    void OnRtmpStreamingStateChangedHandler(string url, RTMP_STREAM_PUBLISH_STATE state, RTMP_STREAM_PUBLISH_ERROR_TYPE code)
    {
        _logger.UpdateLog(string.Format("OnRtmpStreamingStateChanged url: {0}, state: {1}, code: {2}", url, state,
            code));
    }

    void OnRtmpStreamingEventHandler(string url, RTMP_STREAMING_EVENT code)
    {
        _logger.UpdateLog(string.Format("OnRtmpStreamingEvent url: {0}, code: {1}", url, code));
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        if (_rtcEngine != null)
        {
            _rtcEngine.RemovePublishStreamUrl(RTMP_URL);
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;

public class AgoraVideoRawData : MonoBehaviour
{

    [SerializeField]
    public string APP_ID = "YOUR_APPID";

    [SerializeField]
    public string TOKEN = "";

    [SerializeField]
    public string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

    public Text LogText;
    private Logger _logger;
    private IRtcEngine _rtcEngine = null;
    private VideoRawDataManager _videoRawDataManager;

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
        _videoRawDataManager = VideoRawDataManager.GetInstance(_rtcEngine);
        _videoRawDataManager.SetOnCaptureVideoFrameCallback(OnCaptureVideoFrameHandler);
        _videoRawDataManager.SetOnRenderVideoFrameCallback(OnRenderVideoFrameHandler);
     
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
        _rtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
    }

    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("sdk version: ${0}", IRtcEngine.GetSdkVersion()));
        _logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
    }

    void OnLeaveChannelHandler(RtcStats stats)
    {
        _logger.UpdateLog("OnLeaveChannelSuccess");
    }

    void OnUserJoinedHandler(uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
    }

    void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        _logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid, (int)reason));
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

    void OnCaptureVideoFrameHandler(VideoFrame videoFrame)
    {
        //logger.UpdateLog(string.Format("OnCaptureVideoFrameHandler  width: ${1}, height: ${2}", videoFrame.width, videoFrame.height));
        Debug.Log("OnCaptureVideoFrame--------------");
    }

    void OnRenderVideoFrameHandler(uint uid, VideoFrame videoFrame)
    {
        //logger.UpdateLog(string.Format("OnRenderVideoFrameHandler uid: ${0}, width: ${1}, height: ${2}", uid, videoFrame.width, videoFrame.height));
        Debug.Log("OnRenderVideoFrameHandler-----------");
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit1");
        if (_rtcEngine != null)
        {
            _rtcEngine.LeaveChannel();
            _rtcEngine.DisableVideoObserver();
            IRtcEngine.Destroy();
        }
        Debug.Log("OnApplicationQuit2");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;

public class AgoraVideoRawData : MonoBehaviour {

    [SerializeField]
    private string APP_ID = "YOUR_APPID";

    [SerializeField]
    private string TOKEN = "";

    [SerializeField]
    private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";
    public Text logText;
	private Logger logger;
	private IRtcEngine mRtcEngine = null;
    private const float Offset = 100;
    private static string channelName = "Agora_Channel";
    private VideoRawDataManager videoRawDataManager;

	void Start () {
		CheckAppId();
        InitEngine();
        JoinChannel();	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void CheckAppId()
    {
        logger = new Logger(logText);
        logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
    }

	void InitEngine()
	{
        mRtcEngine = IRtcEngine.GetEngine(APP_ID);
		mRtcEngine.SetLogFile("log.txt");

		mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
		mRtcEngine.SetClientRole(CLIENT_ROLE.BROADCASTER);
		videoRawDataManager = VideoRawDataManager.GetInstance(mRtcEngine);
		videoRawDataManager.SetOnCaptureVideoFrameCallback(OnCaptureVideoFrameHandler);
		videoRawDataManager.SetOnRenderVideoFrameCallback(OnRenderVideoFrameHandler);
		mRtcEngine.EnableAudio();
		mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();
        mRtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
        mRtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        mRtcEngine.OnWarning += OnSDKWarningHandler;
        mRtcEngine.OnError += OnSDKErrorHandler;
        mRtcEngine.OnConnectionLost += OnConnectionLostHandler;
        mRtcEngine.OnUserJoined += OnUserJoinedHandler;
        mRtcEngine.OnUserOffline += OnUserOfflineHandler;	
	}

    void JoinChannel()
    {
        mRtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
    }

	void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        logger.UpdateLog(string.Format("sdk version: ${0}", IRtcEngine.GetSdkVersion()));
        logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
    }

    void OnLeaveChannelHandler(RtcStats stats)
    {
        logger.UpdateLog("OnLeaveChannelSuccess");
    }

    void OnUserJoinedHandler(uint uid, int elapsed)
    {
        logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
    }

    void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid, (int)reason));
    }

    void OnSDKWarningHandler(int warn, string msg)
    {
        logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
    }
    
    void OnSDKErrorHandler(int error, string msg)
    {
        logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
    }
    
    void OnConnectionLostHandler()
    {
        logger.UpdateLog(string.Format("OnConnectionLost "));
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
        if (mRtcEngine != null)
        {
			mRtcEngine.LeaveChannel();
			mRtcEngine.DisableVideoObserver();
            IRtcEngine.Destroy();
        }
        Debug.Log("OnApplicationQuit2");
    }
}

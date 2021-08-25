using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using Logger = agora.util.Logger;

public class AgoraVideoRawData : MonoBehaviour {

    [SerializeField]
    private string APP_ID = "YOUR_APPID";

    [SerializeField]
    private string TOKEN = "";

    [SerializeField]
    private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";
    public Text logText;
	private Logger logger;
	internal IAgoraRtcEngine mRtcEngine;
    private const float Offset = 100;
    private static string channelName = "Agora_Channel";
    //private IAgoraRtcVideoFrameObserver videoFrameObserver;

	void Start () 
	{
		CheckAppId();
        InitEngine();
        JoinChannel();	
	}
	
	// Update is called once per frame
	void Update () 
	{
		
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
		mRtcEngine.RegisterVideoFrameObserver(new VideoFrameObserver(this));
		mRtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
		mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
		mRtcEngine.EnableAudio();
		mRtcEngine.EnableVideo();
	}

    void JoinChannel()
    {
	    mRtcEngine.JoinChannel(TOKEN, CHANNEL_NAME, "");
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit1");
        if (mRtcEngine != null)
        {
	        mRtcEngine.UnRegisterVideoFrameObserver();
			mRtcEngine.LeaveChannel();
			mRtcEngine.Dispose();
        }
        Debug.Log("OnApplicationQuit2");
    }

    internal class UserEventHandler : IAgoraRtcEngineEventHandler
    {
	    private readonly AgoraVideoRawData _agoraVideoRawData;

	    internal UserEventHandler(AgoraVideoRawData agoraVideoRawData)
	    {
		    _agoraVideoRawData = agoraVideoRawData;
	    }
	    
	    public override void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
	    {
		    _agoraVideoRawData.logger.UpdateLog(string.Format("sdk version: ${0}", _agoraVideoRawData.mRtcEngine.GetVersion()));
		    _agoraVideoRawData.logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
	    }

	    public override void OnLeaveChannel(RtcStats stats)
	    {
		    _agoraVideoRawData.logger.UpdateLog("OnLeaveChannelSuccess");
	    }

	    public override void OnUserJoined(uint uid, int elapsed)
	    {
		    _agoraVideoRawData.logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
	    }

	    public override void OnUserOffline(uint uid, USER_OFFLINE_REASON_TYPE reason)
	    {
		    _agoraVideoRawData.logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid, (int)reason));
	    }

	    public override void OnWarning(int warn, string msg)
	    {
		    _agoraVideoRawData.logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
	    }
    
	    public override void OnError(int error, string msg)
	    {
		    _agoraVideoRawData.logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
	    }

	    public override void OnConnectionLost()
	    {
		    _agoraVideoRawData.logger.UpdateLog(string.Format("OnConnectionLost "));
	    }
    }
    
    internal class VideoFrameObserver : IAgoraRtcVideoFrameObserver
    {
	    private readonly AgoraVideoRawData _agoraVideoRawData;

	    internal VideoFrameObserver(AgoraVideoRawData agoraVideoRawData)
	    {
		    _agoraVideoRawData = agoraVideoRawData;
	    }
	    
	    public override bool OnCaptureVideoFrame(VideoFrame videoFrame)
	    {
		    //logger.UpdateLog(string.Format("OnCaptureVideoFrameHandler  width: ${1}, height: ${2}", videoFrame.width, videoFrame.height));
		    Debug.Log("OnCaptureVideoFrame--------------");
		    return true;
	    }

	    public override bool OnRenderVideoFrame(uint uid, VideoFrame videoFrame)
	    {
		    //logger.UpdateLog(string.Format("OnRenderVideoFrameHandler uid: ${0}, width: ${1}, height: ${2}", uid, videoFrame.width, videoFrame.height));
		    Debug.Log("OnRenderVideoFrameHandler-----------");
		    return true;
	    }
	    
    }
}

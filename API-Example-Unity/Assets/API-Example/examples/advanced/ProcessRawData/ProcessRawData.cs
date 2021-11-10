using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using agora.rtc;
using agora.util;
using Logger = agora.util.Logger;

public class ProcessRawData : MonoBehaviour {

    [FormerlySerializedAs("APP_ID")] [SerializeField]
	private string appID = "";

	[FormerlySerializedAs("TOKEN")] [SerializeField]
	private string token = "";

	[FormerlySerializedAs("CHANNEL_NAME")] [SerializeField]
	private string channelName = "YOUR_CHANNEL_NAME";

    public Text logText;
	private Logger Logger;
	private IAgoraRtcEngine _mRtcEngine;

    private const float Offset = 100;
    //private static string channelName = "Agora_Channel";
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
        Logger = new Logger(logText);
        Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
    }

	void InitEngine()
	{
		_mRtcEngine = agora.rtc.AgoraRtcEngine.CreateAgoraRtcEngine();
		UserEventHandler handler = new UserEventHandler(this);
		RtcEngineContext context = new RtcEngineContext(handler, appID, null, true, 
									CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
									AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
		_mRtcEngine.Initialize(context);
		_mRtcEngine.InitEventHandler(handler);
	}

    void JoinChannel()
    {
		_mRtcEngine.RegisterVideoFrameObserver(new VideoFrameObserver(this));
		_mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
		_mRtcEngine.EnableAudio();
		_mRtcEngine.EnableVideo();
	    _mRtcEngine.JoinChannel(token, channelName, "");
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit1");
        if (_mRtcEngine != null)
        {
	        _mRtcEngine.UnRegisterVideoFrameObserver();
			_mRtcEngine.LeaveChannel();
			_mRtcEngine.Dispose();
        }
        Debug.Log("OnApplicationQuit2");
    }

    internal class UserEventHandler : IAgoraRtcEngineEventHandler
    {
	    private readonly ProcessRawData _agoraVideoRawData;

	    internal UserEventHandler(ProcessRawData agoraVideoRawData)
	    {
		    _agoraVideoRawData = agoraVideoRawData;
	    }
	    
	    public override void OnWarning(int warn, string msg)
        {
            _agoraVideoRawData.Logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _agoraVideoRawData.Logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            // _agoraVideoRawData.Logger.UpdateLog(string.Format("sdk version: ${0}",
            //     _agoraVideoRawData.AgoraRtcEngine.GetVersion()));
            _agoraVideoRawData.Logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", 
                                connection.channelId, connection.localUid, elapsed));
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _agoraVideoRawData.Logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _agoraVideoRawData.Logger.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _agoraVideoRawData.Logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _agoraVideoRawData.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _agoraVideoRawData.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int) reason));
        }
    }
     
    internal class VideoFrameObserver : IAgoraRtcVideoFrameObserver
    {
	    private readonly ProcessRawData _agoraVideoRawData;

	    internal VideoFrameObserver(ProcessRawData agoraVideoRawData)
	    {
		    _agoraVideoRawData = agoraVideoRawData;
	    }
	    
	    public override bool OnCaptureVideoFrame(VideoFrame videoFrame, VideoSourceType source_type)
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

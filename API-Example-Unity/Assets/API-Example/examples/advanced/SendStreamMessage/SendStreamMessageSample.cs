using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;

public class SendStreamMessageSample : MonoBehaviour 
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

    // Start is called before the first frame update
    void Start()
    {
        if (CheckAppId())
        {
            InitRtcEngine();
            JoinChannel();
        }
    }

    void Update() 
    {
        PermissionHelper.RequestMicrophontPermission();
    }

    bool CheckAppId()
    {
        _logger = new Logger(LogText);
        return _logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in Canvas!!!!!");
    }

    void InitRtcEngine()
    {
        _rtcEngine = IRtcEngine.GetEngine(APP_ID);
        _rtcEngine.SetLogFile("log.txt");
        _rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
        _rtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        _rtcEngine.OnWarning += OnSDKWarningHandler;
        _rtcEngine.OnError += OnSDKErrorHandler;
        _rtcEngine.OnConnectionLost += OnConnectionLostHandler;
        _rtcEngine.OnStreamMessage += OnStreamMessageHandler;
        _rtcEngine.OnUserJoined += OnUserJoinedHandler;
        _rtcEngine.OnUserOffline += OnUserOfflineHandler;
    }

    void JoinChannel() 
    {
        _rtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
    }

	void SendMessage(string message)
    {
        int streamId = 0;
		DataStreamConfig config = new DataStreamConfig();
        config.syncWithAudio = false;
        config.ordered = true;
        streamId = _rtcEngine.CreateDataStream(config);
        if (streamId < 0)
        {
            _logger.UpdateLog("CreateDataStream failed!");
            return;
        }

        byte[] byteArray = System.Text.Encoding.Default.GetBytes(message);
        var rc = _rtcEngine.SendStreamMessage(streamId, byteArray);
        this._logger.UpdateLog("SendStreamMessage :" + rc);
    }

    void OnLeaveBtnClick() 
    {
        _rtcEngine.LeaveChannel();
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        if (_rtcEngine != null)
        {
            IRtcEngine.Destroy();
        }
    }

    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("sdk version: {0}", IRtcEngine.GetSdkVersion()));
        _logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
        SendMessage("Hello Agora!");
    }

    void OnLeaveChannelHandler(RtcStats stats)
    {
        _logger.UpdateLog("OnLeaveChannelSuccess");
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

    void OnStreamMessageHandler(uint userId, int streamId, byte[] data, int length)
    {
        string stringstr = System.Text.Encoding.Default.GetString (data);
        _logger.UpdateLog(string.Format("userId: {0}, streamId: {1}, message: {2}", userId, streamId, stringstr));
    }
    
    void OnUserJoinedHandler(uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        SendMessage("Hello Agora!");
    }

    void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        _logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid, (int)reason));

    }
}

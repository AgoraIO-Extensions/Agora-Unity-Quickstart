using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;

public class HelloAgoraVoice : MonoBehaviour
{
    [SerializeField]
    private string APP_ID = "YOUR_APPID";
    public InputField ChannelInputField;
    public Button JoinBtn;
    public Button LeaveBtn;
    public Text logText;
    private Logger logger;
    private IRtcEngine mRtcEngine = null;

    // Start is called before the first frame update
    void Start()
    {
        CheckAppId();
        InitUI();
        InitRtcEngine();
    }

    void Update() 
    {
        PermissionHelper.RequestMicrophontPermission();
    }

    void CheckAppId()
    {
        logger = new Logger(logText);
        logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in Canvas!!!!!");
    }

    void InitUI()
    {
        JoinBtn.onClick.AddListener(OnJoinBtnClick);
        LeaveBtn.onClick.AddListener(OnLeaveBtnClick);
    }

    void InitRtcEngine()
    {
        mRtcEngine = IRtcEngine.GetEngine(APP_ID);
        mRtcEngine.SetLogFile("log.txt");
        mRtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
        mRtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        mRtcEngine.OnWarning += OnSDKWarningHandler;
        mRtcEngine.OnError += OnSDKErrorHandler;
        mRtcEngine.OnConnectionLost += OnConnectionLostHandler;
    }

    void OnJoinBtnClick() 
    {
        logger.DebugAssert(ChannelInputField.text != null && ChannelInputField.text.Length > 0, "Please input your channelName!");
        mRtcEngine.JoinChannel(ChannelInputField.text, "", 0);
    }

    void OnLeaveBtnClick() 
    {
        mRtcEngine.LeaveChannel();
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();
        }
    }

    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        logger.UpdateLog(string.Format("sdk version: {0}", IRtcEngine.GetSdkVersion()));
        logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
    }

    void OnLeaveChannelHandler(RtcStats stats)
    {
        logger.UpdateLog("OnLeaveChannelSuccess");
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
}

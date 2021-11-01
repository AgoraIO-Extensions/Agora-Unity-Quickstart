using System.Text;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;

public class EncryptionSample : MonoBehaviour
{
    [SerializeField] private string APP_ID = "YOUR_APPID";

    [SerializeField] private string TOKEN = "";

    [SerializeField] private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

    [SerializeField] private ENCRYPTION_MODE ENCRYPTION_MODE = ENCRYPTION_MODE.AES_128_GCM2;

    [SerializeField] private string SECRET = "";

    public Text logText;
    private Logger logger;
    private IRtcEngine mRtcEngine = null;

    // Start is called before the first frame update
    void Start()
    {
        CheckAppId();
        InitRtcEngine();
        SetEncryption();
        JoinChannel();
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

    byte[] GetEncryptionSaltFromServer()
    {
        return Encoding.UTF8.GetBytes("EncryptionKdfSaltInBase64Strings");
    }

    void SetEncryption()
    {
        var config = new EncryptionConfig
        {
            encryptionMode = ENCRYPTION_MODE,
            encryptionKey = SECRET,
            encryptionKdfSalt = GetEncryptionSaltFromServer()
        };
        logger.UpdateLog(string.Format("encryption mode: {0} secret: {1}", ENCRYPTION_MODE, SECRET));
        mRtcEngine.EnableEncryption(true, config);
    }

    void JoinChannel()
    {
        mRtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
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
        logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName,
            uid, elapsed));
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

internal class Encording
{
}
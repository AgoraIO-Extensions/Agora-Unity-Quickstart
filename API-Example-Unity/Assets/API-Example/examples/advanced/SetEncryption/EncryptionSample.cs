using System.Text;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;

public class EncryptionSample : MonoBehaviour
{
    [SerializeField]
    public string APP_ID = "YOUR_APPID";

    [SerializeField]
    public string TOKEN = "";

    [SerializeField]
    public string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

    [SerializeField]
    public ENCRYPTION_MODE ENCRYPTION_MODE = ENCRYPTION_MODE.AES_128_GCM2;

    [SerializeField]
    public string SECRET = "";

    public Text LogText;
    private Logger _logger;
    private IRtcEngine _rtcEngine = null;

    // Start is called before the first frame update
    void Start()
    {
        if (CheckAppId())
        {
            InitRtcEngine();
            SetEncryption();
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
        _logger.UpdateLog(string.Format("encryption mode: {0} secret: {1}", ENCRYPTION_MODE, SECRET));
        _rtcEngine.EnableEncryption(true, config);
    }

    void JoinChannel()
    {
        _rtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
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
        _logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName,
            uid, elapsed));
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
}

internal class Encording
{
}
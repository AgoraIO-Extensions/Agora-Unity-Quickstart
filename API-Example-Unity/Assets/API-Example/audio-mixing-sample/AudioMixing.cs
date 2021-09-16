using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;

public class AudioMixing : MonoBehaviour
{
    [SerializeField]
    private string APP_ID = "YOUR_APPID";
    
    [SerializeField]
    private string TOKEN = "";

    [SerializeField]
    private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";
    public Text logText;
    private Logger logger;
    private IRtcEngine mRtcEngine = null;
    private AgoraChannel channel = null;
    private IAudioPlaybackDeviceManager manager = null;

    // Start is called before the first frame update
    void Start()
    {
        CheckAppId();
        InitRtcEngine();
        //StartAudioPlaybackTest();
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
        //mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY,
        //    AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
        //mRtcEngine.EnableAudio();
        mRtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
        mRtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        mRtcEngine.OnWarning += OnSDKWarningHandler;
        mRtcEngine.OnError += OnSDKErrorHandler;
        mRtcEngine.OnConnectionLost += OnConnectionLostHandler;
        
    }

    void JoinChannel()
    {
        mRtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
    }

    void StartAudioMixing()
    {
        string path = "https://agoracdn.s3.us-west-1.amazonaws.com/videos/Agora.io-Interactions.mp4";
        
#if UNITY_ANDROID
        string localPath = "/assets/audio/DESERTMUSIC.wav";
#else
        string localPath = Application.streamingAssetsPath + "/Audio/" + "DESERTMUSIC.wav";
#endif
        logger.UpdateLog(string.Format("the audio file path: {0}", localPath));
        mRtcEngine.StartAudioMixing(localPath, true, false, -1, 0);
    }

    void StartAudioPlaybackTest()
    {
        manager = mRtcEngine.GetAudioPlaybackDeviceManager();
        string fileStreamName =
            Application.streamingAssetsPath + "/Audio/" + "DESERTMUSIC.wav";
        string path = "https://agoracdn.s3.us-west-1.amazonaws.com/videos/Agora.io-Interactions.mp4";
        manager.CreateAAudioPlaybackDeviceManager();
        manager.StartAudioPlaybackDeviceTest(fileStreamName);
    }
    
    public void PlayEffectTest () {
        IAudioEffectManager effectManager = mRtcEngine.GetAudioEffectManager ();
#if UNITY_ANDROID
        string localPath = "/assets/audio/DESERTMUSIC.wav";
#else
        string localPath = Application.streamingAssetsPath + "/Audio/" + "DESERTMUSIC.wav";
#endif
        
        effectManager.PlayEffect (1, localPath, 1, 1.0, 0, 100, true);
    }

    void OnLeaveBtnClick() 
    {
        mRtcEngine.LeaveChannel();
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        if (manager != null)
        {
            manager.StopAudioPlaybackDeviceTest();
            manager.ReleaseAAudioPlaybackDeviceManager();
        }
        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();
        }
    }

    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        logger.UpdateLog(string.Format("sdk version: {0}", IRtcEngine.GetSdkVersion()));
        logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
        StartAudioMixing();
        //PlayEffectTest();
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

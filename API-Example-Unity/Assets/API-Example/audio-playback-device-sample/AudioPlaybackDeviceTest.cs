using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;

public class AudioPlaybackDeviceTest : MonoBehaviour {

	[SerializeField]
    string APP_ID = "YOUR_APPID";
    
    [SerializeField]
    string TOKEN = "";

    [SerializeField]
    string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

    string localPath = "";

    public Text logText;
    private Logger logger;
    private IRtcEngine mRtcEngine = null;
    private IAudioPlaybackDeviceManager manager = null;

    // Start is called before the first frame update
    void Start()
    {
        CheckAppId();
        InitRtcEngine();
        SetupUI();
    }

    void Update() 
    {
        //PermissionHelper.RequestMicrophontPermission();
    }

    void CheckAppId()
    {
        //logger = new Logger(logText);
        //logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in Canvas!!!!!");
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
    void SetupUI()
    { 
        TestingButton = GameObject.Find("TestButton").GetComponent<Button>();
        TestingButton.onClick.AddListener(HandleAudioTesingButton);

#if UNITY_ANDROID && !UNITY_EDITOR
        localPath = "/assets/audio/DESERTMUSIC.wav";
#else
        localPath = Application.streamingAssetsPath + "/audio/" + "DESERTMUSIC.wav";
#endif
        //logger.UpdateLog(string.Format("the audio file path: {0}", localPath));

        EnableUI(true);
    }

    void EnableUI(bool enable)
    { 
        TestingButton.enabled = enable;
    }

    void JoinChannel()
    {
        mRtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
    }

    void StartAudioPlaybackTest()
    {
        manager = mRtcEngine.GetAudioPlaybackDeviceManager();
        manager.CreateAAudioPlaybackDeviceManager();
        manager.StartAudioPlaybackDeviceTest(localPath);
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
        //logger.UpdateLog(string.Format("sdk version: {0}", IRtcEngine.GetSdkVersion()));
        //logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
    }

    void OnLeaveChannelHandler(RtcStats stats)
    {
        //logger.UpdateLog("OnLeaveChannelSuccess");
    }

    void OnSDKWarningHandler(int warn, string msg)
    {
        //logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
    }
    
    void OnSDKErrorHandler(int error, string msg)
    {
        //logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
    }
    
    void OnConnectionLostHandler()
    {
        //logger.UpdateLog(string.Format("OnConnectionLost "));
    }

    bool _isTesting = false;
    Button TestingButton { get; set; }
    void HandleAudioTesingButton()
    {
        if (_isTesting)
        {
            manager.StopAudioPlaybackDeviceTest();
        }
        else
        {
            StartAudioPlaybackTest();
        }

        _isTesting = !_isTesting;
        TestingButton.GetComponentInChildren<Text>().text = (_isTesting ? "Stop Test" : "Start Test");
    }
    
}

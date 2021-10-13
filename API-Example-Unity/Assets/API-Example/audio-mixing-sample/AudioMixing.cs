using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;

public class AudioMixing : MonoBehaviour
{
    [SerializeField]
    string APP_ID = "YOUR_APPID";
    
    [SerializeField]
    string TOKEN = "";

    [SerializeField]
    string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

    [SerializeField]
    string Sound_URL = "";

    [SerializeField]
    Toggle loopbackToggle;

    string localPath = "";

    public Text logText;
    private Logger logger;
    private IRtcEngine mRtcEngine = null;
    private IAudioPlaybackDeviceManager manager = null;
    const string SampleAudioSubpath = "/audio/Agora.io-Interactions.mp3";

    // Start is called before the first frame update
    void Start()
    {
        CheckAppId();
        InitRtcEngine();
        SetupUI();
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
    void SetupUI()
    { 
        MixingButton = GameObject.Find("MixButton").GetComponent<Button>();
        MixingButton.onClick.AddListener(HandleAudioMixingButton);
        EffectButton = GameObject.Find("EffectButton").GetComponent<Button>();
        EffectButton.onClick.AddListener(HandleEffectButton);
        urlToggle = GameObject.Find("Toggle").GetComponent<Toggle>();
        urlToggle.onValueChanged.AddListener(OnToggle);
        _useURL = urlToggle.isOn;

#if UNITY_ANDROID && !UNITY_EDITOR
        // On Android, the StreamingAssetPath is just accessed by /assets instead of Application.streamingAssetPath
        localPath = "/assets" + SampleAudioSubpath;
#else
        localPath = Application.streamingAssetsPath + SampleAudioSubpath;
#endif
        logger.UpdateLog(string.Format("the audio file path: {0}", localPath));

        EnableUI(false); // enable it after joining
    }

    void EnableUI(bool enable)
    { 
        MixingButton.enabled = enable;
        EffectButton.enabled = enable;
    }

    void JoinChannel()
    {
        mRtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
    }

    #region -- Test Control logic ---
    void StartAudioMixing()
    {
        Debug.Log("Playing with " + ( _useURL? "URL" : "local file") );
        bool bLoopback = loopbackToggle.isOn;

        mRtcEngine.StartAudioMixing( filePath: _useURL? Sound_URL : localPath, 
	                                 loopback: bLoopback, 
				                      replace: true, 
				                        cycle: -1, 
					                 startPos: 0);
    }

    void StartAudioPlaybackTest()
    {
        manager = mRtcEngine.GetAudioPlaybackDeviceManager();
        manager.CreateAAudioPlaybackDeviceManager();
        manager.StartAudioPlaybackDeviceTest(localPath);
    }
    
    void PlayEffectTest () {
        Debug.Log("Playing with " + ( _useURL? "URL" : "local file") );
        IAudioEffectManager effectManager = mRtcEngine.GetAudioEffectManager ();
        effectManager.PlayEffect (1, _useURL? Sound_URL : localPath, 1, 1.0, 0, 100, true);
    }

    void StopEffectTest() {
        IAudioEffectManager effectManager = mRtcEngine.GetAudioEffectManager();
        effectManager.StopAllEffects();
    }

    #endregion

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

    #region -- SDK callbacks ----
    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        logger.UpdateLog(string.Format("sdk version: {0}", IRtcEngine.GetSdkVersion()));
        logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
        EnableUI(true);
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

    #endregion

    #region -- Application UI Logic ---
    bool _isMixing = false;
    Button MixingButton { get; set; }
    void HandleAudioMixingButton()
    {
        if (_effectOn)
        {
            logger.UpdateLog("Testing Effect right now, can't play effect...");
            return;
        }

        if (_isMixing)
        {
            mRtcEngine.StopAudioMixing();
        }
        else
        {
            StartAudioMixing();
        }

        _isMixing = !_isMixing;
        MixingButton.GetComponentInChildren<Text>().text = (_isMixing ? "Stop Mixing" : "Start Mixing");
    }


    bool _effectOn = false;
    Button EffectButton { get; set; }
    void HandleEffectButton()
    {
        if (_isMixing)
        { 
	        logger.UpdateLog("Testing Mixing right now, can't play effect...");
            return;
	    }

        if (_effectOn)
        {
            StopEffectTest();
        }
        else
        {
            PlayEffectTest();
        }

        _effectOn = !_effectOn;
        EffectButton.GetComponentInChildren<Text>().text = (_effectOn ? "Stop Effect" : "Play Effect");
    }

    bool _useURL { get; set;}
    Toggle urlToggle { get; set; }
    void OnToggle(bool enable)
    { 
        _useURL = enable;
    }

    #endregion
}

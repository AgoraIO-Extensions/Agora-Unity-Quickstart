using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;

public class AudioMixing : MonoBehaviour
{
    [SerializeField]
    public string APP_ID = "YOUR_APPID";
    
    [SerializeField]
    public string TOKEN = "";

    [SerializeField]
    public string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

    [SerializeField]
    public string Sound_URL = "";

    [SerializeField]
    public Toggle loopbackToggle;

    private string _localPath = "";

    public Text LogText;
    private Logger _logger;
    private IRtcEngine _rtcEngine = null;
    private IAudioPlaybackDeviceManager _manager = null;
    private const string _sampleAudioSubpath = "/audio/Agora.io-Interactions.mp3";

    // Start is called before the first frame update
    void Start()
    {
        if (CheckAppId())
        {
            InitRtcEngine();
            SetupUI();
            //StartAudioPlaybackTest();
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
        //mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY,
        //    AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
        //mRtcEngine.EnableAudio();
        _rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
        _rtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        _rtcEngine.OnWarning += OnSDKWarningHandler;
        _rtcEngine.OnError += OnSDKErrorHandler;
        _rtcEngine.OnConnectionLost += OnConnectionLostHandler;
        
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
        _localPath = "/assets" + SampleAudioSubpath;
#else
        _localPath = Application.streamingAssetsPath + _sampleAudioSubpath;
#endif
        _logger.UpdateLog(string.Format("the audio file path: {0}", _localPath));

        EnableUI(false); // enable it after joining
    }

    void EnableUI(bool enable)
    { 
        MixingButton.enabled = enable;
        EffectButton.enabled = enable;
    }

    void JoinChannel()
    {
        _rtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
    }

    #region -- Test Control logic ---
    void StartAudioMixing()
    {
        Debug.Log("Playing with " + ( _useURL? "URL" : "local file") );
        bool bLoopback = loopbackToggle.isOn;

        _rtcEngine.StartAudioMixing( filePath: _useURL? Sound_URL : _localPath, 
	                                 loopback: bLoopback, 
				                      replace: true, 
				                        cycle: -1, 
					                 startPos: 0);
    }

    void StartAudioPlaybackTest()
    {
        _manager = _rtcEngine.GetAudioPlaybackDeviceManager();
        _manager.CreateAAudioPlaybackDeviceManager();
        _manager.StartAudioPlaybackDeviceTest(_localPath);
    }
    
    void PlayEffectTest () {
        Debug.Log("Playing with " + ( _useURL? "URL" : "local file") );
        IAudioEffectManager effectManager = _rtcEngine.GetAudioEffectManager ();
        effectManager.PlayEffect (1, _useURL? Sound_URL : _localPath, 1, 1.0, 0, 100, true);
    }

    void StopEffectTest() {
        IAudioEffectManager effectManager = _rtcEngine.GetAudioEffectManager();
        effectManager.StopAllEffects();
    }

    #endregion

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        if (_manager != null)
        {
            _manager.StopAudioPlaybackDeviceTest();
            _manager.ReleaseAAudioPlaybackDeviceManager();
        }
        if (_rtcEngine != null)
        {
            IRtcEngine.Destroy();
        }
    }

    #region -- SDK callbacks ----
    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("sdk version: {0}", IRtcEngine.GetSdkVersion()));
        _logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
        EnableUI(true);
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

    #endregion

    #region -- Application UI Logic ---
    bool _isMixing = false;
    Button MixingButton { get; set; }
    void HandleAudioMixingButton()
    {
        if (_effectOn)
        {
            _logger.UpdateLog("Testing Effect right now, can't play effect...");
            return;
        }

        if (_isMixing)
        {
            _rtcEngine.StopAudioMixing();
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
	        _logger.UpdateLog("Testing Mixing right now, can't play effect...");
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

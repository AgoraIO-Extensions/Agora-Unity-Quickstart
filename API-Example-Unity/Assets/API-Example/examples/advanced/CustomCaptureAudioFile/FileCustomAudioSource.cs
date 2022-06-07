using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using RingBuffer;

/// <summary>
///   This is another example using CustomAudioSource with an audio file.
///   The code will load the sample file MyPCMByteFile from Assets/StreamingAssets
///   and send out AudioFrames based on the bytes from that file in an infinite loop.
///
///    Note file should be in PCM16 format.  
///      .wav file should also work with the first 44 header bytes treated as noise.
/// </summary>
public class FileCustomAudioSource : MonoBehaviour
{
    [SerializeField]
    private string APP_ID = "YOUR_APPID";

    [SerializeField]
    private string TOKEN = "";

    [SerializeField]
    private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

    public Text LogText;

    private Logger _logger;
    private IRtcEngine _rtcEngine = null;


    private const int CHANNEL = 1;
    private const int SAMPLE_RATE = 48000;
    private const int PUSH_FREQ_PER_SEC = 100;

    // Copy this file to Assets/StreamingAssets
    [Tooltip("Load file from StreamingAssets folder")]
    [SerializeField]
    public string MyPCMByteFile = "audio/myaudio.bytes";


    private RingBuffer<byte> _audioBuffer;

    private Thread _pushAudioFrameThread;
    private bool _pushAudioFrameThreadSignal = false;
    private bool _startSignal = false;

    private byte[] _mediaBuffer;

    // Use this for initialization
    void Start()
    {
        if (CheckAppId())
        {
            InitRtcEngine();
            JoinChannel();
            LoadSound();
            StartPushAudioFrame();
        }
    }

    // Update is called once per frame
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
        _rtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
        _rtcEngine.SetExternalAudioSource(true, SAMPLE_RATE, CHANNEL);
        _rtcEngine.SetLogFile("log.txt");
       
        _rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
        _rtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        _rtcEngine.OnWarning += OnSDKWarningHandler;
        _rtcEngine.OnError += OnSDKErrorHandler;
        _rtcEngine.OnConnectionLost += OnConnectionLostHandler;
    }

    void LoadSound()
    {
        var bufferLength = SAMPLE_RATE / PUSH_FREQ_PER_SEC * CHANNEL * 10000;
        _audioBuffer = new RingBuffer<byte>(bufferLength);

#if UNITY_ANDROID && !UNITY_EDITOR
        // On Android, the StreamingAssetPath is just accessed by /assets instead of Application.streamingAssetPath
        string fileStreamName = "/assets/" + MyPCMByteFile;
#else
        string fileStreamName = Application.streamingAssetsPath + "/" + MyPCMByteFile;
#endif
        _mediaBuffer = File.ReadAllBytes(fileStreamName);
        _logger.UpdateLog("Read " + _mediaBuffer.Length + " bytes from " + fileStreamName);
        AppendRingBuffer();
    }

    void StartPushAudioFrame()
    {
        _pushAudioFrameThreadSignal = true;
        _pushAudioFrameThread = new Thread(PushAudioFrameThread);
        _pushAudioFrameThread.Start();
    }

    void JoinChannel()
    {
        _rtcEngine.EnableVideo();
        _rtcEngine.EnableVideoObserver();
        _rtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        _pushAudioFrameThreadSignal = false;
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
        _logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, IRtcEngine.GetErrorDescription(warn)));
    }

    void OnSDKErrorHandler(int error, string msg)
    {
        _logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, IRtcEngine.GetErrorDescription(error)));
    }

    void OnConnectionLostHandler()
    {
        _logger.UpdateLog(string.Format("OnConnectionLost "));
    }

    void PushAudioFrameThread()
    {
        var bytesPerSample = 2;
        var type = AUDIO_FRAME_TYPE.FRAME_TYPE_PCM16;
        var channels = CHANNEL;
        var samples = SAMPLE_RATE / PUSH_FREQ_PER_SEC;
        var samplesPerSec = SAMPLE_RATE;
        var buffer = new byte[samples * bytesPerSample * CHANNEL];
        var freq = 1000 / PUSH_FREQ_PER_SEC;

        var tic = new TimeSpan(DateTime.Now.Ticks);

        while (_pushAudioFrameThreadSignal)
        {
            if (!_startSignal)
            {
                tic = new TimeSpan(DateTime.Now.Ticks);
            }

            var toc = new TimeSpan(DateTime.Now.Ticks);

            if (toc.Subtract(tic).Duration().Milliseconds >= freq)
            {
                tic = new TimeSpan(DateTime.Now.Ticks);

                lock (_audioBuffer)
                {
                    if (_audioBuffer.Size > samples * bytesPerSample * CHANNEL)
                    {
                        for (var j = 0; j < samples * bytesPerSample * CHANNEL; j++)
                        {
                            buffer[j] = _audioBuffer.Get();
                        }

                        var audioFrame = new AudioFrame
                        {
                            bytesPerSample = bytesPerSample,
                            type = type,
                            samples = samples,
                            samplesPerSec = samplesPerSec,
                            channels = channels,
                            buffer = buffer,
                            renderTimeMs = DateTime.Now.Ticks
                        };

                        _rtcEngine.PushAudioFrame(audioFrame);
                    }
                    else
                    {
                        AppendRingBuffer();
                    }
                }
            }
        }
    }

    private void AppendRingBuffer()
    {
        foreach (var s in _mediaBuffer)
        {
            _audioBuffer.Put(s);
        }
        _startSignal = true;
    }
}
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using RingBuffer;

public class CustomAudioSource : MonoBehaviour
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

    private const int CHANNEL = 2;

    // Please do not change this value because Unity re-samples the sample rate to 48000.
    private const int SAMPLE_RATE = 48000; 

    private const int PUSH_FREQ_PER_SEC = 100;

    private RingBuffer<byte> _audioBuffer;
    private bool _startConvertSignal = false;

    private Thread _pushAudioFrameThread;
    private bool _pushAudioFrameThreadSignal = false;
    private int _count;
    private bool _startSignal = false;


    // Use this for initialization
    void Start()
    {
        var game = GameObject.Find("Canvas");

        if (CheckAppId())
        {
            InitRtcEngine();
            JoinChannel();
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

    void StartPushAudioFrame()
    {
        var bufferLength = SAMPLE_RATE / PUSH_FREQ_PER_SEC * CHANNEL * 10000;
        _audioBuffer = new RingBuffer<byte>(bufferLength);
        _startConvertSignal = true;

        _pushAudioFrameThreadSignal = true;
        _pushAudioFrameThread = new Thread(PushAudioFrameThread);
        _pushAudioFrameThread.Start();
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

                for (var i = 0; i < 2; i++)
                {
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
                                renderTimeMs = freq
                            };

                            _rtcEngine.PushAudioFrame(audioFrame);
                        }
                    }
                }
            }
        }
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (!_startConvertSignal) return;

        var rescaleFactor = 32767;
        foreach (var t in data)
        {
            var sample = t;
            if (sample > 1) sample = 1;
            else if (sample < -1) sample = -1;

            var shortData = (short)(sample * rescaleFactor);
            var byteArr = new byte[2];
            byteArr = BitConverter.GetBytes(shortData);
            lock (_audioBuffer)
            {
                _audioBuffer.Put(byteArr[0]);
                _audioBuffer.Put(byteArr[1]);
            }
        }

        _count += 1;
        if (_count == 5) _startSignal = true;
    }
}
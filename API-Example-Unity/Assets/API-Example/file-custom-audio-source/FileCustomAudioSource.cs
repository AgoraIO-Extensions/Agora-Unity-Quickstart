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
    [SerializeField] private string APP_ID = "YOUR_APPID";

    [SerializeField] private string TOKEN = "";

    [SerializeField] private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

    [SerializeField] Text logText;

    internal static Logger logger;
    private IRtcEngine mRtcEngine = null;

    [SerializeField]
    private int CHANNEL = 1;

    [SerializeField]
    private int SAMPLE_RATE = 48000;
    private int PUSH_FREQ_PER_SEC = 100;

    // Copy this file to Assets/StreamingAssets
    [Tooltip("Load file from StreamingAssets folder")]
    [SerializeField]
    string MyPCMByteFile = "myaudio.bytes";


    private RingBuffer<byte> audioBuffer;

    private Thread _pushAudioFrameThread;
    private bool _pushAudioFrameThreadSignal = false;
    private bool _startSignal = false;

    byte[] MediaBuffer;

    // Use this for initialization
    void Start()
    {
        var game = GameObject.Find("Canvas");

        CheckAppId();
        InitRtcEngine();
        JoinChannel();
        LoadSound();
        StartPushAudioFrame();
    }

    // Update is called once per frame
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
        mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
        mRtcEngine.SetExternalAudioSource(true, SAMPLE_RATE, CHANNEL);
        mRtcEngine.SetLogFile("log.txt");
        mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();
        mRtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
        mRtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        mRtcEngine.OnWarning += OnSDKWarningHandler;
        mRtcEngine.OnError += OnSDKErrorHandler;
        mRtcEngine.OnConnectionLost += OnConnectionLostHandler;
    }

    void LoadSound()
    {
        var bufferLength = SAMPLE_RATE / PUSH_FREQ_PER_SEC * CHANNEL * 10000;
        audioBuffer = new RingBuffer<byte>(bufferLength);

        string fileStreamName = Application.streamingAssetsPath + "/" + MyPCMByteFile;
        MediaBuffer = File.ReadAllBytes(fileStreamName);
        logger.UpdateLog("Read " + MediaBuffer.Length + " bytes from " + fileStreamName);
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
        mRtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        _pushAudioFrameThreadSignal = false;
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

                lock (audioBuffer)
                {
                    if (audioBuffer.Size > samples * bytesPerSample * CHANNEL)
                    {
                        for (var j = 0; j < samples * bytesPerSample * CHANNEL; j++)
                        {
                            buffer[j] = audioBuffer.Get();
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

                        mRtcEngine.PushAudioFrame(audioFrame);
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
        foreach(var s in MediaBuffer)
        {
            audioBuffer.Put(s);  
	    }
        _startSignal = true;
    }
}
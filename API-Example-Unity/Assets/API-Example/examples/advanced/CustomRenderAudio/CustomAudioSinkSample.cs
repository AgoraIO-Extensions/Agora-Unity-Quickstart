using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using RingBuffer;

namespace CustomAudioSink
{
    public class CustomAudioSinkSample : MonoBehaviour
    {
        [SerializeField] private string APP_ID = "YOUR_APPID";

        [SerializeField] private string TOKEN = "";

        [SerializeField] private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

        public Text LogText;
        private Logger _logger;
        private IRtcEngine _rtcEngine = null;
        private IAudioRawDataManager _audioRawDataManager;

        public const int CHANNEL = 1;
        public const int SAMPLE_RATE = 44100;
        public const int PULL_FREQ_PER_SEC = 100;
        public const bool DebugFlag = false;
        public const int BYTES_PER_SAMPLE = 2;

        int SAMPLES;
        int FREQ;
        int BUFFER_SIZE;

        private int _writeCount = 0;
        private int _readCount = 0;

        private RingBuffer<float> _audioBuffer;
        private AudioClip _audioClip;


        private Thread _pullAudioFrameThread = null;
        private bool _pullAudioFrameThreadSignal = true;

        IntPtr BufferPtr { get; set; }

        // Start is called before the first frame update
        void Start()
        {
            var ifValid = CheckAppId();

            if (CheckAppId())
            {
                InitRtcEngine();
                JoinChannel();

                var aud = GetComponent<AudioSource>();
                if (aud == null)
                {
                    gameObject.AddComponent<AudioSource>();
                }

                SAMPLES = SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL;
                FREQ = 1000 / PULL_FREQ_PER_SEC;
                BUFFER_SIZE = SAMPLES * BYTES_PER_SAMPLE;

                KickStartAudio(aud, "externalClip");
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
            var nRet = _rtcEngine.SetExternalAudioSink(true, SAMPLE_RATE, CHANNEL);
            this._logger.UpdateLog("SetExternalAudioSink:nRet" + nRet);
            _rtcEngine.SetLogFile("log.txt");
            _rtcEngine.SetDefaultAudioRouteToSpeakerphone(true);
            _rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
            _rtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
            _rtcEngine.OnWarning += OnSDKWarningHandler;
            _rtcEngine.OnError += OnSDKErrorHandler;
            _rtcEngine.OnConnectionLost += OnConnectionLostHandler;
        }

        void JoinChannel()
        {
            _rtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
        }

        void KickStartAudio(AudioSource aud, string clipName)
        {
            var bufferLength = SAMPLES * 100; // 1-sec-length buffer
            _audioBuffer = new RingBuffer<float>(bufferLength, overflow: true);

            _audioRawDataManager = AudioRawDataManager.GetInstance(_rtcEngine);

            // Create and start the AudioClip playback, OnAudioRead will feed it
            _audioClip = AudioClip.Create(clipName,
                SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL, CHANNEL, SAMPLE_RATE, true,
                OnAudioRead);
            aud.clip = _audioClip;
            aud.loop = true;
            aud.Play();

            StartPullAudioThread();
        }

        void StartPullAudioThread()
        {
            if (_pullAudioFrameThread != null)
            {
                Debug.LogWarning("Stopping previous thread");
                _pullAudioFrameThread.Abort();
            }

            _pullAudioFrameThread = new Thread(PullAudioFrameThread);
            _pullAudioFrameThread.Start("pullAudio" + _writeCount);
        }

        bool _paused = false;
        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                Debug.LogWarning("Application paused. AudioBuffer length = " + _audioBuffer.Size);
                // Invalidate the buffer
                Debug.LogWarning("PullAudioFrameThread state = " + _pullAudioFrameThread.ThreadState + " signal =" + _pullAudioFrameThreadSignal);
                _pullAudioFrameThread.Abort();
                _pullAudioFrameThread = null;
                _paused = true;
            }
            else
            {
                if (_paused) // had been paused, not from starting up
                {
                    Debug.LogWarning("Resuming Thread");
                    _audioBuffer.Clear();
                    StartPullAudioThread();
                }
            }
        }


        void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            _pullAudioFrameThreadSignal = false;
            _audioBuffer.Clear();
            if (BufferPtr != IntPtr.Zero)
            {
                Debug.LogWarning("cleanning up IntPtr buffer");
                Marshal.FreeHGlobal(BufferPtr);
                BufferPtr = IntPtr.Zero;
            }
            if (_rtcEngine != null)
            {
                IRtcEngine.Destroy();
            }
        }

        #region -- Agora callbacks --
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

        #endregion

        private void PullAudioFrameThread()
        {
            BufferPtr = Marshal.AllocHGlobal(BUFFER_SIZE);

            var tic = new TimeSpan(DateTime.Now.Ticks);

            var byteArray = new byte[BUFFER_SIZE];

            while (_pullAudioFrameThreadSignal)
            {
                var toc = new TimeSpan(DateTime.Now.Ticks);
                if (toc.Subtract(tic).Duration().Milliseconds >= FREQ)
                {
                    tic = new TimeSpan(DateTime.Now.Ticks);
                    int rc = _audioRawDataManager.PullAudioFrame(BufferPtr,
                        type: (int)AUDIO_FRAME_TYPE.FRAME_TYPE_PCM16,
                     samples: SAMPLES,
              bytesPerSample: 2,
                    channels: CHANNEL,
               samplesPerSec: SAMPLE_RATE,
                renderTimeMs: 0,
                 avsync_type: 0);

                    if (rc < 0)
                    {
                        Debug.LogWarning("PullAudioFrame returns " + rc);
                        continue;
                    }

                    Marshal.Copy(BufferPtr, byteArray, 0, BUFFER_SIZE);

                    var floatArray = ConvertByteToFloat16(byteArray);
                    lock (_audioBuffer)
                    {
                        _audioBuffer.Put(floatArray);
                    }

                    _writeCount += floatArray.Length;
                    if (DebugFlag) Debug.Log("PullAudioFrame rc = " + rc + " writeCount = " + _writeCount);
                }

            }

            if (BufferPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(BufferPtr);
                BufferPtr = IntPtr.Zero;
            }

            Debug.Log("Done running pull audio thread");
        }

        private static float[] ConvertByteToFloat16(byte[] byteArray)
        {
            var floatArray = new float[byteArray.Length / 2];
            for (var i = 0; i < floatArray.Length; i++)
            {
                floatArray[i] = BitConverter.ToInt16(byteArray, i * 2) / 32768f; // -Int16.MinValue
            }

            return floatArray;
        }

        // This Monobehavior method feeds data into the audio source
        private void OnAudioRead(float[] data)
        {
            for (var i = 0; i < data.Length; i++)
            {
                lock (_audioBuffer)
                {
                    if (_audioBuffer.Count > 0)
                    {
                        data[i] = _audioBuffer.Get();
                    }
                    else
                    {
                        // no data
                        data[i] = 0;
                    }
                }

                _readCount += 1;
            }

            if (DebugFlag)
            {
                Debug.LogFormat("buffer length remains: {0}", _writeCount - _readCount);
            }
        }
    }
}

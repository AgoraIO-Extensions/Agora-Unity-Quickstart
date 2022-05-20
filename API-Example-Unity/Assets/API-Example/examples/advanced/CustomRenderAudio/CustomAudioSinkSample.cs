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

        public Text logText;
        private Logger logger;
        private IRtcEngine mRtcEngine = null;
        private IAudioRawDataManager _audioRawDataManager;

        public int CHANNEL = 1;
        public int SAMPLE_RATE = 44100;
        public int PULL_FREQ_PER_SEC = 100;
        public bool DebugFlag = false;

        const int BYTES_PER_SAMPLE = 2;

        int SAMPLES;
        int FREQ;
        int BUFFER_SIZE;

        private int writeCount = 0;
        private int readCount = 0;

        private RingBuffer<float> audioBuffer;
        private AudioClip _audioClip;


        private Thread _pullAudioFrameThread = null;
        private bool _pullAudioFrameThreadSignal = true;

        private bool _startSignal;

        IntPtr BufferPtr { get; set; }

        // Start is called before the first frame update
        void Start()
        {
            var ifValid = CheckAppId();
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

            if (ifValid) KickStartAudio(aud, "externalClip");
        }

        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
        }

        bool CheckAppId()
        {
            logger = new Logger(logText);
            return logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in Canvas!!!!!");
        }

        void InitRtcEngine()
        {
            mRtcEngine = IRtcEngine.GetEngine(APP_ID);
            mRtcEngine.SetExternalAudioSink(true, SAMPLE_RATE, CHANNEL);
            mRtcEngine.SetLogFile("log.txt");
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

        void KickStartAudio(AudioSource aud, string clipName)
        {
            var bufferLength = SAMPLES * 1000; // 10-sec-length buffer
            audioBuffer = new RingBuffer<float>(bufferLength);

            _audioRawDataManager = AudioRawDataManager.GetInstance(mRtcEngine);

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
            _pullAudioFrameThread.Start("pullAudio" + writeCount);
        }

        bool _paused = false;
        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                Debug.LogWarning("Application paused. AudioBuffer length = " + audioBuffer.Size);
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
                    audioBuffer.Clear();
                    StartPullAudioThread();
                }
            }
        }


        void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            _pullAudioFrameThreadSignal = false;
            _startSignal = false;
            audioBuffer.Clear();
            if (BufferPtr != IntPtr.Zero)
            {
                Debug.LogWarning("cleanning up IntPtr buffer");
                Marshal.FreeHGlobal(BufferPtr);
                BufferPtr = IntPtr.Zero;
            }
            if (mRtcEngine != null)
            {
                IRtcEngine.Destroy();
            }
        }

        #region -- Agora callbacks --
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

        #endregion

        private void PullAudioFrameThread()
        {
            BufferPtr = Marshal.AllocHGlobal(BUFFER_SIZE);

            int count = 0;

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
                    lock (audioBuffer)
                    {
                        audioBuffer.Put(floatArray);
                    }

                    writeCount += floatArray.Length;
                    if (DebugFlag) Debug.Log("PullAudioFrame rc = " + rc + " writeCount = " + writeCount);
                    count += 1;
                }

                if (count == 100)
                {
                    _startSignal = true;
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
            if (!_startSignal) return;
            for (var i = 0; i < data.Length; i++)
            {
                lock (audioBuffer)
                {
                    try
                    {
                        data[i] = audioBuffer.Get();
                    }
                    catch
                    {
                        // no data
                        data[i] = 0;
                    }
                }

                readCount += 1;
            }

            if (DebugFlag)
            {
                Debug.LogFormat("buffer length remains: {0}", writeCount - readCount);
            }
        }
    }
}
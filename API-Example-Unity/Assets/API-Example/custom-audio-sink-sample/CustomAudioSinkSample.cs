using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using NUnit.Framework.Constraints;
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
        private int CHANNEL = 1;
        private int SAMPLE_RATE = 44100;
        private int PULL_FREQ_PER_SEC = 100;

        private readonly Queue<Action> _actionQueue = new Queue<Action>();
        
        private int count;

        private int writeCount;
        private int readCount;

        private RingBuffer<float> audioBuffer;
        private AudioClip _audioClip;
        

        private Thread _pullAudioFrameThread;
        private bool _pullAudioFrameThreadSignal = true;
        
        private bool _startSignal;

        // Start is called before the first frame update
        void Start()
        {
            CheckAppId();
            InitRtcEngine();
            JoinChannel();
            StartPullAudioFrame();
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

        void StartPullAudioFrame()
        {
            _audioRawDataManager = AudioRawDataManager.GetInstance(mRtcEngine);
            
            var bufferLength = SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL * 1000; // 10-sec-length buffer
            audioBuffer = new RingBuffer<float>(bufferLength);
            
            _pullAudioFrameThread = new Thread(PullAudioFrameThread);
            _pullAudioFrameThread.Start();

            var aud = GetComponent<AudioSource>();
            _audioClip = AudioClip.Create("externalAudio",
                SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL, CHANNEL, SAMPLE_RATE, true,
                OnAudioRead);
            aud.clip = _audioClip;
            aud.loop = true;
            aud.Play();
        }

        void OnLeaveBtnClick()
        {
            mRtcEngine.LeaveChannel();
        }

        void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            _pullAudioFrameThreadSignal = false;
            _pullAudioFrameThread.Abort();
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

        private void PullAudioFrameThread()
        {
            var avsync_type = 0;
            var bytesPerSample = 2;
            var type = AUDIO_FRAME_TYPE.FRAME_TYPE_PCM16;
            var channels = CHANNEL;
            var samples = SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL;
            var samplesPerSec = SAMPLE_RATE;

            var tic = new TimeSpan(DateTime.Now.Ticks);

            while (_pullAudioFrameThreadSignal)
            {
                var toc = new TimeSpan(DateTime.Now.Ticks);
                if (toc.Subtract(tic).Duration().Milliseconds >= 10)
                {
                    lock (audioBuffer)
                    {
                        tic = new TimeSpan(DateTime.Now.Ticks);
                        var buffer = Marshal.AllocHGlobal(samples * bytesPerSample);
                        _audioRawDataManager.PullAudioFrame(buffer, (int) type, samples, bytesPerSample, channels,
                            samplesPerSec, 0, avsync_type);

                        var byteArray = new byte[samples * bytesPerSample];
                        Marshal.Copy(buffer, byteArray, 0, samples * bytesPerSample);

                        var floatArray = ConvertByteToFloat16(byteArray);
                        audioBuffer.Put(floatArray);

                        writeCount += floatArray.Length;
                        Marshal.FreeHGlobal(buffer);

                        count += 1;
                    }
                }

                if (count == 100)
                {
                    _startSignal = true;
                }
            }

            // Marshal.FreeHGlobal(buffer);
        }

        private static float[] ConvertByteToFloat16(byte[] byteArray)
        {
            var floatArray = new float[byteArray.Length / 2];
            for (var i = 0; i < floatArray.Length; i++)
            {
                floatArray[i] = BitConverter.ToInt16(byteArray, i * 2) / 32768f;
            }

            return floatArray;
        }


        private void OnAudioRead(float[] data)
        {
            if (!_startSignal) return;
            for (var i = 0; i < data.Length; i++)
            {
                lock (audioBuffer)
                {
                    data[i] = audioBuffer.Get();
                    readCount += 1;
                }
            }

            Debug.LogFormat("buffer length remains: {0}", writeCount - readCount);
        }
    }
}
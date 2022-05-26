using System;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using RingBuffer;

namespace CustomAudioSink
{
    public class PlaybackAudioFrameSourceSample : MonoBehaviour
    {
        [SerializeField] private string APP_ID = "YOUR_APPID";

        [SerializeField] private string TOKEN = "";

        [SerializeField] private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";
        public Text logText;
        private Logger logger;
        private IRtcEngine mRtcEngine = null;
        private IAudioRawDataManager _audioRawDataManager;

        private int CHANNEL = 1;
        private int PULL_FREQ_PER_SEC = 100;
        public int SAMPLE_RATE = 32000; // this should = CLIP_SAMPLES x PULL_FREQ_PER_SEC
        public int CLIP_SAMPLES = 320;

        private int count;

        private int writeCount;
        private int readCount;

        private RingBuffer<float> audioBuffer;
        private AudioClip _audioClip;
        
        private bool _startSignal;

        // Start is called before the first frame update
        void Start()
        {
            bool appIdOK = CheckAppId();
            if (!appIdOK) return;

            InitRtcEngine();
            JoinChannel();

            var aud = GetComponent<AudioSource>();
            if (aud == null)
            {
                gameObject.AddComponent<AudioSource>(); 
	        }
            SetupAudio(aud, "externalClip");
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
            if (mRtcEngine == null)
            {
                logger.UpdateLog("Engine creation failure!!!! App not running");
                return;
	        }
            mRtcEngine.SetLogFile("log.txt");
            mRtcEngine.SetDefaultAudioRouteToSpeakerphone(true);
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

        void SetupAudio(AudioSource aud, string clipName)
        {
            _audioRawDataManager = AudioRawDataManager.GetInstance(mRtcEngine);
            var nRet = _audioRawDataManager.RegisterAudioRawDataObserver();
            this.logger.UpdateLog("RegisterAudioRawDataObserver: +" + nRet);

            mRtcEngine.SetParameter("che.audio.external_render", true);

            // //The larger the buffer, the higher the delay
            var bufferLength = SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL * 100; // 1-sec-length buffer
            audioBuffer = new RingBuffer<float>(bufferLength,true);
            
            _audioClip = AudioClip.Create(clipName,
                CLIP_SAMPLES,
		        CHANNEL, SAMPLE_RATE, true,
                OnAudioRead);
            aud.clip = _audioClip;
            aud.loop = true;
            aud.Play();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                _startSignal = false;
                if (audioBuffer != null) {
		            audioBuffer.Clear();
                }
                count = 0;
	        } else {
                Debug.Log("Application resumed.");
	        }
        }

        void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
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

            _audioRawDataManager.SetOnPlaybackAudioFrameCallback(OnPlaybackAudioFrameHandler);
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

        private static float[] ConvertByteToFloat16(byte[] byteArray)
        {
            var floatArray = new float[byteArray.Length / 2];
            for (var i = 0; i < floatArray.Length; i++)
            {
                floatArray[i] = BitConverter.ToInt16(byteArray, i * 2) / 32768f; // -Int16.MinValue
            }

            return floatArray;
        }

        void OnPlaybackAudioFrameHandler(AudioFrame audioFrame)
        {
            if (count == 1)
            {
                Debug.LogWarning("audioFrame = "+ audioFrame);
            }
            var floatArray = ConvertByteToFloat16(audioFrame.buffer);

            lock (audioBuffer)
            {
                audioBuffer.Put(floatArray);
                writeCount += floatArray.Length;
                count++;
            }
        }

        private void OnAudioRead(float[] data)
        {
           
            for (var i = 0; i < data.Length; i++)
            {
                lock (audioBuffer)
                {
                    if (audioBuffer.Count > 0)
                    {
                        data[i] = audioBuffer.Get();
                        readCount += 1;
                    }
                }
            }

            Debug.LogFormat("buffer length remains: {0}", writeCount - readCount);
        }
    }
}
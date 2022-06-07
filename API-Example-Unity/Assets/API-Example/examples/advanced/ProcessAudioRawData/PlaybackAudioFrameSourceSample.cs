using System;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using RingBuffer;

namespace CustomAudioSink
{
    public class PlaybackAudioFrameSourceSample : MonoBehaviour
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
        private IAudioRawDataManager _audioRawDataManager;

        private const int CHANNEL = 1;
        private const int PULL_FREQ_PER_SEC = 100;
        public const int SAMPLE_RATE = 32000; // this should = CLIP_SAMPLES x PULL_FREQ_PER_SEC
        public const int CLIP_SAMPLES = 320;

        private int _count;

        private int _writeCount;
        private int _readCount;

        private RingBuffer<float> _audioBuffer;
        private AudioClip _audioClip;
        
        private bool _startSignal;

        // Start is called before the first frame update
        void Start()
        {
            if (CheckAppId())
            {
                InitRtcEngine();
                JoinChannel();

                var aud = GetComponent<AudioSource>();
                if (aud == null)
                {
                    gameObject.AddComponent<AudioSource>();
                }
                SetupAudio(aud, "externalClip");
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
            if (_rtcEngine == null)
            {
                _logger.UpdateLog("Engine creation failure!!!! App not running");
                return;
	        }
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

        void SetupAudio(AudioSource aud, string clipName)
        {
            _audioRawDataManager = AudioRawDataManager.GetInstance(_rtcEngine);
            var nRet = _audioRawDataManager.RegisterAudioRawDataObserver();
            this._logger.UpdateLog("RegisterAudioRawDataObserver: +" + nRet);

            _rtcEngine.SetParameter("che.audio.external_render", true);

            // //The larger the buffer, the higher the delay
            var bufferLength = SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL * 100; // 1-sec-length buffer
            _audioBuffer = new RingBuffer<float>(bufferLength,true);
            
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
                if (_audioBuffer != null) {
		            _audioBuffer.Clear();
                }
                _count = 0;
	        } else {
                Debug.Log("Application resumed.");
	        }
        }

        void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
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

            _audioRawDataManager.SetOnPlaybackAudioFrameCallback(OnPlaybackAudioFrameHandler);
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
            if (_count == 1)
            {
                Debug.LogWarning("audioFrame = "+ audioFrame);
            }
            var floatArray = ConvertByteToFloat16(audioFrame.buffer);

            lock (_audioBuffer)
            {
                _audioBuffer.Put(floatArray);
                _writeCount += floatArray.Length;
                _count++;
            }
        }

        private void OnAudioRead(float[] data)
        {
           
            for (var i = 0; i < data.Length; i++)
            {
                lock (_audioBuffer)
                {
                    if (_audioBuffer.Count > 0)
                    {
                        data[i] = _audioBuffer.Get();
                        _readCount += 1;
                    }
                }
            }

            Debug.LogFormat("buffer length remains: {0}", _writeCount - _readCount);
        }
    }
}
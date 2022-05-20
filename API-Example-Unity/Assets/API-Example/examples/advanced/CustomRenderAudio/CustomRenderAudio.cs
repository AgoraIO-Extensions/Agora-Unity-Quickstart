using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using RingBuffer;
using UnityEngine.Serialization;
using agora.rtc;
using agora.util;
using Logger = agora.util.Logger;

namespace CustomRenderAudio
{
    public class CustomRenderAudio : MonoBehaviour
    {
        [FormerlySerializedAs("AgoraBaseProfile")] [SerializeField]
        private AgoraBaseProfile agoraBaseProfile;
        
        [Header("_____________Basic Configuration_____________")]
        [FormerlySerializedAs("APP_ID")] [SerializeField]
        private string appID = "";

        [FormerlySerializedAs("TOKEN")] [SerializeField]
        private string token = "";

        [FormerlySerializedAs("CHANNEL_NAME")] [SerializeField]
        private string channelName = "";

        public Text logText;
        internal Logger Logger;
        internal IAgoraRtcEngine AgoraRtcEngine;
        //private IAudioRawDataManager _audioRawDataManager;

        private int CHANNEL = 1;
        private int SAMPLE_RATE = 44100;
        private int PULL_FREQ_PER_SEC = 100;

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
            LoadAssetData();
            var ifValid = CheckAppId();
            InitRtcEngine();
            JoinChannel();

            var aud = GetComponent<AudioSource>();
            if (aud == null)
            {
                gameObject.AddComponent<AudioSource>();
            }

            if (ifValid) StartPullAudioFrame(aud, "externalClip");
        }

        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
        }

        bool CheckAppId()
        {
            Logger = new Logger(logText);
            return Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in Canvas!!!!!");
        }
        
        //Show data in AgoraBasicProfile
        [ContextMenu("ShowAgoraBasicProfileData")]
        public void LoadAssetData()
        {
            if (agoraBaseProfile == null) return;
            appID = agoraBaseProfile.appID;
            token = agoraBaseProfile.token;
            channelName = agoraBaseProfile.channelName;
        }

        void InitRtcEngine()
        {
            AgoraRtcEngine = agora.rtc.AgoraRtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(appID, 0, true,
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            var ret = AgoraRtcEngine.Initialize(context);
            AgoraRtcEngine.InitEventHandler(handler);
            var nRet = AgoraRtcEngine.SetExternalAudioSink(SAMPLE_RATE, CHANNEL);
            this.Logger.UpdateLog("SetExternalAudioSink ret:" + nRet);
        }

        void JoinChannel()
        {
            AgoraRtcEngine.JoinChannel(token, channelName, "");
        }

        void StartPullAudioFrame(AudioSource aud, string clipName)
        {
            //_audioRawDataManager = AudioRawDataManager.GetInstance(AgoraRtcEngine);

            var bufferLength = SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL * 1000; // 10-sec-length buffer
            audioBuffer = new RingBuffer<float>(bufferLength);

            _pullAudioFrameThread = new Thread(PullAudioFrameThread);
            _pullAudioFrameThread.Start();

            _audioClip = AudioClip.Create(clipName,
                SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL, CHANNEL, SAMPLE_RATE, true,
                OnAudioRead);
            aud.clip = _audioClip;
            aud.loop = true;
            aud.Play();
        }

        void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            _pullAudioFrameThreadSignal = false;
            _pullAudioFrameThread.Abort();
            if (AgoraRtcEngine == null) return;
            AgoraRtcEngine.LeaveChannel();
            AgoraRtcEngine.Dispose();
        }

        private void PullAudioFrameThread()
        {
            var avsync_type = 0;
            var bytesPerSample = 2;
            var type = AUDIO_FRAME_TYPE.FRAME_TYPE_PCM16;
            var channels = CHANNEL;
            var samples = SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL;
            var samplesPerSec = SAMPLE_RATE;
            var buffer = new byte[samples * bytesPerSample];
            var freq = 1000 / PULL_FREQ_PER_SEC;

            var tic = new TimeSpan(DateTime.Now.Ticks);

            AudioFrame audioFrame = new AudioFrame(type, samples, BYTES_PER_SAMPLE.TWO_BYTES_PER_SAMPLE, channels, samplesPerSec, buffer, 0, avsync_type);
            while (_pullAudioFrameThreadSignal)
            {
                var toc = new TimeSpan(DateTime.Now.Ticks);
                if (toc.Subtract(tic).Duration().Milliseconds >= freq)
                {
                    tic = new TimeSpan(DateTime.Now.Ticks);
                    var ret = AgoraRtcEngine.PullAudioFrame(audioFrame);
                    Debug.Log("PullAudioFrame returns: " + ret);

                    var byteArray = buffer;
                    //Marshal.Copy(buffer, byteArray, 0, samples * bytesPerSample);

                    var floatArray = ConvertByteToFloat16(byteArray);
                    lock (audioBuffer)
                    {
                        audioBuffer.Put(floatArray);
                    }

                    writeCount += floatArray.Length;
                    count += 1;
                }

                if (count == 100)
                {
                    _startSignal = true;
                }
            }

            //Marshal.FreeHGlobal(buffer);
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


        private void OnAudioRead(float[] data)
        {
            if (!_startSignal) return;
            for (var i = 0; i < data.Length; i++)
            {
                lock (audioBuffer)
                {
                    data[i] = audioBuffer.Get();
                }

                readCount += 1;
            }

            //Debug.LogFormat("buffer length remains: {0}", writeCount - readCount);
        }
    }

    internal class UserEventHandler : IAgoraRtcEngineEventHandler
    {
        private readonly CustomRenderAudio _customAudioSinkSample;

        internal UserEventHandler(CustomRenderAudio customAudioSinkSample)
        {
            _customAudioSinkSample = customAudioSinkSample;
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _customAudioSinkSample.Logger.UpdateLog(string.Format("sdk version: {0}", _customAudioSinkSample.AgoraRtcEngine.GetVersion()));
            _customAudioSinkSample.Logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", connection.channelId,
                connection.localUid, elapsed));
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _customAudioSinkSample.Logger.UpdateLog("OnLeaveChannelSuccess");
        }

        public override void OnWarning(int warn, string msg)
        {
            _customAudioSinkSample.Logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int error, string msg)
        {
            _customAudioSinkSample.Logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
        }

        public override void OnConnectionLost(RtcConnection connection)
        {
            _customAudioSinkSample.Logger.UpdateLog(string.Format("OnConnectionLost "));
        }
    }
}
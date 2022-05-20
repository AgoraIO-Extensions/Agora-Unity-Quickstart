using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using RingBuffer;
using UnityEngine.Serialization;
using Logger = agora.util.Logger;

namespace Agora_Plugin.API_Example.examples.advanced.CustomCaptureAudio
{
    public class CustomCaptureAudio : MonoBehaviour
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
        internal Logger logger;
        internal IAgoraRtcEngine AgoraRtcEngine = null;

        private int CHANNEL = 1;

        private const int
            SAMPLE_RATE = 48000; // Please do not change this value because Unity re-samples the sample rate to 48000.

        private int PUSH_FREQ_PER_SEC = 100;

        private RingBuffer<byte> audioBuffer;
        private bool _startConvertSignal = false;

        private Thread _pushAudioFrameThread;
        private bool _pushAudioFrameThreadSignal = false;
        private int count;
        private bool _startSignal = false;


        // Use this for initialization
        void Start()
        {
            LoadAssetData();
            CheckAppId();
            InitRtcEngine();
            SetExternalAudioSource();
            JoinChannel();
            StartPushAudioFrame();
        }

        // Update is called once per frame
        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
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
        
        void CheckAppId()
        {
            logger = new Logger(logText);
            logger.DebugAssert(appID.Length > 10, "Please fill in your appId in Canvas!!!!!");
        }

        private void InitRtcEngine()
        {
            AgoraRtcEngine = agora.rtc.AgoraRtcEngine.CreateAgoraRtcEngine();

            RtcEngineContext context = new RtcEngineContext(appID, 0, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            AgoraRtcEngine.Initialize(context);
            AgoraRtcEngine.InitEventHandler(new UserEventHandler(this));
            AgoraRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            AgoraRtcEngine.EnableAudio();
            var nRet = AgoraRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            this.logger.UpdateLog("SetClientRole nRet:" + nRet);
        }

        private void SetExternalAudioSource()
        {
            var nRet = AgoraRtcEngine.SetExternalAudioSource(true, SAMPLE_RATE, CHANNEL, 1);
            this.logger.UpdateLog("SetExternalAudioSource nRet:" + nRet);
        }

        void StartPushAudioFrame()
        {
            var bufferLength = SAMPLE_RATE / PUSH_FREQ_PER_SEC * CHANNEL * 10000;
            audioBuffer = new RingBuffer<byte>(bufferLength);
            _startConvertSignal = true;

            _pushAudioFrameThreadSignal = true;
            _pushAudioFrameThread = new Thread(PushAudioFrameThread);
            _pushAudioFrameThread.Start();
        }

        void JoinChannel()
        {
            AgoraRtcEngine.JoinChannel(token, channelName, "");
        }

        void OnLeaveBtnClick()
        {
            AgoraRtcEngine.LeaveChannel();
        }

        void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            _pushAudioFrameThreadSignal = false;
            Debug.Log("OnApplicationQuit");
            if (AgoraRtcEngine == null) return;
            AgoraRtcEngine.LeaveChannel();
            AgoraRtcEngine.Dispose();
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
                                    bytesPerSample = BYTES_PER_SAMPLE.TWO_BYTES_PER_SAMPLE,
                                    type = type,
                                    samplesPerChannel = samples,
                                    samplesPerSec = samplesPerSec,
                                    channels = channels,
                                    buffer = buffer,
                                    renderTimeMs = freq
                                };
                                var ret = AgoraRtcEngine.PushAudioFrame(MEDIA_SOURCE_TYPE.AUDIO_PLAYOUT_SOURCE, audioFrame);
                                Debug.Log("PushAudioFrame returns: " + ret);
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

                var shortData = (short) (sample * rescaleFactor);
                var byteArr = new byte[2];
                byteArr = BitConverter.GetBytes(shortData);
                lock (audioBuffer)
                {
                    audioBuffer.Put(byteArr[0]);
                    audioBuffer.Put(byteArr[1]);
                }
            }

            count += 1;
            if (count == 20) _startSignal = true;
        }

        internal class UserEventHandler : IAgoraRtcEngineEventHandler
        {
            private readonly CustomCaptureAudio _customAudioSource;

            internal UserEventHandler(CustomCaptureAudio customAudioSource)
            {
                _customAudioSource = customAudioSource;
            }

            public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _customAudioSource.logger.UpdateLog(string.Format("sdk version: {0}",
                    _customAudioSource.AgoraRtcEngine.GetVersion()));
                _customAudioSource.logger.UpdateLog(string.Format(
                    "onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", connection.channelId,
                    connection.localUid, elapsed));
            }

            public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
            {
                _customAudioSource.logger.UpdateLog("OnLeaveChannelSuccess");
            }

            public override void OnWarning(int warn, string msg)
            {
                _customAudioSource.logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
            }

            public override void OnError(int error, string msg)
            {
                _customAudioSource.logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
            }

            public override void OnConnectionLost(RtcConnection connection)
            {
                _customAudioSource.logger.UpdateLog(string.Format("OnConnectionLost "));
            }
        }
    }
}
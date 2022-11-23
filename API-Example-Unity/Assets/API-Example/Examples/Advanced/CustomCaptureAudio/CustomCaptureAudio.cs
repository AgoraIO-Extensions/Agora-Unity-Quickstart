using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using System.Runtime.InteropServices;
using Agora.Rtc;
using Agora.Util;
using Logger = Agora.Util.Logger;
using RingBuffer;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.CustomCaptureAudio
{
    public class CustomCaptureAudio : MonoBehaviour
    {
        [FormerlySerializedAs("appIdInput")]
        [SerializeField]
        private AppIdInput _appIdInput;

        [Header("_____________Basic Configuration_____________")]
        [FormerlySerializedAs("APP_ID")]
        [SerializeField]
        private string _appID = "";

        [FormerlySerializedAs("TOKEN")]
        [SerializeField]
        private string _token = "";

        [FormerlySerializedAs("CHANNEL_NAME")]
        [SerializeField]
        private string _channelName = "";

        public Text LogText;
        internal Logger Log;
        internal IRtcEngine RtcEngine = null;

        private const int CHANNEL = 2;
        // Please do not change this value because Unity re-samples the sample rate to 48000.
        private const int SAMPLE_RATE = 48000;
        private const int PUSH_FREQ_PER_SEC = 10;

        private RingBuffer<byte> _audioBuffer;
        private bool _startConvertSignal = false;

        private Thread _pushAudioFrameThread;
        private System.Object _rtcLock = new System.Object();



        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitRtcEngine();
                SetExternalAudioSource();
                JoinChannel();
                StartPushAudioFrame();
            }
        }

        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
        }

        //Show data in AgoraBasicProfile
        [ContextMenu("ShowAgoraBasicProfileData")]
        private void LoadAssetData()
        {
            if (_appIdInput == null) return;
            _appID = _appIdInput.appID;
            _token = _appIdInput.token;
            _channelName = _appIdInput.channelName;
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        private void InitRtcEngine()
        {
            lock (_rtcLock)
            {

                RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();

                RtcEngineContext context = new RtcEngineContext(_appID, 0,
                    CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                    AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
                RtcEngine.Initialize(context);
                RtcEngine.InitEventHandler(new UserEventHandler(this));
            }
        }

        private void SetExternalAudioSource()
        {
            lock (_rtcLock)
            {
                var nRet = RtcEngine.SetExternalAudioSource(true, SAMPLE_RATE, CHANNEL, 1);
                this.Log.UpdateLog("SetExternalAudioSource nRet:" + nRet);
            }
        }

        private void StartPushAudioFrame()
        {
            // 1-sec-length buffer
            var bufferLength = SAMPLE_RATE * CHANNEL;
            _audioBuffer = new RingBuffer<byte>(bufferLength, true);
            _startConvertSignal = true;

            _pushAudioFrameThread = new Thread(PushAudioFrameThread);
            _pushAudioFrameThread.Start();

            this.Log.UpdateLog("Because the api of rtcEngine is called in different threads, it is necessary to use locks to ensure that different threads do not call the api of rtcEngine at the same time");
        }

        private void JoinChannel()
        {
            lock (_rtcLock)
            {
                RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
                RtcEngine.EnableAudio();
                RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
                RtcEngine.JoinChannel(_token, _channelName, "");
            }
        }

        private void OnLeaveBtnClick()
        {
            RtcEngine.LeaveChannel();
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");

            lock (_rtcLock)
            {
                if (RtcEngine == null) return;
                RtcEngine.InitEventHandler(null);
                RtcEngine.LeaveChannel();
                RtcEngine.Dispose();
                RtcEngine = null;
            }
            //need wait pullAudioFrameThread stop 
            _pushAudioFrameThread.Join();

        }

        private void PushAudioFrameThread()
        {
            var bytesPerSample = 2;
            var type = AUDIO_FRAME_TYPE.FRAME_TYPE_PCM16;
            var channels = CHANNEL;
            var samples = SAMPLE_RATE / PUSH_FREQ_PER_SEC;
            var samplesPerSec = SAMPLE_RATE;

            var freq = 1000 / PUSH_FREQ_PER_SEC;

            var audioFrame = new AudioFrame
            {
                bytesPerSample = BYTES_PER_SAMPLE.TWO_BYTES_PER_SAMPLE,
                type = type,
                samplesPerChannel = samples,
                samplesPerSec = samplesPerSec,
                channels = channels,
                RawBuffer = new byte[samples * bytesPerSample * CHANNEL],
                renderTimeMs = freq
            };


            double startMillisecond = GetTimestamp();
            long tick = 0;

            while (true)
            {
                lock (_rtcLock)
                {
                    if (RtcEngine == null)
                    {
                        break;
                    }

                    int nRet = -1;
                    lock (_audioBuffer)
                    {
                        if (_audioBuffer.Size > samples * bytesPerSample * CHANNEL)
                        {
                            for (var j = 0; j < samples * bytesPerSample * CHANNEL; j++)
                            {
                                audioFrame.RawBuffer[j] = _audioBuffer.Get();
                            }
                            nRet = RtcEngine.PushAudioFrame(MEDIA_SOURCE_TYPE.AUDIO_PLAYOUT_SOURCE, audioFrame);
                            //Debug.Log("PushAudioFrame returns: " + nRet);

                        }
                    }

                    if (nRet == 0)
                    {
                        tick++;
                        double nextMillisecond = startMillisecond + tick * freq;
                        double curMillisecond = GetTimestamp();
                        int sleepMillisecond = (int)Math.Ceiling(nextMillisecond - curMillisecond);
                        //Debug.Log("sleepMillisecond : " + sleepMillisecond);
                        if (sleepMillisecond > 0)
                        {
                            Thread.Sleep(sleepMillisecond);
                        }
                    }

                }

            }
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!_startConvertSignal) return;
            var rescaleFactor = 32767;
            lock (_audioBuffer)
            {
                foreach (var t in data)
                {
                    var sample = t;
                    if (sample > 1) sample = 1;
                    else if (sample < -1) sample = -1;

                    var shortData = (short)(sample * rescaleFactor);
                    var byteArr = new byte[2];
                    byteArr = BitConverter.GetBytes(shortData);

                    _audioBuffer.Put(byteArr[0]);
                    _audioBuffer.Put(byteArr[1]);
                }
            }
        }

        //get timestamp millisecond
        private double GetTimestamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return ts.TotalMilliseconds;
        }
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly CustomCaptureAudio _customAudioSource;

        internal UserEventHandler(CustomCaptureAudio customAudioSource)
        {
            _customAudioSource = customAudioSource;
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _customAudioSource.Log.UpdateLog(string.Format("sdk version: {0}",
                _customAudioSource.RtcEngine.GetVersion(ref build)));
            _customAudioSource.Log.UpdateLog(string.Format(
                "onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", connection.channelId,
                connection.localUid, elapsed));
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _customAudioSource.Log.UpdateLog("OnLeaveChannelSuccess");
        }

        public override void OnError(int error, string msg)
        {
            _customAudioSource.Log.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
        }

        public override void OnConnectionLost(RtcConnection connection)
        {
            _customAudioSource.Log.UpdateLog(string.Format("OnConnectionLost "));
        }
    }

    #endregion
}
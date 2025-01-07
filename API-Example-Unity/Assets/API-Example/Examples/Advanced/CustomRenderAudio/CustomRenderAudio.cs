using System;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;


using RingBuffer;
using io.agora.rtc.demo;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.CustomRenderAudio
{
    public class CustomRenderAudio : MonoBehaviour
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
        internal IRtcEngine RtcEngine;


        private const int CHANNEL = 2;
        private const int SAMPLE_RATE = 44100;
        private const int PULL_FREQ_PER_SEC = 100;


        private RingBuffer<float> _audioBuffer;
        private AudioClip _audioClip;


        private Thread _pullAudioFrameThread;
        private System.Object _rtcLock = new System.Object();
        private bool _startedSignal = false;
        private System.Object _signalLock = new System.Object();



        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitRtcEngine(() =>
                {
                    JoinChannel();
                    var aud = InitAudioSource();
                    StartPullAudioFrame(aud, "externalClip");
                });

            }
        }

        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
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

        private void InitRtcEngine(Action action)
        {
            lock (_rtcLock)
            {
                RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
                UserEventHandler handler = new UserEventHandler(this);
                //be care, enableAudioDevice need be false

                RtcEngineContext context = new RtcEngineContext();
                context.appId = _appID;
                context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
                context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
                context.areaCode = AREA_CODE.AREA_CODE_GLOB;
                RtcEngine.Initialize(context).ContinueWith(task =>
                {
                    Debug.Log("RtcEngine.Initialize: " + task.Result);
                    RtcEngine.InitEventHandler(handler);
                    action();
                });
            }
        }

        private void JoinChannel()
        {
            lock (_rtcLock)
            {
                RtcEngine.EnableAudio();
                //no enableAudioDevice to set false？ how this methond work?
                var nRet = RtcEngine.SetExternalAudioSink(true, SAMPLE_RATE, CHANNEL);
                this.Log.UpdateLog("SetExternalAudioSink ret:" + nRet);
                RtcEngine.JoinChannel(_token, _channelName, "", 0);
            }
        }

        private AudioSource InitAudioSource()
        {
            var aud = GetComponent<AudioSource>();
            if (aud == null)
            {
                aud = gameObject.AddComponent<AudioSource>();
            }
            return aud;
        }

        private void StartPullAudioFrame(AudioSource aud, string clipName)
        {

            // 1-sec-length buffer
            var bufferLength = SAMPLE_RATE * CHANNEL * 2;
            _audioBuffer = new RingBuffer<float>(bufferLength, true);



            _audioClip = AudioClip.Create(clipName,
                SAMPLE_RATE / PULL_FREQ_PER_SEC, CHANNEL, SAMPLE_RATE, true,
                OnAudioRead);
            aud.clip = _audioClip;
            aud.loop = true;
            aud.Play();


            _startedSignal = true;
            _pullAudioFrameThread = new Thread(PullAudioFrameThread);
            _pullAudioFrameThread.Start();
            this.Log.UpdateLog("Because the api of rtcEngine is called in different threads, it is necessary to use locks to ensure that different threads do not call the api of rtcEngine at the same time");

        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            lock (_signalLock)
            {
                _startedSignal = false;
            }
            //need wait pullAudioFrameThread stop 
            _pullAudioFrameThread.Join();

            lock (_rtcLock)
            {
                if (RtcEngine == null) return;
                RtcEngine.InitEventHandler(null);
                RtcEngine.LeaveChannel();
                var task = RtcEngine.Dispose().ContinueWith(task =>
                {
                    Debug.Log("RtcEngine.Dispose: " + task.Result);
                });
            }

        }

        private void PullAudioFrameThread()
        {
            var avsync_type = 0;
            var bytesPerSample = 2;
            var type = AUDIO_FRAME_TYPE.FRAME_TYPE_PCM16;
            var channels = CHANNEL;
            var samplesPerChannel = SAMPLE_RATE / PULL_FREQ_PER_SEC;
            var samplesPerSec = SAMPLE_RATE;
            var byteBuffer = new byte[samplesPerChannel * bytesPerSample * channels];
            var freq = 1000 / PULL_FREQ_PER_SEC;

            AudioFrame audioFrame = new AudioFrame
            {
                type = type,
                samplesPerChannel = samplesPerChannel,
                bytesPerSample = BYTES_PER_SAMPLE.TWO_BYTES_PER_SAMPLE,
                channels = channels,
                samplesPerSec = samplesPerSec,
                avsync_type = avsync_type
            };
            audioFrame.buffer = Marshal.AllocHGlobal(samplesPerChannel * bytesPerSample * channels);

            double startMillisecond = GetTimestamp();
            long tick = 0;

            while (true)
            {
                lock (_signalLock)
                {
                    if (_startedSignal == false)
                    {
                        break;
                    }
                }

                int nRet;
                lock (_rtcLock)
                {
                    if (RtcEngine == null)
                    {
                        break;
                    }
                    nRet = -1;
                    nRet = RtcEngine.PullAudioFrame(audioFrame);
                    Debug.Log("PullAudioFrame returns: " + nRet);

                    if (nRet == 0)
                    {
                        Marshal.Copy((IntPtr)audioFrame.buffer, byteBuffer, 0, byteBuffer.Length);
                        var floatArray = ConvertByteToFloat16(byteBuffer);
                        lock (_audioBuffer)
                        {
                            _audioBuffer.Put(floatArray);
                        }
                      
                        Debug.Log(string.Format("byteBuffer {0},{1},{2},{3},{4}", byteBuffer[0], byteBuffer[1], byteBuffer[2], byteBuffer[3], byteBuffer[4]));
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

            Marshal.FreeHGlobal(audioFrame.buffer);
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
            //if (!_startSignal) return;
            lock (_audioBuffer)
            {
                for (var i = 0; i < data.Length; i++)
                {

                    if (_audioBuffer.Count > 0)
                    {
                        data[i] = _audioBuffer.Get();
                    }
                    else
                    {
                        data[i] = 0;
                    }
                }

                //readCount += 1;
            }

            Debug.LogFormat("buffer length remains: ");
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
        private readonly CustomRenderAudio _customAudioSinkSample;

        internal UserEventHandler(CustomRenderAudio customAudioSinkSample)
        {
            _customAudioSinkSample = customAudioSinkSample;
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _customAudioSinkSample.Log.UpdateLog(string.Format("sdk version: {0}", _customAudioSinkSample.RtcEngine.GetVersion(ref build)));
            _customAudioSinkSample.Log.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", connection.channelId,
                connection.localUid, elapsed));
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _customAudioSinkSample.Log.UpdateLog("OnLeaveChannelSuccess");
        }

        public override void OnError(int error, string msg)
        {
            _customAudioSinkSample.Log.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
        }

        public override void OnConnectionLost(RtcConnection connection)
        {
            _customAudioSinkSample.Log.UpdateLog(string.Format("OnConnectionLost "));
        }
    }

    #endregion
}
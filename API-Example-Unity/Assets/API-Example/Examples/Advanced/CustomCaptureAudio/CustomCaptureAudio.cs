using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using System.Runtime.InteropServices;
using Agora.Rtc;

using RingBuffer;
using io.agora.rtc.demo;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Analytics;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;
using System.Data.Common;

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
        internal uint AUDIO_TRACK_ID = 0;
        // This depends on the audio file you open
        private const int CHANNEL = 2;
        // This depends on the audio file you open
        private const int SAMPLE_RATE = 44100;

        // Number of push audio frame per second.
        private const int PUSH_FREQ_PER_SEC = 20;

        private Thread _pushAudioFrameThread;
        private System.Object _rtcLock = new System.Object();



        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitRtcEngine();
                CreateCustomAudioSource();
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

                RtcEngineContext context = new RtcEngineContext();
                context.appId = _appID;
                context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
                context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
                context.areaCode = AREA_CODE.AREA_CODE_GLOB;

                RtcEngine.Initialize(context);
                RtcEngine.InitEventHandler(new UserEventHandler(this));
            }
        }

        private void CreateCustomAudioSource()
        {
            lock (_rtcLock)
            {
                AudioTrackConfig audioTrackConfig = new AudioTrackConfig(true);
                AUDIO_TRACK_ID = RtcEngine.CreateCustomAudioTrack(AUDIO_TRACK_TYPE.AUDIO_TRACK_MIXABLE, audioTrackConfig);
                this.Log.UpdateLog("CreateCustomAudioTrack id:" + AUDIO_TRACK_ID);
            }
        }

        private IEnumerator PreparationFilePath(Action<string> callback)
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR          
            string fromPath = Path.Combine(Application.streamingAssetsPath, "audio/pcm16.wav");
            string filePath = Path.Combine(Application.persistentDataPath, "pcm16.wav");
            if (fromPath.Contains("://") || fromPath.Contains(":///"))
            {
                using (UnityWebRequest www = UnityWebRequest.Get(fromPath))
                {
                    yield return www.SendWebRequest();


                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("Failed to load file: " + www.error);
                    }
                    else
                    {

                        try
                        {
                            File.WriteAllBytes(filePath, www.downloadHandler.data);
                            Debug.Log("File successfully copied to " + filePath);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Failed to save file: " + e.Message);
                        }
                    }
                }
            }
            else
            {
                try
                {
                    File.Copy(fromPath, filePath, true);
                    Debug.Log("File successfully copied to " + filePath);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to copy file: " + e.Message);
                }
            }
#else
            string filePath = Path.Combine(Application.streamingAssetsPath, "audio/pcm16.wav");
#endif

            callback(filePath);
            yield break;
        }


        private void StartPushAudioFrame()
        {
            Action<string> action = (filePath) =>
            {
                ParameterizedThreadStart threadStart = new ParameterizedThreadStart(PushAudioFrameThread);
                _pushAudioFrameThread = new Thread(threadStart);
                _pushAudioFrameThread.Start(filePath);
            };
            this.Log.UpdateLog("Because the api of rtcEngine is called in different threads, it is necessary to use locks to ensure that different threads do not call the api of rtcEngine at the same time");
            StartCoroutine(PreparationFilePath(action));
        }

        private void JoinChannel()
        {
            lock (_rtcLock)
            {
                RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
                RtcEngine.EnableAudio();
                RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
                ChannelMediaOptions channelMediaOptions = new ChannelMediaOptions();
                channelMediaOptions.publishCustomAudioTrack.SetValue(true);
                channelMediaOptions.publishCustomAudioTrackId.SetValue((int)AUDIO_TRACK_ID);
                channelMediaOptions.publishMicrophoneTrack.SetValue(false);

                RtcEngine.JoinChannel(_token, _channelName, 0, channelMediaOptions);
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
                RtcEngine.DestroyCustomAudioTrack(AUDIO_TRACK_ID);
                RtcEngine.Dispose();
                RtcEngine = null;
            }
            //need wait pullAudioFrameThread stop 
            _pushAudioFrameThread.Join();

        }

        private void PushAudioFrameThread(object file)
        {
            string filePath = (string)file;
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
                renderTimeMs = 0
            };


            double startMillisecond = GetTimestamp();
            long tick = 0;
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            while (true)
            {
                int nRet = -1;
                lock (_rtcLock)
                {
                    if (RtcEngine == null)
                    {
                        break;
                    }

                    int bytesRead =  fileStream.Read(audioFrame.RawBuffer, 0, audioFrame.RawBuffer.Length);
                    nRet = RtcEngine.PushAudioFrame(audioFrame, AUDIO_TRACK_ID);

                    if (bytesRead == 0)
                    {
                        //Set the position of FileStream to return to the file header to restart reading data
                        fileStream.Seek(0, SeekOrigin.Begin);
                    }
                }

                if (nRet == 0)
                {
                    tick++;
                    double nextMillisecond = startMillisecond + tick * freq;
                    double curMillisecond = GetTimestamp();
                    int sleepMillisecond = (int)Math.Ceiling(nextMillisecond - curMillisecond);
                    Debug.Log("sleepMillisecond : " + sleepMillisecond);
                    if (sleepMillisecond > 0)
                    {
                        Thread.Sleep(sleepMillisecond);
                    }
                    else
                    {
                        Debug.Log("Sleep 1ms--1");
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    Debug.Log("Sleep freq");
                    Thread.Sleep(freq);
                    startMillisecond = GetTimestamp();
                    tick = 0;

                }
            }
            fileStream.Close();
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
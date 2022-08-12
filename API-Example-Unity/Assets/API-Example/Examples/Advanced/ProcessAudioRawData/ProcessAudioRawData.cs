﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using Agora.Util;
using Logger = Agora.Util.Logger;
using RingBuffer;
using System;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.ProcessAudioRawData
{
    public class ProcessAudioRawData : MonoBehaviour
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
        private Logger Log;
        private IRtcEngine RtcEngine;

        private const int CHANNEL = 1;
        private const int PULL_FREQ_PER_SEC = 100;
        public const int SAMPLE_RATE = 32000; // this should = CLIP_SAMPLES x PULL_FREQ_PER_SEC
        public const int CLIP_SAMPLES = 320;

        internal int _count;

        internal int _writeCount;
        internal int _readCount;

        internal RingBuffer<float> _audioBuffer;
        internal AudioClip _audioClip;

        private bool _startSignal;


        void Start()
        {
#if UNITY_WEBGL
            this.LogText.text = "Not Support in this platform!";
#else
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
                JoinChannel();

                var aud = GetComponent<AudioSource>();
                if (aud == null)
                {
                    gameObject.AddComponent<AudioSource>();
                }
                SetupAudio(aud, "externalClip");
            }
#endif
        }

        // Update is called once per frame
        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        //Show data in AgoraBasicProfile
        [ContextMenu("ShowAgoraBasicProfileData")]
        public void LoadAssetData()
        {
            if (_appIdInput == null) return;
            _appID = _appIdInput.appID;
            _token = _appIdInput.token;
            _channelName = _appIdInput.channelName;
        }

        void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
            RtcEngine.RegisterAudioFrameObserver(new AudioFrameObserver(this), OBSERVER_MODE.RAW_DATA);
            RtcEngine.SetPlaybackAudioFrameParameters(SAMPLE_RATE, 1, RAW_AUDIO_FRAME_OP_MODE_TYPE.RAW_AUDIO_FRAME_OP_MODE_READ_ONLY, 1024);
        }

        void JoinChannel()
        {
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.JoinChannel(_token, _channelName, "");
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine != null)
            {
                RtcEngine.UnRegisterVideoFrameObserver();
                RtcEngine.InitEventHandler(null);
                RtcEngine.LeaveChannel();
                RtcEngine.Dispose();
            }
        }

        void SetupAudio(AudioSource aud, string clipName)
        {
            // //The larger the buffer, the higher the delay
            var bufferLength = SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL * 100; // 1-sec-length buffer
            _audioBuffer = new RingBuffer<float>(bufferLength, true);

            _audioClip = AudioClip.Create(clipName,
                CLIP_SAMPLES,
                CHANNEL, SAMPLE_RATE, true,
                OnAudioRead);
            aud.clip = _audioClip;
            aud.loop = true;
            aud.Play();
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

        private static float[] ConvertByteToFloat16(byte[] byteArray)
        {
            var floatArray = new float[byteArray.Length / 2];
            for (var i = 0; i < floatArray.Length; i++)
            {
                floatArray[i] = BitConverter.ToInt16(byteArray, i * 2) / 32768f; // -Int16.MinValue
            }

            return floatArray;
        }

        internal class UserEventHandler : IRtcEngineEventHandler
        {
            private readonly ProcessAudioRawData _agoraVideoRawData;

            internal UserEventHandler(ProcessAudioRawData agoraVideoRawData)
            {
                _agoraVideoRawData = agoraVideoRawData;
            }

            public override void OnWarning(int warn, string msg)
            {
                _agoraVideoRawData.Log.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
            }

            public override void OnError(int err, string msg)
            {
                _agoraVideoRawData.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
            }

            public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _agoraVideoRawData.Log.UpdateLog(string.Format("sdk version: ${0}",
                    _agoraVideoRawData.RtcEngine.GetVersion()));
                _agoraVideoRawData.Log.UpdateLog(
                    string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                        connection.channelId, connection.localUid, elapsed));
            }

            public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _agoraVideoRawData.Log.UpdateLog("OnRejoinChannelSuccess");
            }

            public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
            {
                _agoraVideoRawData.Log.UpdateLog("OnLeaveChannel");
            }

            public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
                CLIENT_ROLE_TYPE newRole)
            {
                _agoraVideoRawData.Log.UpdateLog("OnClientRoleChanged");
            }

            public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
            {
                _agoraVideoRawData.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid,
                    elapsed));
            }

            public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
            {
                _agoraVideoRawData.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                    (int)reason));
            }
        }

        internal class AudioFrameObserver : IAudioFrameObserver
        {
            private readonly ProcessAudioRawData _agoraAudioRawData;

            internal AudioFrameObserver(ProcessAudioRawData agoraAudioRawData)
            {
                _agoraAudioRawData = agoraAudioRawData;
            }

            public override bool OnRecordAudioFrame(string channelId, AudioFrame audioFrame)
            {
                Debug.Log("OnRecordAudioFrame-----------");
                return true;
            }

            public override bool OnPlaybackAudioFrame(string channelId, AudioFrame audioFrame)
            {
                Debug.Log("OnPlaybackAudioFrame-----------");
                if (_agoraAudioRawData._count == 1)
                {
                    Debug.LogWarning("audioFrame = " + audioFrame);
                }
                var floatArray = ConvertByteToFloat16(audioFrame.RawBuffer);

                lock (_agoraAudioRawData._audioBuffer)
                {
                    _agoraAudioRawData._audioBuffer.Put(floatArray);
                    _agoraAudioRawData._writeCount += floatArray.Length;
                    _agoraAudioRawData._count++;
                }
                return true;
            }
        }
    }
}

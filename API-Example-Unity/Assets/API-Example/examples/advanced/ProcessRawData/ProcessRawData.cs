using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using agora.rtc;
using agora.util;
using Logger = agora.util.Logger;

namespace Agora_Plugin.API_Example.examples.advanced.ProcessRawData
{
    public class ProcessRawData : MonoBehaviour
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


        void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
                JoinChannel();
            }
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
            RtcEngine = agora.rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
            RtcEngine.RegisterVideoFrameObserver(new VideoFrameObserver(this), OBSERVER_MODE.RAW_DATA);
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
        }

        void JoinChannel()
        {
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

        //void OnApplicationQuit()
        //{
        //	Debug.Log("OnApplicationQuit1");
        //	if (mRtcEngine != null)
        //	{
        //		mRtcEngine.UnRegisterVideoFrameObserver();
        //		mRtcEngine.InitEventHandler(null);
        //          mRtcEngine.LeaveChannel();
        //		mRtcEngine.Dispose();
        //	}

        //	Debug.Log("OnApplicationQuit2");
        //}

        internal class UserEventHandler : IRtcEngineEventHandler
        {
            private readonly ProcessRawData _agoraVideoRawData;

            internal UserEventHandler(ProcessRawData agoraVideoRawData)
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

        internal class VideoFrameObserver : IVideoFrameObserver
        {
            private readonly ProcessRawData _agoraVideoRawData;

            internal VideoFrameObserver(ProcessRawData agoraVideoRawData)
            {
                _agoraVideoRawData = agoraVideoRawData;
            }

            public override bool OnCaptureVideoFrame(VideoFrame videoFrame, VideoFrameBufferConfig config)
            {
                Debug.Log("OnCaptureVideoFrame-----------" + " width:" + videoFrame.width + " height:" +
                          videoFrame.height);
                return true;
            }

            public override bool OnRenderVideoFrame(uint uid, VideoFrame videoFrame)
            {
                Debug.Log("OnRenderVideoFrameHandler-----------" + " uid:" + uid + " width:" + videoFrame.width +
                          " height:" + videoFrame.height);
                return true;
            }

        }
    }
}

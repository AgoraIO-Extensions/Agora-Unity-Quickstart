using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using agora.rtc;
using agora.util;
using UnityEditor;
using Logger = agora.util.Logger;
using System.IO;
using System;

namespace Agora_Plugin.API_Example.examples.basic.StartRhythmPlayer
{

    public class StartRhythmPlayer : MonoBehaviour
    {


        [FormerlySerializedAs("AgoraBaseProfile")]
        [SerializeField]
        private AgoraBaseProfile agoraBaseProfile;

        [Header("_____________Basic Configuration_____________")]
        [FormerlySerializedAs("APP_ID")]
        [SerializeField]
        private string appID = "";

        [FormerlySerializedAs("TOKEN")]
        [SerializeField]
        private string token = "";

        [FormerlySerializedAs("CHANNEL_NAME")]
        [SerializeField]
        public string channelName = "";

        public Text logText;
        internal Logger Logger;
        internal IAgoraRtcEngine _mRtcEngine = null;
        private const float Offset = 100;
        public uint localUid = 0;

        // Use this for initialization
        private void Start()
        {
#if UNITY_IPHONE || UNITY_ANDROID
        LoadAssetData();
        CheckAppId();
        InitEngine();
        JoinChannel();
#else
            throw new PlatformNotSupportedException();
#endif
        }

        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
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


        private void CheckAppId()
        {
            Logger = new Logger(logText);
            Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
        }

        private void InitEngine()
        {
            _mRtcEngine = AgoraRtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(null, appID, null, true,
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            _mRtcEngine.Initialize(context);
            _mRtcEngine.InitEventHandler(handler);
            _mRtcEngine.EnableAudio();
            _mRtcEngine.EnableVideo();
            _mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        }

        private void JoinChannel()
        {
            _mRtcEngine.JoinChannel(token, channelName);
        }


        internal class UserEventHandler : IAgoraRtcEngineEventHandler
        {
            private readonly StartRhythmPlayer _startRhythmPlayer;

            internal UserEventHandler(StartRhythmPlayer videoSample)
            {
                _startRhythmPlayer = videoSample;
            }

            public override void OnWarning(int warn, string msg)
            {
                _startRhythmPlayer.Logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
            }

            public override void OnError(int err, string msg)
            {
                _startRhythmPlayer.Logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
            }

            public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                Debug.Log("Agora: OnJoinChannelSuccess ");
                _startRhythmPlayer.Logger.UpdateLog(string.Format("sdk version: ${0}",
                    _startRhythmPlayer._mRtcEngine.GetVersion()));
                _startRhythmPlayer.Logger.UpdateLog(
                    string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                    connection.channelId, connection.localUid, elapsed));

                _startRhythmPlayer.localUid = connection.localUid;
                _startRhythmPlayer.EnableUI(true);
                TakeSnapshot.MakeVideoView(0);
            }

            public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _startRhythmPlayer.Logger.UpdateLog("OnRejoinChannelSuccess");
            }

            public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
            {
                _startRhythmPlayer.Logger.UpdateLog("OnLeaveChannel");
                TakeSnapshot.DestroyVideoView(0);
            }

            public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
            {
                _startRhythmPlayer.Logger.UpdateLog("OnClientRoleChanged");
            }

            public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
            {
                _startRhythmPlayer.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
                TakeSnapshot.MakeVideoView(uid, _startRhythmPlayer.channelName);
                _startRhythmPlayer.EnableUI(true);
            }

            public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
            {
                _startRhythmPlayer.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                    (int)reason));
                TakeSnapshot.DestroyVideoView(uid);
            }


        }
    }
}

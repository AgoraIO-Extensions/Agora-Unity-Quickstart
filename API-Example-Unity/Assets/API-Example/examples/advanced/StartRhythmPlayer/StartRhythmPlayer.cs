using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using agora.rtc;
using agora.util;

using Logger = agora.util.Logger;
using System.IO;
using System;

namespace Agora_Plugin.API_Example.examples.basic.StartRhythmPlayer
{

    public class StartRhythmPlayer : MonoBehaviour
    {


        [FormerlySerializedAs("appIdInput")]
        [SerializeField]
        private AppIdInput appIdInput;

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
        internal Logger logger;
        internal IRtcEngine mRtcEngine = null;

        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            CheckAppId();
            SetupUI();
            InitEngine();
            JoinChannel();
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
            if (appIdInput == null) return;
            appID = appIdInput.appID;
            token = appIdInput.token;
            channelName = appIdInput.channelName;
        }


        private void CheckAppId()
        {
            logger = new Logger(logText);
            logger.DebugAssert(appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }


        private void SetupUI()
        {
            var btn = this.gameObject.transform.Find("StartButton").GetComponent<Button>();
            btn.onClick.AddListener(OnStartButtonPress);

            btn = this.gameObject.transform.Find("StopButton").GetComponent<Button>();
            btn.onClick.AddListener(OnStopButtonPress);

            btn = this.gameObject.transform.Find("ConfigButton").GetComponent<Button>();
            btn.onClick.AddListener(OnConfigButtonPress);
        }

        private void InitEngine()
        {
            mRtcEngine = RtcEngineImpl.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(appID, 0, true,
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            mRtcEngine.Initialize(context);
            mRtcEngine.InitEventHandler(handler);
            mRtcEngine.EnableAudio();
            mRtcEngine.EnableVideo();
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        }

        private void JoinChannel()
        {
            mRtcEngine.JoinChannel(token, channelName);
        }

        void OnStartButtonPress()
        {
            string sound1 = Path.Combine(Application.streamingAssetsPath, "audio/ding.mp3");
            string sound2 = Path.Combine(Application.streamingAssetsPath, "audio/dang.mp3");
            AgoraRhythmPlayerConfig config = new AgoraRhythmPlayerConfig()
            {
                beatsPerMeasure = 4,
                beatsPerMinute = 60
            };

            int nRet = mRtcEngine.StartRhythmPlayer(sound1, sound2, config);
            this.logger.UpdateLog("StartRhythmPlayer nRet:" + nRet);
        }

        void OnStopButtonPress()
        {
            int nRet = mRtcEngine.StopRhythmPlayer();
            this.logger.UpdateLog("StopRhythmPlayer nRet:" + nRet);
        }

        void OnConfigButtonPress()
        {
            AgoraRhythmPlayerConfig config = new AgoraRhythmPlayerConfig()
            {
                beatsPerMeasure = 4,
                beatsPerMinute = 60
            };
            int nRet = mRtcEngine.ConfigRhythmPlayer(config);
            this.logger.UpdateLog("ConfigRhythmPlayer nRet:" + nRet);
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (mRtcEngine == null) return;
            mRtcEngine.InitEventHandler(null);
            mRtcEngine.LeaveChannel();
        }

        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            if (mRtcEngine == null) return;
            mRtcEngine.InitEventHandler(null);
            mRtcEngine.LeaveChannel();
            mRtcEngine.Dispose();
        }


        internal class UserEventHandler : IRtcEngineEventHandler
        {
            private readonly StartRhythmPlayer _startRhythmPlayer;

            internal UserEventHandler(StartRhythmPlayer videoSample)
            {
                _startRhythmPlayer = videoSample;
            }

            public override void OnWarning(int warn, string msg)
            {
                _startRhythmPlayer.logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
            }

            public override void OnError(int err, string msg)
            {
                _startRhythmPlayer.logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
            }

            public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                Debug.Log("Agora: OnJoinChannelSuccess ");
                _startRhythmPlayer.logger.UpdateLog(string.Format("sdk version: ${0}",
                    _startRhythmPlayer.mRtcEngine.GetVersion()));
                _startRhythmPlayer.logger.UpdateLog(
                    string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                    connection.channelId, connection.localUid, elapsed));
            }

            public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _startRhythmPlayer.logger.UpdateLog("OnRejoinChannelSuccess");
            }

            public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
            {
                _startRhythmPlayer.logger.UpdateLog("OnLeaveChannel");

            }

            public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
            {
                _startRhythmPlayer.logger.UpdateLog("OnClientRoleChanged");
            }

            public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
            {
                _startRhythmPlayer.logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));

            }

            public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
            {
                _startRhythmPlayer.logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                    (int)reason));
            }

            public override void OnRhythmPlayerStateChanged(RHYTHM_PLAYER_STATE_TYPE state, RHYTHM_PLAYER_ERROR_TYPE errorCode)
            {
                _startRhythmPlayer.logger.UpdateLog(string.Format("OnRhythmPlayerStateChanged {0},{1}", state, errorCode));
            }

        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using Agora.Rtc;
using Agora.Util;
using Logger = Agora.Util.Logger;
using System.IO;
using System;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.StartRhythmPlayer
{
    public class StartRhythmPlayer : MonoBehaviour
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

        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                SetupUI();
                InitEngine();
                JoinChannel();
            }
           
        }

        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
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
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
        }

        private void JoinChannel()
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.JoinChannel(_token, _channelName);
        }

        private void OnStartButtonPress()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // On Android, the StreamingAssetPath is just accessed by /assets instead of Application.streamingAssetPath
            string sound1 = "/assets/audio/ding.mp3";
            string sound2 = "/assets/audio/dang.mp3";
#else
            string sound1 = Path.Combine(Application.streamingAssetsPath, "audio/ding.mp3");
            string sound2 = Path.Combine(Application.streamingAssetsPath, "audio/dang.mp3");
#endif 
            AgoraRhythmPlayerConfig config = new AgoraRhythmPlayerConfig()
            {
                beatsPerMeasure = 4,
                beatsPerMinute = 60
            };

            int nRet = RtcEngine.StartRhythmPlayer(sound1, sound2, config);
            this.Log.UpdateLog("StartRhythmPlayer nRet:" + nRet);
        }

        private void OnStopButtonPress()
        {
            int nRet = RtcEngine.StopRhythmPlayer();
            this.Log.UpdateLog("StopRhythmPlayer nRet:" + nRet);
        }

        private void OnConfigButtonPress()
        {
            AgoraRhythmPlayerConfig config = new AgoraRhythmPlayerConfig()
            {
                beatsPerMeasure = 6,
                beatsPerMinute = 60
            };
            int nRet = RtcEngine.ConfigRhythmPlayer(config);
            this.Log.UpdateLog("ConfigRhythmPlayer nRet:" + nRet);
            this.Log.UpdateLog("beatsPerMeasure is config from 4 to 6");
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }

        internal string GetChannelName()
        {
            return _channelName;
        }
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly StartRhythmPlayer _startRhythmPlayer;

        internal UserEventHandler(StartRhythmPlayer videoSample)
        {
            _startRhythmPlayer = videoSample;
        }

        public override void OnError(int err, string msg)
        {
            _startRhythmPlayer.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _startRhythmPlayer.Log.UpdateLog(string.Format("sdk version: ${0}",
                _startRhythmPlayer.RtcEngine.GetVersion(ref build)));
            _startRhythmPlayer.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _startRhythmPlayer.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _startRhythmPlayer.Log.UpdateLog("OnLeaveChannel");

        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _startRhythmPlayer.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _startRhythmPlayer.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));

        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _startRhythmPlayer.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
        }

        public override void OnRhythmPlayerStateChanged(RHYTHM_PLAYER_STATE_TYPE state, RHYTHM_PLAYER_ERROR_TYPE errorCode)
        {
            _startRhythmPlayer.Log.UpdateLog(string.Format("OnRhythmPlayerStateChanged {0},{1}", state, errorCode));
        }

    }

    #endregion
}

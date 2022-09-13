using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using Agora.Util;
using Logger = Agora.Util.Logger;


namespace Agora_RTC_Plugin.API_Example.Examples.Basic.JoinChannelAudio
{
    public class JoinChannelAudio : MonoBehaviour
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

        private Button _stopPublishButton;
        public Button _startPublishButton;


        // Start is called before the first frame update
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitRtcEngine();
                JoinChannel();
                SetupUI();
            }
        }

        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset!!!!!");
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

        private void InitRtcEngine()
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
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.JoinChannel(_token, _channelName);
        }

        private void SetupUI()
        {
            _stopPublishButton = GameObject.Find("StopButton").GetComponent<Button>();
            _stopPublishButton.onClick.AddListener(StopPublishAudio);

            _startPublishButton = GameObject.Find("StartButton").GetComponent<Button>();
            _startPublishButton.onClick.AddListener(StartPublishAudio);
            _startPublishButton.gameObject.SetActive(false);
        }

        private void StopPublishAudio()
        {
            var options = new ChannelMediaOptions();
            options.publishMicrophoneTrack.SetValue(false);
            var nRet =  RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions: " + nRet);

            _stopPublishButton.gameObject.SetActive(false);
            _startPublishButton.gameObject.SetActive(true);
        }

        private void StartPublishAudio()
        {
            var options = new ChannelMediaOptions();
            options.publishMicrophoneTrack.SetValue(true);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions: " + nRet);

            _stopPublishButton.gameObject.SetActive(true);
            _startPublishButton.gameObject.SetActive(false);
        }

        private void OnLeaveBtnClick()
        {
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly JoinChannelAudio _audioSample;

        internal UserEventHandler(JoinChannelAudio audioSample)
        {
            _audioSample = audioSample;
        }


        public override void OnError(int err, string msg)
        {
            _audioSample.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _audioSample.Log.UpdateLog(string.Format("sdk version: ${0}",
                _audioSample.RtcEngine.GetVersion(ref build)));
            _audioSample.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _audioSample.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _audioSample.Log.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _audioSample.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _audioSample.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _audioSample.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
        }
    }

    #endregion
}
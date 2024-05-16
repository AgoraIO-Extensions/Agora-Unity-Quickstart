using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using io.agora.rtc.demo;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.AudioMixing
{
    public class AudioMixing : MonoBehaviour
    {
        [FormerlySerializedAs("AppIdInput")]
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

        [SerializeField] public string Sound_URL = "";

        private string _localPath = "";

        public Text LogText;
        internal Logger Log;
        internal IRtcEngine RtcEngine = null;

        private Toggle _urlToggle;
        private Toggle _loopbackToggle;

        // Start is called before the first frame update
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitRtcEngine();
                SetupUI();
                // enable it after joining
                EnableUI(false);
                JoinChannel();
            }
        }

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
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);

            RtcEngineContext context = new RtcEngineContext();
            context.appId = _appID;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
            context.areaCode = AREA_CODE.AREA_CODE_GLOB;


            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
        }

        private void SetupUI()
        {
            _mixingButton = GameObject.Find("MixButton").GetComponent<Button>();
            _mixingButton.onClick.AddListener(HandleAudioMixingButton);
            _effectButton = GameObject.Find("EffectButton").GetComponent<Button>();
            _effectButton.onClick.AddListener(HandleEffectButton);
            _urlToggle = GameObject.Find("Toggle").GetComponent<Toggle>();
            _loopbackToggle = GameObject.Find("Loopback").GetComponent<Toggle>();


#if UNITY_ANDROID && !UNITY_EDITOR
            // On Android, the StreamingAssetPath is just accessed by /assets instead of Application.streamingAssetPath
            _localPath = "/assets/audio/Agora.io-Interactions.mp3";
#else
            _localPath = Application.streamingAssetsPath + "/audio/" + "Agora.io-Interactions.mp3";
#endif
            Log.UpdateLog(string.Format("the audio file path: {0}", _localPath));

        }

        internal void EnableUI(bool enable)
        {
            _mixingButton.enabled = enable;
            _effectButton.enabled = enable;
        }

        private void JoinChannel()
        {
            RtcEngine.EnableAudio();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.JoinChannel(_token, _channelName, "", 0);
        }

        #region -- Test Control logic ---

        private void StartAudioMixing()
        {
            Debug.Log("Playing with " + (_urlToggle.isOn ? "URL" : "local file"));

            var ret = RtcEngine.StartAudioMixing(_urlToggle.isOn ? Sound_URL : _localPath, _loopbackToggle.isOn, 1);
            Debug.Log("StartAudioMixing returns: " + ret);
        }


        private void PlayEffectTest()
        {
            Debug.Log("Playing with " + (_urlToggle.isOn ? "URL" : "local file"));
            RtcEngine.PlayEffect(1, _urlToggle.isOn ? Sound_URL : _localPath, 1, 1.0, 0, 100, !_loopbackToggle.isOn, 0);
        }

        private void StopEffectTest()
        {
            RtcEngine.StopAllEffects();
        }

        #endregion

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }


        #region -- Application UI Logic ---

        private bool _isMixing = false;
        private Button _mixingButton { get; set; }

        private void HandleAudioMixingButton()
        {
            if (_effectOn)
            {
                Log.UpdateLog("Testing Effect right now, can't play effect...");
                return;
            }

            if (_isMixing)
            {
                RtcEngine.StopAudioMixing();
            }
            else
            {
                StartAudioMixing();
            }

            _isMixing = !_isMixing;
            _mixingButton.GetComponentInChildren<Text>().text = (_isMixing ? "Stop Mixing" : "Start Mixing");
        }


        private bool _effectOn = false;
        private Button _effectButton { get; set; }

        private void HandleEffectButton()
        {
            if (_isMixing)
            {
                Log.UpdateLog("Testing Mixing right now, can't play effect...");
                return;
            }

            if (_effectOn)
            {
                StopEffectTest();
            }
            else
            {
                PlayEffectTest();
            }

            _effectOn = !_effectOn;
            _effectButton.GetComponentInChildren<Text>().text = (_effectOn ? "Stop Effect" : "Play Effect");
        }




        #endregion
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly AudioMixing _audioMixing;

        internal UserEventHandler(AudioMixing audioMixing)
        {
            _audioMixing = audioMixing;
        }

        public override void OnError(int err, string msg)
        {
            _audioMixing.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _audioMixing.Log.UpdateLog(string.Format("sdk version: ${0}",
                _audioMixing.RtcEngine.GetVersion(ref build)));
            _audioMixing.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                    connection.channelId, connection.localUid, elapsed));
            _audioMixing.EnableUI(true);
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _audioMixing.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _audioMixing.Log.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _audioMixing.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _audioMixing.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _audioMixing.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
        }

        public override void OnAudioMixingStateChanged(AUDIO_MIXING_STATE_TYPE state, AUDIO_MIXING_REASON_TYPE errorCode)
        {
            _audioMixing.Log.UpdateLog(string.Format("AUDIO_MIXING_STATE_TYPE: ${0}, AUDIO_MIXING_REASON_TYPE: ${1}",
                state, errorCode));
        }
    }

    #endregion
}
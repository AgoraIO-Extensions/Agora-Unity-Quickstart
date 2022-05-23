using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using UnityEngine.Serialization;
using Logger = agora.util.Logger;

namespace Agora_Plugin.API_Example.examples.advanced.AudioMixing
{
    public class AudioMixing : MonoBehaviour
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
        private string channelName = "";

        [SerializeField] string Sound_URL = "";

        string localPath = "";

        public Text logText;
        internal Logger Logger;
        private IRtcEngine mRtcEngine = null;

        // Start is called before the first frame update
        void Start()
        {
            LoadAssetData();
            CheckAppId();
            InitRtcEngine();
            SetupUI();
            //StartAudioPlaybackTest();
            JoinChannel();
        }

        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
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

        void CheckAppId()
        {
            Logger = new Logger(logText);
            Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in API-Example/profile/AgoraBaseProfile.asset");
        }

        void InitRtcEngine()
        {
            mRtcEngine = RtcEngineImpl.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(appID, 0, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            var ret = mRtcEngine.Initialize(context);
            mRtcEngine.InitEventHandler(handler);
            mRtcEngine.EnableAudio();
        }

        void SetupUI()
        {
            MixingButton = GameObject.Find("MixButton").GetComponent<Button>();
            MixingButton.onClick.AddListener(HandleAudioMixingButton);
            EffectButton = GameObject.Find("EffectButton").GetComponent<Button>();
            EffectButton.onClick.AddListener(HandleEffectButton);
            urlToggle = GameObject.Find("Toggle").GetComponent<Toggle>();
            urlToggle.onValueChanged.AddListener(OnToggle);
            _useURL = urlToggle.isOn;

#if UNITY_ANDROID && !UNITY_EDITOR
        // On Android, the StreamingAssetPath is just accessed by /assets instead of Application.streamingAssetPath
        localPath = "/assets/audio/Agora.io-Interactions.mp3";
#else
            localPath = Application.streamingAssetsPath + "/audio/" + "Agora.io-Interactions.mp3";
#endif
            Logger.UpdateLog(string.Format("the audio file path: {0}", localPath));

            EnableUI(false); // enable it after joining
        }

        internal void EnableUI(bool enable)
        {
            MixingButton.enabled = enable;
            EffectButton.enabled = enable;
        }

        void JoinChannel()
        {
            mRtcEngine.JoinChannel(token, channelName);
        }

        #region -- Test Control logic ---

        void StartAudioMixing()
        {
            Debug.Log("Playing with " + (_useURL ? "URL" : "local file"));

            var ret = mRtcEngine.StartAudioMixing(_useURL ? Sound_URL : localPath, true, false, -1);
            Debug.Log("StartAudioMixing returns: " + ret);
        }

        void PlayEffectTest()
        {
            Debug.Log("Playing with " + (_useURL ? "URL" : "local file"));
            //IAudioEffectManager effectManager = mRtcEngine.GetAudioEffectManager();
            mRtcEngine.PlayEffect(1, _useURL ? Sound_URL : localPath, 1, 1.0, 0, 100, true);
        }

        void StopEffectTest()
        {
            //IAudioEffectManager effectManager = mRtcEngine.GetAudioEffectManager();
            mRtcEngine.StopAllEffects();
        }

        #endregion

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
            if (mRtcEngine != null)
            {
                mRtcEngine.InitEventHandler(null);
                mRtcEngine.LeaveChannel();
                mRtcEngine.Dispose();
                mRtcEngine = null;
            }
        }

        #region -- Application UI Logic ---

        bool _isMixing = false;
        Button MixingButton { get; set; }

        void HandleAudioMixingButton()
        {
            if (_effectOn)
            {
                Logger.UpdateLog("Testing Effect right now, can't play effect...");
                return;
            }

            if (_isMixing)
            {
                mRtcEngine.StopAudioMixing();
            }
            else
            {
                StartAudioMixing();
            }

            _isMixing = !_isMixing;
            MixingButton.GetComponentInChildren<Text>().text = (_isMixing ? "Stop Mixing" : "Start Mixing");
        }


        bool _effectOn = false;
        Button EffectButton { get; set; }

        void HandleEffectButton()
        {
            if (_isMixing)
            {
                Logger.UpdateLog("Testing Mixing right now, can't play effect...");
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
            EffectButton.GetComponentInChildren<Text>().text = (_effectOn ? "Stop Effect" : "Play Effect");
        }

        bool _useURL { get; set; }
        Toggle urlToggle { get; set; }

        void OnToggle(bool enable)
        {
            _useURL = enable;
        }

        #endregion
    }

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly AudioMixing _audioMixing;

        internal UserEventHandler(AudioMixing audioMixing)
        {
            _audioMixing = audioMixing;
        }

        public override void OnWarning(int warn, string msg)
        {
            _audioMixing.Logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _audioMixing.Logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            // _audioSample.Logger.UpdateLog(string.Format("sdk version: ${0}",
            //     _audioSample.AgoraRtcEngine.GetVersion()));
            _audioMixing.Logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                    connection.channelId, connection.localUid, elapsed));
            _audioMixing.EnableUI(true);
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _audioMixing.Logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _audioMixing.Logger.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
            CLIENT_ROLE_TYPE newRole)
        {
            _audioMixing.Logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _audioMixing.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _audioMixing.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
        }

        public override void OnAudioMixingStateChanged(AUDIO_MIXING_STATE_TYPE state, AUDIO_MIXING_ERROR_TYPE errorCode)
        {
            _audioMixing.Logger.UpdateLog(string.Format("AUDIO_MIXING_STATE_TYPE: ${0}, AUDIO_MIXING_ERROR_TYPE: ${1}",
                state, errorCode));
        }
    }
}
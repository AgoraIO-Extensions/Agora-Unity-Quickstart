using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using UnityEngine.Serialization;
using Logger = agora.util.Logger;

namespace Agora_Plugin.API_Example.examples.advanced.VoiceChanger
{

    public class VoiceChanger : MonoBehaviour
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
        private string channelName = "";

        public Text logText;
        private Logger logger;
        internal IRtcEngine mRtcEngine = null;


        void Start()
        {
            LoadAssetData();
            CheckAppId();
            InitEngine();
            SetupUI();
            JoinChannel();
        }


        [ContextMenu("ShowAgoraBasicProfileData")]
        public void LoadAssetData()
        {
            if (appIdInput == null) return;
            appID = appIdInput.appID;
            token = appIdInput.token;
            channelName = appIdInput.channelName;
        }

        void CheckAppId()
        {
            logger = new Logger(logText);
            logger.DebugAssert(appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        void InitEngine()
        {
            mRtcEngine = RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(appID, 0, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            mRtcEngine.Initialize(context);
            mRtcEngine.InitEventHandler(handler);
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            mRtcEngine.EnableAudio();
        }

        void JoinChannel()
        {
            mRtcEngine.JoinChannel(token, channelName, "");
        }

        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
        }


        void SetupUI()
        {
            Transform content = GameObject.Find("Canvas/Scroll View/Viewport/Content").transform;

            Button but = content.Find("ChatBeautifierButton").GetComponent<Button>();
            but.onClick.AddListener(OnChatBeautifierButtonPress);

            but = content.Find("SingingBeautifierButton").GetComponent<Button>();
            but.onClick.AddListener(OnSingingBeautifierButtonPress);

            but = content.Find("TimbreTransformationButton").GetComponent<Button>();
            but.onClick.AddListener(OnTimbreTransformationButtonPress);

            but = content.Find("OffVoiceBeautifierButton").GetComponent<Button>();
            but.onClick.AddListener(OnOffVoiceBeautifierButtonPress);

          
            but = content.Find("StyleTransformationButton").GetComponent<Button>();
            but.onClick.AddListener(OnStyleTransformationButtonPress);

            but = content.Find("RoomAcoustuicsButton").GetComponent<Button>();
            but.onClick.AddListener(OnRoomAcoustuicsButtonPress);

            but = content.Find("PitchButton").GetComponent<Button>();
            but.onClick.AddListener(OnPitchButtonPress);

            but = content.Find("OffAudioEffectButton").GetComponent<Button>();
            but.onClick.AddListener(OnOffAudioEffectButtonPress);


            but = content.Find("VoiceChangerButton").GetComponent<Button>();
            but.onClick.AddListener(OnVoiceChangerButtonPress);

            but = content.Find("OffVoiceChangerButton").GetComponent<Button>();
            but.onClick.AddListener(OnOffVoiceChangerButtonPress);


            but = content.Find("CustomVocalEffectsButton").GetComponent<Button>();
            but.onClick.AddListener(OnCustomVocalEffectsButtonPress);
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (mRtcEngine == null) return;
            mRtcEngine.InitEventHandler(null);
            mRtcEngine.LeaveChannel();
        }

        void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            if (mRtcEngine != null)
            {
                mRtcEngine.LeaveChannel();
                mRtcEngine.Dispose();
            }
        }

        #region VoiceBeautifier
        void OnChatBeautifierButtonPress()
        {
            mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = mRtcEngine.SetVoiceBeautifierPreset(VOICE_BEAUTIFIER_PRESET.CHAT_BEAUTIFIER_MAGNETIC);
            this.logger.UpdateLog(string.Format("SetVoiceBeautifierPreset nRet:{0}", nRet));
        }

        void OnSingingBeautifierButtonPress()
        {
            mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = mRtcEngine.SetVoiceBeautifierPreset(VOICE_BEAUTIFIER_PRESET.SINGING_BEAUTIFIER);
            this.logger.UpdateLog(string.Format("SetVoiceBeautifierPreset nRet:{0}", nRet));
        }

        void OnTimbreTransformationButtonPress()
        {
            mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = mRtcEngine.SetVoiceBeautifierPreset(VOICE_BEAUTIFIER_PRESET.TIMBRE_TRANSFORMATION_VIGOROUS);
            this.logger.UpdateLog(string.Format("SetVoiceBeautifierPreset nRet:{0}", nRet));
        }

        void OnOffVoiceBeautifierButtonPress()
        {
            mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = mRtcEngine.SetVoiceBeautifierPreset(VOICE_BEAUTIFIER_PRESET.VOICE_BEAUTIFIER_OFF);
            this.logger.UpdateLog(string.Format("SetVoiceBeautifierPreset nRet:{0}", nRet));
        }
        #endregion

        #region AudioEffect
        void OnStyleTransformationButtonPress()
        {
            mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = mRtcEngine.SetAudioEffectPreset(AUDIO_EFFECT_PRESET.STYLE_TRANSFORMATION_POPULAR);
            this.logger.UpdateLog(string.Format("SetAudioEffectPreset nRet:{0}", nRet));
        }

        void OnRoomAcoustuicsButtonPress()
        {
            mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = mRtcEngine.SetAudioEffectPreset(AUDIO_EFFECT_PRESET.ROOM_ACOUSTICS_KTV);
            this.logger.UpdateLog(string.Format("SetAudioEffectPreset nRet:{0}", nRet));
        }

        void OnPitchButtonPress()
        {
            mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = mRtcEngine.SetAudioEffectPreset(AUDIO_EFFECT_PRESET.PITCH_CORRECTION);
            this.logger.UpdateLog(string.Format("SetAudioEffectPreset nRet:{0}", nRet));
        }

        void OnOffAudioEffectButtonPress()
        {
            mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = mRtcEngine.SetAudioEffectPreset(AUDIO_EFFECT_PRESET.AUDIO_EFFECT_OFF);
            this.logger.UpdateLog(string.Format("SetAudioEffectPreset nRet:{0}", nRet));
        }
        #endregion

        #region VoiceConversion
        void OnVoiceChangerButtonPress()
        {
            mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = mRtcEngine.SetVoiceConversionPreset(VOICE_CONVERSION_PRESET.VOICE_CHANGER_NEUTRAL);
            this.logger.UpdateLog(string.Format("SetVoiceConversionPreset nRet:{0}", nRet));
        }

        void OnOffVoiceChangerButtonPress()
        {
            mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = mRtcEngine.SetVoiceConversionPreset(VOICE_CONVERSION_PRESET.VOICE_CONVERSION_OFF);
            this.logger.UpdateLog(string.Format("SetVoiceConversionPreset nRet:{0}", nRet));
        }

        #endregion

        #region Custom vocal effects
        void OnCustomVocalEffectsButtonPress()
        {

            //Set the tone. It can be set in the range of [0.5, 2.0]. The smaller the value, the lower the tone. The default value is 1.0, which means there is no need to modify the tone.
            int nRet = mRtcEngine.SetLocalVoicePitch(0.5);
            this.logger.UpdateLog(string.Format("SetLocalVoicePitch nRet:{0}", nRet));

            /**
             * Set the center frequency of the local vocal equalization band
             * The first parameter is the spectrum subband index, with a value range of [0,9], representing 10 frequency bands respectively, and the corresponding center frequency is [31,62,125,250,500,1000,2000,4000,8000,16000] Hz
             * The second parameter is the gain value of each frequency interval, the value range is [- 15,15], the unit is dB, and the default value is 0
             */
            nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_31, -15);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_62, 3);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_125, -9);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_250, -8);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_500, -6);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_1K, -4);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_2K, -3);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_4K, -2);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_8K, -1);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_16K, 1);
            this.logger.UpdateLog(string.Format("SetLocalVoiceEqualization nRet:{0}", nRet));


            // The original vocal intensity, the so-called dry signal, has a value range of [- 20,10], and the unit is dB
            nRet = mRtcEngine.SetLocalVoiceReverb(AUDIO_REVERB_TYPE.AUDIO_REVERB_DRY_LEVEL, 10);
            this.logger.UpdateLog(string.Format("SetLocalVoiceReverb nRet:{0}", nRet));

            // The early reflected signal intensity, the so-called wet signal, has a value range of [- 20,10], and the unit is dB
            nRet = mRtcEngine.SetLocalVoiceReverb(AUDIO_REVERB_TYPE.AUDIO_REVERB_WET_LEVEL, 7);
            this.logger.UpdateLog(string.Format("SetLocalVoiceReverb nRet:{0}", nRet));

            // The room size required for reverberation effect. Generally, the larger the room is, the stronger the reverberation effect is. Value range [0100]
            nRet = mRtcEngine.SetLocalVoiceReverb(AUDIO_REVERB_TYPE.AUDIO_REVERB_ROOM_SIZE, 6);
            this.logger.UpdateLog(string.Format("SetLocalVoiceReverb nRet:{0}", nRet));

            // Initial delay length of wet signal, value range [0200], unit: ms
            nRet = mRtcEngine.SetLocalVoiceReverb(AUDIO_REVERB_TYPE.AUDIO_REVERB_WET_DELAY, 124);
            this.logger.UpdateLog(string.Format("SetLocalVoiceReverb nRet:{0}", nRet));

            // The continuous intensity of reverberation effect. The value range is [0100]. The greater the value, the stronger the reverberation effect
            nRet = mRtcEngine.SetLocalVoiceReverb(AUDIO_REVERB_TYPE.AUDIO_REVERB_STRENGTH, 78);
            this.logger.UpdateLog(string.Format("SetLocalVoiceReverb nRet:{0}", nRet));
        }
        #endregion


        internal class UserEventHandler : IRtcEngineEventHandler
        {
            private readonly VoiceChanger _voiceChanger;

            internal UserEventHandler(VoiceChanger voiceChanger)
            {
                _voiceChanger = voiceChanger;
            }

            public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _voiceChanger.logger.UpdateLog(string.Format("sdk version: ${0}",
                    _voiceChanger.mRtcEngine.GetVersion()));
                _voiceChanger.logger.UpdateLog(string.Format(
                    "onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", connection.channelId,
                    connection.localUid, elapsed));
            }

            public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
            {
                _voiceChanger.logger.UpdateLog("OnLeaveChannelSuccess");
            }

            public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
            {
                _voiceChanger.logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid,
                    elapsed));
            }

            public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
            {
                _voiceChanger.logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                    (int)reason));
            }

            public override void OnWarning(int warn, string msg)
            {
                _voiceChanger.logger.UpdateLog(
                    string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
            }

            public override void OnError(int error, string msg)
            {
                _voiceChanger.logger.UpdateLog(
                    string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
            }

            public override void OnConnectionLost(RtcConnection connection)
            {
                _voiceChanger.logger.UpdateLog(string.Format("OnConnectionLost "));
            }
        }


    }
}

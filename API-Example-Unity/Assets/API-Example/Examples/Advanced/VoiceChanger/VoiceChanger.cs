using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using Agora.Util;
using Logger = Agora.Util.Logger;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.VoiceChanger
{
    public class VoiceChanger : MonoBehaviour
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

        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
                SetupUI();
                JoinChannel();
            }
        }

        [ContextMenu("ShowAgoraBasicProfileData")]
        public void LoadAssetData()
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
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableAudio();
            RtcEngine.JoinChannel(_token, _channelName, "");
        }

        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
        }

        private void SetupUI()
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
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }

        #region VoiceBeautifier
        private void OnChatBeautifierButtonPress()
        {
            RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = RtcEngine.SetVoiceBeautifierPreset(VOICE_BEAUTIFIER_PRESET.CHAT_BEAUTIFIER_MAGNETIC);
            this.Log.UpdateLog(string.Format("SetVoiceBeautifierPreset nRet:{0}", nRet));
        }

        private void OnSingingBeautifierButtonPress()
        {
            RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = RtcEngine.SetVoiceBeautifierPreset(VOICE_BEAUTIFIER_PRESET.SINGING_BEAUTIFIER);
            this.Log.UpdateLog(string.Format("SetVoiceBeautifierPreset nRet:{0}", nRet));
        }

        private void OnTimbreTransformationButtonPress()
        {
            RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = RtcEngine.SetVoiceBeautifierPreset(VOICE_BEAUTIFIER_PRESET.TIMBRE_TRANSFORMATION_VIGOROUS);
            this.Log.UpdateLog(string.Format("SetVoiceBeautifierPreset nRet:{0}", nRet));
        }

        private void OnOffVoiceBeautifierButtonPress()
        {
            RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = RtcEngine.SetVoiceBeautifierPreset(VOICE_BEAUTIFIER_PRESET.VOICE_BEAUTIFIER_OFF);
            this.Log.UpdateLog(string.Format("SetVoiceBeautifierPreset nRet:{0}", nRet));
        }
        #endregion

        #region AudioEffect
        private void OnStyleTransformationButtonPress()
        {
            RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = RtcEngine.SetAudioEffectPreset(AUDIO_EFFECT_PRESET.STYLE_TRANSFORMATION_POPULAR);
            this.Log.UpdateLog(string.Format("SetAudioEffectPreset nRet:{0}", nRet));
        }

        private void OnRoomAcoustuicsButtonPress()
        {
            RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = RtcEngine.SetAudioEffectPreset(AUDIO_EFFECT_PRESET.ROOM_ACOUSTICS_KTV);
            this.Log.UpdateLog(string.Format("SetAudioEffectPreset nRet:{0}", nRet));
        }

        private void OnPitchButtonPress()
        {
            RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = RtcEngine.SetAudioEffectPreset(AUDIO_EFFECT_PRESET.PITCH_CORRECTION);
            this.Log.UpdateLog(string.Format("SetAudioEffectPreset nRet:{0}", nRet));
        }

        private void OnOffAudioEffectButtonPress()
        {
            RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = RtcEngine.SetAudioEffectPreset(AUDIO_EFFECT_PRESET.AUDIO_EFFECT_OFF);
            this.Log.UpdateLog(string.Format("SetAudioEffectPreset nRet:{0}", nRet));
        }
        #endregion

        #region VoiceConversion
        private void OnVoiceChangerButtonPress()
        {
            RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = RtcEngine.SetVoiceConversionPreset(VOICE_CONVERSION_PRESET.VOICE_CHANGER_NEUTRAL);
            this.Log.UpdateLog(string.Format("SetVoiceConversionPreset nRet:{0}", nRet));
        }

        private void OnOffVoiceChangerButtonPress()
        {
            RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = RtcEngine.SetVoiceConversionPreset(VOICE_CONVERSION_PRESET.VOICE_CONVERSION_OFF);
            this.Log.UpdateLog(string.Format("SetVoiceConversionPreset nRet:{0}", nRet));
        }

        #endregion

        #region Custom vocal effects
        private void OnCustomVocalEffectsButtonPress()
        {

            //Set the tone. It can be set in the range of [0.5, 2.0]. The smaller the value, the lower the tone. The default value is 1.0, which means there is no need to modify the tone.
            int nRet = RtcEngine.SetLocalVoicePitch(0.5);
            this.Log.UpdateLog(string.Format("SetLocalVoicePitch nRet:{0}", nRet));

            /**
             * Set the center frequency of the local vocal equalization band
             * The first parameter is the spectrum subband index, with a value range of [0,9], representing 10 frequency bands respectively, and the corresponding center frequency is [31,62,125,250,500,1000,2000,4000,8000,16000] Hz
             * The second parameter is the gain value of each frequency interval, the value range is [- 15,15], the unit is dB, and the default value is 0
             */
            nRet = RtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_31, -15);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_62, 3);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_125, -9);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_250, -8);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_500, -6);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_1K, -4);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_2K, -3);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_4K, -2);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_8K, -1);
            //nRet = mRtcEngine.SetLocalVoiceEqualization(AUDIO_EQUALIZATION_BAND_FREQUENCY.AUDIO_EQUALIZATION_BAND_16K, 1);
            this.Log.UpdateLog(string.Format("SetLocalVoiceEqualization nRet:{0}", nRet));


            // The original vocal intensity, the so-called dry signal, has a value range of [- 20,10], and the unit is dB
            nRet = RtcEngine.SetLocalVoiceReverb(AUDIO_REVERB_TYPE.AUDIO_REVERB_DRY_LEVEL, 10);
            this.Log.UpdateLog(string.Format("SetLocalVoiceReverb nRet:{0}", nRet));

            // The early reflected signal intensity, the so-called wet signal, has a value range of [- 20,10], and the unit is dB
            nRet = RtcEngine.SetLocalVoiceReverb(AUDIO_REVERB_TYPE.AUDIO_REVERB_WET_LEVEL, 7);
            this.Log.UpdateLog(string.Format("SetLocalVoiceReverb nRet:{0}", nRet));

            // The room size required for reverberation effect. Generally, the larger the room is, the stronger the reverberation effect is. Value range [0100]
            nRet = RtcEngine.SetLocalVoiceReverb(AUDIO_REVERB_TYPE.AUDIO_REVERB_ROOM_SIZE, 6);
            this.Log.UpdateLog(string.Format("SetLocalVoiceReverb nRet:{0}", nRet));

            // Initial delay length of wet signal, value range [0200], unit: ms
            nRet = RtcEngine.SetLocalVoiceReverb(AUDIO_REVERB_TYPE.AUDIO_REVERB_WET_DELAY, 124);
            this.Log.UpdateLog(string.Format("SetLocalVoiceReverb nRet:{0}", nRet));

            // The continuous intensity of reverberation effect. The value range is [0100]. The greater the value, the stronger the reverberation effect
            nRet = RtcEngine.SetLocalVoiceReverb(AUDIO_REVERB_TYPE.AUDIO_REVERB_STRENGTH, 78);
            this.Log.UpdateLog(string.Format("SetLocalVoiceReverb nRet:{0}", nRet));
        }
        #endregion
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly VoiceChanger _voiceChanger;

        internal UserEventHandler(VoiceChanger voiceChanger)
        {
            _voiceChanger = voiceChanger;
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _voiceChanger.Log.UpdateLog(string.Format("sdk version: ${0}",
                _voiceChanger.RtcEngine.GetVersion(ref build)));
            _voiceChanger.Log.UpdateLog(string.Format(
                "onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", connection.channelId,
                connection.localUid, elapsed));
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _voiceChanger.Log.UpdateLog("OnLeaveChannelSuccess");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _voiceChanger.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid,
                elapsed));
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _voiceChanger.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
        }

        public override void OnError(int error, string msg)
        {
            _voiceChanger.Log.UpdateLog(
                string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
        }

        public override void OnConnectionLost(RtcConnection connection)
        {
            _voiceChanger.Log.UpdateLog(string.Format("OnConnectionLost "));
        }
    }

    #endregion
}

using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using io.agora.rtc.demo;

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

        private IAudioDeviceManager _audioDeviceManager;
        private DeviceInfo[] _audioPlaybackDeviceInfos;
        public Dropdown _audioDeviceSelect;
        public Dropdown _areaSelect;
        public RectTransform _qualityPanel;
        public GameObject _qualityItemPrefab;

        // Start is called before the first frame update
        private void Start()
        {
            LoadAssetData();
            PrepareAreaList();
            if (CheckAppId())
            {
                RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            }

#if UNITY_IOS || UNITY_ANDROID
            var text = GameObject.Find("Canvas/Scroll View/Viewport/Content/AudioDeviceManager").GetComponent<Text>();
            text.text = "Audio device manager not support in this platform";

            GameObject.Find("Canvas/Scroll View/Viewport/Content/AudioDeviceButton").SetActive(false);
            GameObject.Find("Canvas/Scroll View/Viewport/Content/deviceIdSelect").SetActive(false);
            GameObject.Find("Canvas/Scroll View/Viewport/Content/AudioSelectButton").SetActive(false);
#endif

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

        private void PrepareAreaList()
        {
            int index = 0;
            var areaList = new List<Dropdown.OptionData>();
            var enumNames = Enum.GetNames(typeof(AREA_CODE));
            foreach (var name in enumNames)
            {
                areaList.Add(new Dropdown.OptionData(name));
                if (name == "AREA_CODE_GLOB")
                {
                    index = areaList.Count - 1;
                }
            }
            _areaSelect.ClearOptions();
            _areaSelect.AddOptions(areaList.ToList());
            _areaSelect.value = index;
        }

        #region -- Button Events ---
        public void InitRtcEngine()
        {
            var text = this._areaSelect.captionText.text;
            AREA_CODE areaCode = (AREA_CODE)Enum.Parse(typeof(AREA_CODE), text);
            this.Log.UpdateLog("Select AREA_CODE : " + areaCode);

            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext();
            context.appId = _appID;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
            context.areaCode = areaCode;

            var result = RtcEngine.Initialize(context);
            this.Log.UpdateLog("Initialize result : " + result);

            RtcEngine.InitEventHandler(handler);

            RtcEngine.EnableAudio();
            RtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION);
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableAudioVolumeIndication(200, 3, true);
        }


        public void StartEchoTest()
        {
            EchoTestConfiguration config = new EchoTestConfiguration();
            config.intervalInSeconds = 2;
            config.enableAudio = true;
            config.enableVideo = false;
            config.token = this._appID;
            config.channelId = "echo_test_channel";
            RtcEngine.StartEchoTest(config);
            Log.UpdateLog("StartEchoTest, speak now. You cannot conduct another echo test or join a channel before StopEchoTest");
        }

        public void StopEchoTest()
        {
            RtcEngine.StopEchoTest();
        }

        public void JoinChannel()
        {
            RtcEngine.JoinChannel(_token, _channelName, "", 0);
        }

        public void LeaveChannel()
        {
            RtcEngine.LeaveChannel();
        }

        public void StopPublishAudio()
        {
            var options = new ChannelMediaOptions();
            options.publishMicrophoneTrack.SetValue(false);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions: " + nRet);
        }

        public void StartPublishAudio()
        {
            var options = new ChannelMediaOptions();
            options.publishMicrophoneTrack.SetValue(true);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions: " + nRet);
        }

        public void GetAudioPlaybackDevice()
        {
            _audioDeviceSelect.ClearOptions();
            _audioDeviceManager = RtcEngine.GetAudioDeviceManager();
            _audioPlaybackDeviceInfos = _audioDeviceManager.EnumeratePlaybackDevices();
            Log.UpdateLog(string.Format("AudioPlaybackDevice count: {0}", _audioPlaybackDeviceInfos.Length));
            for (var i = 0; i < _audioPlaybackDeviceInfos.Length; i++)
            {
                Log.UpdateLog(string.Format("AudioPlaybackDevice device index: {0}, name: {1}, id: {2}", i,
                    _audioPlaybackDeviceInfos[i].deviceName, _audioPlaybackDeviceInfos[i].deviceId));
            }

            _audioDeviceSelect.AddOptions(_audioPlaybackDeviceInfos.Select(w =>
                    new Dropdown.OptionData(
                        string.Format("{0} :{1}", w.deviceName, w.deviceId)))
                .ToList());
        }

        public void SelectAudioPlaybackDevice()
        {
            if (_audioDeviceSelect == null) return;
            var option = _audioDeviceSelect.options[_audioDeviceSelect.value].text;
            if (string.IsNullOrEmpty(option)) return;

            var deviceId = option.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            var ret = _audioDeviceManager.SetPlaybackDevice(deviceId);
            Log.UpdateLog("SelectAudioPlaybackDevice ret:" + ret + " , DeviceId: " + deviceId);
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


        public LocalAudioCallQualityPanel GetLocalAudioCallQualityPanel()
        {
            var panels = this._qualityPanel.GetComponentsInChildren<LocalAudioCallQualityPanel>();
            if (panels != null && panels.Length > 0)
            {
                return panels[0];
            }
            else
            {
                return null;
            }
        }

        public bool CreateLocalAudioCallQualityPanel()
        {
            if (GetLocalAudioCallQualityPanel() == null)
            {
                GameObject item = GameObject.Instantiate(this._qualityItemPrefab, this._qualityPanel);
                item.AddComponent<LocalAudioCallQualityPanel>();
                return true;
            }

            return false;
        }

        public bool DestroyLocalAudioCallQualityPanel()
        {
            var panel = GetLocalAudioCallQualityPanel();
            if (panel)
            {
                GameObject.Destroy(panel.gameObject);
                return true;
            }
            else
            {
                return false;
            }
        }

        public RemoteAudioCallQualityPanel GetRemoteAudioCallQualityPanel(uint uid)
        {
            var panels = this._qualityPanel.GetComponentsInChildren<RemoteAudioCallQualityPanel>();
            foreach (var panel in panels)
            {
                if (panel.Uid == uid)
                {
                    return panel;
                }
            }

            return null;
        }

        public bool CreateRemoteAudioCallQualityPanel(uint uid)
        {
            if (GetRemoteAudioCallQualityPanel(uid) == null)
            {
                GameObject item = GameObject.Instantiate(this._qualityItemPrefab, this._qualityPanel);
                var panel = item.AddComponent<RemoteAudioCallQualityPanel>();
                panel.Uid = uid;
                return true;

            }
            return false;
        }

        public bool DestroyRemoteAudioCallQualityPanel(uint uid)
        {
            var panel = GetRemoteAudioCallQualityPanel(uid);
            if (panel)
            {
                GameObject.Destroy(panel.gameObject);
                return true;
            }
            return false;
        }

        public void ClearAudioCallQualityPanel()
        {
            foreach (Transform child in this._qualityPanel)
            {
                GameObject.Destroy(child.gameObject);
            }
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
            _audioSample.CreateLocalAudioCallQualityPanel();
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _audioSample.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _audioSample.Log.UpdateLog("OnLeaveChannel");
            _audioSample.ClearAudioCallQualityPanel();
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _audioSample.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _audioSample.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            _audioSample.CreateRemoteAudioCallQualityPanel(uid);
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _audioSample.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            _audioSample.DestroyRemoteAudioCallQualityPanel(uid);
        }

        //Quality monitoring during calls
        public override void OnRtcStats(RtcConnection connection, RtcStats stats)
        {
            var panel = _audioSample.GetLocalAudioCallQualityPanel();
            if (panel != null)
            {
                panel.Stats = stats;
                panel.RefreshPanel();
            }
        }

        public override void OnLocalAudioStats(RtcConnection connection, LocalAudioStats stats)
        {
            var panel = _audioSample.GetLocalAudioCallQualityPanel();
            if (panel != null)
            {
                panel.AudioStats = stats;
                panel.RefreshPanel();
            }
        }

        public override void OnLocalAudioStateChanged(RtcConnection connection, LOCAL_AUDIO_STREAM_STATE state, LOCAL_AUDIO_STREAM_REASON reason)
        {

        }

        public override void OnRemoteAudioStats(RtcConnection connection, RemoteAudioStats stats)
        {
            var panel = _audioSample.GetRemoteAudioCallQualityPanel(stats.uid);
            if (panel != null)
            {
                panel.AudioStats = stats;
                panel.RefreshPanel();
            }
        }

        public override void OnRemoteAudioStateChanged(RtcConnection connection, uint remoteUid, REMOTE_AUDIO_STATE state, REMOTE_AUDIO_STATE_REASON reason, int elapsed)
        {

        }

        public override void OnAudioVolumeIndication(RtcConnection connection, AudioVolumeInfo[] speakers, uint speakerNumber, int totalVolume)
        {
            foreach (var speaker in speakers)
            {
                if (speaker.uid == 0)
                {
                    var panel = _audioSample.GetLocalAudioCallQualityPanel();
                    if (panel != null)
                    {
                        panel.Volume = (int)speaker.volume;
                        panel.RefreshPanel();
                    }
                }
                else
                {
                    var panel = _audioSample.GetRemoteAudioCallQualityPanel(speaker.uid);
                    if (panel != null)
                    {
                        panel.Volume = (int)speaker.volume;
                        panel.RefreshPanel();
                    }
                }
            }

        }

    }

    #endregion
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using io.agora.rtc.demo;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.DeviceManager
{
    public class DeviceManager : MonoBehaviour
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
        internal IRtcEngine RtcEngine;


        private IAudioDeviceManager _audioDeviceManager;
        private IVideoDeviceManager _videoDeviceManager;
        private DeviceInfo[] _audioRecordingDeviceInfos;
        private DeviceInfo[] _audioPlaybackDeviceInfos;
        private DeviceInfo[] _videoDeviceInfos;
        private const int DEVICE_INDEX = 0;

        // Start is called before the first frame update
        private void Start()
        {
#if UNITY_IPHONE || UNITY_ANDROID
            this.LogText.text = "iOS/Android is not supported, but you could see how it works on the Editor for Windows/MacOS";

#else
            LoadAssetData();
            if (CheckAppId())
            {
                CheckAppId();
                InitRtcEngine();
                CallDeviceManagerApi();
            }
#endif
        }

        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
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
            RtcEngineContext context = new RtcEngineContext();
            context.appId = _appID;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
            context.areaCode = AREA_CODE.AREA_CODE_GLOB;
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
        }

        private void CallDeviceManagerApi()
        {
            GetAudioRecordingDevice();
            GetAudioPlaybackDevice();
            GetVideoDeviceManager();
            SetCurrentDevice();
            SetCurrentDeviceVolume();
        }

        private void GetAudioRecordingDevice()
        {
            _audioDeviceManager = RtcEngine.GetAudioDeviceManager();
            _audioRecordingDeviceInfos = _audioDeviceManager.EnumerateRecordingDevices();
            Log.UpdateLog(string.Format("AudioRecordingDevice count: {0}", _audioRecordingDeviceInfos.Length));
            for (var i = 0; i < _audioRecordingDeviceInfos.Length; i++)
            {
                Log.UpdateLog(string.Format("AudioRecordingDevice device index: {0}, name: {1}, id: {2}", i,
                    _audioRecordingDeviceInfos[i].deviceName, _audioRecordingDeviceInfos[i].deviceId));
            }
        }

        private void GetAudioPlaybackDevice()
        {
            _audioDeviceManager = RtcEngine.GetAudioDeviceManager();
            _audioPlaybackDeviceInfos = _audioDeviceManager.EnumeratePlaybackDevices();
            Log.UpdateLog(string.Format("AudioPlaybackDevice count: {0}", _audioPlaybackDeviceInfos.Length));
            for (var i = 0; i < _audioPlaybackDeviceInfos.Length; i++)
            {
                Log.UpdateLog(string.Format("AudioPlaybackDevice device index: {0}, name: {1}, id: {2}", i,
                    _audioPlaybackDeviceInfos[i].deviceName, _audioPlaybackDeviceInfos[i].deviceId));
            }
        }

        private void GetVideoDeviceManager()
        {
            _videoDeviceManager = RtcEngine.GetVideoDeviceManager();
            _videoDeviceInfos = _videoDeviceManager.EnumerateVideoDevices();
            Log.UpdateLog(string.Format("VideoDeviceManager count: {0}", _videoDeviceInfos.Length));
            for (var i = 0; i < _videoDeviceInfos.Length; i++)
            {
                Log.UpdateLog(string.Format("VideoDeviceManager device index: {0}, name: {1}, id: {2}", i,
                    _videoDeviceInfos[i].deviceName, _videoDeviceInfos[i].deviceId));
            }
        }

        private void SetCurrentDevice()
        {
            if (_audioDeviceManager != null && _audioRecordingDeviceInfos.Length > 0)
                _audioDeviceManager.SetRecordingDevice(_audioRecordingDeviceInfos[DEVICE_INDEX].deviceId);
            if (_audioDeviceManager != null && _audioPlaybackDeviceInfos.Length > 0)
                _audioDeviceManager.SetPlaybackDevice(_audioPlaybackDeviceInfos[DEVICE_INDEX].deviceId);
            if (_videoDeviceManager != null && _videoDeviceInfos.Length > 0)
            {
                var ret = _videoDeviceManager.SetDevice(_videoDeviceInfos[DEVICE_INDEX].deviceId);
                Debug.Log("SetDevice returns: " + ret);
            }
        }

        private void SetCurrentDeviceVolume()
        {
            if (_audioDeviceManager != null) _audioDeviceManager.SetRecordingDeviceVolume(100);
            if (_audioDeviceManager != null) _audioDeviceManager.SetPlaybackDeviceVolume(100);
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
        private readonly DeviceManager _deviceManagerSample;

        internal UserEventHandler(DeviceManager deviceManagerSample)
        {
            _deviceManagerSample = deviceManagerSample;
        }

        public override void OnError(int err, string msg)
        {
            _deviceManagerSample.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _deviceManagerSample.Log.UpdateLog(string.Format("sdk version: ${0}",
                _deviceManagerSample.RtcEngine.GetVersion(ref build)));
            _deviceManagerSample.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _deviceManagerSample.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _deviceManagerSample.Log.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _deviceManagerSample.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _deviceManagerSample.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _deviceManagerSample.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
        }
    }

    #endregion
}
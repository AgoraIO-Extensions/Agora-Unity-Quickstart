using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using UnityEngine.Serialization;
using Logger = agora.util.Logger;

namespace Agora_Plugin.API_Example.examples.advanced.DeviceManager
{
    public class DeviceManager : MonoBehaviour
    {
        [FormerlySerializedAs("AgoraBaseProfile")] [SerializeField]
        private AgoraBaseProfile agoraBaseProfile;
        
        [Header("_____________Basic Configuration_____________")]
        [FormerlySerializedAs("APP_ID")] [SerializeField]
        private string appID = "";

        [FormerlySerializedAs("TOKEN")] [SerializeField]
        private string token = "";

        [FormerlySerializedAs("CHANNEL_NAME")] [SerializeField]
        private string channelName = "";

        public Text logText;
        internal Logger Logger;
        internal IAgoraRtcEngine _mRtcEngine;
        private IAgoraRtcAudioRecordingDeviceManager _audioRecordingDeviceManager;
        private IAgoraRtcAudioPlaybackDeviceManager _audioPlaybackDeviceManager;
        private IAgoraRtcVideoDeviceManager _videoDeviceManager;
        private DeviceInfo[] _audioRecordingDeviceInfos;
        private DeviceInfo[] _audioPlaybackDeviceInfos;
        private DeviceInfo[] _videoDeviceInfos;
        private const int DeviceIndex = 0;

        // Start is called before the first frame update
        private void Start()
        {
            LoadAssetData();
            CheckAppId();
            InitRtcEngine();
            CallDeviceManagerApi();
            //JoinChannel();
        }

        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
        }

        private void CheckAppId()
        {
            Logger = new Logger(logText);
            Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in Canvas!!!!!");
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

        private void InitRtcEngine()
        {
            _mRtcEngine = AgoraRtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(handler, appID, null, true, 
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            _mRtcEngine.Initialize(context);
            _mRtcEngine.InitEventHandler(handler);
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
          
            _audioRecordingDeviceManager = _mRtcEngine.GetAgoraRtcAudioRecordingDeviceManager();
            _audioRecordingDeviceInfos = _audioRecordingDeviceManager.EnumerateRecordingDevices();
            Logger.UpdateLog(string.Format("AudioRecordingDevice count: {0}", _audioRecordingDeviceInfos.Length));
            for (var i = 0; i < _audioRecordingDeviceInfos.Length; i++)
            {
                Logger.UpdateLog(string.Format("AudioRecordingDevice device index: {0}, name: {1}, id: {2}", i,
                    _audioRecordingDeviceInfos[i].deviceName, _audioRecordingDeviceInfos[i].deviceId));
            }
        }

        private void GetAudioPlaybackDevice()
        {
            _audioPlaybackDeviceManager = _mRtcEngine.GetAgoraRtcAudioPlaybackDeviceManager();
            _audioPlaybackDeviceInfos = _audioPlaybackDeviceManager.EnumeratePlaybackDevices();
            Logger.UpdateLog(string.Format("AudioPlaybackDevice count: {0}", _audioPlaybackDeviceInfos.Length));
            for (var i = 0; i < _audioPlaybackDeviceInfos.Length; i++)
            {
                Logger.UpdateLog(string.Format("AudioPlaybackDevice device index: {0}, name: {1}, id: {2}", i,
                    _audioPlaybackDeviceInfos[i].deviceName, _audioPlaybackDeviceInfos[i].deviceId));
            }
        }

        private void GetVideoDeviceManager()
        {
            var nRet = _mRtcEngine.StartPreview();
            this.Logger.UpdateLog("StartPreview: nRet" + nRet);
            _videoDeviceManager = _mRtcEngine.GetAgoraRtcVideoDeviceManager();
            _videoDeviceInfos = _videoDeviceManager.EnumerateVideoDevices();
            Logger.UpdateLog(string.Format("VideoDeviceManager count: {0}", _videoDeviceInfos.Length));
            for (var i = 0; i < _videoDeviceInfos.Length; i++)
            {
                Logger.UpdateLog(string.Format("VideoDeviceManager device index: {0}, name: {1}, id: {2}", i,
                    _videoDeviceInfos[i].deviceName, _videoDeviceInfos[i].deviceId));
            }
        }

        private void SetCurrentDevice()
        {
            if (_audioRecordingDeviceManager != null && _audioRecordingDeviceInfos.Length > 0)
                _audioRecordingDeviceManager.SetRecordingDevice(_audioRecordingDeviceInfos[DeviceIndex].deviceId);
            if (_audioPlaybackDeviceManager != null && _audioPlaybackDeviceInfos.Length > 0)
                _audioPlaybackDeviceManager.SetPlaybackDevice(_audioPlaybackDeviceInfos[DeviceIndex].deviceId);
            if (_videoDeviceManager != null && _videoDeviceInfos.Length > 0)
            {
                var ret = _videoDeviceManager.SetDevice(_videoDeviceInfos[DeviceIndex].deviceId);
                Debug.Log("SetDevice returns: " + ret);
            }
                
        }

        private void SetCurrentDeviceVolume()
        {
            if (_audioRecordingDeviceManager != null) _audioRecordingDeviceManager.SetRecordingDeviceVolume(100);
            if (_audioPlaybackDeviceManager != null) _audioPlaybackDeviceManager.SetPlaybackDeviceVolume(100);
        }

        private void JoinChannel()
        {
            _mRtcEngine.JoinChannel(token, channelName, "");
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
        }

        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            if (_mRtcEngine != null)
            {
                _mRtcEngine.Dispose();
                _mRtcEngine = null;
            }
        }
    }
    
    internal class UserEventHandler : IAgoraRtcEngineEventHandler
    {
        private readonly DeviceManager _deviceManagerSample;

        internal UserEventHandler(DeviceManager deviceManagerSample)
        {
            _deviceManagerSample = deviceManagerSample;
        }

        public override void OnWarning(int warn, string msg)
        {
            _deviceManagerSample.Logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _deviceManagerSample.Logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _deviceManagerSample.Logger.UpdateLog(string.Format("sdk version: ${0}",
                _deviceManagerSample._mRtcEngine.GetVersion()));
            _deviceManagerSample.Logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", 
                                connection.channelId, connection.localUid, elapsed));
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _deviceManagerSample.Logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _deviceManagerSample.Logger.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _deviceManagerSample.Logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _deviceManagerSample.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _deviceManagerSample.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int) reason));
        }
    }
}
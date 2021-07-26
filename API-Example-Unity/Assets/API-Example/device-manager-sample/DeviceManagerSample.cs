using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using agora_utilities;
using UnityEngine.Serialization;
using Logger = agora_utilities.Logger;

namespace DeviceManagerSample
{
    public class DeviceManagerSample : MonoBehaviour
    {
        [FormerlySerializedAs("APP_ID")] [SerializeField]
        private string appID = "YOUR_APPID";

        [FormerlySerializedAs("TOKEN")] [SerializeField]
        private string token = "";

        [FormerlySerializedAs("CHANNEL_NAME")] [SerializeField]
        private string channelName = "YOUR_CHANNEL_NAME";

        public Text logText;
        internal Logger Logger;
        internal IAgoraRtcEngine AgoraRtcEngine;
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
            CheckAppId();
            InitRtcEngine();
            CallDeviceManagerApi();
            JoinChannel();
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

        private void InitRtcEngine()
        {
            AgoraRtcEngine = agora_gaming_rtc.AgoraRtcEngine.CreateAgoraRtcEngine();
            AgoraRtcEngine.Initialize(new RtcEngineContext(appID, AREA_CODE.AREA_CODE_GLOB, new LogConfig("log.txt")));
            AgoraRtcEngine.InitEventHandler(new UserEventHandler(this));
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
            _audioRecordingDeviceManager = AgoraRtcEngine.GetAgoraRtcAudioRecordingDeviceManager();
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
            _audioPlaybackDeviceManager = AgoraRtcEngine.GetAgoraRtcAudioPlaybackDeviceManager();
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
            AgoraRtcEngine.StartPreview();
            _videoDeviceManager = AgoraRtcEngine.GetAgoraRtcVideoDeviceManager();
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
                _videoDeviceManager.SetDevice(_videoDeviceInfos[DeviceIndex].deviceId);
        }

        private void SetCurrentDeviceVolume()
        {
            if (_audioRecordingDeviceManager != null) _audioRecordingDeviceManager.SetRecordingDeviceVolume(100);
            if (_audioPlaybackDeviceManager != null) _audioPlaybackDeviceManager.SetPlaybackDeviceVolume(100);
        }

        private void JoinChannel()
        {
            AgoraRtcEngine.JoinChannel(token, channelName, "");
        }

        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            if (AgoraRtcEngine == null) return;
            AgoraRtcEngine.LeaveChannel();
            AgoraRtcEngine.Dispose();
        }
    }
    
    internal class UserEventHandler : IAgoraRtcEngineEventHandler
    {
        private readonly DeviceManagerSample _deviceManagerSample;

        internal UserEventHandler(DeviceManagerSample deviceManagerSample)
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

        public override void OnJoinChannelSuccess(string channel, uint uid, int elapsed)
        {
            _deviceManagerSample.Logger.UpdateLog(string.Format("sdk version: ${0}",
                _deviceManagerSample.AgoraRtcEngine.GetVersion()));
            _deviceManagerSample.Logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channel, uid, elapsed));
        }

        public override void OnRejoinChannelSuccess(string channel, uint uid, int elapsed)
        {
            _deviceManagerSample.Logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcStats stats)
        {
            _deviceManagerSample.Logger.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _deviceManagerSample.Logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(uint uid, int elapsed)
        {
            _deviceManagerSample.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        }

        public override void OnUserOffline(uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _deviceManagerSample.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int) reason));
        }

        public override void OnLastmileQuality(int quality)
        {
            _deviceManagerSample.Logger.UpdateLog("OnLastmileQuality");
        }

        public override void OnLastmileProbeResult(LastmileProbeResult result)
        {
            _deviceManagerSample.Logger.UpdateLog("OnLastmileProbeResult");
        }

        public override void OnConnectionInterrupted()
        {
            _deviceManagerSample.Logger.UpdateLog("OnConnectionInterrupted");
        }

        public override void OnConnectionLost()
        {
            _deviceManagerSample.Logger.UpdateLog("OnConnectionLost");
        }

        public override void OnConnectionBanned()
        {
            _deviceManagerSample.Logger.UpdateLog("OnConnectionBanned");
        }

        public override void OnApiCallExecuted(int err, string api, string result)
        {
            _deviceManagerSample.Logger.UpdateLog("OnApiCallExecuted");
        }

        public override void OnRequestToken()
        {
            _deviceManagerSample.Logger.UpdateLog("OnRequestToken");
        }

        public override void OnTokenPrivilegeWillExpire(string token)
        {
            _deviceManagerSample.Logger.UpdateLog("OnTokenPrivilegeWillExpire");
        }

        public override void OnAudioQuality(uint uid, int quality, ushort delay, ushort lost)
        {
            _deviceManagerSample.Logger.UpdateLog("OnAudioQuality");
        }

        public override void OnRtcStats(RtcStats stats)
        {
            _deviceManagerSample.Logger.UpdateLog("OnRtcStats");
        }

        public override void OnNetworkQuality(uint uid, int txQuality, int rxQuality)
        {
            _deviceManagerSample.Logger.UpdateLog("OnNetworkQuality");
        }

        public override void OnLocalVideoStats(LocalVideoStats stats)
        {
            _deviceManagerSample.Logger.UpdateLog("OnLocalVideoStats");
        }

        public override void OnRemoteVideoStats(RemoteVideoStats stats)
        {
            _deviceManagerSample.Logger.UpdateLog("OnRemoteVideoStats");
        }

        public override void OnLocalAudioStats(LocalAudioStats stats)
        {
            _deviceManagerSample.Logger.UpdateLog("OnLocalAudioStats");
        }

        public override void OnRemoteAudioStats(RemoteAudioStats stats)
        {
            _deviceManagerSample.Logger.UpdateLog("OnRemoteAudioStats");
        }

        public override void OnLocalAudioStateChanged(LOCAL_AUDIO_STREAM_STATE state, LOCAL_AUDIO_STREAM_ERROR error)
        {
            _deviceManagerSample.Logger.UpdateLog("OnLocalAudioStateChanged");
        }

        public override void OnRemoteAudioStateChanged(uint uid, REMOTE_AUDIO_STATE state,
            REMOTE_AUDIO_STATE_REASON reason, int elapsed)
        {
            _deviceManagerSample.Logger.UpdateLog("OnRemoteAudioStateChanged");
        }

        public override void OnAudioPublishStateChanged(string channel, STREAM_PUBLISH_STATE oldState,
            STREAM_PUBLISH_STATE newState, int elapseSinceLastState)
        {
            _deviceManagerSample.Logger.UpdateLog("OnAudioPublishStateChanged");
        }

        public override void OnVideoPublishStateChanged(string channel, STREAM_PUBLISH_STATE oldState,
            STREAM_PUBLISH_STATE newState, int elapseSinceLastState)
        {
            _deviceManagerSample.Logger.UpdateLog("OnVideoPublishStateChanged");
        }

        public override void OnAudioSubscribeStateChanged(string channel, uint uid, STREAM_SUBSCRIBE_STATE oldState,
            STREAM_SUBSCRIBE_STATE newState, int elapseSinceLastState)
        {
            _deviceManagerSample.Logger.UpdateLog("OnAudioSubscribeStateChanged");
        }

        public override void OnVideoSubscribeStateChanged(string channel, uint uid, STREAM_SUBSCRIBE_STATE oldState,
            STREAM_SUBSCRIBE_STATE newState, int elapseSinceLastState)
        {
            _deviceManagerSample.Logger.UpdateLog("OnVideoSubscribeStateChanged");
        }

        public override void OnAudioVolumeIndication(AudioVolumeInfo[] speakers, uint speakerNumber, int totalVolume)
        {
            _deviceManagerSample.Logger.UpdateLog("OnAudioVolumeIndication");
        }

        public override void OnActiveSpeaker(uint uid)
        {
            _deviceManagerSample.Logger.UpdateLog("OnActiveSpeaker");
        }

        public override void OnVideoStopped()
        {
            _deviceManagerSample.Logger.UpdateLog("OnVideoStopped");
        }

        public override void OnFirstLocalVideoFrame(int width, int height, int elapsed)
        {
            _deviceManagerSample.Logger.UpdateLog("OnFirstLocalVideoFrame");
        }

        public override void OnFirstLocalVideoFramePublished(int elapsed)
        {
            _deviceManagerSample.Logger.UpdateLog("OnFirstLocalVideoFramePublished");
        }

        public override void OnFirstRemoteVideoDecoded(uint uid, int width, int height, int elapsed)
        {
            _deviceManagerSample.Logger.UpdateLog("OnFirstRemoteVideoDecoded");
        }

        public override void OnFirstRemoteVideoFrame(uint uid, int width, int height, int elapsed)
        {
            _deviceManagerSample.Logger.UpdateLog("OnFirstRemoteVideoFrame");
        }

        public override void OnUserMuteAudio(uint uid, bool muted)
        {
            _deviceManagerSample.Logger.UpdateLog("OnUserMuteAudio");
        }

        public override void OnUserMuteVideo(uint uid, bool muted)
        {
            _deviceManagerSample.Logger.UpdateLog("OnUserMuteVideo");
        }

        public override void OnUserEnableVideo(uint uid, bool enabled)
        {
            _deviceManagerSample.Logger.UpdateLog("OnUserEnableVideo");
        }

        public override void OnAudioDeviceStateChanged(string deviceId, int deviceType, int deviceState)
        {
            _deviceManagerSample.Logger.UpdateLog("OnAudioDeviceStateChanged");
        }

        public override void OnAudioDeviceVolumeChanged(MEDIA_DEVICE_TYPE deviceType, int volume, bool muted)
        {
            _deviceManagerSample.Logger.UpdateLog("OnAudioDeviceVolumeChanged");
        }

        public override void OnCameraReady()
        {
            _deviceManagerSample.Logger.UpdateLog("OnCameraReady");
        }

        public override void OnCameraFocusAreaChanged(int x, int y, int width, int height)
        {
            _deviceManagerSample.Logger.UpdateLog("OnCameraFocusAreaChanged");
        }

        public override void OnFacePositionChanged(int imageWidth, int imageHeight, Rectangle vecRectangle,
            int[] vecDistance, int numFaces)
        {
            _deviceManagerSample.Logger.UpdateLog("OnFacePositionChanged");
        }

        public override void OnCameraExposureAreaChanged(int x, int y, int width, int height)
        {
            _deviceManagerSample.Logger.UpdateLog("OnCameraExposureAreaChanged");
        }

        public override void OnAudioMixingFinished()
        {
            _deviceManagerSample.Logger.UpdateLog("OnAudioMixingFinished");
        }

        public override void OnAudioMixingStateChanged(AUDIO_MIXING_STATE_TYPE state, AUDIO_MIXING_REASON_TYPE reason)
        {
            _deviceManagerSample.Logger.UpdateLog("OnAudioMixingStateChanged");
        }

        public override void OnRemoteAudioMixingBegin()
        {
            _deviceManagerSample.Logger.UpdateLog("OnRemoteAudioMixingBegin");
        }

        public override void OnRemoteAudioMixingEnd()
        {
            _deviceManagerSample.Logger.UpdateLog("OnRemoteAudioMixingEnd");
        }

        public override void OnAudioEffectFinished(int soundId)
        {
            _deviceManagerSample.Logger.UpdateLog("OnAudioEffectFinished");
        }

        public override void OnFirstRemoteAudioDecoded(uint uid, int elapsed)
        {
            _deviceManagerSample.Logger.UpdateLog("OnFirstRemoteAudioDecoded");
        }

        public override void OnVideoDeviceStateChanged(string deviceId, int deviceType, int deviceState)
        {
            _deviceManagerSample.Logger.UpdateLog("OnVideoDeviceStateChanged");
        }

        public override void OnLocalVideoStateChanged(LOCAL_VIDEO_STREAM_STATE localVideoState,
            LOCAL_VIDEO_STREAM_ERROR error)
        {
            _deviceManagerSample.Logger.UpdateLog("OnLocalVideoStateChanged");
        }

        public override void OnVideoSizeChanged(uint uid, int width, int height, int rotation)
        {
            _deviceManagerSample.Logger.UpdateLog("OnVideoSizeChanged");
        }

        public override void OnRemoteVideoStateChanged(uint uid, REMOTE_VIDEO_STATE state,
            REMOTE_VIDEO_STATE_REASON reason, int elapsed)
        {
            _deviceManagerSample.Logger.UpdateLog("OnRemoteVideoStateChanged");
        }

        public override void OnUserEnableLocalVideo(uint uid, bool enabled)
        {
            _deviceManagerSample.Logger.UpdateLog("OnUserEnableLocalVideo");
        }

        public override void OnStreamMessage(uint uid, int streamId, byte[] data, uint length)
        {
            _deviceManagerSample.Logger.UpdateLog("OnStreamMessage");
        }

        public override void OnStreamMessageError(uint uid, int streamId, int code, int missed, int cached)
        {
            _deviceManagerSample.Logger.UpdateLog("OnStreamMessageError");
        }

        public override void OnMediaEngineLoadSuccess()
        {
            _deviceManagerSample.Logger.UpdateLog("OnMediaEngineLoadSuccess");
        }

        public override void OnMediaEngineStartCallSuccess()
        {
            _deviceManagerSample.Logger.UpdateLog("OnMediaEngineStartCallSuccess");
        }

        public override void OnUserSuperResolutionEnabled(uint uid, bool enabled, SUPER_RESOLUTION_STATE_REASON reason)
        {
            _deviceManagerSample.Logger.UpdateLog("OnUserSuperResolutionEnabled");
        }

        public override void OnChannelMediaRelayStateChanged(CHANNEL_MEDIA_RELAY_STATE state,
            CHANNEL_MEDIA_RELAY_ERROR code)
        {
            _deviceManagerSample.Logger.UpdateLog("OnChannelMediaRelayStateChanged");
        }

        public override void OnChannelMediaRelayEvent(CHANNEL_MEDIA_RELAY_EVENT code)
        {
            _deviceManagerSample.Logger.UpdateLog("OnChannelMediaRelayEvent");
        }

        public override void OnFirstLocalAudioFrame(int elapsed)
        {
            _deviceManagerSample.Logger.UpdateLog("OnFirstLocalAudioFrame");
        }

        public override void OnFirstLocalAudioFramePublished(int elapsed)
        {
            _deviceManagerSample.Logger.UpdateLog("OnFirstLocalAudioFramePublished");
        }

        public override void OnFirstRemoteAudioFrame(uint uid, int elapsed)
        {
            _deviceManagerSample.Logger.UpdateLog("OnFirstRemoteAudioFrame");
        }

        public override void OnRtmpStreamingStateChanged(string url, RTMP_STREAM_PUBLISH_STATE state,
            RTMP_STREAM_PUBLISH_ERROR errCode)
        {
            _deviceManagerSample.Logger.UpdateLog("OnRtmpStreamingStateChanged");
        }

        public override void OnRtmpStreamingEvent(string url, RTMP_STREAMING_EVENT eventCode)
        {
            _deviceManagerSample.Logger.UpdateLog("OnRtmpStreamingEvent");
        }

        public override void OnStreamPublished(string url, int error)
        {
            _deviceManagerSample.Logger.UpdateLog("OnStreamPublished");
        }

        public override void OnStreamUnpublished(string url)
        {
            _deviceManagerSample.Logger.UpdateLog("OnStreamUnpublished");
        }

        public override void OnTranscodingUpdated()
        {
            _deviceManagerSample.Logger.UpdateLog("OnTranscodingUpdated");
        }

        public override void OnStreamInjectedStatus(string url, uint uid, int status)
        {
            _deviceManagerSample.Logger.UpdateLog("OnStreamInjectedStatus");
        }

        public override void OnAudioRouteChanged(AUDIO_ROUTE_TYPE routing)
        {
            _deviceManagerSample.Logger.UpdateLog("OnAudioRouteChanged");
        }

        public override void OnLocalPublishFallbackToAudioOnly(bool isFallbackOrRecover)
        {
            _deviceManagerSample.Logger.UpdateLog("OnLocalPublishFallbackToAudioOnly");
        }

        public override void OnRemoteSubscribeFallbackToAudioOnly(uint uid, bool isFallbackOrRecover)
        {
            _deviceManagerSample.Logger.UpdateLog("OnRemoteSubscribeFallbackToAudioOnly");
        }

        public override void OnRemoteAudioTransportStats(uint uid, ushort delay, ushort lost, ushort rxKBitRate)
        {
            _deviceManagerSample.Logger.UpdateLog("OnRemoteAudioTransportStats");
        }

        public override void OnRemoteVideoTransportStats(uint uid, ushort delay, ushort lost, ushort rxKBitRate)
        {
            _deviceManagerSample.Logger.UpdateLog("OnRemoteVideoTransportStats");
        }

        public override void OnMicrophoneEnabled(bool enabled)
        {
            _deviceManagerSample.Logger.UpdateLog("OnMicrophoneEnabled");
        }

        public override void OnConnectionStateChanged(CONNECTION_STATE_TYPE state,
            CONNECTION_CHANGED_REASON_TYPE reason)
        {
            _deviceManagerSample.Logger.UpdateLog("OnConnectionStateChanged");
        }

        public override void OnNetworkTypeChanged(NETWORK_TYPE type)
        {
            _deviceManagerSample.Logger.UpdateLog("OnNetworkTypeChanged");
        }

        public override void OnLocalUserRegistered(uint uid, string userAccount)
        {
            _deviceManagerSample.Logger.UpdateLog("OnLocalUserRegistered");
        }

        public override void OnUserInfoUpdated(uint uid, UserInfo info)
        {
            _deviceManagerSample.Logger.UpdateLog("OnUserInfoUpdated");
        }

        public override void OnUploadLogResult(string requestId, bool success, UPLOAD_ERROR_REASON reason)
        {
            _deviceManagerSample.Logger.UpdateLog("OnUploadLogResult");
        }

        public override bool OnReadyToSendMetadata(Metadata metadata)
        {
            _deviceManagerSample.Logger.UpdateLog("OnReadyToSendMetadata");
            return true;
        }

        public override void OnMetadataReceived(Metadata metadata)
        {
            _deviceManagerSample.Logger.UpdateLog("OnMetadataReceived");
        }
    }
}
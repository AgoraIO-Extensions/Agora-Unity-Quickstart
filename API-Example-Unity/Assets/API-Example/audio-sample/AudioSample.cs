using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using UnityEngine.Serialization;


namespace AudioSample
{
    public class AudioSample : MonoBehaviour
    {
        [FormerlySerializedAs("APP_ID")] [SerializeField]
        private string appID = "YOUR_APPID";

        [FormerlySerializedAs("TOKEN")] [SerializeField]
        private string token = "";

        [FormerlySerializedAs("CHANNEL_NAME")] [SerializeField]
        private string channelName = "YOUR_CHANNEL_NAME";

        public Text logText;
        internal Logger Logger;
        internal IAgoraRtcEngine AgoraRtcEngine = null;

        // Start is called before the first frame update
        private void Start()
        {
            CheckAppId();
            InitRtcEngine();
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

        private void JoinChannel()
        {
            AgoraRtcEngine.EnableAudio();
            AgoraRtcEngine.JoinChannel(token, channelName, "");
        }

        private void OnLeaveBtnClick()
        {
            AgoraRtcEngine.LeaveChannel();
        }
    }

    internal class UserEventHandler : IAgoraRtcEngineEventHandler
    {
        private AudioSample _helloVideoAgora;

        internal UserEventHandler(AudioSample helloVideoAgora)
        {
            _helloVideoAgora = helloVideoAgora;
        }

        public override void OnWarning(int warn, string msg)
        {
            _helloVideoAgora.Logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _helloVideoAgora.Logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(string channel, uint uid, int elapsed)
        {
            _helloVideoAgora.Logger.UpdateLog(string.Format("sdk version: ${0}",
                _helloVideoAgora.AgoraRtcEngine.GetVersion()));
            _helloVideoAgora.Logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channel, uid, elapsed));
        }

        public override void OnRejoinChannelSuccess(string channel, uint uid, int elapsed)
        {
            _helloVideoAgora.Logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcStats stats)
        {
            _helloVideoAgora.Logger.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _helloVideoAgora.Logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(uint uid, int elapsed)
        {
            _helloVideoAgora.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        }

        public override void OnUserOffline(uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _helloVideoAgora.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int) reason));
        }

        public override void OnLastmileQuality(int quality)
        {
            _helloVideoAgora.Logger.UpdateLog("OnLastmileQuality");
        }

        public override void OnLastmileProbeResult(LastmileProbeResult result)
        {
            _helloVideoAgora.Logger.UpdateLog("OnLastmileProbeResult");
        }

        public override void OnConnectionInterrupted()
        {
            _helloVideoAgora.Logger.UpdateLog("OnConnectionInterrupted");
        }

        public override void OnConnectionLost()
        {
            _helloVideoAgora.Logger.UpdateLog("OnConnectionLost");
        }

        public override void OnConnectionBanned()
        {
            _helloVideoAgora.Logger.UpdateLog("OnConnectionBanned");
        }

        public override void OnApiCallExecuted(int err, string api, string result)
        {
            _helloVideoAgora.Logger.UpdateLog("OnApiCallExecuted");
        }

        public override void OnRequestToken()
        {
            _helloVideoAgora.Logger.UpdateLog("OnRequestToken");
        }

        public override void OnTokenPrivilegeWillExpire(string token)
        {
            _helloVideoAgora.Logger.UpdateLog("OnTokenPrivilegeWillExpire");
        }

        public override void OnAudioQuality(uint uid, int quality, ushort delay, ushort lost)
        {
            _helloVideoAgora.Logger.UpdateLog("OnAudioQuality");
        }

        public override void OnRtcStats(RtcStats stats)
        {
            _helloVideoAgora.Logger.UpdateLog("OnRtcStats");
        }

        public override void OnNetworkQuality(uint uid, int txQuality, int rxQuality)
        {
            _helloVideoAgora.Logger.UpdateLog("OnNetworkQuality");
        }

        public override void OnLocalVideoStats(LocalVideoStats stats)
        {
            _helloVideoAgora.Logger.UpdateLog("OnLocalVideoStats");
        }

        public override void OnRemoteVideoStats(RemoteVideoStats stats)
        {
            _helloVideoAgora.Logger.UpdateLog("OnRemoteVideoStats");
        }

        public override void OnLocalAudioStats(LocalAudioStats stats)
        {
            _helloVideoAgora.Logger.UpdateLog("OnLocalAudioStats");
        }

        public override void OnRemoteAudioStats(RemoteAudioStats stats)
        {
            _helloVideoAgora.Logger.UpdateLog("OnRemoteAudioStats");
        }

        public override void OnLocalAudioStateChanged(LOCAL_AUDIO_STREAM_STATE state, LOCAL_AUDIO_STREAM_ERROR error)
        {
            _helloVideoAgora.Logger.UpdateLog("OnLocalAudioStateChanged");
        }

        public override void OnRemoteAudioStateChanged(uint uid, REMOTE_AUDIO_STATE state,
            REMOTE_AUDIO_STATE_REASON reason, int elapsed)
        {
            _helloVideoAgora.Logger.UpdateLog("OnRemoteAudioStateChanged");
        }

        public override void OnAudioPublishStateChanged(string channel, STREAM_PUBLISH_STATE oldState,
            STREAM_PUBLISH_STATE newState, int elapseSinceLastState)
        {
            _helloVideoAgora.Logger.UpdateLog("OnAudioPublishStateChanged");
        }

        public override void OnVideoPublishStateChanged(string channel, STREAM_PUBLISH_STATE oldState,
            STREAM_PUBLISH_STATE newState, int elapseSinceLastState)
        {
            _helloVideoAgora.Logger.UpdateLog("OnVideoPublishStateChanged");
        }

        public override void OnAudioSubscribeStateChanged(string channel, uint uid, STREAM_SUBSCRIBE_STATE oldState,
            STREAM_SUBSCRIBE_STATE newState, int elapseSinceLastState)
        {
            _helloVideoAgora.Logger.UpdateLog("OnAudioSubscribeStateChanged");
        }

        public override void OnVideoSubscribeStateChanged(string channel, uint uid, STREAM_SUBSCRIBE_STATE oldState,
            STREAM_SUBSCRIBE_STATE newState, int elapseSinceLastState)
        {
            _helloVideoAgora.Logger.UpdateLog("OnVideoSubscribeStateChanged");
        }

        public override void OnAudioVolumeIndication(AudioVolumeInfo[] speakers, uint speakerNumber, int totalVolume)
        {
            _helloVideoAgora.Logger.UpdateLog("OnAudioVolumeIndication");
        }

        public override void OnActiveSpeaker(uint uid)
        {
            _helloVideoAgora.Logger.UpdateLog("OnActiveSpeaker");
        }

        public override void OnVideoStopped()
        {
            _helloVideoAgora.Logger.UpdateLog("OnVideoStopped");
        }

        public override void OnFirstLocalVideoFrame(int width, int height, int elapsed)
        {
            _helloVideoAgora.Logger.UpdateLog("OnFirstLocalVideoFrame");
        }

        public override void OnFirstLocalVideoFramePublished(int elapsed)
        {
            _helloVideoAgora.Logger.UpdateLog("OnFirstLocalVideoFramePublished");
        }

        public override void OnFirstRemoteVideoDecoded(uint uid, int width, int height, int elapsed)
        {
            _helloVideoAgora.Logger.UpdateLog("OnFirstRemoteVideoDecoded");
        }

        public override void OnFirstRemoteVideoFrame(uint uid, int width, int height, int elapsed)
        {
            _helloVideoAgora.Logger.UpdateLog("OnFirstRemoteVideoFrame");
        }

        public override void OnUserMuteAudio(uint uid, bool muted)
        {
            _helloVideoAgora.Logger.UpdateLog("OnUserMuteAudio");
        }

        public override void OnUserMuteVideo(uint uid, bool muted)
        {
            _helloVideoAgora.Logger.UpdateLog("OnUserMuteVideo");
        }

        public override void OnUserEnableVideo(uint uid, bool enabled)
        {
            _helloVideoAgora.Logger.UpdateLog("OnUserEnableVideo");
        }

        public override void OnAudioDeviceStateChanged(string deviceId, int deviceType, int deviceState)
        {
            _helloVideoAgora.Logger.UpdateLog("OnAudioDeviceStateChanged");
        }

        public override void OnAudioDeviceVolumeChanged(MEDIA_DEVICE_TYPE deviceType, int volume, bool muted)
        {
            _helloVideoAgora.Logger.UpdateLog("OnAudioDeviceVolumeChanged");
        }

        public override void OnCameraReady()
        {
            _helloVideoAgora.Logger.UpdateLog("OnCameraReady");
        }

        public override void OnCameraFocusAreaChanged(int x, int y, int width, int height)
        {
            _helloVideoAgora.Logger.UpdateLog("OnCameraFocusAreaChanged");
        }

        public override void OnFacePositionChanged(int imageWidth, int imageHeight, Rectangle vecRectangle,
            int[] vecDistance, int numFaces)
        {
            _helloVideoAgora.Logger.UpdateLog("OnFacePositionChanged");
        }

        public override void OnCameraExposureAreaChanged(int x, int y, int width, int height)
        {
            _helloVideoAgora.Logger.UpdateLog("OnCameraExposureAreaChanged");
        }

        public override void OnAudioMixingFinished()
        {
            _helloVideoAgora.Logger.UpdateLog("OnAudioMixingFinished");
        }

        public override void OnAudioMixingStateChanged(AUDIO_MIXING_STATE_TYPE state, AUDIO_MIXING_ERROR_TYPE errorCode)
        {
            _helloVideoAgora.Logger.UpdateLog("OnAudioMixingStateChanged");
        }

        public override void OnRemoteAudioMixingBegin()
        {
            _helloVideoAgora.Logger.UpdateLog("OnRemoteAudioMixingBegin");
        }

        public override void OnRemoteAudioMixingEnd()
        {
            _helloVideoAgora.Logger.UpdateLog("OnRemoteAudioMixingEnd");
        }

        public override void OnAudioEffectFinished(int soundId)
        {
            _helloVideoAgora.Logger.UpdateLog("OnAudioEffectFinished");
        }

        public override void OnFirstRemoteAudioDecoded(uint uid, int elapsed)
        {
            _helloVideoAgora.Logger.UpdateLog("OnFirstRemoteAudioDecoded");
        }

        public override void OnVideoDeviceStateChanged(string deviceId, int deviceType, int deviceState)
        {
            _helloVideoAgora.Logger.UpdateLog("OnVideoDeviceStateChanged");
        }

        public override void OnLocalVideoStateChanged(LOCAL_VIDEO_STREAM_STATE localVideoState,
            LOCAL_VIDEO_STREAM_ERROR error)
        {
            _helloVideoAgora.Logger.UpdateLog("OnLocalVideoStateChanged");
        }

        public override void OnVideoSizeChanged(uint uid, int width, int height, int rotation)
        {
            _helloVideoAgora.Logger.UpdateLog("OnVideoSizeChanged");
        }

        public override void OnRemoteVideoStateChanged(uint uid, REMOTE_VIDEO_STATE state,
            REMOTE_VIDEO_STATE_REASON reason, int elapsed)
        {
            _helloVideoAgora.Logger.UpdateLog("OnRemoteVideoStateChanged");
        }

        public override void OnUserEnableLocalVideo(uint uid, bool enabled)
        {
            _helloVideoAgora.Logger.UpdateLog("OnUserEnableLocalVideo");
        }

        public override void OnStreamMessage(uint uid, int streamId, byte[] data, uint length)
        {
            _helloVideoAgora.Logger.UpdateLog("OnStreamMessage");
        }

        public override void OnStreamMessageError(uint uid, int streamId, int code, int missed, int cached)
        {
            _helloVideoAgora.Logger.UpdateLog("OnStreamMessageError");
        }

        public override void OnMediaEngineLoadSuccess()
        {
            _helloVideoAgora.Logger.UpdateLog("OnMediaEngineLoadSuccess");
        }

        public override void OnMediaEngineStartCallSuccess()
        {
            _helloVideoAgora.Logger.UpdateLog("OnMediaEngineStartCallSuccess");
        }

        public override void OnUserSuperResolutionEnabled(uint uid, bool enabled, SUPER_RESOLUTION_STATE_REASON reason)
        {
            _helloVideoAgora.Logger.UpdateLog("OnUserSuperResolutionEnabled");
        }

        public override void OnChannelMediaRelayStateChanged(CHANNEL_MEDIA_RELAY_STATE state,
            CHANNEL_MEDIA_RELAY_ERROR code)
        {
            _helloVideoAgora.Logger.UpdateLog("OnChannelMediaRelayStateChanged");
        }

        public override void OnChannelMediaRelayEvent(CHANNEL_MEDIA_RELAY_EVENT code)
        {
            _helloVideoAgora.Logger.UpdateLog("OnChannelMediaRelayEvent");
        }

        public override void OnFirstLocalAudioFrame(int elapsed)
        {
            _helloVideoAgora.Logger.UpdateLog("OnFirstLocalAudioFrame");
        }

        public override void OnFirstLocalAudioFramePublished(int elapsed)
        {
            _helloVideoAgora.Logger.UpdateLog("OnFirstLocalAudioFramePublished");
        }

        public override void OnFirstRemoteAudioFrame(uint uid, int elapsed)
        {
            _helloVideoAgora.Logger.UpdateLog("OnFirstRemoteAudioFrame");
        }

        public override void OnRtmpStreamingStateChanged(string url, RTMP_STREAM_PUBLISH_STATE state,
            RTMP_STREAM_PUBLISH_ERROR errCode)
        {
            _helloVideoAgora.Logger.UpdateLog("OnRtmpStreamingStateChanged");
        }

        public override void OnRtmpStreamingEvent(string url, RTMP_STREAMING_EVENT eventCode)
        {
            _helloVideoAgora.Logger.UpdateLog("OnRtmpStreamingEvent");
        }

        public override void OnStreamPublished(string url, int error)
        {
            _helloVideoAgora.Logger.UpdateLog("OnStreamPublished");
        }

        public override void OnStreamUnpublished(string url)
        {
            _helloVideoAgora.Logger.UpdateLog("OnStreamUnpublished");
        }

        public override void OnTranscodingUpdated()
        {
            _helloVideoAgora.Logger.UpdateLog("OnTranscodingUpdated");
        }

        public override void OnStreamInjectedStatus(string url, uint uid, int status)
        {
            _helloVideoAgora.Logger.UpdateLog("OnStreamInjectedStatus");
        }

        public override void OnAudioRouteChanged(AUDIO_ROUTE_TYPE routing)
        {
            _helloVideoAgora.Logger.UpdateLog("OnAudioRouteChanged");
        }

        public override void OnLocalPublishFallbackToAudioOnly(bool isFallbackOrRecover)
        {
            _helloVideoAgora.Logger.UpdateLog("OnLocalPublishFallbackToAudioOnly");
        }

        public override void OnRemoteSubscribeFallbackToAudioOnly(uint uid, bool isFallbackOrRecover)
        {
            _helloVideoAgora.Logger.UpdateLog("OnRemoteSubscribeFallbackToAudioOnly");
        }

        public override void OnRemoteAudioTransportStats(uint uid, ushort delay, ushort lost, ushort rxKBitRate)
        {
            _helloVideoAgora.Logger.UpdateLog("OnRemoteAudioTransportStats");
        }

        public override void OnRemoteVideoTransportStats(uint uid, ushort delay, ushort lost, ushort rxKBitRate)
        {
            _helloVideoAgora.Logger.UpdateLog("OnRemoteVideoTransportStats");
        }

        public override void OnMicrophoneEnabled(bool enabled)
        {
            _helloVideoAgora.Logger.UpdateLog("OnMicrophoneEnabled");
        }

        public override void OnConnectionStateChanged(CONNECTION_STATE_TYPE state,
            CONNECTION_CHANGED_REASON_TYPE reason)
        {
            _helloVideoAgora.Logger.UpdateLog("OnConnectionStateChanged");
        }

        public override void OnNetworkTypeChanged(NETWORK_TYPE type)
        {
            _helloVideoAgora.Logger.UpdateLog("OnNetworkTypeChanged");
        }

        public override void OnLocalUserRegistered(uint uid, string userAccount)
        {
            _helloVideoAgora.Logger.UpdateLog("OnLocalUserRegistered");
        }

        public override void OnUserInfoUpdated(uint uid, UserInfo info)
        {
            _helloVideoAgora.Logger.UpdateLog("OnUserInfoUpdated");
        }

        public override void OnUploadLogResult(string requestId, bool success, UPLOAD_ERROR_REASON reason)
        {
            _helloVideoAgora.Logger.UpdateLog("OnUploadLogResult");
        }

        public override bool OnReadyToSendMetadata(Metadata metadata)
        {
            _helloVideoAgora.Logger.UpdateLog("OnReadyToSendMetadata");
            return true;
        }

        public override void OnMetadataReceived(Metadata metadata)
        {
            _helloVideoAgora.Logger.UpdateLog("OnMetadataReceived");
        }
    }
}
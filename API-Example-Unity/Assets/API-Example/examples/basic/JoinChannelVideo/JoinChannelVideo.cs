using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using UnityEngine.Serialization;
using Logger = agora.util.Logger;


namespace JoinChannelVideo
{
    public class JoinChannelVideo : MonoBehaviour
    {
        [FormerlySerializedAs("APP_ID")] [SerializeField]
        private string appID = "";

        [FormerlySerializedAs("TOKEN")] [SerializeField]
        private string token = "";

        [FormerlySerializedAs("CHANNEL_NAME")] [SerializeField]
        private string channelName = "YOUR_CHANNEL_NAME";

        public Text logText;
        internal Logger Logger;
        internal IAgoraRtcEngine AgoraRtcEngine;
        private const float Offset = 100;

        // Use this for initialization
        private void Start()
        {
            CheckAppId();
            InitEngine();
            JoinChannel();
        }

        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        private void CheckAppId()
        {
            Logger = new Logger(logText);
            Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
        }

        private void InitEngine()
        {
            AgoraRtcEngine = agora.rtc.AgoraRtcEngine.CreateAgoraRtcEngine();
            AgoraRtcEngine.Initialize(new RtcEngineContext(appID, AREA_CODE.AREA_CODE_GLOB, new LogConfig("log.txt")));
            AgoraRtcEngine.InitEventHandler(new UserEventHandler(this));
        }

        private void JoinChannel()
        {
            AgoraRtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
            AgoraRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            AgoraRtcEngine.EnableAudio();
            AgoraRtcEngine.EnableVideo();
            AgoraRtcEngine.JoinChannel(token, channelName, "");
        }

        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            if (AgoraRtcEngine == null) return;
            AgoraRtcEngine.LeaveChannel();
            AgoraRtcEngine.Dispose();
        }

        internal string GetChannelName()
        {
            return channelName;
        }

        internal static void DestroyVideoView(uint uid)
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                Destroy(go);
            }
        }

        internal static void MakeVideoView(uint uid, string channelId = "")
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            // create a GameObject and assign to this new user
            var videoSurface = MakeImageSurface(uid.ToString());
            if (ReferenceEquals(videoSurface, null)) return;
            // configure videoSurface
            videoSurface.SetForUser(uid, channelId);
            videoSurface.SetEnable(true);
        }

        // VIDEO TYPE 1: 3D Object
        private VideoSurface MakePlaneSurface(string goName)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // set up transform
            go.transform.Rotate(-90.0f, 0.0f, 0.0f);
            var yPos = Random.Range(3.0f, 5.0f);
            var xPos = Random.Range(-2.0f, 2.0f);
            go.transform.position = new Vector3(xPos, yPos, 0f);
            go.transform.localScale = new Vector3(0.25f, 0.5f, .5f);

            // configure videoSurface
            var videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

        // Video TYPE 2: RawImage
        private static VideoSurface MakeImageSurface(string goName)
        {
            GameObject go = new GameObject();

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // to be renderered onto
            go.AddComponent<RawImage>();
            // make the object draggable
            go.AddComponent<UIElementDrag>();
            var canvas = GameObject.Find("VideoCanvas");
            if (canvas != null)
            {
                go.transform.parent = canvas.transform;
                Debug.Log("add video view");
            }
            else
            {
                Debug.Log("Canvas is null video view");
            }

            // set up transform
            go.transform.Rotate(0f, 0.0f, 180.0f);
            var xPos = Random.Range(Offset - Screen.width / 2f, Screen.width / 2f - Offset);
            var yPos = Random.Range(Offset, Screen.height / 2f - Offset);
            Debug.Log("position x " + xPos + " y: " + yPos);
            go.transform.localPosition = new Vector3(xPos, yPos, 0f);
            go.transform.localScale = new Vector3(3f, 4f, 1f);

            // configure videoSurface
            var videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }
    }

    internal class UserEventHandler : IAgoraRtcEngineEventHandler
    {
        private readonly JoinChannelVideo _videoSample;

        internal UserEventHandler(JoinChannelVideo videoSample)
        {
            _videoSample = videoSample;
        }

        public override void OnWarning(int warn, string msg)
        {
            _videoSample.Logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _videoSample.Logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(string channel, uint uid, int elapsed)
        {
            _videoSample.Logger.UpdateLog(string.Format("sdk version: ${0}",
                _videoSample.AgoraRtcEngine.GetVersion()));
            _videoSample.Logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channel, uid, elapsed));
            JoinChannelVideo.MakeVideoView(0);
        }

        public override void OnRejoinChannelSuccess(string channel, uint uid, int elapsed)
        {
            _videoSample.Logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcStats stats)
        {
            _videoSample.Logger.UpdateLog("OnLeaveChannel");
            JoinChannelVideo.DestroyVideoView(0);
        }

        public override void OnClientRoleChanged(CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _videoSample.Logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(uint uid, int elapsed)
        {
            _videoSample.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            JoinChannelVideo.MakeVideoView(uid, _videoSample.GetChannelName());
        }

        public override void OnUserOffline(uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _videoSample.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int) reason));
            JoinChannelVideo.DestroyVideoView(uid);
        }

        public override void OnLastmileQuality(int quality)
        {
            _videoSample.Logger.UpdateLog("OnLastmileQuality");
        }

        public override void OnLastmileProbeResult(LastmileProbeResult result)
        {
            _videoSample.Logger.UpdateLog("OnLastmileProbeResult");
        }

        public override void OnConnectionInterrupted()
        {
            _videoSample.Logger.UpdateLog("OnConnectionInterrupted");
        }

        public override void OnConnectionLost()
        {
            _videoSample.Logger.UpdateLog("OnConnectionLost");
        }

        public override void OnConnectionBanned()
        {
            _videoSample.Logger.UpdateLog("OnConnectionBanned");
        }

        public override void OnApiCallExecuted(int err, string api, string result)
        {
        }

        public override void OnRequestToken()
        {
            _videoSample.Logger.UpdateLog("OnRequestToken");
        }

        public override void OnTokenPrivilegeWillExpire(string token)
        {
            _videoSample.Logger.UpdateLog("OnTokenPrivilegeWillExpire");
        }

        public override void OnAudioQuality(uint uid, int quality, ushort delay, ushort lost)
        {
            _videoSample.Logger.UpdateLog("OnAudioQuality");
        }

        public override void OnRtcStats(RtcStats stats)
        {
            _videoSample.Logger.UpdateLog("OnRtcStats");
        }

        public override void OnNetworkQuality(uint uid, int txQuality, int rxQuality)
        {
            _videoSample.Logger.UpdateLog("OnNetworkQuality");
        }

        public override void OnLocalVideoStats(LocalVideoStats stats)
        {
            _videoSample.Logger.UpdateLog("OnLocalVideoStats");
        }

        public override void OnRemoteVideoStats(RemoteVideoStats stats)
        {
            _videoSample.Logger.UpdateLog("OnRemoteVideoStats");
        }

        public override void OnLocalAudioStats(LocalAudioStats stats)
        {
            _videoSample.Logger.UpdateLog("OnLocalAudioStats");
        }

        public override void OnRemoteAudioStats(RemoteAudioStats stats)
        {
            _videoSample.Logger.UpdateLog("OnRemoteAudioStats");
        }

        public override void OnLocalAudioStateChanged(LOCAL_AUDIO_STREAM_STATE state, LOCAL_AUDIO_STREAM_ERROR error)
        {
            _videoSample.Logger.UpdateLog("OnLocalAudioStateChanged");
        }

        public override void OnRemoteAudioStateChanged(uint uid, REMOTE_AUDIO_STATE state,
            REMOTE_AUDIO_STATE_REASON reason, int elapsed)
        {
            _videoSample.Logger.UpdateLog("OnRemoteAudioStateChanged");
        }

        public override void OnAudioPublishStateChanged(string channel, STREAM_PUBLISH_STATE oldState,
            STREAM_PUBLISH_STATE newState, int elapseSinceLastState)
        {
            _videoSample.Logger.UpdateLog("OnAudioPublishStateChanged");
        }

        public override void OnVideoPublishStateChanged(string channel, STREAM_PUBLISH_STATE oldState,
            STREAM_PUBLISH_STATE newState, int elapseSinceLastState)
        {
            _videoSample.Logger.UpdateLog("OnVideoPublishStateChanged");
        }

        public override void OnAudioSubscribeStateChanged(string channel, uint uid, STREAM_SUBSCRIBE_STATE oldState,
            STREAM_SUBSCRIBE_STATE newState, int elapseSinceLastState)
        {
            _videoSample.Logger.UpdateLog("OnAudioSubscribeStateChanged");
        }

        public override void OnVideoSubscribeStateChanged(string channel, uint uid, STREAM_SUBSCRIBE_STATE oldState,
            STREAM_SUBSCRIBE_STATE newState, int elapseSinceLastState)
        {
            _videoSample.Logger.UpdateLog("OnVideoSubscribeStateChanged");
        }

        public override void OnAudioVolumeIndication(AudioVolumeInfo[] speakers, uint speakerNumber, int totalVolume)
        {
            _videoSample.Logger.UpdateLog("OnAudioVolumeIndication");
        }

        public override void OnActiveSpeaker(uint uid)
        {
            _videoSample.Logger.UpdateLog("OnActiveSpeaker");
        }

        public override void OnVideoStopped()
        {
            _videoSample.Logger.UpdateLog("OnVideoStopped");
        }

        public override void OnFirstLocalVideoFrame(int width, int height, int elapsed)
        {
            _videoSample.Logger.UpdateLog("OnFirstLocalVideoFrame");
        }

        public override void OnFirstLocalVideoFramePublished(int elapsed)
        {
            _videoSample.Logger.UpdateLog("OnFirstLocalVideoFramePublished");
        }

        public override void OnFirstRemoteVideoDecoded(uint uid, int width, int height, int elapsed)
        {
            _videoSample.Logger.UpdateLog("OnFirstRemoteVideoDecoded");
        }

        public override void OnFirstRemoteVideoFrame(uint uid, int width, int height, int elapsed)
        {
            _videoSample.Logger.UpdateLog("OnFirstRemoteVideoFrame");
        }

        public override void OnUserMuteAudio(uint uid, bool muted)
        {
            _videoSample.Logger.UpdateLog("OnUserMuteAudio");
        }

        public override void OnUserMuteVideo(uint uid, bool muted)
        {
            _videoSample.Logger.UpdateLog("OnUserMuteVideo");
        }

        public override void OnUserEnableVideo(uint uid, bool enabled)
        {
            _videoSample.Logger.UpdateLog("OnUserEnableVideo");
        }

        public override void OnAudioDeviceStateChanged(string deviceId, int deviceType, int deviceState)
        {
            _videoSample.Logger.UpdateLog("OnAudioDeviceStateChanged");
        }

        public override void OnAudioDeviceVolumeChanged(MEDIA_DEVICE_TYPE deviceType, int volume, bool muted)
        {
            _videoSample.Logger.UpdateLog("OnAudioDeviceVolumeChanged");
        }

        public override void OnCameraReady()
        {
            _videoSample.Logger.UpdateLog("OnCameraReady");
        }

        public override void OnCameraFocusAreaChanged(int x, int y, int width, int height)
        {
            _videoSample.Logger.UpdateLog("OnCameraFocusAreaChanged");
        }

        public override void OnFacePositionChanged(int imageWidth, int imageHeight, Rectangle vecRectangle,
            int[] vecDistance, int numFaces)
        {
            _videoSample.Logger.UpdateLog("OnFacePositionChanged");
        }

        public override void OnCameraExposureAreaChanged(int x, int y, int width, int height)
        {
            _videoSample.Logger.UpdateLog("OnCameraExposureAreaChanged");
        }

        public override void OnAudioMixingFinished()
        {
            _videoSample.Logger.UpdateLog("OnAudioMixingFinished");
        }

        public override void OnAudioMixingStateChanged(AUDIO_MIXING_STATE_TYPE state, AUDIO_MIXING_REASON_TYPE reason)
        {
            _videoSample.Logger.UpdateLog("OnAudioMixingStateChanged");
        }

        public override void OnRemoteAudioMixingBegin()
        {
            _videoSample.Logger.UpdateLog("OnRemoteAudioMixingBegin");
        }

        public override void OnRemoteAudioMixingEnd()
        {
            _videoSample.Logger.UpdateLog("OnRemoteAudioMixingEnd");
        }

        public override void OnAudioEffectFinished(int soundId)
        {
            _videoSample.Logger.UpdateLog("OnAudioEffectFinished");
        }

        public override void OnFirstRemoteAudioDecoded(uint uid, int elapsed)
        {
            _videoSample.Logger.UpdateLog("OnFirstRemoteAudioDecoded");
        }

        public override void OnVideoDeviceStateChanged(string deviceId, int deviceType, int deviceState)
        {
            _videoSample.Logger.UpdateLog("OnVideoDeviceStateChanged");
        }

        public override void OnLocalVideoStateChanged(LOCAL_VIDEO_STREAM_STATE localVideoState,
            LOCAL_VIDEO_STREAM_ERROR error)
        {
            _videoSample.Logger.UpdateLog("OnLocalVideoStateChanged");
        }

        public override void OnVideoSizeChanged(uint uid, int width, int height, int rotation)
        {
            _videoSample.Logger.UpdateLog("OnVideoSizeChanged");
        }

        public override void OnRemoteVideoStateChanged(uint uid, REMOTE_VIDEO_STATE state,
            REMOTE_VIDEO_STATE_REASON reason, int elapsed)
        {
            _videoSample.Logger.UpdateLog("OnRemoteVideoStateChanged");
        }

        public override void OnUserEnableLocalVideo(uint uid, bool enabled)
        {
            _videoSample.Logger.UpdateLog("OnUserEnableLocalVideo");
        }

        public override void OnStreamMessage(uint uid, int streamId, byte[] data, uint length)
        {
            _videoSample.Logger.UpdateLog("OnStreamMessage");
        }

        public override void OnStreamMessageError(uint uid, int streamId, int code, int missed, int cached)
        {
            _videoSample.Logger.UpdateLog("OnStreamMessageError");
        }

        public override void OnMediaEngineLoadSuccess()
        {
            _videoSample.Logger.UpdateLog("OnMediaEngineLoadSuccess");
        }

        public override void OnMediaEngineStartCallSuccess()
        {
            _videoSample.Logger.UpdateLog("OnMediaEngineStartCallSuccess");
        }

        public override void OnUserSuperResolutionEnabled(uint uid, bool enabled, SUPER_RESOLUTION_STATE_REASON reason)
        {
            _videoSample.Logger.UpdateLog("OnUserSuperResolutionEnabled");
        }

        public override void OnChannelMediaRelayStateChanged(CHANNEL_MEDIA_RELAY_STATE state,
            CHANNEL_MEDIA_RELAY_ERROR code)
        {
            _videoSample.Logger.UpdateLog("OnChannelMediaRelayStateChanged");
        }

        public override void OnChannelMediaRelayEvent(CHANNEL_MEDIA_RELAY_EVENT code)
        {
            _videoSample.Logger.UpdateLog("OnChannelMediaRelayEvent");
        }

        public override void OnFirstLocalAudioFrame(int elapsed)
        {
            _videoSample.Logger.UpdateLog("OnFirstLocalAudioFrame");
        }

        public override void OnFirstLocalAudioFramePublished(int elapsed)
        {
            _videoSample.Logger.UpdateLog("OnFirstLocalAudioFramePublished");
        }

        public override void OnFirstRemoteAudioFrame(uint uid, int elapsed)
        {
            _videoSample.Logger.UpdateLog("OnFirstRemoteAudioFrame");
        }

        public override void OnRtmpStreamingStateChanged(string url, RTMP_STREAM_PUBLISH_STATE state,
            RTMP_STREAM_PUBLISH_ERROR_TYPE errCode)
        {
            _videoSample.Logger.UpdateLog("OnRtmpStreamingStateChanged");
        }

        public override void OnRtmpStreamingEvent(string url, RTMP_STREAMING_EVENT eventCode)
        {
            _videoSample.Logger.UpdateLog("OnRtmpStreamingEvent");
        }

        public override void OnStreamPublished(string url, int error)
        {
            _videoSample.Logger.UpdateLog("OnStreamPublished");
        }

        public override void OnStreamUnpublished(string url)
        {
            _videoSample.Logger.UpdateLog("OnStreamUnpublished");
        }

        public override void OnTranscodingUpdated()
        {
            _videoSample.Logger.UpdateLog("OnTranscodingUpdated");
        }

        public override void OnStreamInjectedStatus(string url, uint uid, int status)
        {
            _videoSample.Logger.UpdateLog("OnStreamInjectedStatus");
        }

        public override void OnAudioRouteChanged(AUDIO_ROUTE_TYPE routing)
        {
            _videoSample.Logger.UpdateLog("OnAudioRouteChanged");
        }

        public override void OnLocalPublishFallbackToAudioOnly(bool isFallbackOrRecover)
        {
            _videoSample.Logger.UpdateLog("OnLocalPublishFallbackToAudioOnly");
        }

        public override void OnRemoteSubscribeFallbackToAudioOnly(uint uid, bool isFallbackOrRecover)
        {
            _videoSample.Logger.UpdateLog("OnRemoteSubscribeFallbackToAudioOnly");
        }

        public override void OnRemoteAudioTransportStats(uint uid, ushort delay, ushort lost, ushort rxKBitRate)
        {
            _videoSample.Logger.UpdateLog("OnRemoteAudioTransportStats");
        }

        public override void OnRemoteVideoTransportStats(uint uid, ushort delay, ushort lost, ushort rxKBitRate)
        {
            _videoSample.Logger.UpdateLog("OnRemoteVideoTransportStats");
        }

        public override void OnMicrophoneEnabled(bool enabled)
        {
            _videoSample.Logger.UpdateLog("OnMicrophoneEnabled");
        }

        public override void OnConnectionStateChanged(CONNECTION_STATE_TYPE state,
            CONNECTION_CHANGED_REASON_TYPE reason)
        {
            _videoSample.Logger.UpdateLog("OnConnectionStateChanged");
        }

        public override void OnNetworkTypeChanged(NETWORK_TYPE type)
        {
            _videoSample.Logger.UpdateLog("OnNetworkTypeChanged");
        }

        public override void OnLocalUserRegistered(uint uid, string userAccount)
        {
            _videoSample.Logger.UpdateLog("OnLocalUserRegistered");
        }

        public override void OnUserInfoUpdated(uint uid, UserInfo info)
        {
            _videoSample.Logger.UpdateLog("OnUserInfoUpdated");
        }

        public override void OnUploadLogResult(string requestId, bool success, UPLOAD_ERROR_REASON reason)
        {
            _videoSample.Logger.UpdateLog("OnUploadLogResult");
        }

        public override bool OnReadyToSendMetadata(Metadata metadata)
        {
            _videoSample.Logger.UpdateLog("OnReadyToSendMetadata");
            return true;
        }

        public override void OnMetadataReceived(Metadata metadata)
        {
            _videoSample.Logger.UpdateLog("OnMetadataReceived");
        }
    }
}
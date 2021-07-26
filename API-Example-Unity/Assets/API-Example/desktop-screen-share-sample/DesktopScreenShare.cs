#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using agora_utilities;
using UnityEngine.Serialization;
using Logger = agora_utilities.Logger;
using Random = UnityEngine.Random;

namespace DesktopScreenShare
{
    public class DesktopScreenShare : MonoBehaviour
    {
        [FormerlySerializedAs("APP_ID")] [SerializeField]
        private string appID = "";

        [FormerlySerializedAs("TOKEN")] [SerializeField]
        private string token = "";

        [FormerlySerializedAs("CHANNEL_NAME")] [SerializeField]
        private string channelName = "YOUR_CHANNEL_NAME";

        internal Dictionary<AgoraEngineType, IAgoraRtcEngine> AgoraRtcEngineDict =
            new Dictionary<AgoraEngineType, IAgoraRtcEngine>();

        private const float Offset = 100;
        public Text logText;
        internal Logger Logger;
        private Dropdown _winIdSelect;
        private Button _startShareBtn;
        private Button _stopShareBtn;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    private Dictionary<uint, Rectangle> _dispRect;
#endif

        // Use this for initialization
        private void Start()
        {
#if UNITY_IPHONE || UNITY_ANDROID
            throw new PlatformNotSupportedException();
#else
            Logger = new Logger(logText);
            CheckAppId();
            InitEngine();
            JoinChannel();
            InitEngine(AgoraEngineType.SubProcess);
            JoinChannel(AgoraEngineType.SubProcess);
            PrepareScreenCapture();
#endif
        }

        private void CheckAppId()
        {
            Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
        }

        private void JoinChannel(AgoraEngineType agoraEngineType = AgoraEngineType.MainProcess)
        {
            AgoraRtcEngineDict[agoraEngineType].EnableAudio();
            AgoraRtcEngineDict[agoraEngineType].EnableVideo();
            AgoraRtcEngineDict[agoraEngineType].JoinChannel(token, channelName, "");
        }

        private void InitEngine(AgoraEngineType agoraEngineType = AgoraEngineType.MainProcess)
        {
            AgoraRtcEngineDict[agoraEngineType] = AgoraRtcEngine.CreateAgoraRtcEngine(agoraEngineType);
            AgoraRtcEngineDict[agoraEngineType]
                .Initialize(new RtcEngineContext(appID, AREA_CODE.AREA_CODE_GLOB, new LogConfig("log.txt")));
            if (agoraEngineType == AgoraEngineType.SubProcess)
            {
                // Optional Choice (for reducing unnecessary cost)
                AgoraRtcEngineDict[agoraEngineType].MuteAllRemoteAudioStreams(true);
                AgoraRtcEngineDict[agoraEngineType].MuteAllRemoteVideoStreams(true);
            }

            AgoraRtcEngineDict[agoraEngineType].InitEventHandler(new UserEventHandler(this, agoraEngineType));
        }


        private void PrepareScreenCapture()
        {
            _winIdSelect = GameObject.Find("winIdSelect").GetComponent<Dropdown>();

            if (_winIdSelect == null || AgoraRtcEngineDict[AgoraEngineType.SubProcess] == null) return;

            _winIdSelect.ClearOptions();

            var displayInfos = AgoraRtcEngineDict[AgoraEngineType.SubProcess].GetDisplayInfos();
            var windowInfos = AgoraRtcEngineDict[AgoraEngineType.SubProcess].GetWindowInfos();

            _winIdSelect.AddOptions(displayInfos.Select(w =>
                new Dropdown.OptionData(
                    string.Format("Display {0}", w.DisplayId))).ToList());
            _winIdSelect.AddOptions(windowInfos.Select(w =>
                    new Dropdown.OptionData(
                        string.Format("{0}-{1} | {2}", w.AppName, w.WindowName, w.WindowId)))
                .ToList());
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            _dispRect = new Dictionary<uint, Rectangle>();
            foreach (var displayInfo in displayInfos)
            {
                _dispRect[displayInfo.DisplayId] = displayInfo.WorkArea;
            }
#endif
            _startShareBtn = GameObject.Find("startShareBtn").GetComponent<Button>();
            _stopShareBtn = GameObject.Find("stopShareBtn").GetComponent<Button>();
            if (_startShareBtn != null) _startShareBtn.onClick.AddListener(OnStartShareBtnClick);
            if (_stopShareBtn != null)
            {
                _stopShareBtn.onClick.AddListener(OnStopShareBtnClick);
                _stopShareBtn.gameObject.SetActive(false);
            }
        }

        private void OnStartShareBtnClick()
        {
            if (AgoraRtcEngineDict[AgoraEngineType.SubProcess] == null) return;

            if (_startShareBtn != null) _startShareBtn.gameObject.SetActive(false);
            if (_stopShareBtn != null) _stopShareBtn.gameObject.SetActive(true);
            AgoraRtcEngineDict[AgoraEngineType.SubProcess].StopScreenCapture();

            if (_winIdSelect == null) return;
            var option = _winIdSelect.options[_winIdSelect.value].text;
            if (string.IsNullOrEmpty(option)) return;
            if (option.Contains("|"))
            {
                var windowId = option.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
                Logger.UpdateLog(string.Format(">>>>> Start sharing {0}", windowId));
                AgoraRtcEngineDict[AgoraEngineType.SubProcess]
                    .StartScreenCaptureByWindowId(ulong.Parse(windowId), default(Rectangle),
                        default(ScreenCaptureParameters));
            }
            else
            {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                var dispId = uint.Parse(option.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1]);
                Logger.UpdateLog(string.Format(">>>>> Start sharing display {0}", dispId));
                AgoraRtcEngineDict[AgoraEngineType.SubProcess].StartScreenCaptureByDisplayId(dispId, default(Rectangle),
                    new ScreenCaptureParameters {captureMouseCursor = true, frameRate = 30});
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            var diapFlag = uint.Parse(option.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1]);
            var screenRect = new Rectangle
            {
                x = _dispRect[diapFlag].x,
                y = _dispRect[diapFlag].y,
                width = _dispRect[diapFlag].width,
                height = _dispRect[diapFlag].height
            };
            Logger.UpdateLog(string.Format(">>>>> Start sharing display {0}: {1} {2} {3} {4}", diapFlag, screenRect.x,
                screenRect.y, screenRect.width, screenRect.height));
            var ret = AgoraRtcEngineDict[AgoraEngineType.SubProcess].StartScreenCaptureByScreenRect(screenRect,
                new Rectangle {x = 0, y = 0, width = 0, height = 0}, default(ScreenCaptureParameters));
#endif
            }
        }

        private void OnStopShareBtnClick()
        {
            if (_startShareBtn != null) _startShareBtn.gameObject.SetActive(true);
            if (_stopShareBtn != null) _stopShareBtn.gameObject.SetActive(false);
            AgoraRtcEngineDict[AgoraEngineType.SubProcess].StopScreenCapture();
        }

        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            if (AgoraRtcEngineDict == null) return;

            // Release the sub-process engine first.
            if (AgoraRtcEngineDict.ContainsKey(AgoraEngineType.SubProcess))
            {
                AgoraRtcEngineDict[AgoraEngineType.SubProcess].LeaveChannel();
                AgoraRtcEngineDict[AgoraEngineType.SubProcess].Dispose(true);
            }

            if (AgoraRtcEngineDict.ContainsKey(AgoraEngineType.MainProcess))
            {
                AgoraRtcEngineDict[AgoraEngineType.MainProcess].LeaveChannel();
                AgoraRtcEngineDict[AgoraEngineType.MainProcess].Dispose();
            }
        }

        internal string GetChannelName()
        {
            return channelName;
        }

        internal static void DestroyVideoView(uint uid, AgoraEngineType agoraEngineType)
        {
            var go = GameObject.Find(agoraEngineType + uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                Destroy(go);
            }
        }

        internal static void MakeVideoView(AgoraEngineType agoraEngineType, uint uid, string channelId = "")
        {
            var go = GameObject.Find(agoraEngineType + uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            // create a GameObject and assign to this new user
            var videoSurface = MakeImageSurface(agoraEngineType + uid.ToString());
            if (ReferenceEquals(videoSurface, null)) return;
            // configure videoSurface
            videoSurface.SetForUser(uid, channelId);
            videoSurface.SetEngineType(agoraEngineType);
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
            var go = new GameObject();

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
        private readonly DesktopScreenShare _desktopScreenShare;
        private AgoraEngineType _agoraEngineType;

        internal UserEventHandler(DesktopScreenShare desktopScreenShare, AgoraEngineType agoraEngineType)
        {
            _desktopScreenShare = desktopScreenShare;
            _agoraEngineType = agoraEngineType;
        }

        public override void OnWarning(int warn, string msg)
        {
            _desktopScreenShare.Logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _desktopScreenShare.Logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(string channel, uint uid, int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog(string.Format("sdk version: ${0}",
                _desktopScreenShare.AgoraRtcEngineDict[_agoraEngineType].GetVersion()));
            _desktopScreenShare.Logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channel, uid, elapsed));
            DesktopScreenShare.MakeVideoView(_agoraEngineType, 0);
        }

        public override void OnRejoinChannelSuccess(string channel, uint uid, int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcStats stats)
        {
            _desktopScreenShare.Logger.UpdateLog("OnLeaveChannel");
            DesktopScreenShare.DestroyVideoView(0, _agoraEngineType);
        }

        public override void OnClientRoleChanged(CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _desktopScreenShare.Logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(uint uid, int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            if (_agoraEngineType == AgoraEngineType.MainProcess)
                DesktopScreenShare.MakeVideoView(_agoraEngineType, uid, _desktopScreenShare.GetChannelName());
        }

        public override void OnUserOffline(uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _desktopScreenShare.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int) reason));
            if (_agoraEngineType == AgoraEngineType.MainProcess)
                DesktopScreenShare.DestroyVideoView(uid, _agoraEngineType);
        }

        public override void OnLastmileQuality(int quality)
        {
            _desktopScreenShare.Logger.UpdateLog("OnLastmileQuality");
        }

        public override void OnLastmileProbeResult(LastmileProbeResult result)
        {
            _desktopScreenShare.Logger.UpdateLog("OnLastmileProbeResult");
        }

        public override void OnConnectionInterrupted()
        {
            _desktopScreenShare.Logger.UpdateLog("OnConnectionInterrupted");
        }

        public override void OnConnectionLost()
        {
            _desktopScreenShare.Logger.UpdateLog("OnConnectionLost");
        }

        public override void OnConnectionBanned()
        {
            _desktopScreenShare.Logger.UpdateLog("OnConnectionBanned");
        }

        public override void OnApiCallExecuted(int err, string api, string result)
        {
        }

        public override void OnRequestToken()
        {
            _desktopScreenShare.Logger.UpdateLog("OnRequestToken");
        }

        public override void OnTokenPrivilegeWillExpire(string token)
        {
            _desktopScreenShare.Logger.UpdateLog("OnTokenPrivilegeWillExpire");
        }

        public override void OnAudioQuality(uint uid, int quality, ushort delay, ushort lost)
        {
            _desktopScreenShare.Logger.UpdateLog("OnAudioQuality");
        }

        public override void OnRtcStats(RtcStats stats)
        {
            _desktopScreenShare.Logger.UpdateLog("OnRtcStats");
        }

        public override void OnNetworkQuality(uint uid, int txQuality, int rxQuality)
        {
            _desktopScreenShare.Logger.UpdateLog("OnNetworkQuality");
        }

        public override void OnLocalVideoStats(LocalVideoStats stats)
        {
            _desktopScreenShare.Logger.UpdateLog("OnLocalVideoStats");
        }

        public override void OnRemoteVideoStats(RemoteVideoStats stats)
        {
            _desktopScreenShare.Logger.UpdateLog("OnRemoteVideoStats");
        }

        public override void OnLocalAudioStats(LocalAudioStats stats)
        {
            _desktopScreenShare.Logger.UpdateLog("OnLocalAudioStats");
        }

        public override void OnRemoteAudioStats(RemoteAudioStats stats)
        {
            _desktopScreenShare.Logger.UpdateLog("OnRemoteAudioStats");
        }

        public override void OnLocalAudioStateChanged(LOCAL_AUDIO_STREAM_STATE state, LOCAL_AUDIO_STREAM_ERROR error)
        {
            _desktopScreenShare.Logger.UpdateLog("OnLocalAudioStateChanged");
        }

        public override void OnRemoteAudioStateChanged(uint uid, REMOTE_AUDIO_STATE state,
            REMOTE_AUDIO_STATE_REASON reason, int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog("OnRemoteAudioStateChanged");
        }

        public override void OnAudioPublishStateChanged(string channel, STREAM_PUBLISH_STATE oldState,
            STREAM_PUBLISH_STATE newState, int elapseSinceLastState)
        {
            _desktopScreenShare.Logger.UpdateLog("OnAudioPublishStateChanged");
        }

        public override void OnVideoPublishStateChanged(string channel, STREAM_PUBLISH_STATE oldState,
            STREAM_PUBLISH_STATE newState, int elapseSinceLastState)
        {
            _desktopScreenShare.Logger.UpdateLog("OnVideoPublishStateChanged");
        }

        public override void OnAudioSubscribeStateChanged(string channel, uint uid, STREAM_SUBSCRIBE_STATE oldState,
            STREAM_SUBSCRIBE_STATE newState, int elapseSinceLastState)
        {
            _desktopScreenShare.Logger.UpdateLog("OnAudioSubscribeStateChanged");
        }

        public override void OnVideoSubscribeStateChanged(string channel, uint uid, STREAM_SUBSCRIBE_STATE oldState,
            STREAM_SUBSCRIBE_STATE newState, int elapseSinceLastState)
        {
            _desktopScreenShare.Logger.UpdateLog("OnVideoSubscribeStateChanged");
        }

        public override void OnAudioVolumeIndication(AudioVolumeInfo[] speakers, uint speakerNumber, int totalVolume)
        {
            _desktopScreenShare.Logger.UpdateLog("OnAudioVolumeIndication");
        }

        public override void OnActiveSpeaker(uint uid)
        {
            _desktopScreenShare.Logger.UpdateLog("OnActiveSpeaker");
        }

        public override void OnVideoStopped()
        {
            _desktopScreenShare.Logger.UpdateLog("OnVideoStopped");
        }

        public override void OnFirstLocalVideoFrame(int width, int height, int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog("OnFirstLocalVideoFrame");
        }

        public override void OnFirstLocalVideoFramePublished(int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog("OnFirstLocalVideoFramePublished");
        }

        public override void OnFirstRemoteVideoDecoded(uint uid, int width, int height, int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog("OnFirstRemoteVideoDecoded");
        }

        public override void OnFirstRemoteVideoFrame(uint uid, int width, int height, int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog("OnFirstRemoteVideoFrame");
        }

        public override void OnUserMuteAudio(uint uid, bool muted)
        {
            _desktopScreenShare.Logger.UpdateLog("OnUserMuteAudio");
        }

        public override void OnUserMuteVideo(uint uid, bool muted)
        {
            _desktopScreenShare.Logger.UpdateLog("OnUserMuteVideo");
        }

        public override void OnUserEnableVideo(uint uid, bool enabled)
        {
            _desktopScreenShare.Logger.UpdateLog("OnUserEnableVideo");
        }

        public override void OnAudioDeviceStateChanged(string deviceId, int deviceType, int deviceState)
        {
            _desktopScreenShare.Logger.UpdateLog("OnAudioDeviceStateChanged");
        }

        public override void OnAudioDeviceVolumeChanged(MEDIA_DEVICE_TYPE deviceType, int volume, bool muted)
        {
            _desktopScreenShare.Logger.UpdateLog("OnAudioDeviceVolumeChanged");
        }

        public override void OnCameraReady()
        {
            _desktopScreenShare.Logger.UpdateLog("OnCameraReady");
        }

        public override void OnCameraFocusAreaChanged(int x, int y, int width, int height)
        {
            _desktopScreenShare.Logger.UpdateLog("OnCameraFocusAreaChanged");
        }

        public override void OnFacePositionChanged(int imageWidth, int imageHeight, Rectangle vecRectangle,
            int[] vecDistance, int numFaces)
        {
            _desktopScreenShare.Logger.UpdateLog("OnFacePositionChanged");
        }

        public override void OnCameraExposureAreaChanged(int x, int y, int width, int height)
        {
            _desktopScreenShare.Logger.UpdateLog("OnCameraExposureAreaChanged");
        }

        public override void OnAudioMixingFinished()
        {
            _desktopScreenShare.Logger.UpdateLog("OnAudioMixingFinished");
        }

        public override void OnAudioMixingStateChanged(AUDIO_MIXING_STATE_TYPE state, AUDIO_MIXING_REASON_TYPE reason)
        {
            _desktopScreenShare.Logger.UpdateLog("OnAudioMixingStateChanged");
        }

        public override void OnRemoteAudioMixingBegin()
        {
            _desktopScreenShare.Logger.UpdateLog("OnRemoteAudioMixingBegin");
        }

        public override void OnRemoteAudioMixingEnd()
        {
            _desktopScreenShare.Logger.UpdateLog("OnRemoteAudioMixingEnd");
        }

        public override void OnAudioEffectFinished(int soundId)
        {
            _desktopScreenShare.Logger.UpdateLog("OnAudioEffectFinished");
        }

        public override void OnFirstRemoteAudioDecoded(uint uid, int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog("OnFirstRemoteAudioDecoded");
        }

        public override void OnVideoDeviceStateChanged(string deviceId, int deviceType, int deviceState)
        {
            _desktopScreenShare.Logger.UpdateLog("OnVideoDeviceStateChanged");
        }

        public override void OnLocalVideoStateChanged(LOCAL_VIDEO_STREAM_STATE localVideoState,
            LOCAL_VIDEO_STREAM_ERROR error)
        {
            _desktopScreenShare.Logger.UpdateLog("OnLocalVideoStateChanged");
        }

        public override void OnVideoSizeChanged(uint uid, int width, int height, int rotation)
        {
            _desktopScreenShare.Logger.UpdateLog("OnVideoSizeChanged");
        }

        public override void OnRemoteVideoStateChanged(uint uid, REMOTE_VIDEO_STATE state,
            REMOTE_VIDEO_STATE_REASON reason, int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog("OnRemoteVideoStateChanged");
        }

        public override void OnUserEnableLocalVideo(uint uid, bool enabled)
        {
            _desktopScreenShare.Logger.UpdateLog("OnUserEnableLocalVideo");
        }

        public override void OnStreamMessage(uint uid, int streamId, byte[] data, uint length)
        {
            _desktopScreenShare.Logger.UpdateLog("OnStreamMessage");
        }

        public override void OnStreamMessageError(uint uid, int streamId, int code, int missed, int cached)
        {
            _desktopScreenShare.Logger.UpdateLog("OnStreamMessageError");
        }

        public override void OnMediaEngineLoadSuccess()
        {
            _desktopScreenShare.Logger.UpdateLog("OnMediaEngineLoadSuccess");
        }

        public override void OnMediaEngineStartCallSuccess()
        {
            _desktopScreenShare.Logger.UpdateLog("OnMediaEngineStartCallSuccess");
        }

        public override void OnUserSuperResolutionEnabled(uint uid, bool enabled, SUPER_RESOLUTION_STATE_REASON reason)
        {
            _desktopScreenShare.Logger.UpdateLog("OnUserSuperResolutionEnabled");
        }

        public override void OnChannelMediaRelayStateChanged(CHANNEL_MEDIA_RELAY_STATE state,
            CHANNEL_MEDIA_RELAY_ERROR code)
        {
            _desktopScreenShare.Logger.UpdateLog("OnChannelMediaRelayStateChanged");
        }

        public override void OnChannelMediaRelayEvent(CHANNEL_MEDIA_RELAY_EVENT code)
        {
            _desktopScreenShare.Logger.UpdateLog("OnChannelMediaRelayEvent");
        }

        public override void OnFirstLocalAudioFrame(int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog("OnFirstLocalAudioFrame");
        }

        public override void OnFirstLocalAudioFramePublished(int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog("OnFirstLocalAudioFramePublished");
        }

        public override void OnFirstRemoteAudioFrame(uint uid, int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog("OnFirstRemoteAudioFrame");
        }

        public override void OnRtmpStreamingStateChanged(string url, RTMP_STREAM_PUBLISH_STATE state,
            RTMP_STREAM_PUBLISH_ERROR errCode)
        {
            _desktopScreenShare.Logger.UpdateLog("OnRtmpStreamingStateChanged");
        }

        public override void OnRtmpStreamingEvent(string url, RTMP_STREAMING_EVENT eventCode)
        {
            _desktopScreenShare.Logger.UpdateLog("OnRtmpStreamingEvent");
        }

        public override void OnStreamPublished(string url, int error)
        {
            _desktopScreenShare.Logger.UpdateLog("OnStreamPublished");
        }

        public override void OnStreamUnpublished(string url)
        {
            _desktopScreenShare.Logger.UpdateLog("OnStreamUnpublished");
        }

        public override void OnTranscodingUpdated()
        {
            _desktopScreenShare.Logger.UpdateLog("OnTranscodingUpdated");
        }

        public override void OnStreamInjectedStatus(string url, uint uid, int status)
        {
            _desktopScreenShare.Logger.UpdateLog("OnStreamInjectedStatus");
        }

        public override void OnAudioRouteChanged(AUDIO_ROUTE_TYPE routing)
        {
            _desktopScreenShare.Logger.UpdateLog("OnAudioRouteChanged");
        }

        public override void OnLocalPublishFallbackToAudioOnly(bool isFallbackOrRecover)
        {
            _desktopScreenShare.Logger.UpdateLog("OnLocalPublishFallbackToAudioOnly");
        }

        public override void OnRemoteSubscribeFallbackToAudioOnly(uint uid, bool isFallbackOrRecover)
        {
            _desktopScreenShare.Logger.UpdateLog("OnRemoteSubscribeFallbackToAudioOnly");
        }

        public override void OnRemoteAudioTransportStats(uint uid, ushort delay, ushort lost, ushort rxKBitRate)
        {
            _desktopScreenShare.Logger.UpdateLog("OnRemoteAudioTransportStats");
        }

        public override void OnRemoteVideoTransportStats(uint uid, ushort delay, ushort lost, ushort rxKBitRate)
        {
            _desktopScreenShare.Logger.UpdateLog("OnRemoteVideoTransportStats");
        }

        public override void OnMicrophoneEnabled(bool enabled)
        {
            _desktopScreenShare.Logger.UpdateLog("OnMicrophoneEnabled");
        }

        public override void OnConnectionStateChanged(CONNECTION_STATE_TYPE state,
            CONNECTION_CHANGED_REASON_TYPE reason)
        {
            _desktopScreenShare.Logger.UpdateLog("OnConnectionStateChanged");
        }

        public override void OnNetworkTypeChanged(NETWORK_TYPE type)
        {
            _desktopScreenShare.Logger.UpdateLog("OnNetworkTypeChanged");
        }

        public override void OnLocalUserRegistered(uint uid, string userAccount)
        {
            _desktopScreenShare.Logger.UpdateLog("OnLocalUserRegistered");
        }

        public override void OnUserInfoUpdated(uint uid, UserInfo info)
        {
            _desktopScreenShare.Logger.UpdateLog("OnUserInfoUpdated");
        }

        public override void OnUploadLogResult(string requestId, bool success, UPLOAD_ERROR_REASON reason)
        {
            _desktopScreenShare.Logger.UpdateLog("OnUploadLogResult");
        }

        public override bool OnReadyToSendMetadata(Metadata metadata)
        {
            _desktopScreenShare.Logger.UpdateLog("OnReadyToSendMetadata");
            return true;
        }

        public override void OnMetadataReceived(Metadata metadata)
        {
            _desktopScreenShare.Logger.UpdateLog("OnMetadataReceived");
        }
    }
}
#endif
#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using UnityEngine.Serialization;
using Logger = agora.util.Logger;
using Random = UnityEngine.Random;

namespace Agora_Plugin.API_Example.examples.advanced.ScreenShareWhileVideoCall
{
    public class ScreenShareWhileVideoCall : MonoBehaviour
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

        internal IRtcEngine mRtcEngine = null;

        private const float Offset = 100;
        public Text logText;
        internal Logger Logger;
        private Dropdown _winIdSelect;
        private Button _startShareBtn;
        private Button _stopShareBtn;

        // Use this for initialization
        private void Start()
        {
#if UNITY_IPHONE || UNITY_ANDROID
            throw new PlatformNotSupportedException();
#else
            LoadAssetData();
            CheckAppId();
            InitEngine();
            PrepareScreenCapture();
            JoinChannel();
#endif
        }

        private void CheckAppId()
        {
            Logger = new Logger(logText);
            Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        //Show data in AgoraBasicProfile
        [ContextMenu("ShowAgoraBasicProfileData")]
        public void LoadAssetData()
        {
            if (appIdInput == null) return;
            appID = appIdInput.appID;
            token = appIdInput.token;
            channelName = appIdInput.channelName;
        }

        private void JoinChannel()
        {
            ChannelMediaOptions options = new ChannelMediaOptions();
            options.autoSubscribeAudio.SetValue(true);
            options.autoSubscribeVideo.SetValue(true);
            options.publishAudioTrack.SetValue(true);
            options.publishCameraTrack.SetValue(true);
            options.publishScreenTrack.SetValue(false);
            options.enableAudioRecordingOrPlayout.SetValue(true);
            options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            mRtcEngine.JoinChannel(token, channelName, 123, options);
        }

        private void ScreenShareJoinChannel()
        {
            ChannelMediaOptions options = new ChannelMediaOptions();
            options.autoSubscribeAudio.SetValue(false);
            options.autoSubscribeVideo.SetValue(false);
            options.publishAudioTrack.SetValue(false);
            options.publishCameraTrack.SetValue(false);
            options.publishScreenTrack.SetValue(true);
            options.enableAudioRecordingOrPlayout.SetValue(false);
            options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            var ret = mRtcEngine.JoinChannelEx(token, new RtcConnection(channelName, 456), options);
            Debug.Log("JoinChannelEx returns: " + ret);
        }

        private void ScreenShareLeaveChannel()
        {
            mRtcEngine.LeaveChannelEx(new RtcConnection(channelName, 456));
        }

        private void InitEngine()
        {
            mRtcEngine = RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(appID, 0, true,
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            mRtcEngine.Initialize(context);
            mRtcEngine.InitEventHandler(new UserEventHandler(this));
            mRtcEngine.EnableAudio();
            mRtcEngine.EnableVideo();
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        }


        private void PrepareScreenCapture()
        {
            _winIdSelect = GameObject.Find("winIdSelect").GetComponent<Dropdown>();

            if (_winIdSelect == null || mRtcEngine == null) return;

            _winIdSelect.ClearOptions();

            // var displayInfos = mRtcEngine.GetDisplayInfos();
            // var windowInfos = mRtcEngine.GetWindowInfos();
            SIZE t = new SIZE();
            t.width = 360;
            t.height = 240;
            SIZE s = new SIZE();
            s.width = 360;
            s.height = 240;
            var info = mRtcEngine.GetScreenCaptureSources(t, s, true);

            //_winIdSelect.AddOptions(info.Select(w =>
            //    new Dropdown.OptionData(
            //        string.Format("Display {0}", w.sourceId))).ToList());
            _winIdSelect.AddOptions(info.Select(w =>
                    new Dropdown.OptionData(
                        string.Format("{0}-{1} | {2}", w.sourceName, w.sourceTitle, w.sourceId)))
                .ToList());
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
            if (mRtcEngine == null) return;
            ScreenShareJoinChannel();

            if (_startShareBtn != null) _startShareBtn.gameObject.SetActive(false);
            if (_stopShareBtn != null) _stopShareBtn.gameObject.SetActive(true);
            mRtcEngine.StopScreenCapture();

            if (_winIdSelect == null) return;
            var option = _winIdSelect.options[_winIdSelect.value].text;
            if (string.IsNullOrEmpty(option)) return;
            if (option.Contains("|"))
            {
                var windowId = option.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
                Logger.UpdateLog(string.Format(">>>>> Start sharing {0}", windowId));
                mRtcEngine.StartScreenCaptureByWindowId(ulong.Parse(windowId), default(Rectangle),
                        default(ScreenCaptureParameters));
            }
            else
            {
                var dispId = uint.Parse(option.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1]);
                Logger.UpdateLog(string.Format(">>>>> Start sharing display {0}", dispId));
                mRtcEngine.StartScreenCaptureByDisplayId(dispId, default(Rectangle),
                    new ScreenCaptureParameters { captureMouseCursor = true, frameRate = 30 });
            }
        }

        private void OnStopShareBtnClick()
        {
            ScreenShareLeaveChannel();
            if (_startShareBtn != null) _startShareBtn.gameObject.SetActive(true);
            if (_stopShareBtn != null) _stopShareBtn.gameObject.SetActive(false);
            mRtcEngine.StopScreenCapture();
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (mRtcEngine == null) return;
            mRtcEngine.InitEventHandler(null);
            mRtcEngine.LeaveChannel();
            mRtcEngine.Dispose();
        }

        //private void OnApplicationQuit()
        //{
        //    Debug.Log("OnApplicationQuit");
        //    if (mRtcEngine != null)
        //    {
        //        mRtcEngine.InitEventHandler(null);
        //        mRtcEngine.LeaveChannel();
        //        mRtcEngine.Dispose();
        //        mRtcEngine = null;
        //    }
        //}

        internal string GetChannelName()
        {
            return channelName;
        }

        internal static void DestroyVideoView(string name)
        {
            var go = GameObject.Find(name);
            if (!ReferenceEquals(go, null))
            {
                Destroy(go);
            }
        }

        internal static void MakeVideoView(uint uid, string channelId = "", VIDEO_SOURCE_TYPE videoSourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA)
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            // create a GameObject and assign to this new user
            VideoSurface videoSurface = new VideoSurface();

            if (videoSourceType == VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA)
            {
                videoSurface = MakeImageSurface("MainCameraView");
            }
            else if (videoSourceType == VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN)
            {
                videoSurface = MakeImageSurface("ScreenShareView");
            }
            else
            {
                videoSurface = MakeImageSurface(uid.ToString());
            }
            if (ReferenceEquals(videoSurface, null)) return;
            // configure videoSurface
            videoSurface.SetForUser(uid, channelId, videoSourceType);
            videoSurface.SetEnable(true);
            videoSurface.EnableFilpTextureApply(true, false);
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
            go.transform.position = Vector3.zero;
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
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = new Vector3(3f, 4f, 1f);

            // configure videoSurface
            var videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }
    }

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly ScreenShareWhileVideoCall _desktopScreenShare;

        internal UserEventHandler(ScreenShareWhileVideoCall desktopScreenShare)
        {
            _desktopScreenShare = desktopScreenShare;
        }

        public override void OnWarning(int warn, string msg)
        {
            _desktopScreenShare.Logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _desktopScreenShare.Logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog(string.Format("sdk version: ${0}",
                _desktopScreenShare.mRtcEngine.GetVersion()));
            _desktopScreenShare.Logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));
            if (connection.localUid == 123)
            {
                ScreenShareWhileVideoCall.MakeVideoView(0);
            }
            else if (connection.localUid == 456)
            {
                ScreenShareWhileVideoCall.MakeVideoView(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN);
            }
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _desktopScreenShare.Logger.UpdateLog("OnLeaveChannel");
            if (connection.localUid == 123)
            {
                ScreenShareWhileVideoCall.DestroyVideoView("MainCameraView");
            }
            else if (connection.localUid == 456)
            {
                ScreenShareWhileVideoCall.DestroyVideoView("ScreenShareView");
            }
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _desktopScreenShare.Logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            if (uid != 123 && uid != 456)
            {
                ScreenShareWhileVideoCall.MakeVideoView(uid, _desktopScreenShare.GetChannelName(), VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            }
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _desktopScreenShare.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            if (uid != 123 && uid != 456)
            {
                ScreenShareWhileVideoCall.DestroyVideoView(uid.ToString());
            }
        }
    }
}
#endif
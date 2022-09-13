using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using Agora.Util;
using UnityEngine.Serialization;
using Logger = Agora.Util.Logger;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.ScreenShare
{
    public class ScreenShare : MonoBehaviour
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

        private Dropdown _winIdSelect;
        private Button _startShareBtn;
        private Button _stopShareBtn;
        private Button _updateShareBtn;
        private Button _publishBtn;
        private Button _unpublishBtn;

        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                EnableUI();
                InitEngine();
#if UNITY_ANDROID || UNITY_IPHONE
                GameObject.Find("winIdSelect").SetActive(false);
                _updateShareBtn.gameObject.SetActive(true);
#else
                _updateShareBtn.gameObject.SetActive(false);
                PrepareScreenCapture();
#endif
                JoinChannel();
            }
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

        private void JoinChannel()
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

            var ret = RtcEngine.JoinChannel(_token, _channelName);
            Debug.Log("JoinChannel returns: " + ret);
        }

        private void OnPublishButtonClick()
        {
            ChannelMediaOptions options = new ChannelMediaOptions();
            options.publishCameraTrack.SetValue(false);
            options.publishScreenTrack.SetValue(true);

#if UNITY_ANDROID || UNITY_IPHONE
            options.publishScreenCaptureAudio.SetValue(true);
            options.publishScreenCaptureVideo.SetValue(true);
#endif
            var ret = RtcEngine.UpdateChannelMediaOptions(options);
            Debug.Log("UpdateChannelMediaOptions returns: " + ret);

            _publishBtn.gameObject.SetActive(false);
            _unpublishBtn.gameObject.SetActive(true);
        }

        private void OnUnplishButtonClick()
        {
            ChannelMediaOptions options = new ChannelMediaOptions();
            options.publishCameraTrack.SetValue(true);
            options.publishScreenTrack.SetValue(false);

#if UNITY_ANDROID || UNITY_IPHONE
            options.publishScreenCaptureAudio.SetValue(false);
            options.publishScreenCaptureVideo.SetValue(false);
#endif
            var ret = RtcEngine.UpdateChannelMediaOptions(options);
            Debug.Log("UpdateChannelMediaOptions returns: " + ret);

            _publishBtn.gameObject.SetActive(true);
            _unpublishBtn.gameObject.SetActive(false);
        }

        private void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(new UserEventHandler(this));
        }

        private void PrepareScreenCapture()
        {
            _winIdSelect = GameObject.Find("winIdSelect").GetComponent<Dropdown>();

            if (_winIdSelect == null || RtcEngine == null) return;

            _winIdSelect.ClearOptions();

            SIZE t = new SIZE();
            t.width = 360;
            t.height = 240;
            SIZE s = new SIZE();
            s.width = 360;
            s.height = 240;
            var info = RtcEngine.GetScreenCaptureSources(t, s, true);

            _winIdSelect.AddOptions(info.Select(w =>
                    new Dropdown.OptionData(
                        string.Format("{0}: {1}-{2} | {3}", w.type, w.sourceName, w.sourceTitle, w.sourceId)))
                .ToList());
        }


        private void EnableUI()
        {
            _startShareBtn = GameObject.Find("startShareBtn").GetComponent<Button>();
            _stopShareBtn = GameObject.Find("stopShareBtn").GetComponent<Button>();
            if (_startShareBtn != null) _startShareBtn.onClick.AddListener(OnStartShareBtnClick);
            if (_stopShareBtn != null)
            {
                _stopShareBtn.onClick.AddListener(OnStopShareBtnClick);
                _stopShareBtn.gameObject.SetActive(false);
            }

            var gameObject = GameObject.Find("updateShareBtn");
            if (gameObject != null)
            {
                _updateShareBtn = gameObject.GetComponent<Button>();
                _updateShareBtn.onClick.AddListener(OnUpdateShareBtnClick);
            }

            _publishBtn = GameObject.Find("publishBtn").GetComponent<Button>();
            _publishBtn.onClick.AddListener(OnPublishButtonClick);
            _publishBtn.gameObject.SetActive(false);

            _unpublishBtn = GameObject.Find("unpublishBtn").GetComponent<Button>();
            _unpublishBtn.onClick.AddListener(OnUnplishButtonClick);
            _unpublishBtn.gameObject.SetActive(false);
        }

        private void OnStartShareBtnClick()
        {
            if (RtcEngine == null) return;

            if (_startShareBtn != null) _startShareBtn.gameObject.SetActive(false);
            if (_stopShareBtn != null) _stopShareBtn.gameObject.SetActive(true);

#if UNITY_ANDROID || UNITY_IPHONE
            var parameters2 = new ScreenCaptureParameters2();
            parameters2.captureAudio = true;
            parameters2.captureVideo = true;
            var nRet = RtcEngine.StartScreenCapture(parameters2);
            this.Log.UpdateLog("StartScreenCapture :" + nRet);
#else
            RtcEngine.StopScreenCapture();
            if (_winIdSelect == null) return;
            var option = _winIdSelect.options[_winIdSelect.value].text;
            if (string.IsNullOrEmpty(option)) return;

            if (option.Contains("ScreenCaptureSourceType_Window"))
            {
                var windowId = option.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
                Log.UpdateLog(string.Format(">>>>> Start sharing {0}", windowId));
                var nRet = RtcEngine.StartScreenCaptureByWindowId(ulong.Parse(windowId), default(Rectangle),
                        default(ScreenCaptureParameters));
                this.Log.UpdateLog("StartScreenCaptureByWindowId:" + nRet);
            }
            else
            {
                var dispId = uint.Parse(option.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1]);
                Log.UpdateLog(string.Format(">>>>> Start sharing display {0}", dispId));
                var nRet = RtcEngine.StartScreenCaptureByDisplayId(dispId, default(Rectangle),
                    new ScreenCaptureParameters { captureMouseCursor = true, frameRate = 30 });
                this.Log.UpdateLog("StartScreenCaptureByDisplayId:" + nRet);
            }

#endif

            OnPublishButtonClick();

        }

        private void OnStopShareBtnClick()
        {
            if (_startShareBtn != null) _startShareBtn.gameObject.SetActive(true);
            if (_stopShareBtn != null) _stopShareBtn.gameObject.SetActive(false);


            _publishBtn.gameObject.SetActive(false);
            _unpublishBtn.gameObject.SetActive(false);

            RtcEngine.StopScreenCapture();
        }

        private void OnUpdateShareBtnClick()
        {
            //only work in ios or android
            var config = new ScreenCaptureParameters2();
            config.captureAudio = true;
            config.captureVideo = true;
            config.videoParams.dimensions.width = 960;
            config.videoParams.dimensions.height = 640;
            var nRet = RtcEngine.UpdateScreenCapture(config);
            this.Log.UpdateLog("UpdateScreenCapture: " + nRet);
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }

        internal string GetChannelName()
        {
            return _channelName;
        }

        #region -- Video Render UI Logic ---

        internal static void MakeVideoView(uint uid, string channelId = "", VIDEO_SOURCE_TYPE videoSourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA)
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
            videoSurface.SetForUser(uid, channelId, videoSourceType);
            videoSurface.SetEnable(true);

            videoSurface.OnTextureSizeModify += (int width, int height) =>
            {
                float scale = (float)height / (float)width;
                videoSurface.transform.localScale = new Vector3(-5, 5 * scale, 1);
                Debug.Log("OnTextureSizeModify: " + width + "  " + height);
            };
        }

        // VIDEO TYPE 1: 3D Object
        private static VideoSurface MakePlaneSurface(string goName)
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

        internal static void DestroyVideoView(uint uid)
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                Destroy(go);
            }
        }

        #endregion
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly ScreenShare _desktopScreenShare;

        internal UserEventHandler(ScreenShare desktopScreenShare)
        {
            _desktopScreenShare = desktopScreenShare;
        }

        public override void OnError(int err, string msg)
        {
            _desktopScreenShare.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _desktopScreenShare.Log.UpdateLog(string.Format("sdk version: ${0}",
                _desktopScreenShare.RtcEngine.GetVersion(ref build)));
            _desktopScreenShare.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));
            ScreenShare.MakeVideoView(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN);
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _desktopScreenShare.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _desktopScreenShare.Log.UpdateLog("OnLeaveChannel");
            ScreenShare.DestroyVideoView(connection.localUid);
            ScreenShare.DestroyVideoView(0);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _desktopScreenShare.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _desktopScreenShare.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            ScreenShare.MakeVideoView(uid, _desktopScreenShare.GetChannelName(), VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _desktopScreenShare.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            ScreenShare.DestroyVideoView(uid);
        }
    }

    #endregion
}
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

namespace Agora_Plugin.API_Example.examples.advanced.ScreenShare
{
    public class ScreenShare : MonoBehaviour
    {
        [FormerlySerializedAs("APP_ID")] [SerializeField]
        private string appID = "";

        [FormerlySerializedAs("TOKEN")] [SerializeField]
        private string token = "";

        [FormerlySerializedAs("CHANNEL_NAME")] [SerializeField]
        private string channelName = "YOUR_CHANNEL_NAME";

        internal IAgoraRtcEngine _mRtcEngine = null;

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
            Logger = new Logger(logText);
            CheckAppId();
            InitEngine();
            PrepareScreenCapture();
            JoinChannel();
#endif
        }

        private void CheckAppId()
        {
            Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
        }

        private void JoinChannel()
        {
            _mRtcEngine.EnableAudio();
            _mRtcEngine.EnableVideo();
            _mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            _mRtcEngine.JoinChannel(token, channelName);
        }

        private void InitEngine()
        {
            _mRtcEngine = AgoraRtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(null, appID, null, true, 
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            _mRtcEngine.Initialize(context);
            _mRtcEngine.InitEventHandler(new UserEventHandler(this));
        }


        private void PrepareScreenCapture()
        {
            _winIdSelect = GameObject.Find("winIdSelect").GetComponent<Dropdown>();

            if (_winIdSelect == null || _mRtcEngine == null) return;

            _winIdSelect.ClearOptions();

            var displayInfos = _mRtcEngine.GetDisplayInfos();
            var windowInfos = _mRtcEngine.GetWindowInfos();

            _winIdSelect.AddOptions(displayInfos.Select(w =>
                new Dropdown.OptionData(
                    string.Format("Display {0}", w.DisplayId))).ToList());
            _winIdSelect.AddOptions(windowInfos.Select(w =>
                    new Dropdown.OptionData(
                        string.Format("{0}-{1} | {2}", w.AppName, w.WindowName, w.WindowId)))
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
            if (_mRtcEngine == null) return;

            if (_startShareBtn != null) _startShareBtn.gameObject.SetActive(false);
            if (_stopShareBtn != null) _stopShareBtn.gameObject.SetActive(true);
            _mRtcEngine.StopScreenCapture();

            if (_winIdSelect == null) return;
            var option = _winIdSelect.options[_winIdSelect.value].text;
            if (string.IsNullOrEmpty(option)) return;
            if (option.Contains("|"))
            {
                var windowId = option.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
                Logger.UpdateLog(string.Format(">>>>> Start sharing {0}", windowId));
                _mRtcEngine.StartScreenCaptureByWindowId(ulong.Parse(windowId), default(Rectangle),
                        default(ScreenCaptureParameters));
            }
            else
            {
                var dispId = uint.Parse(option.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1]);
                Logger.UpdateLog(string.Format(">>>>> Start sharing display {0}", dispId));
                _mRtcEngine.StartScreenCaptureByDisplayId(dispId, default(Rectangle),
                    new ScreenCaptureParameters {captureMouseCursor = true, frameRate = 30});
            }
        }

        private void OnStopShareBtnClick()
        {
            if (_startShareBtn != null) _startShareBtn.gameObject.SetActive(true);
            if (_stopShareBtn != null) _stopShareBtn.gameObject.SetActive(false);
            _mRtcEngine.StopScreenCapture();
        }

        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            if (_mRtcEngine == null) return;

            _mRtcEngine.LeaveChannel();
            _mRtcEngine.Dispose();
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
            videoSurface.EnableFilpTextureApply(true, false);
        }

        // VIDEO TYPE 1: 3D Object
        private AgoraVideoSurface MakePlaneSurface(string goName)
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
            var videoSurface = go.AddComponent<AgoraVideoSurface>();
            return videoSurface;
        }

        // Video TYPE 2: RawImage
        private static AgoraVideoSurface MakeImageSurface(string goName)
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
            var videoSurface = go.AddComponent<AgoraVideoSurface>();
            return videoSurface;
        }
    }

    internal class UserEventHandler : IAgoraRtcEngineEventHandler
    {
        private readonly ScreenShare _desktopScreenShare;

        internal UserEventHandler(ScreenShare desktopScreenShare)
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
                _desktopScreenShare._mRtcEngine.GetVersion()));
            _desktopScreenShare.Logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", 
                                connection.channelId, connection.localUid, elapsed));
            ScreenShare.MakeVideoView(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN);
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _desktopScreenShare.Logger.UpdateLog("OnLeaveChannel");
            ScreenShare.DestroyVideoView(connection.localUid);
            ScreenShare.DestroyVideoView(0);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _desktopScreenShare.Logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _desktopScreenShare.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            ScreenShare.MakeVideoView(uid, _desktopScreenShare.GetChannelName(), VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _desktopScreenShare.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int) reason));
            ScreenShare.DestroyVideoView(uid);
        }
    }
}
#endif
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using UnityEngine.Serialization;
using io.agora.rtc.demo;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.ChannelMediaRelayS
{
    public class ChannelMediaRelayS : MonoBehaviour
    {
        [FormerlySerializedAs("appIdInput")]
        [SerializeField]
        private AppIdInput _appIdInput;

        [Header("_____________Basic Configuration_____________")]
        [FormerlySerializedAs("APP_ID")]
        [SerializeField]
        public string _appID = "";

        [FormerlySerializedAs("TOKEN")]
        [SerializeField]
        public string _token = "";

        [FormerlySerializedAs("CHANNEL_NAME")]
        [SerializeField]
        public string _channelName = "";

        public Text LogText;
        internal Logger Log;
        internal IRtcEngineS RtcEngine = null;


        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
                SetupUI();
                EnableUI(false);
                JoinChannel();
            }
        }

        [ContextMenu("ShowAgoraBasicProfileData")]
        private void LoadAssetData()
        {
            if (_appIdInput == null) return;
            _appID = _appIdInput.appID;
            _token = _appIdInput.token;
            _channelName = _appIdInput.channelName;
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        private void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngineS.CreateAgoraRtcEngine();
            UserEventHandlerS handler = new UserEventHandlerS(this);
            RtcEngineContextS context = new RtcEngineContextS();
            context.appId = _appID;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING;
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
        }

        private void JoinChannel()
        {
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.JoinChannel(_token, _channelName, "", "123");
        }

        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        private void SetupUI()
        {
            var ui = this.transform.Find("UI");

            var btn = ui.Find("StartButton").GetComponent<Button>();
            btn.onClick.AddListener(OnStartButtonClick);

            btn = ui.Find("UpdateButton").GetComponent<Button>();
            btn.onClick.AddListener(onUpdateButtonClick);

            btn = ui.Find("StopButton").GetComponent<Button>();
            btn.onClick.AddListener(OnStopButtonClick);

            btn = ui.Find("PauseAllButton").GetComponent<Button>();
            btn.onClick.AddListener(onPauseAllButtonClick);

            btn = ui.Find("ResumAllButton").GetComponent<Button>();
            btn.onClick.AddListener(OnResumeAllButtonClick);
        }

        public void EnableUI(bool visible)
        {
            var ui = this.transform.Find("UI");
            ui.gameObject.SetActive(visible);
        }

        private void OnStartButtonClick()
        {
            ChannelMediaRelayConfigurationS config = new ChannelMediaRelayConfigurationS();
            config.srcInfoS = new ChannelMediaInfoS
            {
                channelName = this._appIdInput.channelName,
                userAccount = "123",
                token = this._appIdInput.token
            };

            //you can relay to another channels (limit max is 4)
            config.destInfosS = new ChannelMediaInfoS[1];
            config.destInfosS[0] = new ChannelMediaInfoS
            {
                channelName = this._appIdInput.channelName + "_2",
                userAccount = "123",
                token = this._appIdInput.token
            };
            config.destCount = 1;

            var nRet = RtcEngine.StartOrUpdateChannelMediaRelay(config);
            this.Log.UpdateLog("StartOrUpdateChannelMediaRelay nRet:" + nRet + " new ChannelName: " + this._appIdInput.channelName + "_2");
        }

        private void onUpdateButtonClick()
        {
            ChannelMediaRelayConfigurationS config = new ChannelMediaRelayConfigurationS();
            config.srcInfoS = new ChannelMediaInfoS
            {
                channelName = this._appIdInput.channelName,
                userAccount = "123",
                token = this._appIdInput.token
            };

            config.destInfosS = new ChannelMediaInfoS[1];
            config.destInfosS[0] = new ChannelMediaInfoS
            {
                channelName = this._appIdInput.channelName + "_3",
                userAccount = "123",
                token = this._appIdInput.token
            };
            config.destCount = 1;

            //after StartChannelMediaRelay you can use StartChannelMediaRelay to remove or relay to anthoner channel
            var nRet = RtcEngine.StartOrUpdateChannelMediaRelay(config);
            this.Log.UpdateLog("UpdateChannelMediaRelay nRet:" + nRet + " new ChannelName: " + this._appIdInput.channelName + "_3");
        }

        private void onPauseAllButtonClick()
        {
            var nRet = RtcEngine.PauseAllChannelMediaRelay();
            this.Log.UpdateLog("onPauseAllButtonClick nRet:" + nRet);
        }

        private void OnResumeAllButtonClick()
        {
            var nRet = RtcEngine.ResumeAllChannelMediaRelay();
            this.Log.UpdateLog("OnResumeAllButtonClick nRet:" + nRet);
        }

        private void OnStopButtonClick()
        {
            var nRet = RtcEngine.StopChannelMediaRelay();
            this.Log.UpdateLog("OnStopButtonClick nRet:" + nRet);
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
            RtcEngine = null;
        }

        #region -- Video Render UI Logic ---

        internal static void MakeVideoView(string userAccount, string channelId = "", VIDEO_SOURCE_TYPE type = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA)
        {
            var go = GameObject.Find(userAccount);
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            // create a GameObject and assign to this new user
            var videoSurface = MakeImageSurface(userAccount);
            if (ReferenceEquals(videoSurface, null)) return;
            // configure videoSurface
            videoSurface.SetForUser(userAccount, channelId, type);

            videoSurface.OnTextureSizeModify += (int width, int height) =>
            {
                float scale = (float)height / (float)width;
                videoSurface.transform.localScale = new Vector3(-5, 5 * scale, 1);
                Debug.Log("OnTextureSizeModify: " + width + "  " + height);
            };
            videoSurface.SetEnable(true);
        }

        // VIDEO TYPE 1: 3D Object
        private static VideoSurfaceS MakePlaneSurface(string goName)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            var mesh = go.GetComponent<MeshRenderer>();
            if (mesh != null)
            {
                Debug.LogWarning("VideoSureface update shader");
                mesh.material = new Material(Shader.Find("Unlit/Texture"));
            }
            // set up transform
            go.transform.Rotate(-90.0f, 0.0f, 0.0f);

            go.transform.position = Vector3.zero;
            go.transform.localScale = new Vector3(0.25f, 0.5f, 0.5f);

            // configure videoSurface
            var videoSurface = go.AddComponent<VideoSurfaceS>();
            return videoSurface;
        }

        // Video TYPE 2: RawImage
        private static VideoSurfaceS MakeImageSurface(string goName)
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
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = new Vector3(2f, 3f, 1f);

            // configure videoSurface
            var videoSurface = go.AddComponent<VideoSurfaceS>();
            return videoSurface;
        }

        internal static void DestroyVideoView(string userAccount)
        {
            var go = GameObject.Find(userAccount);
            if (!ReferenceEquals(go, null))
            {
                Destroy(go);
            }
        }

        #endregion
    }

    #region -- Agora Event ---

    internal class UserEventHandlerS : IRtcEngineEventHandlerS
    {
        private readonly ChannelMediaRelayS _channelMediaRelay;

        internal UserEventHandlerS(ChannelMediaRelayS videoSample)
        {
            _channelMediaRelay = videoSample;
        }

        public override void OnError(int err, string msg)
        {
            _channelMediaRelay.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnectionS connection, int elapsed)
        {
            int build = 0;
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _channelMediaRelay.Log.UpdateLog(string.Format("sdk version: ${0}",
                _channelMediaRelay.RtcEngine.GetVersion(ref build)));
            _channelMediaRelay.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUserAccount, elapsed));
            _channelMediaRelay.EnableUI(true);
            ChannelMediaRelayS.MakeVideoView("");

        }

        public override void OnRejoinChannelSuccess(RtcConnectionS connection, int elapsed)
        {
            _channelMediaRelay.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnectionS connection, RtcStats stats)
        {
            _channelMediaRelay.Log.UpdateLog("OnLeaveChannel");
            _channelMediaRelay.EnableUI(false);
            ChannelMediaRelayS.DestroyVideoView("");
        }

        public override void OnClientRoleChanged(RtcConnectionS connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _channelMediaRelay.Log.UpdateLog(string.Format("OnClientRoleChanged {0}, {1}", oldRole, newRole));
        }

        public override void OnUserJoined(RtcConnectionS connection, string userAccount, int elapsed)
        {
            _channelMediaRelay.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", userAccount, elapsed));
            ChannelMediaRelayS.MakeVideoView(userAccount, _channelMediaRelay._channelName, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
        }

        public override void OnUserOffline(RtcConnectionS connection, string userAccount, USER_OFFLINE_REASON_TYPE reason)
        {
            _channelMediaRelay.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", userAccount,
                (int)reason));
            ChannelMediaRelayS.DestroyVideoView(userAccount);
        }

        public override void OnChannelMediaRelayStateChanged(int state, int code)
        {
            _channelMediaRelay.Log.UpdateLog(string.Format("OnChannelMediaRelayStateChanged state: {0}, code: {1}", state, code));
        }
    }

    #endregion
}

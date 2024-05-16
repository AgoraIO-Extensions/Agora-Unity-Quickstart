using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using UnityEngine.Serialization;
using io.agora.rtc.demo;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.ChannelMediaRelay
{
    public class ChannelMediaRelay : MonoBehaviour
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
        internal IRtcEngine RtcEngine = null;


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
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext();
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
            RtcEngine.JoinChannel(_token, _channelName, "",0);
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
            ChannelMediaRelayConfiguration config = new ChannelMediaRelayConfiguration();
            config.srcInfo = new ChannelMediaInfo
            {
                channelName = this._appIdInput.channelName,
                uid = 0,
                token = this._appIdInput.token
            };

            //you can relay to another channels (limit max is 4)
            config.destInfos = new ChannelMediaInfo[1];
            config.destInfos[0] = new ChannelMediaInfo
            {
                channelName = this._appIdInput.channelName + "_2",
                uid = 0,
                token = this._appIdInput.token
            };
            config.destCount = 1;

            var nRet = RtcEngine.StartOrUpdateChannelMediaRelay(config);
            this.Log.UpdateLog("StartOrUpdateChannelMediaRelay nRet:" + nRet + " new ChannelName: " + this._appIdInput.channelName + "_2");
        }

        private void onUpdateButtonClick()
        {
            ChannelMediaRelayConfiguration config = new ChannelMediaRelayConfiguration();
            config.srcInfo = new ChannelMediaInfo
            {
                channelName = this._appIdInput.channelName,
                uid = 0,
                token = this._appIdInput.token
            };

            config.destInfos = new ChannelMediaInfo[1];
            config.destInfos[0] = new ChannelMediaInfo
            {
                channelName = this._appIdInput.channelName + "_3",
                uid = 0,
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

        internal static void MakeVideoView(uint uid, string channelId = "", VIDEO_SOURCE_TYPE type = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA)
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
            videoSurface.SetForUser(uid, channelId, type);

            videoSurface.OnTextureSizeModify += (int width, int height) =>
            {
                var transform = videoSurface.GetComponent<RectTransform>();
                if (transform)
                {
                    //If render in RawImage. just set rawImage size.
                    transform.sizeDelta = new Vector2(width / 2, height / 2);
                    transform.localScale = Vector3.one;
                }
                else
                {
                    //If render in MeshRenderer, just set localSize with MeshRenderer
                    float scale = (float)height / (float)width;
                    videoSurface.transform.localScale = new Vector3(-1, 1, scale);
                }
                Debug.Log("OnTextureSizeModify: " + width + "  " + height);
            };
            videoSurface.SetEnable(true);
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
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = new Vector3(2f, 3f, 1f);

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
        private readonly ChannelMediaRelay _channelMediaRelay;

        internal UserEventHandler(ChannelMediaRelay videoSample)
        {
            _channelMediaRelay = videoSample;
        }

        public override void OnError(int err, string msg)
        {
            _channelMediaRelay.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _channelMediaRelay.Log.UpdateLog(string.Format("sdk version: ${0}",
                _channelMediaRelay.RtcEngine.GetVersion(ref build)));
            _channelMediaRelay.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));
            _channelMediaRelay.EnableUI(true);
            ChannelMediaRelay.MakeVideoView(0);

        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _channelMediaRelay.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _channelMediaRelay.Log.UpdateLog("OnLeaveChannel");
            _channelMediaRelay.EnableUI(false);
            ChannelMediaRelay.DestroyVideoView(0);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _channelMediaRelay.Log.UpdateLog(string.Format("OnClientRoleChanged {0}, {1}", oldRole, newRole));
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _channelMediaRelay.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            ChannelMediaRelay.MakeVideoView(uid, _channelMediaRelay._channelName, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _channelMediaRelay.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            ChannelMediaRelay.DestroyVideoView(uid);
        }

        public override void OnChannelMediaRelayStateChanged(int state, int code)
        {
            _channelMediaRelay.Log.UpdateLog(string.Format("OnChannelMediaRelayStateChanged state: {0}, code: {1}", state, code));
        }
    }

    #endregion
}

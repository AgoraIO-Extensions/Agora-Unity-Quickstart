using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using io.agora.rtc.demo;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.ContentInspectS
{
    public class ContentInspectS : MonoBehaviour
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
        internal IRtcEngineS RtcEngine = null;


        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
                JoinChannel();
                SetupUI();
            }
        }

        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
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
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
            context.areaCode = AREA_CODE.AREA_CODE_GLOB;
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
        }

        private void JoinChannel()
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.JoinChannel(_token, _channelName, "", "123");
        }

        private void SetupUI()
        {
            var ui = this.transform.Find("UI");

            var btn = ui.Find("StartButton").GetComponent<Button>();
            btn.onClick.AddListener(OnStartButtonClick);

            btn = ui.Find("StopButton").GetComponent<Button>();
            btn.onClick.AddListener(OnStopButtonClick);
        }

        private void OnStartButtonClick()
        {
            var config = new ContentInspectConfig();
            config.modules = new ContentInspectModule[1];
            config.modules[0] = new ContentInspectModule
            {
                type = CONTENT_INSPECT_TYPE.CONTENT_INSPECT_MODERATION,
                interval = 1
            };
            config.moduleCount = 1;

            var nRet = RtcEngine.EnableContentInspect(true, config);
            this.Log.UpdateLog("StartContentInspect: " + nRet);
        }

        private void OnStopButtonClick()
        {
            var config = new ContentInspectConfig();
            config.modules = new ContentInspectModule[1];
            config.modules[0] = new ContentInspectModule
            {
                type = CONTENT_INSPECT_TYPE.CONTENT_INSPECT_MODERATION,
                interval = 1
            };
            config.moduleCount = 1;

            var nRet = RtcEngine.EnableContentInspect(false, config);
            this.Log.UpdateLog("StopContentInspect: " + nRet);
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

        internal static void MakeVideoView(string userAccount, string channelId = "")
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
            if (userAccount == "")
            {
                videoSurface.SetForUser(userAccount, channelId);
            }
            else
            {
                videoSurface.SetForUser(userAccount, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            }

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
        private readonly ContentInspectS _sample;

        internal UserEventHandlerS(ContentInspectS sample)
        {
            _sample = sample;
        }

        public override void OnError(int err, string msg)
        {
            _sample.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnectionS connection, int elapsed)
        {
            int build = 0;
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _sample.Log.UpdateLog(string.Format("sdk version: ${0}",
                _sample.RtcEngine.GetVersion(ref build)));
            _sample.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUserAccount, elapsed));

            ContentInspectS.MakeVideoView("");
        }

        public override void OnRejoinChannelSuccess(RtcConnectionS connection, int elapsed)
        {
            _sample.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnectionS connection, RtcStats stats)
        {
            _sample.Log.UpdateLog("OnLeaveChannel");
            ContentInspectS.DestroyVideoView("");
        }

        public override void OnClientRoleChanged(RtcConnectionS connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _sample.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnectionS connection, string userAccount, int elapsed)
        {
            _sample.Log.UpdateLog(string.Format("OnUserJoined userAccount: ${0} elapsed: ${1}", userAccount, elapsed));
            ContentInspectS.MakeVideoView(userAccount, _sample.GetChannelName());
        }

        public override void OnUserOffline(RtcConnectionS connection, string userAccount, USER_OFFLINE_REASON_TYPE reason)
        {
            _sample.Log.UpdateLog(string.Format("OnUserOffLine userAccount: ${0}, reason: ${1}", userAccount,
                (int)reason));
            ContentInspectS.DestroyVideoView(userAccount);
        }

        public override void OnContentInspectResult(CONTENT_INSPECT_RESULT result)
        {
            _sample.Log.UpdateLog("OnContentInspectResult :" + result);
        }
    }

    #endregion
}

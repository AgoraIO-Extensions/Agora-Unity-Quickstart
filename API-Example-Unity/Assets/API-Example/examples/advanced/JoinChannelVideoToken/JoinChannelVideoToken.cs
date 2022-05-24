using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using UnityEngine.Serialization;
using Logger = agora.util.Logger;

namespace Agora_Plugin.API_Example.examples.advanced.JoinChannelVideoToken
{
    public class JoinChannelVideoToken : MonoBehaviour
    {
        [FormerlySerializedAs("appIdInput")] [SerializeField]
        private AppIdInput appIdInput;

        [Header("_____________Basic Configuration_____________")] 
        [FormerlySerializedAs("APP_ID")] [SerializeField]
        private string appID = "";

        [FormerlySerializedAs("TOKEN")] [SerializeField]
        private string token = "";

        [FormerlySerializedAs("CHANNEL_NAME")] [SerializeField]
        private string channelName = "";

        public Text logText;
        private Logger Logger;
        private const float Offset = 100;

        private static string channelToken = "";
        private static string tokenBase = "http://localhost:8080";
        private CONNECTION_STATE_TYPE state = CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED;

        internal IRtcEngine mRtcEngine = null;

        // Use this for initialization
        void Start()
        {
            LoadAssetData();
            CheckAppId();
            InitEngine();
            JoinChannel();
        }

        void RenewOrJoinToken(string newToken)
        {
            JoinChannelVideoToken.channelToken = newToken;
            if (state == CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED
                || state == CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED
                || state == CONNECTION_STATE_TYPE.CONNECTION_STATE_FAILED
            )
            {
                // If we are not connected yet, connect to the channel as normal
                JoinChannel();
            }
            else
            {
                // If we are already connected, we should just update the token
                UpdateToken();
            }
        }

        // Update is called once per frame
        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        void UpdateToken()
        {
            mRtcEngine.RenewToken(JoinChannelVideoToken.channelToken);
        }

        void CheckAppId()
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
            channelToken = appIdInput.token;
            channelName = appIdInput.channelName;
        }

        void InitEngine()
        {
            mRtcEngine = RtcEngineImpl.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(appID, 0, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            mRtcEngine.Initialize(context);
            mRtcEngine.InitEventHandler(handler);
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            mRtcEngine.EnableAudio();
            mRtcEngine.EnableVideo();
        }

        void JoinChannel()
        {
            if (channelToken.Length == 0)
            {
                StartCoroutine(HelperClass.FetchToken(tokenBase, channelName, 0, this.RenewOrJoinToken));
                return;
            }

            mRtcEngine.JoinChannel(channelToken, channelName, "");
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (mRtcEngine == null) return;
            mRtcEngine.InitEventHandler(null);
            mRtcEngine.LeaveChannel();
        }

        void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            if (mRtcEngine != null)
            {
                mRtcEngine.InitEventHandler(null);
            mRtcEngine.LeaveChannel();
                mRtcEngine.Dispose();
            }
        }

        internal string GetChannelName()
        {
            return channelName;
        }

        private void DestroyVideoView(uint uid)
        {
            GameObject go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                Object.Destroy(go);
            }
        }

        private void MakeVideoView(uint uid, string channelId = "")
        {
            GameObject go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            // create a GameObject and assign to this new user
            VideoSurface videoSurface = MakeImageSurface(uid.ToString());
            if (!ReferenceEquals(videoSurface, null))
            {
                // configure videoSurface
                if (uid == 0)
                {
                    videoSurface.SetForUser(uid, channelId);
                }
                else
                {
                    videoSurface.SetForUser(uid, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
                }

                videoSurface.SetEnable(true);
            }
        }

        // VIDEO TYPE 1: 3D Object
        public VideoSurface MakePlaneSurface(string goName)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);

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
        public VideoSurface MakeImageSurface(string goName)
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
            GameObject canvas = GameObject.Find("VideoCanvas");
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

        internal class UserEventHandler : IRtcEngineEventHandler
        {
            private readonly JoinChannelVideoToken _helloVideoTokenAgora;

            internal UserEventHandler(JoinChannelVideoToken helloVideoTokenAgora)
            {
                _helloVideoTokenAgora = helloVideoTokenAgora;
            }

            public override void OnWarning(int warn, string msg)
            {
                _helloVideoTokenAgora.Logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
            }

            public override void OnError(int err, string msg)
            {
                _helloVideoTokenAgora.Logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
            }

            public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _helloVideoTokenAgora.Logger.UpdateLog(string.Format("sdk version: ${0}",
                    _helloVideoTokenAgora.mRtcEngine.GetVersion()));
                _helloVideoTokenAgora.Logger.UpdateLog(
                    string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                        connection.channelId, connection.localUid, elapsed));
                _helloVideoTokenAgora.Logger.UpdateLog(string.Format("New Token: {0}",
                    JoinChannelVideoToken.channelToken));
                // HelperClass.FetchToken(tokenBase, channelName, 0, this.RenewOrJoinToken);
                _helloVideoTokenAgora.MakeVideoView(0);
            }

            public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _helloVideoTokenAgora.Logger.UpdateLog("OnRejoinChannelSuccess");
            }

            public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
            {
                _helloVideoTokenAgora.Logger.UpdateLog("OnLeaveChannel");
                _helloVideoTokenAgora.DestroyVideoView(0);
            }

            public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
                CLIENT_ROLE_TYPE newRole)
            {
                _helloVideoTokenAgora.Logger.UpdateLog("OnClientRoleChanged");
            }

            public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
            {
                _helloVideoTokenAgora.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid,
                    elapsed));
                _helloVideoTokenAgora.MakeVideoView(uid, _helloVideoTokenAgora.GetChannelName());
            }

            public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
            {
                _helloVideoTokenAgora.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                    (int) reason));
                _helloVideoTokenAgora.DestroyVideoView(uid);
            }

            public override void OnTokenPrivilegeWillExpire(RtcConnection connection, string token)
            {
                _helloVideoTokenAgora.StartCoroutine(HelperClass.FetchToken(tokenBase,
                    _helloVideoTokenAgora.channelName, 0, _helloVideoTokenAgora.RenewOrJoinToken));
            }

            public override void OnConnectionStateChanged(RtcConnection connection, CONNECTION_STATE_TYPE state,
                CONNECTION_CHANGED_REASON_TYPE reason)
            {
                _helloVideoTokenAgora.state = state;
            }

            public override void OnConnectionLost(RtcConnection connection)
            {
                _helloVideoTokenAgora.Logger.UpdateLog(string.Format("OnConnectionLost "));
            }
        }
    }
}

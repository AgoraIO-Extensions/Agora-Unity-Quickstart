using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using io.agora.rtc.demo;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.TexturePushS
{
    public class TexturePushS : MonoBehaviour
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

        private Texture2D _texture;
        private Rect _rect;
        public RawImage SampleaImage;
        private byte[] _shareData;

        public Texture2D ShareTexture;

        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
                SetExternalVideoSource();
                InitTexture();
                JoinChannel();
            }
        }

        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            StartCoroutine(ShareScreen());
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

        private IEnumerator ShareScreen()
        {
            yield return new WaitForEndOfFrame();
            IRtcEngine rtc = Agora.Rtc.RtcEngine.Instance;
            if (rtc != null)
            {
                if (_shareData != null && _shareData.Length > 0)
                {
                    var timetick = System.DateTime.Now.Ticks / 10000;
                    ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
                    externalVideoFrame.type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
                    externalVideoFrame.format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;
                    externalVideoFrame.buffer = _shareData;
                    externalVideoFrame.stride = (int)_rect.width;
                    externalVideoFrame.height = (int)_rect.height;
                    externalVideoFrame.rotation = 180;
                    externalVideoFrame.timestamp = timetick;
                    var ret = rtc.PushVideoFrame(externalVideoFrame);
                    Debug.Log("PushVideoFrame ret = " + ret + " time: " + timetick);
                }
            }
        }


        private byte[] convertColor32(Color32[] colors)
        {
            byte[] ret = new byte[colors.Length * 4];
            int i = 0;
            foreach (var color in colors)
            {
                ret[i++] = color.r;
                ret[i++] = color.g;
                ret[i++] = color.b;
                ret[i++] = color.a;
            }
            return ret;
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

        private void SetExternalVideoSource()
        {

            var ret = RtcEngine.SetExternalVideoSource(true, false, EXTERNAL_VIDEO_SOURCE_TYPE.VIDEO_FRAME, new SenderOptions());
            this.Log.UpdateLog("SetExternalVideoSource returns:" + ret);
        }

        private void JoinChannel()
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.JoinChannel(_token, _channelName, "", "123");
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in Canvas!!!!");
        }

        private void InitTexture()
        {
            _texture = ShareTexture;

            //If it's just a fixed image. Then you only need to calculate once to get _shareData, and then push it every frame.
            //The reason why you can't use _texture.GetRawTextureData() to get byte data is because this _texture.format is not RGBA8888,
            //On different platforms this _texture.foramt may be some other format. So you need to get Color32 first, and then replace it with RGBA8888 byte data
            Color32[] color32s = _texture.GetPixels32(0);
            _shareData = convertColor32(color32s);

            Debug.Log("_shareData length: " + _shareData.Length);

            _rect = new UnityEngine.Rect(0, 0, _texture.width, _texture.height);
            Debug.Log("InitTexture rect = " + _rect.width + "  " + _rect.height);

            VideoEncoderConfiguration videoEncoderConfiguration = new VideoEncoderConfiguration();
            videoEncoderConfiguration.dimensions = new VideoDimensions((int)_rect.width, (int)_rect.height);
            RtcEngine.SetVideoEncoderConfiguration(videoEncoderConfiguration);

            SampleaImage.texture = _texture;
        }


        private void OnDestroy()
        {
            Debug.Log("OnDestroy");

            if (RtcEngine != null)
            {
                RtcEngine.InitEventHandler(null);
                RtcEngine.LeaveChannel();
                RtcEngine.Dispose();
            }
        }

        internal string GetChannelName()
        {
            return _channelName;
        }

        #region -- Video Render UI Logic ---

        internal static void MakeVideoView(string userAccount, string channelId = "")
        {
            GameObject go = GameObject.Find(userAccount);
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            // create a GameObject and assign to this new user
            VideoSurfaceS videoSurface = makeImageSurface(userAccount);
            if (!ReferenceEquals(videoSurface, null))
            {
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
        }

        // Video TYPE 2: RawImage
        private static VideoSurfaceS makeImageSurface(string goName)
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
            GameObject canvas = GameObject.Find("Canvas");
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
            VideoSurfaceS videoSurface = go.AddComponent<VideoSurfaceS>();
            return videoSurface;
        }

        internal static void DestroyVideoView(string userAccount)
        {
            GameObject go = GameObject.Find(userAccount);
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
        private readonly TexturePushS _TexturePush;

        internal UserEventHandlerS(TexturePushS TexturePush)
        {
            _TexturePush = TexturePush;
        }

        public override void OnError(int err, string msg)
        {
            _TexturePush.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnectionS connection, int elapsed)
        {
            int build = 0;
            _TexturePush.Log.UpdateLog(string.Format("sdk version: ${0}",
                _TexturePush.RtcEngine.GetVersion(ref build)));
            _TexturePush.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                    connection.channelId, connection.localUserAccount, elapsed));
        }

        public override void OnRejoinChannelSuccess(RtcConnectionS connection, int elapsed)
        {
            _TexturePush.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnectionS connection, RtcStats stats)
        {
            _TexturePush.Log.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(RtcConnectionS connection, CLIENT_ROLE_TYPE oldRole,
            CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _TexturePush.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnectionS connection, string userAccount, int elapsed)
        {
            _TexturePush.Log.UpdateLog(
                string.Format("OnUserJoined uid: ${0} elapsed: ${1}", userAccount, elapsed));
            TexturePushS.MakeVideoView(userAccount, _TexturePush.GetChannelName());
        }

        public override void OnUserOffline(RtcConnectionS connection, string userAccount, USER_OFFLINE_REASON_TYPE reason)
        {
            _TexturePush.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", userAccount,
                (int)reason));
            TexturePushS.DestroyVideoView(userAccount);
        }
    }

    #endregion
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using Agora.Util;
using Logger = Agora.Util.Logger;

#if UNITY_2018_1_OR_NEWER
using Unity.Collections;
#endif

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.CustomCaptureVideo
{
    public class CustomCaptureVideo : MonoBehaviour
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

       
        private Texture2D _texture;
        private Rect _rect;
        private int i = 0;
        private WebCamTexture _webCameraTexture;
        public RawImage RawImage;
        public Vector2 CameraSize = new Vector2(640, 480);
        public int CameraFPS = 15;
        private byte[] _shareData;


        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitCameraDevice();
                InitTexture();
                InitEngine();
                SetExternalVideoSource();
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
                _texture.ReadPixels(_rect, 0, 0);
                _texture.Apply();

#if UNITY_2018_1_OR_NEWER
                NativeArray<byte> nativeByteArray = _texture.GetRawTextureData<byte>();
                if (_shareData?.Length != nativeByteArray.Length)
                {
                    _shareData = new byte[nativeByteArray.Length];
                }
                nativeByteArray.CopyTo(_shareData);
#else
                _shareData = _texture.GetRawTextureData();
#endif

                ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
                externalVideoFrame.type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
                externalVideoFrame.format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;
                externalVideoFrame.buffer = _shareData;
                externalVideoFrame.stride = (int)_rect.width;
                externalVideoFrame.height = (int)_rect.height;
                externalVideoFrame.cropLeft = 10;
                externalVideoFrame.cropTop = 10;
                externalVideoFrame.cropRight = 10;
                externalVideoFrame.cropBottom = 10;
                externalVideoFrame.rotation = 180;
                externalVideoFrame.timestamp = System.DateTime.Now.Ticks / 10000;
                var ret = rtc.PushVideoFrame(externalVideoFrame);
                Debug.Log("PushVideoFrame ret = " + ret + "time: " + System.DateTime.Now.Millisecond);
            }
        }

        private void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
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
            RtcEngine.JoinChannel(_token, _channelName);
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in Canvas!!!!");
        }

        private void InitTexture()
        {
            _rect = new UnityEngine.Rect(0, 0, Screen.width, Screen.height);
            _texture = new Texture2D((int)_rect.width, (int)_rect.height, TextureFormat.RGBA32, false);
        }

        private void InitCameraDevice()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            _webCameraTexture = new WebCamTexture(devices[0].name, (int)CameraSize.x, (int)CameraSize.y, CameraFPS);
            RawImage.texture = _webCameraTexture;
            _webCameraTexture.Play();
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (_webCameraTexture)
            {
                _webCameraTexture.Stop();
            }
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

        internal static void MakeVideoView(uint uid, string channelId = "")
        {
            GameObject go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            // create a GameObject and assign to this new user
            VideoSurface videoSurface = makeImageSurface(uid.ToString());
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

                videoSurface.OnTextureSizeModify += (int width, int height) =>
                {
                    float scale = (float)height / (float)width;
                    videoSurface.transform.localScale = new Vector3(-5, 5 * scale, 1);
                    Debug.Log("OnTextureSizeModify: " + width + "  " + height);
                };

                videoSurface.SetEnable(true);
            }
        }

        // VIDEO TYPE 1: 3D Object
        private static VideoSurface MakePlaneSurface(string goName)
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
            VideoSurface videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

        // Video TYPE 2: RawImage
        private static VideoSurface makeImageSurface(string goName)
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
            VideoSurface videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

        internal static void DestroyVideoView(uint uid)
        {
            GameObject go = GameObject.Find(uid.ToString());
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
        private readonly CustomCaptureVideo _customCaptureVideo;

        internal UserEventHandler(CustomCaptureVideo customCaptureVideo)
        {
            _customCaptureVideo = customCaptureVideo;
        }

        public override void OnError(int err, string msg)
        {
            _customCaptureVideo.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _customCaptureVideo.Log.UpdateLog(string.Format("sdk version: ${0}",
                _customCaptureVideo.RtcEngine.GetVersion(ref build)));
            _customCaptureVideo.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                    connection.channelId, connection.localUid, elapsed));
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _customCaptureVideo.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _customCaptureVideo.Log.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
            CLIENT_ROLE_TYPE newRole)
        {
            _customCaptureVideo.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _customCaptureVideo.Log.UpdateLog(
                string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            CustomCaptureVideo.MakeVideoView(uid, _customCaptureVideo.GetChannelName());
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _customCaptureVideo.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            CustomCaptureVideo.DestroyVideoView(uid);
        }
    }

    #endregion
}
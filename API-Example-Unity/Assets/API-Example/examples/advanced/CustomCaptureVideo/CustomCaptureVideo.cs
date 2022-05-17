using System.Collections;
using UnityEngine;
using agora.rtc;
using UnityEngine.UI;
using agora.util;
using UnityEngine.Serialization;
using Logger = agora.util.Logger;

#if UNITY_2018_1_OR_NEWER
using Unity.Collections;
#endif

namespace Agora_Plugin.API_Example.examples.advanced.CustomCaptureVideo
{
    public class CustomCaptureVideo : MonoBehaviour
    {
        [FormerlySerializedAs("AgoraBaseProfile")] [SerializeField]
        private AgoraBaseProfile agoraBaseProfile;
        
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

        private Texture2D mTexture;
        private UnityEngine.Rect mRect;
        private int i = 0;
        private WebCamTexture webCameraTexture;
        public RawImage rawImage;
        public Vector2 cameraSize = new Vector2(640, 480);
        public int cameraFPS = 15;
        private byte[] shareData;

        private IAgoraRtcEngine _mRtcEngine = null;

        // Use this for initialization
        void Start()
        {
            LoadAssetData();
            InitCameraDevice();
            InitTexture();
            CheckAppId();
            InitEngine();
            SetExternalVideoSource();
            JoinChannel();
        }

        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            StartCoroutine(ShareScreen());
        }
        
        //Show data in AgoraBasicProfile
        [ContextMenu("ShowAgoraBasicProfileData")]
        public void LoadAssetData()
        {
            if (agoraBaseProfile == null) return;
            appID = agoraBaseProfile.appID;
            token = agoraBaseProfile.token;
            channelName = agoraBaseProfile.channelName;
        }

        private IEnumerator ShareScreen()
        {
            yield return new WaitForEndOfFrame();
            IAgoraRtcEngine rtc = AgoraRtcEngine.Get();
            if (rtc != null)
            {
                mTexture.ReadPixels(mRect, 0, 0);
                mTexture.Apply();

#if UNITY_2018_1_OR_NEWER
            NativeArray<byte> nativeByteArray = mTexture.GetRawTextureData<byte>();
            if (shareData?.Length != nativeByteArray.Length)
            {
                shareData = new byte[nativeByteArray.Length];
            }
            nativeByteArray.CopyTo(shareData);
#else
                shareData = mTexture.GetRawTextureData();
#endif

                ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
                externalVideoFrame.type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
                externalVideoFrame.format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;
                externalVideoFrame.buffer = shareData;
                externalVideoFrame.stride = (int) mRect.width;
                externalVideoFrame.height = (int) mRect.height;
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
            _mRtcEngine = AgoraRtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(null, appID, null, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            _mRtcEngine.Initialize(context);
            _mRtcEngine.InitEventHandler(handler);
            _mRtcEngine.EnableAudio();
            _mRtcEngine.EnableVideo();
            _mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        }

        private void SetExternalVideoSource()
        {
            var ret = _mRtcEngine.SetExternalVideoSource(true, false, EXTERNAL_VIDEO_SOURCE_TYPE.VIDEO_FRAME);
            this.Logger.UpdateLog("SetExternalVideoSource returns:" + ret);
        }

        private void JoinChannel()
        {
            _mRtcEngine.JoinChannel(token, channelName);
        }

        private void CheckAppId()
        {
            Logger = new Logger(logText);
            Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in Canvas!!!!");
        }

        private void InitTexture()
        {
            mRect = new UnityEngine.Rect(0, 0, Screen.width, Screen.height);
            mTexture = new Texture2D((int) mRect.width, (int) mRect.height, TextureFormat.RGBA32, false);
        }

        private void InitCameraDevice()
        {

            WebCamDevice[] devices = WebCamTexture.devices;
            webCameraTexture = new WebCamTexture(devices[0].name, (int) cameraSize.x, (int) cameraSize.y, cameraFPS);
            rawImage.texture = webCameraTexture;
            webCameraTexture.Play();
        }

        private void OnApplicationQuit()
        {
            if (webCameraTexture)
            {
                webCameraTexture.Stop();
            }

            if (_mRtcEngine != null)
            {
                _mRtcEngine.LeaveChannel();
                _mRtcEngine.Dispose();
                _mRtcEngine = null;
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
                Destroy(go);
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
            AgoraVideoSurface videoSurface = makeImageSurface(uid.ToString());
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
        public AgoraVideoSurface MakePlaneSurface(string goName)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // set up transform
            go.transform.Rotate(-90.0f, 0.0f, 0.0f);
            float yPos = Random.Range(3.0f, 5.0f);
            float xPos = Random.Range(-2.0f, 2.0f);
            go.transform.position = new Vector3(xPos, yPos, 0f);
            go.transform.localScale = new Vector3(0.25f, 0.5f, .5f);

            // configure videoSurface
            AgoraVideoSurface videoSurface = go.AddComponent<AgoraVideoSurface>();
            return videoSurface;
        }

        // Video TYPE 2: RawImage
        public AgoraVideoSurface makeImageSurface(string goName)
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
            float xPos = Random.Range(Offset - Screen.width / 2f, Screen.width / 2f - Offset);
            float yPos = Random.Range(Offset, Screen.height / 2f - Offset);
            Debug.Log("position x " + xPos + " y: " + yPos);
            go.transform.localPosition = new Vector3(xPos, yPos, 0f);
            go.transform.localScale = new Vector3(3f, 4f, 1f);

            // configure videoSurface
            AgoraVideoSurface videoSurface = go.AddComponent<AgoraVideoSurface>();
            return videoSurface;
        }

        internal class UserEventHandler : IAgoraRtcEngineEventHandler
        {
            private readonly CustomCaptureVideo _customCaptureVideo;

            internal UserEventHandler(CustomCaptureVideo customCaptureVideo)
            {
                _customCaptureVideo = customCaptureVideo;
            }

            public override void OnWarning(int warn, string msg)
            {
                _customCaptureVideo.Logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
            }

            public override void OnError(int err, string msg)
            {
                _customCaptureVideo.Logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
            }

            public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _customCaptureVideo.Logger.UpdateLog(string.Format("sdk version: ${0}",
                    _customCaptureVideo._mRtcEngine.GetVersion()));
                _customCaptureVideo.Logger.UpdateLog(
                    string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                        connection.channelId, connection.localUid, elapsed));
            }

            public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _customCaptureVideo.Logger.UpdateLog("OnRejoinChannelSuccess");
            }

            public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
            {
                _customCaptureVideo.Logger.UpdateLog("OnLeaveChannel");
            }

            public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
                CLIENT_ROLE_TYPE newRole)
            {
                _customCaptureVideo.Logger.UpdateLog("OnClientRoleChanged");
            }

            public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
            {
                _customCaptureVideo.Logger.UpdateLog(
                    string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
                _customCaptureVideo.MakeVideoView(uid, _customCaptureVideo.GetChannelName());
            }

            public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
            {
                _customCaptureVideo.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                    (int) reason));
                _customCaptureVideo.DestroyVideoView(uid);
            }

        }
    }
}
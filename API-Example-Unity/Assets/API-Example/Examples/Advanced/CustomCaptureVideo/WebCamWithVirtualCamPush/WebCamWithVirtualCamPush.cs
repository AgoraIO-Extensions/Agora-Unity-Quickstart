using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using io.agora.rtc.demo;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.WebCamWithVirtualCamPush
{
    public class WebCamWithVirtualCamPush : MonoBehaviour
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

        [Header("_____________VirtualCam Configuration_____________")]
        [SerializeField]
        private Camera VirtualCam;
        [SerializeField]
        private RenderTexture VirtualCamRT;

        [Header("Video Encoder Config")]
        [SerializeField]
        private VideoDimensions dimensions = new VideoDimensions
        {
            width = 1920,
            height = 1080
        };

        // Pixel format
        public static TextureFormat ConvertFormat = TextureFormat.RGBA32;
        public static VIDEO_PIXEL_FORMAT PixelFormat = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;

        // perspective camera buffer
        private Texture2D BufferTexture = null;


        public Text LogText;
        internal Logger Log;
        internal IRtcEngineEx RtcEngine = null;

        private uint _videoTrack2 = 0;

        private uint _uid1 = 123;
        private uint _uid2 = 456;

        private object _lock = new object();
        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
                InitTexture();
                JoinChannel1();
                JoinChannel2();
            }
        }

        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            ShareRenderTexture();
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

        long _pushCount = 0;
        /// <summary>
        /// Push frame to the remote client.  This is the same code that does ScreenSharing.
        /// </summary>
        /// <param name="bytes">raw video image data</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="onFinish">callback upon finish of the function</param>
        /// <returns></returns>
        IEnumerator PushFrame(byte[] bytes, int width, int height, System.Action onFinish)
        {
            if (bytes == null || bytes.Length == 0)
            {
                Debug.LogError("Zero bytes found!!!!");
                yield break;
            }
            yield return new WaitForEndOfFrame();

            //if the engine is present
            if (RtcEngine != null)
            {
                //Create a new external video frame
                ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
                //Set the buffer type of the video frame
                externalVideoFrame.type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
                // Set the video pixel format
                externalVideoFrame.format = PixelFormat; // VIDEO_PIXEL_RGBA
                                                         //apply raw data you are pulling from the rectangle you created earlier to the video frame
                externalVideoFrame.buffer = bytes;

                //Set the width of the video frame (in pixels)
                externalVideoFrame.stride = width;
                //Set the height of the video frame
                externalVideoFrame.height = height;

                externalVideoFrame.rotation = 180;
                // increment i with the video timestamp
                //externalVideoFrame.timestamp = System.DateTime.Now.Ticks;
                externalVideoFrame.timestamp = 0;
                //Push the external video frame with the frame we just created
                int a = 0;
                lock (_lock)
                {
                    RtcEngine.PushVideoFrame(externalVideoFrame, _videoTrack2);
                }
                if (++_pushCount % 100 == 0) Debug.Log(" pushVideoFrame(" + _pushCount + ") size:" + bytes.Length + " => " + a);
            }

            yield return null;
            onFinish();
        }

        private void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngineEx();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext();
            context.appId = _appID;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
            context.areaCode = AREA_CODE.AREA_CODE_GLOB;
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
        }

        private void JoinChannel1()
        {
            ChannelMediaOptions channelMediaOptions = new ChannelMediaOptions();
            channelMediaOptions.publishCameraTrack.SetValue(true);
            channelMediaOptions.publishMicrophoneTrack.SetValue(true);
            channelMediaOptions.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.JoinChannelEx(_token, new RtcConnection(_channelName, _uid1), channelMediaOptions);
        }


        private void JoinChannel2()
        {
            _videoTrack2 = RtcEngine.CreateCustomVideoTrack();
            ChannelMediaOptions channelMediaOptions = new ChannelMediaOptions();
            channelMediaOptions.publishCustomVideoTrack.SetValue(true);
            channelMediaOptions.publishMicrophoneTrack.SetValue(false);
            channelMediaOptions.autoSubscribeAudio.SetValue(false);
            channelMediaOptions.customVideoTrackId.SetValue(_videoTrack2);
            channelMediaOptions.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.JoinChannelEx(_token, new RtcConnection(_channelName, _uid2), channelMediaOptions);

            VideoEncoderConfiguration config = new VideoEncoderConfiguration
            {
                bitrate = 0,
                minBitrate = 1,
                dimensions = this.dimensions,
                orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_ADAPTIVE,
                degradationPreference = DEGRADATION_PREFERENCE.MAINTAIN_FRAMERATE,
                mirrorMode = VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_ENABLED
            };
            RtcEngine.SetVideoEncoderConfigurationEx(config, new RtcConnection(_channelName, _uid2));
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in Canvas!!!!");
        }

        private void InitTexture()
        {

            RenderTexture renderTexture = VirtualCamRT;
            if (renderTexture != null)
            {
                BufferTexture = new Texture2D(renderTexture.width, renderTexture.height, ConvertFormat, false);
            }
        }

        private void ShareRenderTexture()
        {
            if (BufferTexture == null) // offlined
            {
                return;
            }
            Camera targetCamera = VirtualCam;
            RenderTexture.active = targetCamera.targetTexture; // the targetTexture holds render texture
            Rect rect = new Rect(0, 0, targetCamera.targetTexture.width, targetCamera.targetTexture.height);
            BufferTexture.ReadPixels(rect, 0, 0);
            BufferTexture.Apply();
            byte[] bytes = BufferTexture.GetRawTextureData();

            StartCoroutine(PushFrame(bytes, (int)rect.width, (int)rect.height,
             () =>
             {
                 bytes = null;
             }));

            RenderTexture.active = null;
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
            var mesh = go.GetComponent<MeshRenderer>();
            if (mesh != null)
            {

                Debug.LogWarning("VideoSureface update shader");
                mesh.material = new Material(Shader.Find("Unlit/Texture"));

            }
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
        private readonly WebCamWithVirtualCamPush _sample;

        internal UserEventHandler(WebCamWithVirtualCamPush sample)
        {
            _sample = sample;
        }

        public override void OnError(int err, string msg)
        {
            _sample.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _sample.Log.UpdateLog(string.Format("sdk version: ${0}",
                _sample.RtcEngine.GetVersion(ref build)));
            _sample.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                    connection.channelId, connection.localUid, elapsed));
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _sample.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _sample.Log.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
            CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _sample.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _sample.Log.UpdateLog(
                string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            WebCamWithVirtualCamPush.MakeVideoView(uid, _sample.GetChannelName());
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _sample.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            WebCamWithVirtualCamPush.DestroyVideoView(uid);
        }
    }

    #endregion
}
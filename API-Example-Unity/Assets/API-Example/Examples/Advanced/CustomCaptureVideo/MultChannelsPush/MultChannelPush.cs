using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using io.agora.rtc.demo;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.MultChannelPush
{
    public class MultChannelPush : MonoBehaviour
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
        internal IRtcEngineEx RtcEngine = null;

        public Texture2D ShareTexture1;
        public Texture2D ShareTexture2;

        private byte[] _shareData1 = null;
        private byte[] _shareData2 = null;

        private Rect _rect1;
        private Rect _rect2;

        private uint _videoTrack1 = 0;
        private uint _videoTrack2 = 0;

        private uint _uid1 = 123;
        private uint _uid2 = 456;

        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
                SetExternalVideoSource();
                InitTexture();
                JoinChannel1();
                JoinChannel2();
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

                if (_videoTrack1 != 0)
                {
                    ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
                    externalVideoFrame.type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
                    externalVideoFrame.format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;
                    externalVideoFrame.buffer = _shareData1;
                    externalVideoFrame.stride = (int)_rect1.width;
                    externalVideoFrame.height = (int)_rect1.height;
                    externalVideoFrame.rotation = 180;
                    externalVideoFrame.timestamp = 0;
                    var ret = rtc.PushVideoFrame(externalVideoFrame, _videoTrack1);
                    this.Log.UpdateLog("PushVideoFrame 1: " + ret);
                }


                if (_videoTrack2 != 0)
                {
                    ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
                    externalVideoFrame.type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
                    externalVideoFrame.format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;
                    externalVideoFrame.buffer = _shareData2;
                    externalVideoFrame.stride = (int)_rect2.width;
                    externalVideoFrame.height = (int)_rect2.height;
                    externalVideoFrame.rotation = 180;
                    externalVideoFrame.timestamp = 0;
                    var ret = rtc.PushVideoFrame(externalVideoFrame, _videoTrack2);
                    this.Log.UpdateLog("PushVideoFrame 2: " + ret);
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

        private void SetExternalVideoSource()
        {
            var ret = RtcEngine.SetExternalVideoSource(true, false, EXTERNAL_VIDEO_SOURCE_TYPE.VIDEO_FRAME, new SenderOptions());
            this.Log.UpdateLog("SetExternalVideoSource returns:" + ret);
        }

        private void JoinChannel1()
        {
            _videoTrack1 = RtcEngine.CreateCustomVideoTrack();
            ChannelMediaOptions channelMediaOptions = new ChannelMediaOptions();
            channelMediaOptions.publishCustomVideoTrack.SetValue(true);
            channelMediaOptions.customVideoTrackId.SetValue(_videoTrack1);
            channelMediaOptions.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.JoinChannelEx(_token, new RtcConnection(_channelName, _uid1), channelMediaOptions);

            VideoEncoderConfiguration videoEncoderConfiguration = new VideoEncoderConfiguration();
            videoEncoderConfiguration.dimensions = new VideoDimensions((int)_rect1.width, (int)_rect1.height);
            RtcEngine.SetVideoEncoderConfigurationEx(videoEncoderConfiguration, new RtcConnection(_channelName, _uid1));
        }


        private void JoinChannel2()
        {
            _videoTrack2 = RtcEngine.CreateCustomVideoTrack();
            ChannelMediaOptions channelMediaOptions = new ChannelMediaOptions();
            channelMediaOptions.publishCustomVideoTrack.SetValue(true);
            channelMediaOptions.customVideoTrackId.SetValue(_videoTrack2);
            channelMediaOptions.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.JoinChannelEx(_token, new RtcConnection(_channelName, _uid2), channelMediaOptions);

            VideoEncoderConfiguration videoEncoderConfiguration = new VideoEncoderConfiguration();
            videoEncoderConfiguration.dimensions = new VideoDimensions((int)_rect2.width, (int)_rect2.height);
            RtcEngine.SetVideoEncoderConfigurationEx(videoEncoderConfiguration, new RtcConnection(_channelName, _uid2));
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in Canvas!!!!");
        }

        private void InitTexture()
        {

            Color32[] color32s = ShareTexture1.GetPixels32(0);
            _shareData1 = convertColor32(color32s);
            _rect1 = new UnityEngine.Rect(0, 0, ShareTexture1.width, ShareTexture1.height);

            color32s = ShareTexture2.GetPixels32(0);
            _shareData2 = convertColor32(color32s);
            _rect2 = new UnityEngine.Rect(0, 0, ShareTexture2.width, ShareTexture2.height);

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
                    float scale = (float)height / (float)width;
                    videoSurface.transform.localScale = new Vector3(-5, 5 * scale, 1);
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
        private readonly MultChannelPush _sample;

        internal UserEventHandler(MultChannelPush sample)
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
            MultChannelPush.MakeVideoView(uid, _sample.GetChannelName());
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _sample.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            MultChannelPush.DestroyVideoView(uid);
        }
    }

    #endregion
}
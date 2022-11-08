using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using Agora.Util;
using Logger = Agora.Util.Logger;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.TexturePush
{
    public class TexturePush : MonoBehaviour
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
                    //externalVideoFrame.type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_TEXTURE;
                    externalVideoFrame.format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;
                    externalVideoFrame.buffer = _shareData;
                    externalVideoFrame.stride = (int)_rect.width;
                    externalVideoFrame.height = (int)_rect.height;
                    //externalVideoFrame.rotation = 180;
                    externalVideoFrame.timestamp = timetick;
                    var ret = rtc.PushVideoFrame(externalVideoFrame);
                    //Debug.Log("PushVideoFrame ret = " + ret + " time: " + timetick);
                }
            }
        }


        private byte[] convertColor32(Color32[] colors) {
            byte[] ret = new byte[colors.Length * 4];
            int i = 0;
            foreach (var color in colors) {
                ret[i++] = color.r;
                ret[i++] = color.g;
                ret[i++] = color.b;
                ret[i++] = color.a;
            }
            return ret; 
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
            _texture = ShareTexture;
     

            //如果只是一张固定的图片的话。那么获取_shareData只需要计算一次，然后每帧都推送可以了
            Color32[] color32s = _texture.GetPixels32(0);
            _shareData = convertColor32(color32s);
            Debug.Log("_shareData length: " + _shareData.Length);

            //这里之所以不能用 _texture.GetRawTextureData()来获取byte数据，是因为这个 _texture.format 并不是 RGBA8888 的，
            //在不同的平台上这个_texture.foramt可能是其他的什么之类的格式。所以需要先得到Color32，然后再装换成 RGBA8888的byte数据

            _rect = new UnityEngine.Rect(0, 0, _texture.width, _texture.height);
            Debug.Log("InitTexture rect = " + _rect.width +  "  " + _rect.height);

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
        private readonly TexturePush _TexturePush;

        internal UserEventHandler(TexturePush TexturePush)
        {
            _TexturePush = TexturePush;
        }

        public override void OnError(int err, string msg)
        {
            _TexturePush.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _TexturePush.Log.UpdateLog(string.Format("sdk version: ${0}",
                _TexturePush.RtcEngine.GetVersion(ref build)));
            _TexturePush.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                    connection.channelId, connection.localUid, elapsed));
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _TexturePush.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _TexturePush.Log.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
            CLIENT_ROLE_TYPE newRole)
        {
            _TexturePush.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _TexturePush.Log.UpdateLog(
                string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            TexturePush.MakeVideoView(uid, _TexturePush.GetChannelName());
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _TexturePush.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            TexturePush.DestroyVideoView(uid);
        }
    }

    #endregion
}
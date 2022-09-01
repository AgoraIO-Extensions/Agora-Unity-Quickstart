using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using Agora.Util;
using Logger = Agora.Util.Logger;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.SetBeautyEffectOptions
{
    public class SetBeautyEffectOptions : MonoBehaviour
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

        public Text TextLighteningLevel;
        public Text TextSmoothnessLevel;
        public Text TextRednessLevel;
        public Text TextSharpnessLevel;

        public Slider SliderLighteningLevel;
        public Slider SliderSmoothnessLevel;
        public Slider SliderRednessLevel;
        public Slider SliderSharpnessLevel;

        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                SetUpUI();
                InitEngine();
                InitLogFilePath();
                JoinChannel();
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
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
        }

        private void JoinChannel()
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.JoinChannel(_token, _channelName);
        }

        private void InitLogFilePath()
        {
            var path = Application.persistentDataPath + "/rtc.log";
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
             path = path.Replace('/', '\\');
#endif
            var nRet = RtcEngine.SetLogFile(path);
            this.Log.UpdateLog(string.Format("logPath:{0},nRet:{1}", path, nRet));
        }

        private void SetUpUI()
        {
            this.SliderLighteningLevel.onValueChanged.AddListener((float value) =>
            {
                this.TextLighteningLevel.text = "lighteningLevel:" + value;
            });

            this.SliderSmoothnessLevel.onValueChanged.AddListener((float value) =>
            {
                this.TextSmoothnessLevel.text = "smoothnessLevel:" + value;
            });

            this.SliderRednessLevel.onValueChanged.AddListener((float value) =>
            {
                this.TextRednessLevel.text = "rednessLevel:" + value;
            });

            this.SliderSharpnessLevel.onValueChanged.AddListener((float value) =>
            {
                this.TextSharpnessLevel.text = "sharpnessLevel:" + value;
            });

            var btn = this.transform.Find("UI/StartButton").GetComponent<Button>();
            btn.onClick.AddListener(this.OnStartButtonPress);

            btn = this.transform.Find("UI/StopButton").GetComponent<Button>();
            btn.onClick.AddListener(this.OnStopButtonPress);
        }

        private void OnStartButtonPress()
        {
            var beautyOptions = new BeautyOptions();
            beautyOptions.lighteningContrastLevel = LIGHTENING_CONTRAST_LEVEL.LIGHTENING_CONTRAST_HIGH;

            beautyOptions.lighteningLevel = this.SliderLighteningLevel.value;
            beautyOptions.smoothnessLevel = this.SliderSmoothnessLevel.value;
            beautyOptions.rednessLevel = this.SliderRednessLevel.value;
            beautyOptions.sharpnessLevel = this.SliderSharpnessLevel.value;

            var nRet = RtcEngine.SetBeautyEffectOptions(true, beautyOptions/*, MEDIA_SOURCE_TYPE.PRIMARY_CAMERA_SOURCE*/);
            this.Log.UpdateLog("Start SetBeautyEffectOptions:" + nRet);
        }

        private void OnStopButtonPress()
        {
            var beautyOptions = new BeautyOptions();
            beautyOptions.lighteningContrastLevel = LIGHTENING_CONTRAST_LEVEL.LIGHTENING_CONTRAST_HIGH;

            beautyOptions.lighteningLevel = this.SliderLighteningLevel.value;
            beautyOptions.smoothnessLevel = this.SliderSmoothnessLevel.value;
            beautyOptions.rednessLevel = this.SliderRednessLevel.value;
            beautyOptions.sharpnessLevel = this.SliderSharpnessLevel.value;

            var nRet = RtcEngine.SetBeautyEffectOptions(false, beautyOptions/*, MEDIA_SOURCE_TYPE.PRIMARY_CAMERA_SOURCE*/);
            this.Log.UpdateLog("Stop SetBeautyEffectOptions:" + nRet);
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

        internal static void MakeVideoView(uint uid, string channelId = "")
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

        // VIDEO TYPE 1: 3D Object
        private static VideoSurface MakePlaneSurface(string goName)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // set up transform
            go.transform.Rotate(-90.0f, 0.0f, 0.0f);
            var yPos = Random.Range(3.0f, 5.0f);
            var xPos = Random.Range(-2.0f, 2.0f);
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
        private readonly SetBeautyEffectOptions _sample;

        internal UserEventHandler(SetBeautyEffectOptions sample)
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
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _sample.Log.UpdateLog(string.Format("sdk version: ${0}",
                _sample.RtcEngine.GetVersion(ref build)));
            _sample.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));

            SetBeautyEffectOptions.MakeVideoView(0);
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _sample.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _sample.Log.UpdateLog("OnLeaveChannel");
            SetBeautyEffectOptions.DestroyVideoView(0);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _sample.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _sample.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            SetBeautyEffectOptions.MakeVideoView(uid, _sample.GetChannelName());
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _sample.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            SetBeautyEffectOptions.DestroyVideoView(uid);
        }
    }

    #endregion
}

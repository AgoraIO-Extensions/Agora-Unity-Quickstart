using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using agora.rtc;
using agora.util;
using Logger = agora.util.Logger;


namespace Agora_Plugin.API_Example.examples.basic.SetBeautyEffectOptions
{

    public class SetBeautyEffectOptions : MonoBehaviour
    {

        [FormerlySerializedAs("appIdInput")]
        [SerializeField]
        private AppIdInput appIdInput;

        [Header("_____________Basic Configuration_____________")]
        [FormerlySerializedAs("APP_ID")]
        [SerializeField]
        private string appID = "";

        [FormerlySerializedAs("TOKEN")]
        [SerializeField]
        private string token = "";

        [FormerlySerializedAs("CHANNEL_NAME")]
        [SerializeField]
        private string channelName = "";

        public Text logText;
        internal Logger Logger;
        internal IRtcEngine mRtcEngine = null;


        public Text textLighteningLevel;
        public Text textSmoothnessLevel;
        public Text textRednessLevel;
        public Text textSharpnessLevel;

        public Slider sliderLighteningLevel;
        public Slider sliderSmoothnessLevel;
        public Slider sliderRednessLevel;
        public Slider sliderSharpnessLevel;


        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            CheckAppId();
            SetUpUI();
            InitEngine();
            JoinChannel();

        }

        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        //Show data in AgoraBasicProfile
        [ContextMenu("ShowAgoraBasicProfileData")]
        public void LoadAssetData()
        {
            if (appIdInput == null) return;
            appID = appIdInput.appID;
            token = appIdInput.token;
            channelName = appIdInput.channelName;
        }

        private void CheckAppId()
        {
            Logger = new Logger(logText);
            Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        private void InitEngine()
        {
            mRtcEngine = RtcEngineImpl.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(appID, 0, true,
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            mRtcEngine.Initialize(context);
            mRtcEngine.InitEventHandler(handler);
            mRtcEngine.EnableAudio();
            mRtcEngine.EnableVideo();
            var nRet = mRtcEngine.EnableExtension("agora", "beauty", true);
            this.Logger.UpdateLog("EnableExtension:" + nRet);
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        }

        private void JoinChannel()
        {
            mRtcEngine.JoinChannel(token, channelName);
        }

        void SetUpUI()
        {
            this.sliderLighteningLevel.onValueChanged.AddListener( (float value)=> {
                this.textLighteningLevel.text = "lighteningLevel:" + value; 
            });

            this.sliderSmoothnessLevel.onValueChanged.AddListener((float value) => {
                this.textSmoothnessLevel.text = "smoothnessLevel:" + value;
            });

            this.sliderRednessLevel.onValueChanged.AddListener((float value) => {
                this.textRednessLevel.text = "rednessLevel:" + value;
            });

            this.sliderSharpnessLevel.onValueChanged.AddListener((float value) => {
                this.textSharpnessLevel.text = "sharpnessLevel:" + value;
            });

            var btn = this.transform.Find("UI/StartButton").GetComponent<Button>();
            btn.onClick.AddListener(this.OnStartButtonPress);

            btn = this.transform.Find("UI/StopButton").GetComponent<Button>();
            btn.onClick.AddListener(this.OnStopButtonPress);
        }

        void OnStartButtonPress()
        {
            var beautyOptions= new BeautyOptions();
            beautyOptions.lighteningContrastLevel = LIGHTENING_CONTRAST_LEVEL.LIGHTENING_CONTRAST_HIGH;

            beautyOptions.lighteningLevel = this.sliderLighteningLevel.value;
            beautyOptions.smoothnessLevel = this.sliderSmoothnessLevel.value;
            beautyOptions.rednessLevel = this.sliderRednessLevel.value;
            beautyOptions.sharpnessLevel = this.sliderSharpnessLevel.value;

             var nRet= mRtcEngine.SetBeautyEffectOptions(true, beautyOptions/*, MEDIA_SOURCE_TYPE.PRIMARY_CAMERA_SOURCE*/);
            this.Logger.UpdateLog("Start SetBeautyEffectOptions:" + nRet);
        }

        void OnStopButtonPress()
        {
            var beautyOptions = new BeautyOptions();
            beautyOptions.lighteningContrastLevel = LIGHTENING_CONTRAST_LEVEL.LIGHTENING_CONTRAST_HIGH;

            beautyOptions.lighteningLevel = this.sliderLighteningLevel.value;
            beautyOptions.smoothnessLevel = this.sliderSmoothnessLevel.value;
            beautyOptions.rednessLevel = this.sliderRednessLevel.value;
            beautyOptions.sharpnessLevel = this.sliderSharpnessLevel.value;

            var nRet = mRtcEngine.SetBeautyEffectOptions(false, beautyOptions/*, MEDIA_SOURCE_TYPE.PRIMARY_CAMERA_SOURCE*/);
            this.Logger.UpdateLog("Stop SetBeautyEffectOptions:" + nRet);
        }



        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (mRtcEngine == null) return;
            mRtcEngine.InitEventHandler(null);
            mRtcEngine.LeaveChannel();
        }

        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            if (mRtcEngine != null)
            {
                mRtcEngine.InitEventHandler(null);
                mRtcEngine.LeaveChannel();
                mRtcEngine.Dispose();
                mRtcEngine = null;
            }
        }

        internal string GetChannelName()
        {
            return channelName;
        }

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
            videoSurface.SetEnable(true);
        }

        // VIDEO TYPE 1: 3D Object
        private VideoSurface MakePlaneSurface(string goName)
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
    }

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly SetBeautyEffectOptions _sample;

        internal UserEventHandler(SetBeautyEffectOptions sample)
        {
            _sample = sample;
        }

        public override void OnWarning(int warn, string msg)
        {
            _sample.Logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _sample.Logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _sample.Logger.UpdateLog(string.Format("sdk version: ${0}",
                _sample.mRtcEngine.GetVersion()));
            _sample.Logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));

            SetBeautyEffectOptions .MakeVideoView(0);
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _sample.Logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _sample.Logger.UpdateLog("OnLeaveChannel");
            SetBeautyEffectOptions.DestroyVideoView(0);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _sample.Logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _sample.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            SetBeautyEffectOptions.MakeVideoView(uid, _sample.GetChannelName());
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _sample.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            SetBeautyEffectOptions.DestroyVideoView(uid);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using agora.rtc;
using agora.util;
using Logger = agora.util.Logger;
using System;

namespace Agora_Plugin.API_Example.examples.advanced.SetVideoEncodeConfiguration
{
    public class SpatialAudioWithMediaPlayer : MonoBehaviour
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
        internal IMediaPlayer _mediaPlayer = null;

        private const float Offset = 100;
        public int playerId = 0;
        private const string MPK_URL =
            "https://agora-adc-artifacts.oss-cn-beijing.aliyuncs.com/video/meta_live_mpk.mov";

        private Button button1;
        private Button button2;
        private Button button3;

        public ILocalSpatialAudioEngine _spatialAudioEngine;
        public int x = 0;
        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            CheckAppId();
            SetUpUI();
            InitEngine();
            InitMediaPlayer();
            InitSpatialAudioEngine();
            JoinChannelEx(channelName, 123);
            JoinChannelEx_MPK(channelName, 67890, playerId);
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
            var ret = mRtcEngine.Initialize(context);
            Debug.Log("Agora: Initialize " + ret);
            mRtcEngine.InitEventHandler(handler);
            mRtcEngine.EnableAudio();
            mRtcEngine.EnableVideo();
            mRtcEngine.EnableSpatialAudio(true);
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        }

        private void InitSpatialAudioEngine()
        {
            _spatialAudioEngine = mRtcEngine.GetLocalSpatialAudioEngine();
            var ret = _spatialAudioEngine.Initialize();
            Debug.Log("_spatialAudioEngine: Initialize " + ret);
            _spatialAudioEngine.SetAudioRecvRange(30);
        }

        private void InitMediaPlayer()
        {
            _mediaPlayer = mRtcEngine.GetMediaPlayer();
            if (_mediaPlayer == null)
            {
                Debug.Log("GetAgoraRtcMediaPlayer failed!");
                return;
            }
            playerId = _mediaPlayer.CreateMediaPlayer();
            MpkEventHandler handler = new MpkEventHandler(this);
            _mediaPlayer.InitEventHandler(handler);
            Debug.Log("playerId id: " + playerId);
        }


        public void JoinChannelEx_MPK(string channelName, uint uid, int playerId)
        {
            RtcConnection connection = new RtcConnection();
            connection.channelId = channelName;
            connection.localUid = uid;
            ChannelMediaOptions options = new ChannelMediaOptions();
            options.autoSubscribeAudio.SetValue(false);
            options.autoSubscribeVideo.SetValue(false);
            options.publishAudioTrack.SetValue(false);
            options.publishCameraTrack.SetValue(false);
            options.publishMediaPlayerAudioTrack.SetValue(true);
            options.publishMediaPlayerVideoTrack.SetValue(true);
            options.publishMediaPlayerId.SetValue(playerId);
            options.enableAudioRecordingOrPlayout.SetValue(false);
            options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            var ret = mRtcEngine.JoinChannelEx("", connection, options);
            mRtcEngine.UpdateChannelMediaOptionsEx(options, connection);
            Debug.Log("RtcEngineController JoinChannelEx_MPK returns: " + ret);
        }

        public void JoinChannelEx(string channelName, uint uid)
        {
            RtcConnection connection = new RtcConnection();
            connection.channelId = channelName;
            connection.localUid = uid;
            ChannelMediaOptions options = new ChannelMediaOptions();
            options.autoSubscribeAudio.SetValue(true);
            options.autoSubscribeVideo.SetValue(true);
            options.publishAudioTrack.SetValue(true);
            options.publishCameraTrack.SetValue(false);
            options.enableAudioRecordingOrPlayout.SetValue(true);
            options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            var ret = mRtcEngine.JoinChannelEx("", connection, options);
            Debug.Log("RtcEngineController JoinChannelEx returns: " + ret);
        }

        private void SetUpUI()
        {
            button1 = GameObject.Find("Button1").GetComponent<Button>();
            button1.onClick.AddListener(onLeftLocationPress);
            button2 = GameObject.Find("Button2").GetComponent<Button>();
            button2.onClick.AddListener(onRightLocationPress);
            button3 = GameObject.Find("Button3").GetComponent<Button>();
            button3.onClick.AddListener(onOpenButtonPress);
        }

        private void onLeftLocationPress()
        {
            float[] f1 = { 0.0f, 1.0f, 0.0f };
            var ret = _spatialAudioEngine.UpdateRemotePositionEx(67890, f1, new float[] { 0, 0, 0 }, new RtcConnection(channelName, 123));
            Debug.Log("_spatialAudio.UpdateRemotePosition returns: " + ret);
        }

        private void onRightLocationPress()
        {
            float[] f1 = { 0.0f, -1.0f, 0.0f };
            var ret = _spatialAudioEngine.UpdateRemotePositionEx(67890, f1, new float[] { 0, 0, 0 }, new RtcConnection(channelName, 123));
            Debug.Log("_spatialAudio.UpdateRemotePosition returns: " + ret);
        }

        private void onOpenButtonPress()
        {
            var ret = _mediaPlayer.Open(playerId, MPK_URL, 0);
            Debug.Log("_mediaPlayer.Open returns: " + ret);

            _mediaPlayer.AdjustPlayoutVolume(playerId, 0);
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
            videoSurface.SetForUser(uid, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            videoSurface.SetEnable(true);
            videoSurface.EnableFilpTextureApply(true, false);
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

    internal class MpkEventHandler : IMediaPlayerSourceObserver
    {
        private readonly SpatialAudioWithMediaPlayer _spatialAudio;

        internal MpkEventHandler(SpatialAudioWithMediaPlayer spatialAudio)
        {
            _spatialAudio = spatialAudio;
        }

        public override void OnPlayerSourceStateChanged(int playerId, MEDIA_PLAYER_STATE state, MEDIA_PLAYER_ERROR ec)
        {
            _spatialAudio.Logger.UpdateLog(string.Format(
                "OnPlayerSourceStateChanged state: {0}, ec: {1}, playId: {2}", state, ec, playerId));
            Debug.Log("OnPlayerSourceStateChanged");
            if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_OPEN_COMPLETED)
            {
                _spatialAudio.x = 1;
                var ret = _spatialAudio._mediaPlayer.Play(playerId);
                Debug.Log("Play return" + ret);
                SpatialAudioWithMediaPlayer.MakeVideoView(67890, _spatialAudio.GetChannelName());
            }
            else if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_STOPPED)
            {

            }
        }

        public override void OnPlayerEvent(int playerId, MEDIA_PLAYER_EVENT @event, Int64 elapsedTime, string message)
        {


        }
    }

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly SpatialAudioWithMediaPlayer _spatialAudio;

        internal UserEventHandler(SpatialAudioWithMediaPlayer spatialAudio)
        {
            _spatialAudio = spatialAudio;
        }

        public override void OnWarning(int warn, string msg)
        {
            _spatialAudio.Logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _spatialAudio.Logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _spatialAudio.Logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));
            float[] f1 = new float[] { 0.0f, 0.0f, 0.0f };
            float[] f2 = new float[] { 1.0f, 0.0f, 0.0f };
            float[] f3 = new float[] { 0.0f, 1.0f, 0.0f };
            float[] f4 = new float[] { 0.0f, 0.0f, 1.0f };
            var ret = _spatialAudio._spatialAudioEngine.UpdateSelfPositionEx(f1, f2, f3, f4, connection);
            Debug.Log("UpdateSelfPosition return: " + ret);
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _spatialAudio.Logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _spatialAudio.Logger.UpdateLog("OnLeaveChannel");
            SpatialAudioWithMediaPlayer.DestroyVideoView(0);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _spatialAudio.Logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _spatialAudio.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            if (uid == 67890)
            {
                _spatialAudio.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));

            }
            else if (uid == 12345)
            {
                _spatialAudio.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
                SpatialAudioWithMediaPlayer.MakeVideoView(uid, _spatialAudio.GetChannelName());
            }
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _spatialAudio.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            SpatialAudioWithMediaPlayer.DestroyVideoView(uid);
        }
    }
}
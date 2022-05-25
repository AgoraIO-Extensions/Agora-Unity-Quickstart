using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using agora.rtc;
using agora.util;
using Logger = agora.util.Logger;
using Random = UnityEngine.Random;

namespace Agora_Plugin.API_Example.examples.advanced.MediaPlayer
{
    public class MediaPlayerTest : MonoBehaviour
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
        internal int playerId = 0;

        private const float Offset = 100;
        private const string MPK_URL =
            "https://agora-adc-artifacts.oss-cn-beijing.aliyuncs.com/video/meta_live_mpk.mov";

        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;

        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            CheckAppId();
            SetUpUI();
            EnableUI(false);
            InitEngine();
            InitMediaPlayer();
            JoinChannel_MPK();
        }

        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        private void SetUpUI()
        {
            button1 = GameObject.Find("Button1").GetComponent<Button>();
            button1.onClick.AddListener(onPlayButtonPress);
            button2 = GameObject.Find("Button2").GetComponent<Button>();
            button2.onClick.AddListener(onStopButtonPress);
            button3 = GameObject.Find("Button3").GetComponent<Button>();
            button3.onClick.AddListener(onPauseButtonPress);
            button4 = GameObject.Find("Button4").GetComponent<Button>();
            button4.onClick.AddListener(onResumeButtonPress);
            button5 = GameObject.Find("Button5").GetComponent<Button>();
            button5.onClick.AddListener(onOpenButtonPress);
        }

        public void EnableUI(bool val)
        {
            var obj= this.transform.Find("Button1").gameObject;
            obj.SetActive ( val);

            obj = this.transform.Find("Button2").gameObject;
            obj.SetActive(val);

            obj = this.transform.Find("Button3").gameObject;
            obj.SetActive(val);

            obj = this.transform.Find("Button4").gameObject;
            obj.SetActive(val);
        }

        private void CheckAppId()
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
            channelName = appIdInput.channelName;
        }

        private void InitEngine()
        {
            mRtcEngine = RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(appID, 0, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            mRtcEngine.Initialize(context);
            mRtcEngine.InitEventHandler(handler);
            mRtcEngine.EnableAudio();
            mRtcEngine.EnableVideo();
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        }

        private void JoinChannel_MPK()
        {
            ChannelMediaOptions options = new ChannelMediaOptions();
            options.autoSubscribeAudio.SetValue(true);
            options.autoSubscribeVideo.SetValue(true);
            options.publishAudioTrack.SetValue(false);
            options.publishCameraTrack.SetValue(false);
            options.publishMediaPlayerAudioTrack.SetValue(true);
            options.publishMediaPlayerVideoTrack.SetValue(true);
            options.publishMediaPlayerId.SetValue(playerId);
            options.enableAudioRecordingOrPlayout.SetValue(true);
            options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            var ret = mRtcEngine.JoinChannel(token, channelName, 0, options);
            this.Logger.UpdateLog("RtcEngineController JoinChannel_MPK returns: " + ret);
        }

        private void onPlayButtonPress()
        {
            var ret = _mediaPlayer.Play(playerId);
            this.Logger.UpdateLog("Play return" + ret);
            this.TestMediaPlayer();
        }

        private void onStopButtonPress()
        {
            var ret = _mediaPlayer.Stop(playerId);
            this.Logger.UpdateLog("Stop return" + ret);
        }

        private void onPauseButtonPress()
        {
            var ret = _mediaPlayer.Pause(playerId);
            this.Logger.UpdateLog("Pause return" + ret);
        }

        private void onResumeButtonPress()
        {
            var ret = _mediaPlayer.Resume(playerId);

            this.Logger.UpdateLog("Resume returns: " + ret);
        }

        private void onOpenButtonPress()
        {
            var ret = _mediaPlayer.Open(playerId, MPK_URL, 0);
            this.Logger.UpdateLog("Open returns: " + ret);
        }

        private void InitMediaPlayer()
        {
            _mediaPlayer = mRtcEngine.GetMediaPlayer();
            if (_mediaPlayer == null)
            {
                this.Logger.UpdateLog("GetAgoraRtcMediaPlayer failed!");
                return;
            }
            playerId = _mediaPlayer.CreateMediaPlayer();

            MpkEventHandler handler = new MpkEventHandler(this);
            _mediaPlayer.InitEventHandler(handler);
            this.Logger.UpdateLog("playerId id: " + playerId);
        }

        public void TestMediaPlayer()
        {
            long duration = 0;
            var ret = _mediaPlayer.GetDuration(playerId, ref duration);
            Debug.Log("_mediaPlayer.GetDuration returns: " + ret + "duration: " + duration);

            long pos = 0;
            ret = _mediaPlayer.GetPlayPosition(playerId, ref pos);
            Debug.Log("_mediaPlayer.GetPlayPosition returns: " + ret + "position: " + pos);

            Debug.Log("_mediaPlayer.GetState:" + _mediaPlayer.GetState(playerId));

            bool mute = true;
            ret = _mediaPlayer.GetMute(playerId, ref mute);
            Debug.Log("_mediaPlayer.GetMute returns: " + ret + "mute: " + mute);

            int volume = 0;
            ret = _mediaPlayer.GetPlayoutVolume(playerId, ref volume);
            Debug.Log("_mediaPlayer.GetPlayoutVolume returns: " + ret + "volume: " + volume);

            Debug.Log("SDK Version:" + _mediaPlayer.GetPlayerSdkVersion(playerId));
            Debug.Log("GetPlaySrc:" + _mediaPlayer.GetPlaySrc(playerId));
        }

        private void OnDestroy()
        {
            // Debug.Log("OnDestroy");
            // _mediaPlayer.DestroyMediaPlayer(playerId);
            // if (mRtcEngine == null) return;
            // mRtcEngine.InitEventHandler(null);
            mRtcEngine.LeaveChannel();
        }

        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            _mediaPlayer.DestroyMediaPlayer(playerId);

            if (mRtcEngine == null) return;
            mRtcEngine.InitEventHandler(null);
            mRtcEngine.LeaveChannel();
            mRtcEngine.Dispose(true);
        }

        internal string GetChannelName()
        {
            return channelName;
        }

        internal static void MakeVideoView(uint uid, string channelId = "", VIDEO_SOURCE_TYPE videoSourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA)
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
            videoSurface.SetForUser(uid, channelId, videoSourceType);
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
            go.transform.localScale = new Vector3(1.0f, 1.333f, 0.5f);

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
            go.transform.localScale = new Vector3(4.5f, 3f, 1f);

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
        private readonly MediaPlayerTest _mediaPlayerTest;

        internal MpkEventHandler(MediaPlayerTest videoSample)
        {
            _mediaPlayerTest = videoSample;
        }

        public override void OnPlayerSourceStateChanged(int playerId, MEDIA_PLAYER_STATE state, MEDIA_PLAYER_ERROR ec)
        {
            _mediaPlayerTest.Logger.UpdateLog(string.Format(
                "OnPlayerSourceStateChanged state: {0}, ec: {1}, playId: {2}", state, ec, playerId));
            Debug.Log("OnPlayerSourceStateChanged");
            if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_OPEN_COMPLETED)
            {
                MediaPlayerTest.MakeVideoView((uint)_mediaPlayerTest.playerId, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_MEDIA_PLAYER);
                _mediaPlayerTest.EnableUI(true);
                _mediaPlayerTest.Logger.UpdateLog("Open Complete. Click start to play media");
            }
            else if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_STOPPED)
            {
                MediaPlayerTest.DestroyVideoView((uint)_mediaPlayerTest.playerId);
                _mediaPlayerTest.EnableUI(false);
            }
        }

        public override void OnPlayerEvent(int playerId, MEDIA_PLAYER_EVENT @event, Int64 elapsedTime, string message)
        {
            _mediaPlayerTest.Logger.UpdateLog(string.Format("OnPlayerEvent state: {0}", @event));
        }
    }

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly MediaPlayerTest _mediaPlayerTest;

        internal UserEventHandler(MediaPlayerTest videoSample)
        {
            _mediaPlayerTest = videoSample;
        }

        public override void OnWarning(int warn, string msg)
        {
            _mediaPlayerTest.Logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _mediaPlayerTest.Logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _mediaPlayerTest.Logger.UpdateLog(string.Format("sdk version: ${0}",
                _mediaPlayerTest.mRtcEngine.GetVersion()));
            _mediaPlayerTest.Logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                    connection.channelId, connection.localUid, elapsed));
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _mediaPlayerTest.Logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _mediaPlayerTest.Logger.UpdateLog("OnLeaveChannel");
            MediaPlayerTest.DestroyVideoView(0);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
            CLIENT_ROLE_TYPE newRole)
        {
            _mediaPlayerTest.Logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            if (uid == 67890)
            {
                _mediaPlayerTest.mRtcEngine.EnableSpatialAudio(true);
                _mediaPlayerTest.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
                //MediaPlayerTest.MakeVideoView(uid, "asdf");
            }
            else if (uid == 12345)
            {
                //_mediaPlayerTest.x = 1;
                _mediaPlayerTest.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
                MediaPlayerTest.MakeVideoView(uid, "asdf");
            }
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _mediaPlayerTest.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            MediaPlayerTest.DestroyVideoView(uid);
        }
    }
}

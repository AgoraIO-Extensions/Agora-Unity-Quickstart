using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using io.agora.rtc.demo;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.MediaPlayer
{
    public class MediaPlayerExample : MonoBehaviour
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
        internal IMediaPlayer MediaPlayer = null;

        private const string MPK_URL = "https://agoracdn.s3.us-west-1.amazonaws.com/videos/Agora.io-Interactions.mp4";
        private const string PRELOAD_URL = "https://agoracdn.s3.us-west-1.amazonaws.com/videos/Agora+Quality+Comparison+Jellyfish.mp4";

        public Button PlayButton;
        public Button StopButton;
        public Button PauseButton;
        public Button ResumeButton;
        public Button OpenButton;
        public Button PreloadSrcButton;
        public Button PlayPreloadButton;
        public Button StartPublishButton;
        public Button StopPublishButton;
        public Toggle UrlToggle;
        public Toggle LoopToggle;
        public InputField InputField;
        public Slider Slider;
        internal bool isGragged;
       
        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                EnableUI(false);
                InitEngine();
                SetBasicConfiguration();
                InitMediaPlayer();
            }
        }

        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        public void EnableUI(bool val)
        {
            var obj = PlayButton.gameObject;
            obj.SetActive(val);

            obj = StopButton.gameObject;
            obj.SetActive(val);

            obj = PauseButton.gameObject;
            obj.SetActive(val);

            obj = ResumeButton.gameObject;
            obj.SetActive(val);
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
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

        private void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext();
            context.appId = _appID;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
            context.areaCode = AREA_CODE.AREA_CODE_GLOB;
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
            var logFile = Application.persistentDataPath + "/rtc.log";
            RtcEngine.SetLogFile(logFile);
            this.Log.UpdateLog("logFile:" + logFile);
        }

        private void InitMediaPlayer()
        {
            MediaPlayer = RtcEngine.CreateMediaPlayer();
            if (MediaPlayer == null)
            {
                this.Log.UpdateLog("CreateMediaPlayer failed!");
                return;
            }

            MpkEventHandler handler = new MpkEventHandler(this);
            MediaPlayer.InitEventHandler(handler);
            this.Log.UpdateLog("playerId id: " + MediaPlayer.GetId());
        }

        private void SetBasicConfiguration()
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        }

        #region -- Button Events ---

        public void JoinChannelWithMPK()
        {
            ChannelMediaOptions options = new ChannelMediaOptions();
            options.autoSubscribeAudio.SetValue(true);
            options.autoSubscribeVideo.SetValue(true);
            options.publishCustomAudioTrack.SetValue(false);
            options.publishCameraTrack.SetValue(false);
            options.publishMediaPlayerAudioTrack.SetValue(true);
            options.publishMediaPlayerVideoTrack.SetValue(true);
            options.publishMediaPlayerId.SetValue(MediaPlayer.GetId());
            options.enableAudioRecordingOrPlayout.SetValue(true);
            options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            var ret = RtcEngine.JoinChannel(_token, _channelName, 0, options);
            this.Log.UpdateLog("RtcEngineController JoinChannel_MPK returns: " + ret);
        }

        public void LeaveChannel()
        {
            RtcEngine.LeaveChannel();
        }

        public void OnPlayButtonPress()
        {
            if (this.IsLoop())
            {
                MediaPlayer.SetLoopCount(-1);
            }
            else
            {
                MediaPlayer.SetLoopCount(0);
            }
            var ret = MediaPlayer.Play();
            this.Log.UpdateLog("Play return" + ret);
        }

        public void OnStopButtonPress()
        {
            var ret = MediaPlayer.Stop();
            this.Log.UpdateLog("Stop return" + ret);
        }

        public void OnPauseButtonPress()
        {
            var ret = MediaPlayer.Pause();
            this.Log.UpdateLog("Pause return" + ret);
        }

        public void OnResumeButtonPress()
        {
            var ret = MediaPlayer.Resume();

            this.Log.UpdateLog("Resume returns: " + ret);
        }

        public void OnOpenButtonPress()
        {
            string path = null;
            if (this.UrlToggle.isOn)
            {
                if (this.InputField.text == "")
                {
                    path = MPK_URL;
                }
                else
                {
                    path = this.InputField.text;
                }
            }
            else
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                // On Android, the StreamingAssetPath is just accessed by /assets instead of Application.streamingAssetPath
                path = "/assets/img/MPK.mp4";
#else
                path = Path.Combine(Application.streamingAssetsPath, "img/MPK.mp4");
#endif
            }
            this.Log.UpdateLog("Is opening : " + path);
            var ret = MediaPlayer.Open(path, 0);
            this.Log.UpdateLog("Open returns: " + ret);
        }

        public void OnOpenWithCustomSource()
        {
            var ret = MediaPlayer.OpenWithCustomSource(0, new UserPlayerCustomDataProvider());
            this.Log.UpdateLog("OpenWithCustomSource" + ret);
        }


        public void OnPreloadSrcButtonClick()
        {

            var nRet = MediaPlayer.PreloadSrc(PRELOAD_URL, 0);
            this.Log.UpdateLog("PreloadSrc: " + nRet);
        }

        public void OnPlayPreloadButtonClick()
        {

            var nRet = MediaPlayer.PlayPreloadedSrc(PRELOAD_URL);
            this.Log.UpdateLog("PlayPreloadedSrc: " + nRet);
        }

        internal void TestMediaPlayer()
        {
            long duration = 0;
            var ret = MediaPlayer.GetDuration(ref duration);
            Slider.maxValue = duration;
            Debug.Log("_mediaPlayer.GetDuration returns: " + ret + "duration: " + duration);

            long pos = 0;
            ret = MediaPlayer.GetPlayPosition(ref pos);
            Debug.Log("_mediaPlayer.GetPlayPosition returns: " + ret + "position: " + pos);

            Debug.Log("_mediaPlayer.GetState:" + MediaPlayer.GetState());

            bool mute = true;
            ret = MediaPlayer.GetMute(ref mute);
            Debug.Log("_mediaPlayer.GetMute returns: " + ret + "mute: " + mute);

            int volume = 0;
            ret = MediaPlayer.GetPlayoutVolume(ref volume);
            Debug.Log("_mediaPlayer.GetPlayoutVolume returns: " + ret + "volume: " + volume);

            Debug.Log("SDK Version:" + MediaPlayer.GetPlayerSdkVersion());
            Debug.Log("GetPlaySrc:" + MediaPlayer.GetPlaySrc());
        }

        public void OnStartPublishButtonClick()
        {
            var options = new ChannelMediaOptions();
            options.publishMediaPlayerVideoTrack.SetValue(true);
            options.publishMediaPlayerAudioTrack.SetValue(true);
            options.publishMediaPlayerId.SetValue(MediaPlayer.GetId());
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions: " + nRet);

            StartPublishButton.gameObject.SetActive(false);
            StopPublishButton.gameObject.SetActive(true);
        }

        public void OnStopPublishButtonClick()
        {
            var options = new ChannelMediaOptions();
            options.publishMediaPlayerVideoTrack.SetValue(false);
            options.publishMediaPlayerAudioTrack.SetValue(false);
            options.publishMediaPlayerId.SetValue(MediaPlayer.GetId());

            options.publishCameraTrack.SetValue(false);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions: " + nRet);

            StartPublishButton.gameObject.SetActive(true);
            StopPublishButton.gameObject.SetActive(false);
        }

        public void OnDragging()
        {
            isGragged = true;
        }

        public void OnDragEnd()
        {
            MediaPlayer.Seek((long)Slider.value);
            isGragged = false;
        }

        #endregion

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;

            if (MediaPlayer != null)
                RtcEngine.DestroyMediaPlayer(MediaPlayer);
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
            RtcEngine = null;
        }

        internal string GetChannelName()
        {
            return _channelName;
        }

        internal bool IsLoop()
        {
            return this.LoopToggle.isOn;
        }

        #region -- Video Render UI Logic ---

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
            var mesh = go.GetComponent<MeshRenderer>();
            if (mesh != null)
            {
                Debug.LogWarning("VideoSureface update shader");
                mesh.material = new Material(Shader.Find("Unlit/Texture"));
            }
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

        #endregion
    }

    #region -- Agora Event ---

    internal class MpkEventHandler : IMediaPlayerSourceObserver
    {
        private readonly MediaPlayerExample _sample;

        internal MpkEventHandler(MediaPlayerExample sample)
        {
            _sample = sample;
        }

        public override void OnPlayerSourceStateChanged(MEDIA_PLAYER_STATE state, MEDIA_PLAYER_REASON reason)
        {
            _sample.Log.UpdateLog(string.Format(
                "OnPlayerSourceStateChanged state: {0}, ec: {1}, playId: {2}", state, reason, _sample.MediaPlayer.GetId()));
            Debug.Log("OnPlayerSourceStateChanged");
            if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_OPEN_COMPLETED)
            {
                MediaPlayerExample.MakeVideoView((uint)_sample.MediaPlayer.GetId(), "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_MEDIA_PLAYER);
                _sample.EnableUI(true);
                _sample.TestMediaPlayer();
                _sample.Log.UpdateLog("Open Complete. Click start to play media");
            }
            else if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_STOPPED)
            {
                MediaPlayerExample.DestroyVideoView((uint)_sample.MediaPlayer.GetId());
                _sample.EnableUI(false);
            }
        }

        public override void OnPlayerEvent(MEDIA_PLAYER_EVENT @event, Int64 elapsedTime, string message)
        {
            _sample.Log.UpdateLog(string.Format("OnPlayerEvent state: {0}", @event));
        }

        public override void OnPreloadEvent(string src, PLAYER_PRELOAD_EVENT @event)
        {
            _sample.Log.UpdateLog(string.Format("OnPreloadEvent src: {0}, @event: {1}", src, @event));
        }

        public override void OnPositionChanged(long positionMs, long timestampMs)
        {
            if (!_sample.isGragged)
            {
                _sample.Slider.value = positionMs;
            }
        }
    }

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly MediaPlayerExample _sample;

        internal UserEventHandler(MediaPlayerExample sample)
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
            MediaPlayerExample.DestroyVideoView(0);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
            CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _sample.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _sample.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _sample.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            MediaPlayerExample.DestroyVideoView(uid);
        }
    }

    internal class UserPlayerCustomDataProvider : IMediaPlayerCustomDataProvider
    {
        internal UserPlayerCustomDataProvider()
        {
            
        }

        public override Int64 OnSeek(Int64 offset, int whence)
        {
            Debug.Log("UserPlayerCustomDataProvider OnSeek");
            return 0;
        }

        public override int OnReadData(IntPtr bufferPtr, int bufferSize)
        {
            Debug.Log("UserPlayerCustomDataProvider OnReadData");
            return 0;
        }
    }

    #endregion
}

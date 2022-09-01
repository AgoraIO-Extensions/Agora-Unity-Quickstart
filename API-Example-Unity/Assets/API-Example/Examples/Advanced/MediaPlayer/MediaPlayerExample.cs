﻿using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using Agora.Util;
using Logger = Agora.Util.Logger;

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



        private const string MPK_URL =
            "https://download.agora.io/demo/test/fiture265_60_4.flv";

        private const string PRELOAD_URL = "https://agora-adc-artifacts.oss-cn-beijing.aliyuncs.com/video/meta_live_mpk.mov";

        private Button _button1;
        private Button _button2;
        private Button _button3;
        private Button _button4;
        private Button _button5;
        private Button _button6;
        private Button _button7;
        private Toggle _urlToggle;
        private Toggle _loopToggle;
        private InputField _inputField;

        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                SetUpUI();
                EnableUI(false);
                InitEngine();
                InitMediaPlayer();
                JoinChannelWithMPK();
            }
        }

        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        private void SetUpUI()
        {
            _button1 = GameObject.Find("Button1").GetComponent<Button>();
            _button1.onClick.AddListener(OnPlayButtonPress);
            _button2 = GameObject.Find("Button2").GetComponent<Button>();
            _button2.onClick.AddListener(OnStopButtonPress);
            _button3 = GameObject.Find("Button3").GetComponent<Button>();
            _button3.onClick.AddListener(OnPauseButtonPress);
            _button4 = GameObject.Find("Button4").GetComponent<Button>();
            _button4.onClick.AddListener(OnResumeButtonPress);
            _button5 = GameObject.Find("Button5").GetComponent<Button>();
            _button5.onClick.AddListener(OnOpenButtonPress);
            _button6 = GameObject.Find("Button6").GetComponent<Button>();
            _button6.onClick.AddListener(OnPreloadSrcButtonClick);
            _button7 = GameObject.Find("Button7").GetComponent<Button>();
            _button7.onClick.AddListener(OnPlayPreloadButtonClick);

            _urlToggle = GameObject.Find("UrlToggle").GetComponent<Toggle>();
            _loopToggle = GameObject.Find("LoopToggle").GetComponent<Toggle>();
            _inputField = GameObject.Find("InputField").GetComponent<InputField>();
        }

        public void EnableUI(bool val)
        {
            var obj = this.transform.Find("Button1").gameObject;
            obj.SetActive(val);

            obj = this.transform.Find("Button2").gameObject;
            obj.SetActive(val);

            obj = this.transform.Find("Button3").gameObject;
            obj.SetActive(val);

            obj = this.transform.Find("Button4").gameObject;
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
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
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


        private void JoinChannelWithMPK()
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

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

        private void OnPlayButtonPress()
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
            this.TestMediaPlayer();
        }

        private void OnStopButtonPress()
        {
            var ret = MediaPlayer.Stop();
            this.Log.UpdateLog("Stop return" + ret);
        }

        private void OnPauseButtonPress()
        {
            var ret = MediaPlayer.Pause();
            this.Log.UpdateLog("Pause return" + ret);
        }

        private void OnResumeButtonPress()
        {
            var ret = MediaPlayer.Resume();

            this.Log.UpdateLog("Resume returns: " + ret);
        }

        private void OnOpenButtonPress()
        {
            string path = null;
            if (this._urlToggle.isOn)
            {
                if (this._inputField.text == "")
                {
                    path = MPK_URL;
                }
                else
                {
                    path = this._inputField.text;
                }
            }
            else
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                // On Android, the StreamingAssetPath is just accessed by /assets instead of Application.streamingAssetPath
                path = "/assets/img/MPK.mov";
#else
                path = Path.Combine(Application.streamingAssetsPath, "img/MPK.mov");
#endif
            }
            this.Log.UpdateLog("Is opening : " + path);
            var ret = MediaPlayer.Open(path, 0);
            this.Log.UpdateLog("Open returns: " + ret);
        }

        private void OnOpenWithCustomSource()
        {
            var ret = MediaPlayer.OpenWithCustomSource(0, new UserPlayerCustomDataProvider(this));
            this.Log.UpdateLog("OpenWithCustomSource" + ret);
        }


        private void OnPreloadSrcButtonClick()
        {

            var nRet = MediaPlayer.PreloadSrc(PRELOAD_URL, 0);
            this.Log.UpdateLog("PreloadSrc: " + nRet);
        }

        private void OnPlayPreloadButtonClick()
        {

            var nRet = MediaPlayer.PlayPreloadedSrc(PRELOAD_URL);
            this.Log.UpdateLog("PlayPreloadedSrc: " + nRet);
        }

        private void TestMediaPlayer()
        {
            long duration = 0;
            var ret = MediaPlayer.GetDuration(ref duration);
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
            return this._loopToggle.isOn;
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
                float scale = (float)height / (float)width;
                videoSurface.transform.localScale = new Vector3(-5, 5 * scale, 1);
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

        public override void OnPlayerSourceStateChanged(MEDIA_PLAYER_STATE state, MEDIA_PLAYER_ERROR ec)
        {
            _sample.Log.UpdateLog(string.Format(
                "OnPlayerSourceStateChanged state: {0}, ec: {1}, playId: {2}", state, ec, _sample.MediaPlayer.GetId()));
            Debug.Log("OnPlayerSourceStateChanged");
            if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_OPEN_COMPLETED)
            {
                MediaPlayerExample.MakeVideoView((uint)_sample.MediaPlayer.GetId(), "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_MEDIA_PLAYER);
                _sample.EnableUI(true);
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
            CLIENT_ROLE_TYPE newRole)
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
        MediaPlayerExample _sample;

        internal UserPlayerCustomDataProvider(MediaPlayerExample sample)
        {
            _sample = sample;
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

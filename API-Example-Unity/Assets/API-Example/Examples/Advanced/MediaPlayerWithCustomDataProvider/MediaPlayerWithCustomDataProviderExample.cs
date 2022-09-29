using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using Agora.Util;
using Logger = Agora.Util.Logger;
using Random = UnityEngine.Random;
using System.IO;
using System.Runtime.InteropServices;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.MediaPlayerWithCustomDataProviderExample
{
    public class MediaPlayerWithCustomDataProviderExample : MonoBehaviour
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
        internal UserPlayerCustomDataProvider customDataProvider = null;


        private Button _button1;
        private Button _button2;
        private Button _button3;
        private Button _button4;
        private Button _button5;


        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                SetUpUI();
                //EnableUI(false);
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
            var ret = MediaPlayer.Play();
            this.Log.UpdateLog("Play return" + ret);
            //this.TestMediaPlayer();
        }

        private void OnStopButtonPress()
        {
            var ret = MediaPlayer.Stop();
            this.Log.UpdateLog("Stop return" + ret);
            this.customDataProvider.Close();
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
            this.customDataProvider = new UserPlayerCustomDataProvider(this);
            string file;
#if UNITY_ANDROID && !UNITY_EDITOR
        // On Android, the StreamingAssetPath is just accessed by /assets instead of Application.streamingAssetPath
            file = "/assets/img/MPK.mp4";
#else
            file = Application.streamingAssetsPath + "/img/" + "MPK.mp4";
#endif
            this.customDataProvider.Open(file);

            //var ret = MediaPlayer.OpenWithCustomSource(0, this.customDataProvider);
            //this.Log.UpdateLog("OpenWithCustomSource: " + ret);


            var source = new MediaSource();
            source.url = null;
            source.uri = null;
            source.provider = this.customDataProvider;
            source.autoPlay = false;
            var ret = MediaPlayer.OpenWithMediaSource(source);
            this.Log.UpdateLog("OpenWithMediaSource: " + ret);
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
            {
                MediaPlayer.Stop();
                RtcEngine.DestroyMediaPlayer(MediaPlayer);
            }

            if (customDataProvider != null)
                customDataProvider.Close();

            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
            RtcEngine = null;

        }


        internal string GetChannelName()
        {
            return _channelName;
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
    }

    internal class MpkEventHandler : IMediaPlayerSourceObserver
    {
        private readonly MediaPlayerWithCustomDataProviderExample _sample;

        internal MpkEventHandler(MediaPlayerWithCustomDataProviderExample sample)
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
                MediaPlayerWithCustomDataProviderExample.MakeVideoView((uint)_sample.MediaPlayer.GetId(), "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_MEDIA_PLAYER);
                _sample.EnableUI(true);
                _sample.Log.UpdateLog("Open Complete. Click start to play media");
            }
            else if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_STOPPED)
            {
                MediaPlayerWithCustomDataProviderExample.DestroyVideoView((uint)_sample.MediaPlayer.GetId());
                _sample.EnableUI(false);
            }
        }

        public override void OnPlayerEvent(MEDIA_PLAYER_EVENT @event, Int64 elapsedTime, string message)
        {
            _sample.Log.UpdateLog(string.Format("OnPlayerEvent state: {0}", @event));
        }
    }

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly MediaPlayerWithCustomDataProviderExample _sample;

        internal UserEventHandler(MediaPlayerWithCustomDataProviderExample sample)
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
            MediaPlayerWithCustomDataProviderExample.DestroyVideoView(0);
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
            MediaPlayerWithCustomDataProviderExample.DestroyVideoView(uid);
        }
    }

    internal class UserPlayerCustomDataProvider : IMediaPlayerCustomDataProvider
    {

        MediaPlayerWithCustomDataProviderExample _sample;
        private FileStream fis = null;

        private Int64 fileSize = 0;


        internal UserPlayerCustomDataProvider(MediaPlayerWithCustomDataProviderExample sample)
        {
            _sample = sample;

        }

        public bool Open(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    fis = new FileStream(file, FileMode.Open, FileAccess.Read);
                    fileSize = fis.Length;
                    this._sample.Log.UpdateLog("open file sucess size: " + fileSize);
                }

            }
            catch (Exception e)
            {
                this._sample.Log.UpdateLog("open catch exception " + e);
                return false;
            }
            return true;

        }

        public void Close()
        {
            if (fis == null)
            {
                return;
            }
            try
            {
                fis.Close();
            }
            catch (Exception e)
            {
                this._sample.Log.UpdateLog("close catch exception " + e);
            }
            fis = null;
        }

        public override Int64 OnSeek(Int64 offset, int whence)
        {

            string str = String.Format("OnSeek offset:{0} whence:{1}", offset, whence);
            Debug.Log(str);

            if (whence == 0)
            {
                try
                {
                    if (fis == null)
                    {
                        return -1;
                    }
                    fis.Seek(offset, SeekOrigin.Begin);
                }
                catch (Exception e)
                {
                    Debug.Log("onseek catch exception " + e);
                    return -1;
                }
                return offset;
            }
            else if (whence == 65536)
            {
                return fileSize;
            }
            return 0;
        }

        public override int OnReadData(IntPtr bufferPtr, int bufferSize)
        {
            string str = String.Format("OnReadData bufferPtr:{0} bufferSize:{1}", (System.Int64)bufferPtr, bufferSize);
            Debug.Log(str);


            if (fis == null)
            {
                return -1;
            }
            byte[] byte_buffer = new byte[bufferSize];
            int read_count = -1;
            try
            {
                read_count = fis.Read(byte_buffer, 0, bufferSize);
                if (read_count == -1)
                {
                    return -1;
                }
                UnityEngine.Debug.Log("onReadData: " + read_count);
                Marshal.Copy(byte_buffer, 0, bufferPtr, read_count);
            }
            catch (Exception e)
            {
                Debug.Log("onseek catch exception " + e);
                return -1;
            }

            return read_count;
        }

    }
}

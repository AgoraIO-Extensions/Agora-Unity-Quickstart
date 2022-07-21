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

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.MusicPlayer
{

    public class MusicPlayerExample : MonoBehaviour
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
        internal IMusicContentCenter MusicContentCenter = null;
        internal IMusicPlayer MusicPlayer = null;

        internal Button GetMusicChartsButton;
        internal Button GetMusicChartButton;
        internal Button PreloadButton;
        internal Button OpenButton;
        internal Button GetLyricButton;
       

        public string rtmToken;
        public MusicChartsResult musicChartsResult = null;
        public MusicListResult musicListResult = null;

        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                SetUpUI();
                InitEngine();
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
            GetMusicChartsButton = GameObject.Find("GetMusicCharts").GetComponent<Button>();
            GetMusicChartsButton.onClick.AddListener(OnGetMusicChartsButtonClick);
            GetMusicChartsButton.gameObject.SetActive(false); 

            GetMusicChartButton = GameObject.Find("GetMusicChart").GetComponent<Button>();
            GetMusicChartButton.onClick.AddListener(OnGetMusicChartButtonClick);
            GetMusicChartButton.gameObject.SetActive(false);


            PreloadButton = GameObject.Find("Preload").GetComponent<Button>();
            PreloadButton.onClick.AddListener(OnPreloadButtonClick);
            PreloadButton.gameObject.SetActive(false);

            OpenButton = GameObject.Find("Open").GetComponent<Button>();
            OpenButton.onClick.AddListener(OnOpenButtonClick);
            OpenButton.gameObject.SetActive(false);

            GetLyricButton = GameObject.Find("GetLyric").GetComponent<Button>();
            GetLyricButton.onClick.AddListener(OnGetLyricButtonClick);
            GetLyricButton.gameObject.SetActive(false);

        }

      
        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }


        private void JoinChannelWithMPK()
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

            ChannelMediaOptions options = new ChannelMediaOptions();
            options.autoSubscribeAudio.SetValue(true);
            options.autoSubscribeVideo.SetValue(true);
            options.publishAudioTrack.SetValue(false);
            options.publishCameraTrack.SetValue(false);
            options.publishMediaPlayerAudioTrack.SetValue(true);
            options.publishMediaPlayerVideoTrack.SetValue(true);
            options.enableAudioRecordingOrPlayout.SetValue(true);
            options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            var ret = RtcEngine.JoinChannel(_token, _channelName, 0, options);
            this.Log.UpdateLog("RtcEngineController JoinChannel_MPK returns: " + ret);
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
            RtcEngineContext context = new RtcEngineContext(_appID, 0, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
        }

        public void InitMusicPlayer(UInt64 uid)
        {
            MusicContentCenter = RtcEngine.GetMusicContentCenter();
            AgoraMusicContentCenterConfiguration config = new AgoraMusicContentCenterConfiguration(_appID, rtmToken, uid);
            MusicContentCenter.Initialize(config);
            MusicContentCenter.RegisterEventHandler(new UserMusicContentCenterEventHandler(this));

            MusicPlayer = MusicContentCenter.CreateMusicPlayer();
            ChannelMediaOptions options = new ChannelMediaOptions();
            options.publishMediaPlayerId.SetValue(MusicPlayer.GetId());
            var ret = RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions ret:" + ret);

        }

        void OnGetMusicChartsButtonClick()
        {
            var ret = MusicContentCenter.GetMusicCharts("1");
            this.Log.UpdateLog("GetMusicCharts: " + ret);
        }

        void OnGetMusicChartButtonClick()
        {
            if (this.musicChartsResult == null || this.musicChartsResult.count <= 0)
            {
                Debug.Log("musicChartsResult is empty list");
                return;
            }

            MusicChartsType chartType = this.musicChartsResult.musicChart[0];
            var ret = MusicContentCenter.GetMusicChart("2", chartType.id, 3, 5, "");
            this.Log.UpdateLog("GetMusicChart: " + ret);
        }

        void OnPreloadButtonClick()
        {
            if (this.musicListResult == null || this.musicListResult.count <= 0)
            {
                Debug.Log("musicListResult is empty list");
                return;
            }

            Music music = this.musicListResult.music[0];
            var ret = MusicContentCenter.Preload(music.songCode, AgoraMediaType.AgoraMediaType_mv, "");
            this.Log.UpdateLog("Preload: " + ret);
        }

        void OnOpenButtonClick()
        {
            if (this.musicListResult == null || this.musicListResult.count <= 0)
            {
                Debug.Log("musicListResult is empty list");
                return;
            }

            Music music = this.musicListResult.music[0];
            var ret = MusicContentCenter.IsPreloaded(music.songCode, AgoraMediaType.AgoraMediaType_mv, "");
            this.Log.UpdateLog("IsPreloaded: " + ret);

            ret = MusicPlayer.Open(music.songCode, AgoraMediaType.AgoraMediaType_mv, "", 0);
            this.Log.UpdateLog("MusicPlayer.Open: " + ret);

            MusicPlayerExample.MakeVideoView((uint)MusicPlayer.GetId(), "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_MEDIA_PLAYER);
        }

        void OnGetLyricButtonClick()
        {
            if (this.musicListResult == null || this.musicListResult.count <= 0)
            {
                Debug.Log("musicListResult is empty list");
                return;
            }

            Music music = this.musicListResult.music[0];
            var ret = MusicContentCenter.GetLyric("3", music.songCode, 0);
            this.Log.UpdateLog("GetLyric: " + ret);
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

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly MusicPlayerExample _sample;

        internal UserEventHandler(MusicPlayerExample sample)
        {
            _sample = sample;
        }

        public override void OnWarning(int warn, string msg)
        {
            _sample.Log.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _sample.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _sample.Log.UpdateLog(string.Format("sdk version: ${0}",
                _sample.RtcEngine.GetVersion()));
            _sample.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                    connection.channelId, connection.localUid, elapsed));
            _sample.InitMusicPlayer(connection.localUid);
          

        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _sample.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _sample.Log.UpdateLog("OnLeaveChannel");
            MusicPlayerExample.DestroyVideoView(0);
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
            MusicPlayerExample.DestroyVideoView(uid);
        }
    }


    internal class UserMusicContentCenterEventHandler : IAgoraMusicContentCenterEventHandler
    {
        private MusicPlayerExample _sample;

        internal UserMusicContentCenterEventHandler(MusicPlayerExample sample)
        {
            this._sample = sample;
        }

        public override void OnLyricResult(string requestId, string lyricUrl)
        {
            this._sample.Log.UpdateLog(string.Format("OnLyricResult requestId:{0} lyricUrl:{1}", requestId, lyricUrl));
        }

        public override void OnMusicChartsTypeResult(string requestId, CopyRightMusicStatusCode status, MusicChartsResult result)
        {
            this._sample.Log.UpdateLog(string.Format("OnMusicChartsTypeResult requestId:{0} CopyRightMusicStatusCode:{1} result.count:{2}", requestId, status, result.count));
            var str = AgoraJson.ToJson<MusicChartsResult>(result);
            Debug.Log(str);

            this._sample.GetMusicChartButton.gameObject.SetActive(true);
            this._sample.musicChartsResult = result;
        }

        public override void OnSongListResult(string requestId, CopyRightMusicStatusCode status, MusicListResult result)
        {
            this._sample.Log.UpdateLog(string.Format("OnSongListResult requestId:{0} status:{1} result.count:{2}", requestId, status, result.count));
            var str = AgoraJson.ToJson<MusicListResult>(result);
            Debug.Log(str);

            this._sample.PreloadButton.gameObject.SetActive(true);
            this._sample.musicListResult = result;
           
        }

        public override void OnPreLoadEvent(long songCode, int percent, PreloadStatusCode status, string msg, string lyricUrl)
        {
            this._sample.Log.UpdateLog(string.Format("OnPreLoadEvent songCode:{0} percent:{1} status:{2}, msg:{3}, string:{4}", songCode, percent, status, msg, lyricUrl));
            if (status == PreloadStatusCode.PreloadStatusCode_ok)
            {
                this._sample.OpenButton.gameObject.SetActive(true);
                this._sample.GetLyricButton.gameObject.SetActive(true);
            }
            else if (status == PreloadStatusCode.PreloadStatusCode_err)
            {
                this._sample.Log.UpdateLog("PreLoad Error, Please click PreLoad Button again");
            }
        }


    }
}

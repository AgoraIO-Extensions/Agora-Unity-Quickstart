using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using System.Collections.Generic;
using io.agora.rtc.demo;


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


        public InputField RtmAppidInputField;
        public InputField RtmTokenInputField;
        public InputField RtmUidInputField;
        public Button JoinChannelButton;


        public Button GetMusicChartsButton;
        public Dropdown MusicChartsSelect;
        public Dropdown MusicCollectionSelect;
        public Text SelectedMusic;
        public Button PreloadButton;
        public Button IsPreloadButton;
        public Button OpenButton;
        public Button GetLyricButton;
        public InputField SearchInputField;
        public Button SearchMusicButton;
        public Button GetChachesButton;
        public Button RemoveCacheButton;

        internal MusicChartInfo[] CurMusicChartInfo = null;
        internal MusicCollection CurMusicCollection = null;
        internal Int64 CurSongCode = 0;

        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
            }
            hideUI();
        }
        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }


        private void hideUI()
        {
            GetMusicChartsButton.gameObject.SetActive(false);
            MusicChartsSelect.gameObject.SetActive(false);
            MusicCollectionSelect.gameObject.SetActive(false);

            SelectedMusic.gameObject.SetActive(false);
            PreloadButton.gameObject.SetActive(false);
            IsPreloadButton.gameObject.SetActive(false);
            OpenButton.gameObject.SetActive(false);
            GetLyricButton.gameObject.SetActive(false);
            SearchInputField.gameObject.SetActive(false);
            SearchMusicButton.gameObject.SetActive(false);

            RemoveCacheButton.gameObject.SetActive(false);
        }


        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        public void JoinChannelAndInitMusicContentCenter()
        {
            if (RtmAppidInputField.text == "")
            {
                this.Log.UpdateLog("Please enter you rtm appid first");
                return;
            }

            if (RtmTokenInputField.text == "")
            {
                this.Log.UpdateLog("Please enter you rtm token first");
                return;
            }

            if (RtmUidInputField.text == "")
            {
                this.Log.UpdateLog("Please enter you rtm uid(uint) first");
                return;
            }


            Int64 mccUid = 0;
            Int64.TryParse(RtmUidInputField.text, out mccUid);
            if (mccUid == 0)
            {
                this.Log.UpdateLog("rtm uid must be UInt64");
                return;
            }
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

            var appId = RtmAppidInputField.text;
            var rtmToken = RtmTokenInputField.text;
            MusicContentCenter = RtcEngine.GetMusicContentCenter();
            var config = new MusicContentCenterConfiguration(appId, rtmToken, mccUid, 10, "");
            var Ret = MusicContentCenter.Initialize(config);
            this.Log.UpdateLog("MusicContentCenter.Initialize: " + Ret);

            if (Ret != 0)
            {
                this.Log.UpdateLog("MusicContentCenter Initialize failed. Please check you appid, token or uid");
                return;
            }


            MusicContentCenter.RegisterEventHandler(new UserMusicContentCenterEventHandler(this));
            MusicPlayer = MusicContentCenter.CreateMusicPlayer();

            ChannelMediaOptions options = new ChannelMediaOptions();
            options.autoSubscribeAudio.SetValue(true);
            options.autoSubscribeVideo.SetValue(true);
            options.publishMicrophoneTrack.SetValue(false);
            options.publishCameraTrack.SetValue(false);
            options.publishMediaPlayerAudioTrack.SetValue(true);
            options.publishMediaPlayerVideoTrack.SetValue(true);
            options.enableAudioRecordingOrPlayout.SetValue(true);
            if (MusicPlayer != null)
            {
                options.publishMediaPlayerId.SetValue(MusicPlayer.GetId());
                this.Log.UpdateLog("MusicPlayer id:" + MusicPlayer.GetId());

                MusicPlayer.InitEventHandler(new MpkEventHandler(this));
            }
            else
            {
                this.Log.UpdateLog("MusicPlayer failed");
            }

            options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            var ret = RtcEngine.JoinChannel(_token, _channelName, 0, options);
            this.Log.UpdateLog("RtcEngineController JoinChannel_MPK returns: " + ret);

            this.GetMusicChartsButton.gameObject.SetActive(true);

            RtmAppidInputField.gameObject.SetActive(false);
            RtmTokenInputField.gameObject.SetActive(false);
            RtmUidInputField.gameObject.SetActive(false);
            JoinChannelButton.gameObject.SetActive(false);
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
            var logPath = Application.persistentDataPath + "/rtc.log";
            RtcEngine.SetLogFile(logPath);
            this.Log.UpdateLog(logPath);
        }

        public void OnGetMusicChartsButtonClick()
        {
            string requestId = "";
            var ret = MusicContentCenter.GetMusicCharts(ref requestId);
            this.Log.UpdateLog("GetMusicCharts: " + ret);
            this.Log.UpdateLog("requestId: " + requestId);
        }

        public void OnMusicChartsSelectValueChanged(int value)
        {
            var info = this.CurMusicChartInfo[value];

            string requestId = "";
            var ret = MusicContentCenter.GetMusicCollectionByMusicChartId(ref requestId, info.id, 0, 20);
            this.Log.UpdateLog("GetMusicCharts: " + ret);
            this.Log.UpdateLog("requestId: " + requestId);
        }

        public void OnMusicCollectSelectValueChanged(int value)
        {
            this.CurSongCode = this.CurMusicCollection.music[value].songCode;
            this.SelectedMusic.gameObject.SetActive(true);
            this.SelectedMusic.text = "Selected: " + this.CurMusicCollection.music[value].name;
            this.PreloadButton.gameObject.SetActive(true);
            this.OpenButton.gameObject.SetActive(true);
            this.GetLyricButton.gameObject.SetActive(true);
            this.IsPreloadButton.gameObject.SetActive(true);
        }


        public void OnPreloadButtonClick()
        {
            if (this.CurSongCode == 0)
            {
                Debug.Log("this.CurSongCode is 0");
                return;
            }


            var ret = MusicContentCenter.Preload(this.CurSongCode, "");
            this.Log.UpdateLog("Preload: " + ret);
        }

        public void OnOpenButtonClick()
        {
            if (this.CurSongCode == 0)
            {
                Debug.Log("this.CurSongCode is 0");
                return;
            }

            var ret = MusicPlayer.Open(this.CurSongCode, 0);
            this.Log.UpdateLog("MusicPlayer.Open: " + ret);

            MusicPlayerExample.MakeVideoView((uint)MusicPlayer.GetId(), "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_MEDIA_PLAYER);
        }

        public void OnGetLyricButtonClick()
        {
            if (this.CurSongCode == 0)
            {
                Debug.Log("this.CurSongCode is 0");
                return;
            }
            string requestId = "";
            var ret = MusicContentCenter.GetLyric(ref requestId, this.CurSongCode, 0);
            this.Log.UpdateLog("GetLyric: " + ret);
            this.Log.UpdateLog("requestId: " + requestId);
        }

        public void OnSearchMusicButtonClick()
        {
            if (this.SearchInputField.text == "")
            {
                Debug.Log("SearchInputField text null");
                return;
            }

            string requestId = "";
            var nRet = MusicContentCenter.SearchMusic(ref requestId, this.SearchInputField.text, 0, 5, "");
            this.Log.UpdateLog("SearchSong: " + nRet);
            this.Log.UpdateLog("requestId: " + requestId);
        }

        public void OnIsPrelaodButtonClick()
        {
            if (this.CurSongCode == 0)
            {
                Debug.Log("this.CurSongCode is 0");
                return;
            }

            var ret = MusicContentCenter.IsPreloaded(this.CurSongCode);
            this.Log.UpdateLog("IsPreloaded:" + ret);
        }

        public void OnGetCachesButtonClick()
        {
            MusicCacheInfo[] infos = null;
            int cacheInfoSize = 10;
            int nRet = MusicContentCenter.GetCaches(ref infos, ref cacheInfoSize);
            this.Log.UpdateLog("GetCaches: " + nRet);
            int length = infos.Length;
            for (int i = 0; i < length; i++)
            {
                this.Log.UpdateLog(string.Format("songCode: {0}  status:{1}", infos[i].songCode, infos[i].status));
            }
        }

        public void OnRemoveCacheButtonClick()
        {
            if (this.CurSongCode == 0)
            {
                Debug.Log("this.CurSongCode is 0");
                return;
            }
            int nRet = MusicContentCenter.RemoveCache(this.CurSongCode);
            this.Log.UpdateLog("RemoveCache: " + nRet);
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;

            if (MusicPlayer != null)
                MusicContentCenter.DestroyMusicPlayer(MusicPlayer);

            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
            RtcEngine = null;
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

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly MusicPlayerExample _sample;

        internal UserEventHandler(MusicPlayerExample sample)
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

            _sample.GetMusicChartsButton.gameObject.SetActive(true);
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
            MusicPlayerExample.DestroyVideoView(uid);
        }
    }

    internal class UserMusicContentCenterEventHandler : IMusicContentCenterEventHandler
    {
        private MusicPlayerExample _sample;

        internal UserMusicContentCenterEventHandler(MusicPlayerExample sample)
        {
            this._sample = sample;
        }

        public override void OnLyricResult(string requestId, Int64 songCode, string lyricUrl, MusicContentCenterStateReason reason)
        {
            this._sample.Log.UpdateLog(string.Format("OnLyricResult requestId:{0} songCode:{1} lyricUrl:{2} reason:{3}", requestId, songCode, lyricUrl, reason));
        }

        public override void OnMusicChartsResult(string requestId, MusicChartInfo[] result, MusicContentCenterStateReason reason)
        {
            this._sample.Log.UpdateLog(string.Format("OnMusicChartsResult requestId:{0} result.count:{1} reason:{2} ", requestId, result.Length, reason));
            Debug.Log(result.ToString());

            this._sample.SearchInputField.gameObject.SetActive(true);
            this._sample.SearchMusicButton.gameObject.SetActive(true);
            this._sample.GetChachesButton.gameObject.SetActive(true);
            this._sample.RemoveCacheButton.gameObject.SetActive(true);
            this._sample.CurMusicChartInfo = result;

            this._sample.MusicChartsSelect.gameObject.SetActive(true);
            this._sample.MusicChartsSelect.ClearOptions();
            List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();
            foreach (var info in result)
            {
                optionDatas.Add(new Dropdown.OptionData(info.chartName));
            }
            this._sample.MusicChartsSelect.AddOptions(optionDatas);

            this._sample.MusicCollectionSelect.gameObject.SetActive(false);
            this._sample.SelectedMusic.gameObject.SetActive(false);
            this._sample.OpenButton.gameObject.SetActive(false);
            this._sample.GetLyricButton.gameObject.SetActive(false);
            this._sample.IsPreloadButton.gameObject.SetActive(false);
            this._sample.PreloadButton.gameObject.SetActive(false);
            this._sample.Log.UpdateLog("Select your Music Chart item please");
        }

        public override void OnMusicCollectionResult(string requestId, MusicCollection result, MusicContentCenterStateReason reason)
        {
            this._sample.Log.UpdateLog(string.Format("OnMusicCollectionResult requestId:{0} result.count:{1} reason:{2}", requestId, result.count, reason));
            var str = AgoraJson.ToJson<MusicCollection>(result);
            Debug.Log(str);

            this._sample.CurMusicCollection = result;
            this._sample.MusicCollectionSelect.gameObject.SetActive(true);
            this._sample.MusicCollectionSelect.ClearOptions();
            List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();
            foreach (var info in result.music)
            {
                optionDatas.Add(new Dropdown.OptionData(info.name));
            }
            this._sample.MusicCollectionSelect.ClearOptions();
            this._sample.MusicCollectionSelect.AddOptions(optionDatas);

            this._sample.OpenButton.gameObject.SetActive(false);
            this._sample.GetLyricButton.gameObject.SetActive(false);
            this._sample.IsPreloadButton.gameObject.SetActive(false);
            this._sample.PreloadButton.gameObject.SetActive(false);
            this._sample.SelectedMusic.gameObject.SetActive(false);

            this._sample.Log.UpdateLog("Select your music item please");

        }

        public override void OnSongSimpleInfoResult(string requestId, Int64 songCode, string simpleInfo, MusicContentCenterStateReason reason)
        {

        }


        public override void OnPreLoadEvent(string requestId, Int64 songCode, int percent, string lyricUrl, PreloadState state, MusicContentCenterStateReason reason)
        {
            Debug.Log("OnPreLoadEvent percent:" + percent);
            if (state == PreloadState.kPreloadStateCompleted)
            {
                this._sample.Log.UpdateLog(string.Format("OnPreLoadEvent songCode:{0} percent:{1} lyricUrl:{2}, state:{3}, reason:{4}", songCode, percent, lyricUrl, state, reason));

                this._sample.OpenButton.gameObject.SetActive(true);
                this._sample.GetLyricButton.gameObject.SetActive(true);
            }
            else if (state == PreloadState.kPreloadStateFailed)
            {
                this._sample.Log.UpdateLog(string.Format("OnPreLoadEvent songCode:{0} percent:{1} lyricUrl:{2}, state:{3}, reason:{4}", songCode, percent, lyricUrl, state, reason));

                this._sample.Log.UpdateLog("PreLoad Failed, Please click PreLoad Button again");
            }
            else if (state == PreloadState.kPreloadStateRemoved)
            {
                this._sample.Log.UpdateLog(string.Format("OnPreLoadEvent songCode:{0} percent:{1} lyricUrl:{2}, state:{3}, reason:{4}", songCode, percent, lyricUrl, state, reason));
                this._sample.Log.UpdateLog("Remove completed");
            }
        }
    }

    internal class MpkEventHandler : IMediaPlayerSourceObserver
    {
        private readonly MusicPlayerExample _sample;

        internal MpkEventHandler(MusicPlayerExample sample)
        {
            _sample = sample;
        }

        public override void OnPlayerSourceStateChanged(MEDIA_PLAYER_STATE state, MEDIA_PLAYER_REASON reason)
        {
            _sample.Log.UpdateLog(string.Format(
                "OnPlayerSourceStateChanged state: {0}, reason: {1}, playId: {2}", state, reason, _sample.MusicPlayer.GetId()));
            Debug.Log("OnPlayerSourceStateChanged");
            if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_OPEN_COMPLETED)
            {
                _sample.MusicPlayer.Play();
            }
        }

        public override void OnPlayerEvent(MEDIA_PLAYER_EVENT @event, Int64 elapsedTime, string message)
        {
            _sample.Log.UpdateLog(string.Format("OnPlayerEvent state: {0}", @event));
        }
    }

    #endregion
}
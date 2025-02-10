using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using System.Collections.Generic;
using io.agora.rtc.demo;


namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.MusicPlayer2
{
    public class MusicPlayerExample2 : MonoBehaviour
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

        public InputField songCodeInputField;
        internal Int64 CurSongCode = 0;

        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
            }

        }
        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }


        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        public void JoinChannelAndInitMusicContentCenter()
        {

            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);


            MusicContentCenter = RtcEngine.GetMusicContentCenter();
            var config = new MusicContentCenterConfiguration(10);
            var Ret = MusicContentCenter.Initialize(config);
            this.Log.UpdateLog("MusicContentCenter.Initialize: " + Ret);
            if (Ret != 0)
            {
                this.Log.UpdateLog("MusicContentCenter Initialize failed");
                return;
            }

            var vendorConfig = new MusicContentCenterVendor2Configuration();
            vendorConfig.appId = "appid";
            vendorConfig.appKey = "appkey";
            vendorConfig.token = "token";
            vendorConfig.deviceId = "deviceId";
            vendorConfig.urlTokenExpireTime = 3600;
            vendorConfig.chargeMode = (int)ChargeMode.kChargeModeMonthly;


            string vendorString = Agora.Rtc.AgoraJson.ToJson(vendorConfig);
            Ret = MusicContentCenter.AddVendor(MusicContentCenterVendorID.kMusicContentCenterVendor2, vendorString);
            this.Log.UpdateLog("MusicContentCenter.AddVendor: " + Ret);


            if (Ret != 0)
            {
                this.Log.UpdateLog("MusicContentCenter AddVendor failed. Please check you appid, token or uid");
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


        public void OnSongCodeInputEditEnd()
        {
            var oldSongCode = this.CurSongCode;

            if (long.TryParse(this.songCodeInputField.text, out this.CurSongCode) == false)
            {
                this.CurSongCode = oldSongCode;
            }

            this.Log.UpdateLog("CurSong: " + this.CurSongCode);
        }

        public void OnGetInternalSongCodeButtonClick()
        {
            if (this.songCodeInputField.text == "")
            {
                this.Log.UpdateLog("song code cant be empty!!");
                return;
            }

            long internalSongCode = 0;
            var ret = MusicContentCenter.GetInternalSongCode(MusicContentCenterVendorID.kMusicContentCenterVendor2, this.songCodeInputField.text, "", ref internalSongCode);
            this.Log.UpdateLog("GetInternalSongCode: " + ret);
            this.Log.UpdateLog("internalSongCode: " + internalSongCode);
            this.CurSongCode = internalSongCode;
            this.songCodeInputField.text = this.CurSongCode.ToString();
        }

        public void OnPreloadButtonClick()
        {
            if (this.CurSongCode == 0)
            {
                Debug.Log("this.CurSongCode is 0");
                return;
            }

            string requestId = "";
            var ret = MusicContentCenter.Preload(ref requestId, this.CurSongCode);
            this.Log.UpdateLog("Preload: " + ret);
            this.Log.UpdateLog("requestId: " + requestId);
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

            MusicPlayerExample2.MakeVideoView((uint)MusicPlayer.GetId(), "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_MEDIA_PLAYER);
        }

        public void OnGetLyricInfoButtonClick()
        {
            if (this.CurSongCode == 0)
            {
                Debug.Log("this.CurSongCode is 0");
                return;
            }
            string requestId = "";
            var ret = MusicContentCenter.GetLyricInfo(ref requestId, this.CurSongCode);
            this.Log.UpdateLog("GetLyric: " + ret);
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





        public void OnStartScoreButtonClick()
        {
            if (this.CurSongCode == 0)
            {
                Debug.Log("this.CurSongCode is 0");
                return;
            }

            var ret = MusicContentCenter.RegisterScoreEventHandler(new ScoreEventHandler(this));
            this.Log.UpdateLog("RegisterScoreEventHandler:" + ret);

            ret = MusicContentCenter.SetScoreLevel(ScoreLevel.kScoreLevel1);
            this.Log.UpdateLog("SetScoreLevel:" + ret);

            ret = MusicContentCenter.StartScore(this.CurSongCode);
            this.Log.UpdateLog("StartScore:" + ret);
        }

        public void OnStopScoreButtonClick()
        {
            var ret = MusicContentCenter.StopScore();
            this.Log.UpdateLog("StopScore:" + ret);
            ret = MusicContentCenter.UnregisterScoreEventHandler();
            this.Log.UpdateLog("UnregisterScoreEventHandler:" + ret);
        }

        public void OnPauseScoreButtonClick()
        {
            var ret = MusicContentCenter.PauseScore();
            this.Log.UpdateLog("PauseScore:" + ret);
        }

        public void OnResumeScoreButtonClick()
        {
            var ret = MusicContentCenter.ResumeScore();
            this.Log.UpdateLog("ResumeScore:" + ret);
        }

        public void OnGetCumulativeScoreDataButtonClick()
        {
            CumulativeScoreData scoreData = new CumulativeScoreData();
            var ret = MusicContentCenter.GetCumulativeScoreData(ref scoreData);
            this.Log.UpdateLog("GetCumulativeScoreData:" + ret);
            this.Log.UpdateLog(string.Format("progressInMs:{0}, cumulativePitchScore:{1} energyScore:{2}",
                scoreData.progressInMs, scoreData.cumulativePitchScore, scoreData.energyScore));
        }

        public void OnRegisterAudioFrameObserverButtonClick()
        {
            var ret = MusicContentCenter.RegisterAudioFrameObserver(new AudioFrameObserver(),
                AUDIO_FRAME_POSITION.AUDIO_FRAME_POSITION_PLAYBACK |
                AUDIO_FRAME_POSITION.AUDIO_FRAME_POSITION_RECORD |
                AUDIO_FRAME_POSITION.AUDIO_FRAME_POSITION_MIXED |
                AUDIO_FRAME_POSITION.AUDIO_FRAME_POSITION_BEFORE_MIXING |
                AUDIO_FRAME_POSITION.AUDIO_FRAME_POSITION_EAR_MONITORING,
               OBSERVER_MODE.RAW_DATA);
            this.Log.UpdateLog("RegisterAudioFrameObserver: " + ret);
        }

        public void OnUnregisterAudioFrameObserverButtonClick()
        {
            var ret = MusicContentCenter.UnregisterAudioFrameObserver();
            this.Log.UpdateLog("UnregisterAudioFrameObserver: " + ret);
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;

            if (MusicPlayer != null)
                MusicContentCenter.DestroyMusicPlayer(MusicPlayer);


            MusicContentCenter.UnregisterEventHandler();
            MusicContentCenter.UnregisterScoreEventHandler();
            MusicContentCenter.UnregisterAudioFrameObserver();

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
        private readonly MusicPlayerExample2 _sample;

        internal UserEventHandler(MusicPlayerExample2 sample)
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
        }
    }

    internal class UserMusicContentCenterEventHandler : IMusicContentCenterEventHandler
    {
        private MusicPlayerExample2 _sample;

        internal UserMusicContentCenterEventHandler(MusicPlayerExample2 sample)
        {
            this._sample = sample;
        }

        public override void OnLyricResult(string requestId, Int64 songCode, string lyricUrl, MusicContentCenterStateReason reason)
        {
        }

        public override void OnMusicChartsResult(string requestId, MusicChartInfo[] result, MusicContentCenterStateReason reason)
        {

        }

        public override void OnMusicCollectionResult(string requestId, MusicCollection result, MusicContentCenterStateReason reason)
        {

        }

        public override void OnSongSimpleInfoResult(string requestId, Int64 songCode, string simpleInfo, MusicContentCenterStateReason reason)
        {

        }


        public override void OnPreLoadEvent(string requestId, long internalSongCode, int percent, string payload, MusicContentCenterState status, MusicContentCenterStateReason reason)
        {
            Debug.Log("OnPreLoadEvent percent:" + percent);
            this._sample.Log.UpdateLog(string.Format("OnPreLoadEvent internalSongCode:{0} percent:{1} payload:{2}, status:{3}, reason:{4}", internalSongCode, percent, payload, status, reason));

            if (status == MusicContentCenterState.kMusicContentCenterStatePreloadOk)
            {
                
            }
            else if (status == MusicContentCenterState.kMusicContentCenterStatePreloadFailed)
            {
                this._sample.Log.UpdateLog("PreLoad Failed, Please click PreLoad Button again");
            }
            else if (status == MusicContentCenterState.kMusicContentCenterStatePreloadRemoved)
            {
                this._sample.Log.UpdateLog("Remove completed");
            }
        }

        public override void OnLyricInfoResult(string requestId, long songCode, ILyricInfo lyricInfo, MusicContentCenterStateReason reason)
        {
            this._sample.Log.UpdateLog("OnLyricInfoResult reason: " + reason);
            this._sample.Log.UpdateLog("lyricInfo.sentenceCount: " + lyricInfo.sentenceCount);
            this._sample.Log.UpdateLog("lyricInfo.singer: " + lyricInfo.singer);
        }

        public override void OnStartScoreResult(long internalSongCode, MusicContentCenterState status, MusicContentCenterStateReason reason)
        {
            this._sample.Log.UpdateLog(string.Format("OnStartScoreResult internalSongCode: {0}, status: {1}, reason:{2} ",
                internalSongCode, status, reason));
        }
    }

    internal class MpkEventHandler : IMediaPlayerSourceObserver
    {
        private readonly MusicPlayerExample2 _sample;

        internal MpkEventHandler(MusicPlayerExample2 sample)
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

    internal class AudioFrameObserver : IAudioFrameObserver
    {
        public override bool OnRecordAudioFrame(string channelId, AudioFrame audioFrame)
        {
            Debug.Log("OnRecordAudioFrame-----------");
            return true;
        }

        public override bool OnPlaybackAudioFrame(string channelId, AudioFrame audioFrame)
        {
            Debug.Log("OnPlaybackAudioFrame-----------");
            return true;
        }

        public override bool OnPlaybackAudioFrameBeforeMixing(string channel_id,
                                                        uint uid,
                                                        AudioFrame audio_frame)
        {
            Debug.Log("OnPlaybackAudioFrameBeforeMixing-----------");
            return true;
        }

        public override bool OnPlaybackAudioFrameBeforeMixing(string channel_id,
                                                        string uid,
                                                        AudioFrame audio_frame)
        {
            Debug.Log("OnPlaybackAudioFrameBeforeMixing2-----------");
            return true;
        }
    }

    internal class ScoreEventHandler : IScoreEventHandler
    {
        private MusicPlayerExample2 _sample;
        public ScoreEventHandler(MusicPlayerExample2 sample)
        {
            this._sample = sample;
        }

        public override void OnLineScore(long songCode, LineScoreData lineScoreData)
        {
            string foramt = string.Format("songCode: {0}, progressInMs: {1}, index: {2}, totalLines: {3}, " +
                "pitchScore: {4}, cumulativePitchScore: {5}, energyScore: {6}",
                songCode, lineScoreData.progressInMs, lineScoreData.index, lineScoreData.totalLines,
                lineScoreData.pitchScore, lineScoreData.cumulativePitchScore, lineScoreData.energyScore
                );
            this._sample.Log.UpdateLog(foramt);
        }

        public override void OnPitch(long songCode, RawScoreData rawScoreData)
        {
            string foramt = string.Format("songCode: {0}, progressInMs: {1}, speakerPitch: {2}, pitchScore: {3}",
               songCode, rawScoreData.progressInMs, rawScoreData.speakerPitch, rawScoreData.pitchScore
               );
            this._sample.Log.UpdateLog(foramt);
        }
    }



    #endregion
}
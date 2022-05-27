using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using agora.rtc;
using agora.util;
using Logger = agora.util.Logger;
using System;
using System.Collections.Generic;
using System.IO;

namespace Agora_Plugin.API_Example.examples.advanced.StartLocalVideoTranscoder
{

    public class StartLocalVideoTranscoder : MonoBehaviour
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
        internal IMediaPlayer mediaPlayer = null;
        internal int playerId = 0;
        public List<uint> remoteUserUids = new List<uint>();


        public Toggle toggleRecord;
        public Toggle togglePrimartCamera;
        public Toggle toggleSecondaryCamera;
        public Toggle togglePng;
        public Toggle toggleJpg;
        public Toggle toggleJif;
        public Toggle toggleRemote;
        public Toggle toggleScreenShare;
        public Toggle toggleMediaPlay;


        private void Start()
        {
            LoadAssetData();
            CheckAppId();
            SetUpUI();
            InitEngine();
            //InitMediaPlayer();
            //JoinChannel();
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


        private void SetUpUI()
        {
            var ui = this.transform.Find("UI");

            var btn = ui.Find("StartButton").GetComponent<Button>();
            btn.onClick.AddListener(OnStartButtonPress);

            btn = ui.Find("UpdateButton").GetComponent<Button>();
            btn.onClick.AddListener(OnUpdateButtonPress);

            btn = ui.Find("StopButton").GetComponent<Button>();
            btn.onClick.AddListener(OnStopButtonPress);
        }

        private void CheckAppId()
        {
            Logger = new Logger(logText);
            Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        private void InitEngine()
        {
            mRtcEngine = RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(appID, 0, true,
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            var ret = mRtcEngine.Initialize(context);
            Debug.Log("Agora: Initialize " + ret);
            mRtcEngine.InitEventHandler(handler);
            mRtcEngine.EnableAudio();
            mRtcEngine.EnableVideo();
        
            //var videoCanvas = new VideoCanvas();
            //videoCanvas.sourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_TRANSCODED;
            //mRtcEngine.SetupLocalVideo(videoCanvas);
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            mRtcEngine.StartPreview(VIDEO_SOURCE_TYPE.VIDEO_SOURCE_TRANSCODED);
            MakeVideoView(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_TRANSCODED);
        }

        private void InitMediaPlayer()
        {
            mediaPlayer = mRtcEngine.GetMediaPlayer();
            if (mediaPlayer == null)
            {
                Debug.Log("GetAgoraRtcMediaPlayer failed!");
            }

            playerId = mediaPlayer.CreateMediaPlayer();
            Debug.Log("playerId id: " + playerId);
        }

        private void JoinChannel()
        {
            var options = new ChannelMediaOptions();
            options.publishCameraTrack.SetValue(false);
            options.publishSecondaryCameraTrack.SetValue(false);
            options.publishTrancodedVideoTrack.SetValue(true);
            mRtcEngine.JoinChannel(token, channelName, 0, options);
        }

        LocalTranscoderConfiguration GenerateLocalTranscoderConfiguration()
        {

            List<TranscodingVideoStream> list = new List<TranscodingVideoStream>();

            if (this.toggleRecord.isOn)
            {
                list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.AUDIO_RECORDING_SOURCE, 0, "", 0, 0, 0, 0, 1, 1, false));
            }

            if (this.togglePrimartCamera.isOn)
            {
                var videoDeviceManager = mRtcEngine.GetVideoDeviceManager();
                var devices = videoDeviceManager.EnumerateVideoDevices();

                if (devices.Length >= 1)
                {
                     var configuration = new CameraCapturerConfiguration()
                    {
                        format = new VideoFormat(640, 320, 30),
                        deviceId = devices[0].deviceId
                    };
                    var nRet= this.mRtcEngine.StartPrimaryCameraCapture(configuration);
                    this.Logger.UpdateLog("StartPrimaryCameraCapture :" + nRet);
                    list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.PRIMARY_CAMERA_SOURCE, 0, "", 0, 0, 640, 320, 1, 1, false));
                }
                else
                {
                    this.Logger.UpdateLog("PRIMARY_CAMERA Not Found!");
                }
            }

            if (this.toggleSecondaryCamera.isOn)
            {
                var videoDeviceManager = mRtcEngine.GetVideoDeviceManager();
                var devices = videoDeviceManager.EnumerateVideoDevices();

                if (devices.Length >= 2)
                {
                    var configuration = new CameraCapturerConfiguration()
                    {
                        format = new VideoFormat(640, 320, 30),
                        deviceId = devices[0].deviceId
                    };
                    this.mRtcEngine.StartSecondaryCameraCapture(configuration);

                    list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.SECONDARY_CAMERA_SOURCE, 0, "", 0, 0, 360, 240, 1, 1, false));
                }
                else
                {
                    this.Logger.UpdateLog("SECONDARY_CAMERA Not Found!");
                }
            }

            if (this.togglePng.isOn)
            {
                var filePath = Path.Combine(Application.streamingAssetsPath, "img/png.png");
                list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.RTC_IMAGE_PNG_SOURCE, 0, filePath, 0, 0, 640, 360, 1, 1, false));
            }

            if (this.toggleJpg.isOn)
            {
                var filePath = Path.Combine(Application.streamingAssetsPath, "img/jpg.jpg");
                list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.RTC_IMAGE_JPEG_SOURCE, 0, filePath, 360, 240, 360, 240, 1, 1, false));
            }


            if (this.toggleJif.isOn)
            {
                var filePath = Path.Combine(Application.streamingAssetsPath, "img/gif.git");
                list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.RTC_IMAGE_GIF_SOURCE, 0, filePath, 360, 0, 360, 240, 1, 1, false));
            }

            if (this.toggleRemote.isOn)
            {
                if (this.remoteUserUids.Count >= 1)
                {
                    var remoteUserUid = this.remoteUserUids[0];
                    list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.REMOTE_VIDEO_SOURCE, remoteUserUid, "", 100, 100, 100, 100, 1, 1, false));
                }
                else
                {
                    this.Logger.UpdateLog("remote user not found");
                }
            }

            if (this.toggleScreenShare.isOn)
            {
                list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.PRIMARY_SCREEN_SOURCE, 0, "", 0, 0, 640, 320, 1, 1, false));
            }

            if (this.toggleMediaPlay.isOn)
            {
                var ret = this.mediaPlayer.Open(this.playerId, "https://big-class-test.oss-cn-hangzhou.aliyuncs.com/61102.1592987815092.mp4", 0);
                this.Logger.UpdateLog("Media palyer ret:" + ret);
                var sourceId = this.playerId;
                this.Logger.UpdateLog("Media palyer ret:" + ret);
                list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.MEDIA_PLAYER_SOURCE, 0, sourceId.ToString(), 0, 0, 360, 240, 1, 1, false));
            }

            var conf = new LocalTranscoderConfiguration();
            conf.streamCount = Convert.ToUInt32(list.Count);
            conf.VideoInputStreams = new TranscodingVideoStream[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                conf.VideoInputStreams[i] = list[i];
            }
            conf.videoOutputConfiguration.dimensions.width = 1080;
            conf.videoOutputConfiguration.dimensions.height = 960;

            return conf;
        }

        void OnStartButtonPress()
        {
            var conf = this.GenerateLocalTranscoderConfiguration();
            var nRet = mRtcEngine.StartLocalVideoTranscoder(conf);
            this.Logger.UpdateLog("StartLocalVideoTranscoder:" + nRet);
        }


        void OnUpdateButtonPress()
        {
            var conf = this.GenerateLocalTranscoderConfiguration();
            var nRet = mRtcEngine.UpdateLocalTranscoderConfiguration(conf);
            this.Logger.UpdateLog("UpdateLocalTranscoderConfiguration:" + nRet);
        }

        void OnStopButtonPress()
        {
            var nRet = mRtcEngine.StopLocalVideoTranscoder();
            this.Logger.UpdateLog("StopLocalVideoTranscoder:" + nRet);
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (mRtcEngine == null) return;
            mRtcEngine.InitEventHandler(null);
            mRtcEngine.LeaveChannel();
            mRtcEngine.Dispose();
        }

        //private void OnApplicationQuit()
        //{
        //    Debug.Log("OnApplicationQuit");
        //    if (mRtcEngine != null)
        //    {
        //        mRtcEngine.InitEventHandler(null);
        //        mRtcEngine.LeaveChannel();
        //        mRtcEngine.Dispose();
        //        mRtcEngine = null;
        //    }
        //}

        internal static VideoSurface MakeVideoView(uint uid, string channelId = "", VIDEO_SOURCE_TYPE source = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA)
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return null;
            }

            // create a GameObject and assign to this new user
            var videoSurface = MakeImageSurface(uid.ToString());
            if (ReferenceEquals(videoSurface, null)) return null;
            // configure videoSurface
            if (uid == 0)
            {
                videoSurface.SetForUser(uid, channelId, source);
            }
            else
            {
                videoSurface.SetForUser(uid, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            }
            videoSurface.SetEnable(true);
            return videoSurface;
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

        internal class UserEventHandler : IRtcEngineEventHandler
        {
            private readonly StartLocalVideoTranscoder _sample;

            internal UserEventHandler(StartLocalVideoTranscoder sample)
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

                StartLocalVideoTranscoder.MakeVideoView(0);
            }

            public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _sample.Logger.UpdateLog("OnRejoinChannelSuccess");
            }

            public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
            {
                _sample.Logger.UpdateLog("OnLeaveChannel");
                StartLocalVideoTranscoder.DestroyVideoView(0);
            }

            public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
            {
                _sample.Logger.UpdateLog("OnClientRoleChanged");
            }

            public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
            {
                _sample.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
                StartLocalVideoTranscoder.MakeVideoView(uid, _sample.channelName);

                if (_sample.remoteUserUids.Contains(uid) == false)
                    _sample.remoteUserUids.Add(uid);
            }

            public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
            {
                _sample.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                    (int)reason));
                StartLocalVideoTranscoder.DestroyVideoView(uid);
                _sample.remoteUserUids.Remove(uid);
            }
        }


    }
}

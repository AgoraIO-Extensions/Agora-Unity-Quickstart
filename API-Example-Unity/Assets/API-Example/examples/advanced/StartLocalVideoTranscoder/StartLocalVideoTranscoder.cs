using System;
using System.Collections.Generic;
using System.IO;
using agora.rtc;
using agora.util;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Logger = agora.util.Logger;

namespace Agora_Plugin.API_Example.examples.advanced.StartLocalVideoTranscoder
{

    public class StartLocalVideoTranscoder : MonoBehaviour
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
     
        internal List<uint> RemoteUserUids = new List<uint>();


        public Toggle ToggleRecord;
        public Toggle TogglePrimartCamera;
        public Toggle ToggleSecondaryCamera;
        public Toggle TogglePng;
        public Toggle ToggleJpg;
        public Toggle ToggleGif;
        public Toggle ToggleRemote;
        public Toggle ToggleScreenShare;
        public Toggle ToggleMediaPlay;


        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                SetUpUI();
                InitEngine();
                //InitMediaPlayer();
                //JoinChannel();
            }
        }


        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
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

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        private void InitEngine()
        {
            RtcEngine = agora.rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            var ret = RtcEngine.Initialize(context);
            Debug.Log("Agora: Initialize " + ret);
            RtcEngine.InitEventHandler(handler);
        }

        private void InitMediaPlayer()
        {
            MediaPlayer = RtcEngine.CreateMediaPlayer();
            if (MediaPlayer == null)
            {
                Debug.Log("GetAgoraRtcMediaPlayer failed!");
            }
            Debug.Log("playerId id: " + MediaPlayer.GetId());
        }

        private void JoinChannel()
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.StartPreview(VIDEO_SOURCE_TYPE.VIDEO_SOURCE_TRANSCODED);
            MakeVideoView(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_TRANSCODED);
            
            var options = new ChannelMediaOptions();
            options.publishCameraTrack.SetValue(false);
            options.publishSecondaryCameraTrack.SetValue(false);
            options.publishTrancodedVideoTrack.SetValue(true);
            RtcEngine.JoinChannel(_token, _channelName, 0, options);
        }

        private LocalTranscoderConfiguration GenerateLocalTranscoderConfiguration()
        {

            List<TranscodingVideoStream> list = new List<TranscodingVideoStream>();

            if (this.ToggleRecord.isOn)
            {
                list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.AUDIO_RECORDING_SOURCE, 0, "", 0, 0, 0, 0, 1, 1, false));
            }

            if (this.TogglePrimartCamera.isOn)
            {
                var videoDeviceManager = RtcEngine.GetVideoDeviceManager();
                var devices = videoDeviceManager.EnumerateVideoDevices();

                if (devices.Length >= 1)
                {
                    var configuration = new CameraCapturerConfiguration()
                    {
                        format = new VideoFormat(640, 320, 30),
                        deviceId = devices[0].deviceId
                    };
                    var nRet = this.RtcEngine.StartPrimaryCameraCapture(configuration);
                    this.Log.UpdateLog("StartPrimaryCameraCapture :" + nRet);
                    list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.PRIMARY_CAMERA_SOURCE, 0, "", 0, 0, 640, 320, 1, 1, false));
                }
                else
                {
                    this.Log.UpdateLog("PRIMARY_CAMERA Not Found!");
                }
            }

            if (this.ToggleSecondaryCamera.isOn)
            {
                var videoDeviceManager = RtcEngine.GetVideoDeviceManager();
                var devices = videoDeviceManager.EnumerateVideoDevices();

                if (devices.Length >= 2)
                {
                    var configuration = new CameraCapturerConfiguration()
                    {
                        format = new VideoFormat(640, 320, 30),
                        deviceId = devices[0].deviceId
                    };
                    this.RtcEngine.StartSecondaryCameraCapture(configuration);

                    list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.SECONDARY_CAMERA_SOURCE, 0, "", 0, 0, 360, 240, 1, 1, false));
                }
                else
                {
                    this.Log.UpdateLog("SECONDARY_CAMERA Not Found!");
                }
            }

            if (this.TogglePng.isOn)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                // On Android, the StreamingAssetPath is just accessed by /assets instead of Application.streamingAssetPath
                var filePath = "/assets/img/png.png";
#else
                var filePath = Path.Combine(Application.streamingAssetsPath, "img/png.png");
#endif
                list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.RTC_IMAGE_PNG_SOURCE, 0, filePath, 0, 0, 640, 360, 1, 1, false));
            }

            if (this.ToggleJpg.isOn)
            {

#if UNITY_ANDROID && !UNITY_EDITOR
                // On Android, the StreamingAssetPath is just accessed by /assets instead of Application.streamingAssetPath
                var filePath = "/assets/img/jpg.jpg";
#else
                var filePath = Path.Combine(Application.streamingAssetsPath, "img/jpg.jpg");
#endif
                list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.RTC_IMAGE_JPEG_SOURCE, 0, filePath, 360, 240, 360, 240, 1, 1, false));
            }


            if (this.ToggleGif.isOn)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                // On Android, the StreamingAssetPath is just accessed by /assets instead of Application.streamingAssetPath
                var filePath = "/assets/img/gif.git";
#else
                var filePath = Path.Combine(Application.streamingAssetsPath, "img/gif.git");
#endif
                list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.RTC_IMAGE_GIF_SOURCE, 0, filePath, 360, 0, 360, 240, 1, 1, false));
            }

            if (this.ToggleRemote.isOn)
            {
                if (this.RemoteUserUids.Count >= 1)
                {
                    var remoteUserUid = this.RemoteUserUids[0];
                    list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.REMOTE_VIDEO_SOURCE, remoteUserUid, "", 100, 100, 100, 100, 1, 1, false));
                }
                else
                {
                    this.Log.UpdateLog("remote user not found");
                }
            }

            if (this.ToggleScreenShare.isOn)
            {
                list.Add(new TranscodingVideoStream(MEDIA_SOURCE_TYPE.PRIMARY_SCREEN_SOURCE, 0, "", 0, 0, 640, 320, 1, 1, false));
            }

            if (this.ToggleMediaPlay.isOn)
            {
                var ret = this.MediaPlayer.Open( "https://big-class-test.oss-cn-hangzhou.aliyuncs.com/61102.1592987815092.mp4", 0);
                this.Log.UpdateLog("Media palyer ret:" + ret);
                var sourceId = this.MediaPlayer.GetId();
                this.Log.UpdateLog("Media palyer ret:" + ret);
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

        private void OnStartButtonPress()
        {
            var conf = this.GenerateLocalTranscoderConfiguration();
            var nRet = RtcEngine.StartLocalVideoTranscoder(conf);
            this.Log.UpdateLog("StartLocalVideoTranscoder:" + nRet);
        }


        private void OnUpdateButtonPress()
        {
            var conf = this.GenerateLocalTranscoderConfiguration();
            var nRet = RtcEngine.UpdateLocalTranscoderConfiguration(conf);
            this.Log.UpdateLog("UpdateLocalTranscoderConfiguration:" + nRet);
        }

        private void OnStopButtonPress()
        {
            var nRet = RtcEngine.StopLocalVideoTranscoder();
            this.Log.UpdateLog("StopLocalVideoTranscoder:" + nRet);
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }


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

            videoSurface.OnTextureSizeModify += (int width, int height) =>
            {
                float scale = (float)height / (float)width;
                videoSurface.transform.localScale = new Vector3(5, 5 * scale, 1);
                Debug.Log("OnTextureSizeModify: " + width + "  " + height);
            };

            videoSurface.SetEnable(true);
            return videoSurface;
        }

        // VIDEO TYPE 1: 3D Object
        private static VideoSurface MakePlaneSurface(string goName)
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

            public override void OnError(int err, string msg)
            {
                _sample.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
            }

            public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                Debug.Log("Agora: OnJoinChannelSuccess ");
                _sample.Log.UpdateLog(string.Format("sdk version: ${0}",
                    _sample.RtcEngine.GetVersion()));
                _sample.Log.UpdateLog(
                    string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                    connection.channelId, connection.localUid, elapsed));

                StartLocalVideoTranscoder.MakeVideoView(0);
            }

            public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _sample.Log.UpdateLog("OnRejoinChannelSuccess");
            }

            public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
            {
                _sample.Log.UpdateLog("OnLeaveChannel");
                StartLocalVideoTranscoder.DestroyVideoView(0);
            }

            public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
            {
                _sample.Log.UpdateLog("OnClientRoleChanged");
            }

            public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
            {
                _sample.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
                StartLocalVideoTranscoder.MakeVideoView(uid, _sample._channelName);

                if (_sample.RemoteUserUids.Contains(uid) == false)
                    _sample.RemoteUserUids.Add(uid);
            }

            public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
            {
                _sample.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                    (int)reason));
                StartLocalVideoTranscoder.DestroyVideoView(uid);
                _sample.RemoteUserUids.Remove(uid);
            }
        }


    }
}

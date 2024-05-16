using System;
using System.IO;
using System.Collections.Generic;
using Agora.Rtc;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using io.agora.rtc.demo;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.StartLocalVideoTranscoder
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
                InitMediaPlayer();
                JoinChannel();
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
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext();
            context.appId = _appID;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
            context.areaCode = AREA_CODE.AREA_CODE_GLOB;
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
            MpkEventHandler handler = new MpkEventHandler(this);
            MediaPlayer.InitEventHandler(handler);
            Debug.Log("playerId id: " + MediaPlayer.GetId());
        }

        private void JoinChannel()
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            MakeVideoView(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_TRANSCODED);

            var options = new ChannelMediaOptions();
            options.publishCameraTrack.SetValue(false);
            options.publishSecondaryCameraTrack.SetValue(false);
            options.publishTranscodedVideoTrack.SetValue(true);
            RtcEngine.JoinChannel(_token, _channelName, 0, options);
        }

        private LocalTranscoderConfiguration GenerateLocalTranscoderConfiguration()
        {

            List<TranscodingVideoStream> list = new List<TranscodingVideoStream>();

            if (this.TogglePrimartCamera.isOn)
            {
                var videoDeviceManager = RtcEngine.GetVideoDeviceManager();
                var devices = videoDeviceManager.EnumerateVideoDevices();

                if (devices.Length >= 1)
                {
                    var configuration = new CameraCapturerConfiguration();
                    configuration.format = new VideoFormat(640, 320, 30);
                    configuration.deviceId.SetValue(devices[0].deviceId);
                    var nRet = this.RtcEngine.StartCameraCapture(VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA, configuration);
                    this.Log.UpdateLog("StartCameraCapture :" + nRet);
                    var item = new TranscodingVideoStream();
                    item.sourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA;
                    item.x = 0;
                    item.y = 0;
                    item.width = 640;
                    item.height = 320;
                    list.Add(item);
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
                    var configuration = new CameraCapturerConfiguration();
                    configuration.format = new VideoFormat(640,320,30);
                    configuration.deviceId.SetValue(devices[1].deviceId);
                    this.RtcEngine.StartCameraCapture(VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA_SECONDARY, configuration);
                    var item = new TranscodingVideoStream();
                    item.sourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA_SECONDARY;
                    item.x = 0;
                    item.y = 0;
                    item.width = 360;
                    item.height = 240;
                    list.Add(item);
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
                var item = new TranscodingVideoStream();
                item.sourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_RTC_IMAGE_PNG;
                item.imageUrl = filePath;
                item.x = 320;
                item.y = 180;
                item.width = 640;
                item.height = 360;
                list.Add(item);
            }

            if (this.ToggleJpg.isOn)
            {

#if UNITY_ANDROID && !UNITY_EDITOR
                // On Android, the StreamingAssetPath is just accessed by /assets instead of Application.streamingAssetPath
                var filePath = "/assets/img/jpg.jpg";
#else
                var filePath = Path.Combine(Application.streamingAssetsPath, "img/jpg.jpg");
#endif
                var item = new TranscodingVideoStream();
                item.sourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_RTC_IMAGE_JPEG;
                item.imageUrl = filePath;
                item.x = 320;
                item.y = 240;
                item.width = 360;
                item.height = 240;
                list.Add(item);
            }


            if (this.ToggleGif.isOn)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                // On Android, the StreamingAssetPath is just accessed by /assets instead of Application.streamingAssetPath
                var filePath = "/assets/img/gif.gif";
#else
                var filePath = Path.Combine(Application.streamingAssetsPath, "img/gif.gif");
#endif
                var item = new TranscodingVideoStream();
                item.sourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_RTC_IMAGE_GIF;
                item.imageUrl = filePath;
                item.x = 0;
                item.y = 0;
                item.width = 476;
                item.height = 280;
                list.Add(item);
            }

            if (this.ToggleRemote.isOn)
            {
                if (this.RemoteUserUids.Count >= 1)
                {
                    var remoteUserUid = this.RemoteUserUids[0];
                    var item = new TranscodingVideoStream();
                    item.sourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE;
                    item.x = 200;
                    item.y = 200;
                    item.width = 100;
                    item.height = 100;
                    item.remoteUserUid = remoteUserUid;
                    list.Add(item);
                }
                else
                {
                    this.Log.UpdateLog("remote user not found");
                }
            }

            if (this.ToggleScreenShare.isOn)
            {
                this.StopScreenShare();
                if (this.StartScreenShare())
                {
                    var item = new TranscodingVideoStream();
                    item.sourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN;
                    item.x = 480;
                    item.y = 640;
                    item.width = 640;
                    item.height = 320;
                    list.Add(item);
                }
            }
            else
            {
                this.StopScreenShare();
            }

            if (this.ToggleMediaPlay.isOn)
            {
                this.MediaPlayer.Stop();
                var ret = this.MediaPlayer.Open("https://big-class-test.oss-cn-hangzhou.aliyuncs.com/61102.1592987815092.mp4", 0);
                this.Log.UpdateLog("Media palyer ret:" + ret);
                var item = new TranscodingVideoStream();
                item.sourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_MEDIA_PLAYER;
                item.mediaPlayerId = MediaPlayer.GetId();
                item.x = 0;
                item.y = 0;
                item.width = 1080;
                item.height = 960;
                list.Add(item);
            }
            else
            {
                this.MediaPlayer.Stop();
            }

            var conf = new LocalTranscoderConfiguration();
            conf.streamCount = Convert.ToUInt32(list.Count);
            conf.videoInputStreams = list.ToArray();
            conf.videoOutputConfiguration.dimensions.width = 1080;
            conf.videoOutputConfiguration.dimensions.height = 960;

            return conf;
        }


        private bool StartScreenShare()
        {
#if UNITY_IPHONE || UNITY_ANDROID
            this.Log.UpdateLog("Not Support Screen Share in this platform!");
            return false;
#else
            SIZE t = new SIZE();
            t.width = 360;
            t.height = 240;
            SIZE s = new SIZE();
            s.width = 360;
            s.height = 240;
            var info = RtcEngine.GetScreenCaptureSources(t, s, true);

            if (info.Length > 0)
            {
                ScreenCaptureSourceInfo item = info[0];
                if (item.type == ScreenCaptureSourceType.ScreenCaptureSourceType_Window)
                {
                    RtcEngine.StartScreenCaptureByWindowId(item.sourceId, default(Rectangle),
                       default(ScreenCaptureParameters));
                }
                else
                {
                    RtcEngine.StartScreenCaptureByDisplayId((uint)item.sourceId, default(Rectangle),
                 new ScreenCaptureParameters { captureMouseCursor = true, frameRate = 30 });
                }
                return true;
            }
            else
            {
                this.Log.UpdateLog("Not Screen can share");
                return false;
            }
#endif
        }

        private void StopScreenShare()
        {
#if UNITY_IPHONE || UNITY_ANDROID
            this.Log.UpdateLog("Not Support Screen Share in this platform!");
#else
            RtcEngine.StopScreenCapture();
#endif
        }

        private void OnStartButtonPress()
        {
            var conf = this.GenerateLocalTranscoderConfiguration();
            var nRet = RtcEngine.StartLocalVideoTranscoder(conf);
            this.Log.UpdateLog("StartLocalVideoTranscoder:" + nRet);

            var options = new ChannelMediaOptions();
            options.publishCameraTrack.SetValue(false);
            options.publishSecondaryCameraTrack.SetValue(false);
            options.publishTranscodedVideoTrack.SetValue(true);
            RtcEngine.UpdateChannelMediaOptions(options);
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
            MediaPlayer.Stop();
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }

        internal string GetChannelName()
        {
            return _channelName;
        }

        #region -- Video Render UI Logic ---

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
            var mesh = go.GetComponent<MeshRenderer>();
            if (mesh != null)
            {
                Debug.LogWarning("VideoSureface update shader");
                mesh.material = new Material(Shader.Find("Unlit/Texture"));
            }
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

        #endregion
    }

    #region -- Agora Event ---

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
            int build = 0;
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _sample.Log.UpdateLog(string.Format("sdk version: ${0}",
                _sample.RtcEngine.GetVersion(ref build)));
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

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _sample.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _sample.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            StartLocalVideoTranscoder.MakeVideoView(uid, _sample.GetChannelName());

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

    internal class MpkEventHandler : IMediaPlayerSourceObserver
    {
        private readonly StartLocalVideoTranscoder _sample;

        internal MpkEventHandler(StartLocalVideoTranscoder sample)
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
                _sample.MediaPlayer.Play();
            }
        }

        public override void OnPlayerEvent(MEDIA_PLAYER_EVENT @event, Int64 elapsedTime, string message)
        {
            _sample.Log.UpdateLog(string.Format("OnPlayerEvent state: {0}", @event));
        }
    }

    #endregion
}

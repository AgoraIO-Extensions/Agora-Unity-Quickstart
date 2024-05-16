using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using io.agora.rtc.demo;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.SpatialAudioWithMediaPlayer
{
    public class SpatialAudioWithMediaPlayer : MonoBehaviour
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
        internal IRtcEngineEx RtcEngine = null;
        internal IMediaPlayer MediaPlayer = null;


        private const string MPK_URL =
            "https://agoracdn.s3.us-west-1.amazonaws.com/videos/Agora.io-Interactions.mp4";

        private Button _button1;
        private Button _button2;
        private Button _button3;

        public uint UidUseInEx = 123;
        public uint UidUseInMPK = 67890;

        public ILocalSpatialAudioEngine SpatialAudioEngine;
        public int x = 0;
        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                SetUpUI();
                InitEngine();
                InitMediaPlayer();
                InitSpatialAudioEngine();
                JoinChannelEx(_channelName, UidUseInEx);

                //We use the mpk to simulate the voice of remote users.
                JoinChannelExWithMPK(_channelName, UidUseInMPK, MediaPlayer.GetId());
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

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        private void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngineEx();
            UserEventHandler handler = new UserEventHandler(this);

            //If you use a Bluetooth headset, you need to set AUDIO_SCENARIO_TYPE to AUDIO_SCENARIO_GAME_STREAMING.
            RtcEngineContext context = new RtcEngineContext();
            context.appId = _appID;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING;
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
                return;
            }

            MpkEventHandler handler = new MpkEventHandler(this);
            MediaPlayer.InitEventHandler(handler);
            Debug.Log("playerId id: " + MediaPlayer.GetId());
        }

        private void InitSpatialAudioEngine()
        {
            SpatialAudioEngine = RtcEngine.GetLocalSpatialAudioEngine();
            var ret = SpatialAudioEngine.Initialize();
            Debug.Log("_spatialAudioEngine: Initialize " + ret);
            SpatialAudioEngine.SetAudioRecvRange(30);
        }

        private void JoinChannelEx(string channelName, uint uid)
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.EnableSpatialAudio(true);
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

            RtcConnection connection = new RtcConnection();
            connection.channelId = channelName;
            connection.localUid = uid;
            ChannelMediaOptions options = new ChannelMediaOptions();
            options.autoSubscribeAudio.SetValue(true);
            options.autoSubscribeVideo.SetValue(true);
            options.publishCameraTrack.SetValue(true);
            options.publishMicrophoneTrack.SetValue(false);
            options.enableAudioRecordingOrPlayout.SetValue(true);
            options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            var ret = RtcEngine.JoinChannelEx("", connection, options);
            Debug.Log("RtcEngineController JoinChannelEx returns: " + ret);
        }

        private void JoinChannelExWithMPK(string channelName, uint uid, int playerId)
        {
            RtcConnection connection = new RtcConnection();
            connection.channelId = channelName;
            connection.localUid = uid;
            ChannelMediaOptions options = new ChannelMediaOptions();
            options.autoSubscribeAudio.SetValue(false);
            options.autoSubscribeVideo.SetValue(true);
            options.publishCameraTrack.SetValue(false);
            options.publishMediaPlayerAudioTrack.SetValue(true);
            options.publishMediaPlayerVideoTrack.SetValue(true);
            options.publishMediaPlayerId.SetValue(playerId);
            options.enableAudioRecordingOrPlayout.SetValue(false);
            options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            var ret = RtcEngine.JoinChannelEx("", connection, options);
            RtcEngine.UpdateChannelMediaOptionsEx(options, connection);
            Debug.Log("RtcEngineController JoinChannelEx_MPK returns: " + ret);
        }

        private void SetUpUI()
        {
            _button1 = GameObject.Find("Left").GetComponent<Button>();
            _button1.onClick.AddListener(onLeftLocationPress);
            _button2 = GameObject.Find("Right").GetComponent<Button>();
            _button2.onClick.AddListener(onRightLocationPress);
            _button3 = GameObject.Find("Play").GetComponent<Button>();
            _button3.onClick.AddListener(onOpenButtonPress);
        }

        //If you trigger this button event, the remote user's audio source will appear in your right left direction.
        private void onLeftLocationPress()
        {

            //If remote user is moving, position and trandform could be:
            //var position = remoteUserTransform.position;
            //var forward = remoteUserTransform.forward;
            //float[] positionRemote = { position.x, position.y, position.z };
            //float[] forwardRemote = { forward.x, forward.y, forward.z };


            float[] positionRemote = { 0.0f, -1.0f, 0.0f };
            float[] forwardRemote = { 0, 0, 0 };
            RemoteVoicePositionInfo remoteVoicePositionInfo = new RemoteVoicePositionInfo(positionRemote, forwardRemote);
            var ret = SpatialAudioEngine.UpdateRemotePositionEx(UidUseInMPK, remoteVoicePositionInfo, new RtcConnection(_channelName, UidUseInEx));
            Debug.Log("_spatialAudio.UpdateRemotePosition returns: " + ret);
        }

        //If you trigger this button event, the remote user's audio source will appear in your right right direction.
        private void onRightLocationPress()
        {

            //If remote user is moving, position and trandform could be:
            //
            //var position = remoteUserTransform.position;
            //var forward = remoteUserTransform.forward;
            //float[] positionRemote = { position.x, position.y, position.z };
            //float[] forwardRemote = { forward.x, forward.y, forward.z };

  
            float[] positionRemote = { 0.0f, 1.0f, 0.0f };
            float[] forwardRemote = { 0, 0, 0 };
            RemoteVoicePositionInfo remoteVoicePositionInfo = new RemoteVoicePositionInfo(positionRemote, forwardRemote);
            var ret = SpatialAudioEngine.UpdateRemotePositionEx(UidUseInMPK, remoteVoicePositionInfo, new RtcConnection(_channelName, UidUseInEx));

            Debug.Log("_spatialAudio.UpdateRemotePosition returns: " + ret);
        }

        private void onOpenButtonPress()
        {
            var ret = MediaPlayer.Open(MPK_URL, 0);
            Debug.Log("_mediaPlayer.Open returns: " + ret);

            MediaPlayer.AdjustPlayoutVolume(0);
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

        internal static void MakeVideoView(uint uid, string channelId = "", VIDEO_SOURCE_TYPE source = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE)
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
            videoSurface.SetForUser(uid, channelId, source);
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
            go.transform.Rotate(-90.0f, 0.0f, 0.0f);
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

    internal class MpkEventHandler : IMediaPlayerSourceObserver
    {
        private readonly SpatialAudioWithMediaPlayer _spatialAudio;

        internal MpkEventHandler(SpatialAudioWithMediaPlayer spatialAudio)
        {
            _spatialAudio = spatialAudio;
        }

        public override void OnPlayerSourceStateChanged(MEDIA_PLAYER_STATE state, MEDIA_PLAYER_REASON reason)
        {
            _spatialAudio.Log.UpdateLog(string.Format(
                "OnPlayerSourceStateChanged state: {0}, ec: {1}, playId: {2}", state, reason, _spatialAudio.MediaPlayer.GetId()));
            Debug.Log("OnPlayerSourceStateChanged");
            if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_OPEN_COMPLETED)
            {
                _spatialAudio.x = 1;
                var ret = _spatialAudio.MediaPlayer.Play();
                Debug.Log("Play return" + ret);
                SpatialAudioWithMediaPlayer.MakeVideoView(_spatialAudio.UidUseInMPK, _spatialAudio.GetChannelName());
            }
        }
    }

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly SpatialAudioWithMediaPlayer _spatialAudio;

        internal UserEventHandler(SpatialAudioWithMediaPlayer spatialAudio)
        {
            _spatialAudio = spatialAudio;
        }

        public override void OnError(int err, string msg)
        {
            _spatialAudio.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _spatialAudio.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));

            //If local user is moving, position and trandform could be:
            //Transform transform = this.GetComponent<Transform>();
            //var position = transform.position;

            //float[] positionLocal = { position.x, position.y, position.z };
            //float[] right = { transform.right.x, transform.right.y, transform.right.z };
            //float[] up = { transform.up.x, transform.up.y, transform.up.z };
            //float[] forward = { transform.forward.x, transform.forward.y, transform.forward.z };

            float[] localUserPosition = new float[] { 0.0f, 0.0f, 0.0f };
            float[] forward = new float[] { 1.0f, 0.0f, 0.0f };
            float[] right = new float[] { 0.0f, 1.0f, 0.0f };
            float[] up = new float[] { 0.0f, 0.0f, 1.0f };
            var ret = _spatialAudio.SpatialAudioEngine.UpdateSelfPositionEx(localUserPosition, forward, right, up, connection);
            Debug.Log("UpdateSelfPosition return: " + ret);
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _spatialAudio.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _spatialAudio.Log.UpdateLog("OnLeaveChannel");
            SpatialAudioWithMediaPlayer.DestroyVideoView(0);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _spatialAudio.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _spatialAudio.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            if (uid == _spatialAudio.UidUseInMPK)
            {
                _spatialAudio.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));

            }
            else if (uid == _spatialAudio.UidUseInEx)
            {
                _spatialAudio.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
                SpatialAudioWithMediaPlayer.MakeVideoView(uid, _spatialAudio.GetChannelName());
            }
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _spatialAudio.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            SpatialAudioWithMediaPlayer.DestroyVideoView(uid);
        }
    }

    #endregion
}
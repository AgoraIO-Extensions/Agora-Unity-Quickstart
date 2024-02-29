using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using io.agora.rtc.demo;
using UnityEngine.Serialization;
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif

using Agora.Rtc;


namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.SpatialAudioWithUsers
{
    /// <summary>
    ///   This scene shows a quick demo of the spatial audio effect on a remote
    /// user with respect to the local user.  Move the slider to change remote
    /// user's X value in the coordinate system.
    /// </summary>
    public class SpatialAudioWithUsers : MonoBehaviour
    {

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    private ArrayList permissionList = new ArrayList() { Permission.Camera, Permission.Microphone };
#endif
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

        // A variable to save the remote user uid.
        private uint remoteUid;
        internal VideoSurface LocalView;
        internal VideoSurface RemoteView;
        internal IRtcEngine RtcEngine;
        private ILocalSpatialAudioEngine localSpatial;
        // Slider control for spatial audio.
        private Slider distanceSlider;

        // Start is called before the first frame update
        void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                SetupVideoSDKEngine();
                configureSpatialAudioEngine();

                InitEventHandler();
                SetupUI();
            }
        }

        private void CheckPermissions()
        {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        foreach (string permission in permissionList)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                Permission.RequestUserPermission(permission);
            }
        }
#endif
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
            Debug.Assert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
            return _appID.Length > 10;  
        }

        void Update()
        {
            CheckPermissions();
        }

        void OnApplicationQuit()
        {
            if (RtcEngine != null)
            {
                Leave();
                RtcEngine.Dispose();
                RtcEngine = null;
            }
        }

        private void OnDestroy()
        {
            if (RtcEngine != null)
            {
                Leave();
                RtcEngine.Dispose();
                RtcEngine = null;
            }
        }

        private void SetupVideoSDKEngine()
        {
            // Create an instance of the video SDK.
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            // Specify the context configuration to initialize the created instance.
            RtcEngineContext context = new RtcEngineContext();
            context.appId = _appID;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
            context.areaCode = AREA_CODE.AREA_CODE_GLOB; RtcEngine.Initialize(context);

            // By default Agora subscribes to the audio streams of all remote users.
            // Unsubscribe all remote users; otherwise, the audio reception range you set
            // is invalid.
            RtcEngine.MuteLocalAudioStream(true);
            RtcEngine.MuteAllRemoteAudioStreams(true);
        }

        private void configureSpatialAudioEngine()
        {
            RtcEngine.EnableAudio();
            // RtcEngine.EnableSpatialAudio(true);
            LocalSpatialAudioConfig localSpatialAudioConfig = new LocalSpatialAudioConfig();
            localSpatialAudioConfig.rtcEngine = RtcEngine;
            localSpatial = RtcEngine.GetLocalSpatialAudioEngine();
            localSpatial.Initialize();

            // Doing this here is wrong, see SetupVideoSDKEngine()
            //localSpatial.MuteLocalAudioStream(true);
            //localSpatial.MuteAllRemoteAudioStreams(true);

            // Set the audio reception range, in meters, of the local user
            localSpatial.SetAudioRecvRange(50);
            // Set the length, in meters, of unit distance
            localSpatial.SetDistanceUnit(1);
            // Update self position
            float[] pos = new float[] { 0.0F, 0.0F, 0.0F };
            float[] forward = new float[] { 1.0F, 0.0F, 0.0F };
            float[] right = new float[] { 0.0F, 1.0F, 0.0F };
            float[] up = new float[] { 0.0F, 0.0F, 1.0F };
            localSpatial.UpdateSelfPosition(pos, forward, right, up);
        }

        private void InitEventHandler()
        {
            // Creates a UserEventHandler instance.
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngine.InitEventHandler(handler);
        }

        internal class UserEventHandler : IRtcEngineEventHandler
        {
            private readonly SpatialAudioWithUsers _videoSample;

            internal UserEventHandler(SpatialAudioWithUsers videoSample)
            {
                _videoSample = videoSample;
            }
            // This callback is triggered when the local user joins the channel.
            public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                Debug.Log("You joined channel: " + connection.channelId);
            }
            public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
            {
                // Setup remote view.
                _videoSample.RemoteView.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
                // Save the remote user ID in a variable.
                _videoSample.remoteUid = uid;
            }
            // This callback is triggered when a remote user leaves the channel or drops offline.
            public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
            {
                _videoSample.RemoteView.SetEnable(false);
            }

        }


        public void updateSpatialAudioPosition(float sourceDistance)
        {
            // Set the coordinates in the world coordinate system.
            // This parameter is an array of length 3
            // The three values represent the front, right, and top coordinates
            float[] position = new float[] { sourceDistance, 4.0F, 0.0F };
            // Set the unit vector of the x axis in the coordinate system.
            // This parameter is an array of length 3,
            // The three values represent the front, right, and top coordinates
            float[] forward = new float[] { sourceDistance, 0.0F, 0.0F };
            // Update the spatial position of the specified remote user
            RemoteVoicePositionInfo remotePosInfo = new RemoteVoicePositionInfo(position, forward);
            var rc = localSpatial.UpdateRemotePosition((uint)remoteUid, remotePosInfo);
            Debug.Log("Remote user spatial position updated, rc = " + rc);
        }


        private void SetupUI()
        {
            GameObject go = GameObject.Find("LocalView");
            LocalView = go.AddComponent<VideoSurface>();
            go.transform.Rotate(0.0f, 0.0f, 180.0f);
            go = GameObject.Find("RemoteView");
            RemoteView = go.AddComponent<VideoSurface>();
            go.transform.Rotate(0.0f, 0.0f, 180.0f);
            go = GameObject.Find("Leave");
            go.GetComponent<Button>().onClick.AddListener(Leave);
            go = GameObject.Find("Join");
            go.GetComponent<Button>().onClick.AddListener(Join);

            // Reference the slider from the UI
            go = GameObject.Find("distanceSlider");
            distanceSlider = go.GetComponent<Slider>();
            // Specify a minimum and maximum value for slider.
            distanceSlider.maxValue = 10;
            distanceSlider.minValue = 0;
            // Add a listener to the slider and which invokes distanceSlider when the slider is dragged left or right.
            distanceSlider.onValueChanged.AddListener(delegate { updateSpatialAudioPosition((int)distanceSlider.value); });

        }

        public void Join()
        {
            // Enable the video module.
            RtcEngine.EnableVideo();
            // Set the user role as broadcaster.
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            // Set the local video view.
            LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA);
            // Start rendering local video.
            LocalView.SetEnable(true);
            // Join a channel.
            RtcEngine.JoinChannel(_token, _channelName, "", 0);
        }

        public void Leave()
        {
            // Leaves the channel.
            RtcEngine.LeaveChannel();
            // Disable the video modules.
            RtcEngine.DisableVideo();
            // Stops rendering the remote video.
            RemoteView.SetEnable(false);
            // Stops rendering the local video.
            LocalView.SetEnable(false);
        }

    }
}

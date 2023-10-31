using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Agora.Rtc;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using io.agora.rtc.demo;


namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.PushEncodedVideoImageS
{
    public class PushEncodedVideoImageS : MonoBehaviour
    {
        [FormerlySerializedAs("appIdInput")]
        [SerializeField]
        private AppIdInput _appIdInput;

        [Header("_____________Basic Configuration_____________")]
        [FormerlySerializedAs("APP_ID")]
        [SerializeField]
        public string _appID = "";

        [FormerlySerializedAs("TOKEN")]
        [SerializeField]
        public string _token = "";

        [FormerlySerializedAs("CHANNEL_NAME")]
        [SerializeField]
        public string _channelName = "";

        public GameObject RolePrefab;

        public Text LogText;
        internal Logger Log;
        internal IRtcEngineExS RtcEngine = null;


        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
                JoinChannel();
            }
        }

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
            RtcEngine = Agora.Rtc.RtcEngineS.CreateAgoraRtcEngineEx();
            UserEventHandlerS handler = new UserEventHandlerS(this);
            RtcEngineContextS context = new RtcEngineContextS();
            context.appId = _appID;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING;
            context.areaCode = AREA_CODE.AREA_CODE_GLOB;
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
            RtcEngine.RegisterVideoEncodedFrameObserver(new VideoEncodedImageReceiverS(), OBSERVER_MODE.INTPTR);
        }

        private void JoinChannel()
        {

            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableVideo();

            var option = new ChannelMediaOptions();
            option.autoSubscribeVideo.SetValue(true);
            option.autoSubscribeAudio.SetValue(true);
            option.publishCameraTrack.SetValue(true);
            option.publishMicrophoneTrack.SetValue(true);
            option.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            option.channelProfile.SetValue(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);

            var nRet = RtcEngine.JoinChannel(_token, _channelName, "", "123");
            this.Log.UpdateLog("joinChanne1: nRet" + nRet);
        }



        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();

        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;

            RtcEngine.InitEventHandler(null);
            RtcEngine.UnRegisterVideoEncodedFrameObserver();
            RtcEngine.LeaveChannel();

            RtcEngine.Dispose();
        }



        internal string GetChannelName()
        {
            return _channelName;
        }


        internal static void MakeVideoView(string userAccount, string channelId = "")
        {
            var go = GameObject.Find(userAccount);
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            // create a GameObject and assign to this new user
            var videoSurface = MakeImageSurface(userAccount);
            if (ReferenceEquals(videoSurface, null)) return;
            // configure videoSurface
            if (userAccount == "")
            {
                videoSurface.SetForUser(userAccount, channelId);
            }
            else
            {
                videoSurface.SetForUser(userAccount, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            }

            videoSurface.OnTextureSizeModify += (int width, int height) =>
            {
                float scale = (float)height / (float)width;
                videoSurface.transform.localScale = new Vector3(-5, 5 * scale, 1);
                Debug.Log("OnTextureSizeModify: " + width + "  " + height);
            };

            videoSurface.SetEnable(true);
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
        private static VideoSurfaceS MakeImageSurface(string goName)
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
            var videoSurface = go.AddComponent<VideoSurfaceS>();
            return videoSurface;
        }

        internal static void DestroyVideoView(string userAccount)
        {
            var go = GameObject.Find(userAccount);
            if (!ReferenceEquals(go, null))
            {
                Destroy(go);
            }
        }

    }

    #region -- Agora Event ---

    internal class UserEventHandlerS : IRtcEngineEventHandlerS
    {
        private readonly PushEncodedVideoImageS _pushEncodedVideoImage;

        internal UserEventHandlerS(PushEncodedVideoImageS videoSample)
        {
            _pushEncodedVideoImage = videoSample;
        }

        public override void OnError(int err, string msg)
        {
            _pushEncodedVideoImage.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnectionS connection, int elapsed)
        {
            int build = 0;
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _pushEncodedVideoImage.Log.UpdateLog(string.Format("sdk version: ${0}",
                _pushEncodedVideoImage.RtcEngine.GetVersion(ref build)));
            _pushEncodedVideoImage.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUserAccount, elapsed));


            PushEncodedVideoImageS.MakeVideoView("");

        }

        public override void OnRejoinChannelSuccess(RtcConnectionS connection, int elapsed)
        {
            _pushEncodedVideoImage.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnectionS connection, RtcStats stats)
        {
            _pushEncodedVideoImage.Log.UpdateLog("OnLeaveChannel");

        }

        public override void OnClientRoleChanged(RtcConnectionS connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _pushEncodedVideoImage.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnectionS connection, string userAccount, int elapsed)
        {
            _pushEncodedVideoImage.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", userAccount, elapsed));
            PushEncodedVideoImageS.MakeVideoView(userAccount, _pushEncodedVideoImage.GetChannelName());
            VideoSubscriptionOptions options = new VideoSubscriptionOptions();
            options.encodedFrameOnly.SetValue(false);
            options.type.SetValue(VIDEO_STREAM_TYPE.VIDEO_STREAM_HIGH);
            _pushEncodedVideoImage.RtcEngine.SetRemoteVideoSubscriptionOptions(userAccount, options);
        }

        public override void OnUserOffline(RtcConnectionS connection, string userAccount, USER_OFFLINE_REASON_TYPE reason)
        {
            PushEncodedVideoImageS.DestroyVideoView("");
        }
    }


    internal class VideoEncodedImageReceiverS : IVideoEncodedFrameObserverS
    {
        

        public override bool OnEncodedVideoFrameReceived(string userAccount, IntPtr imageBufferPtr, UInt64 length, EncodedVideoFrameInfoS videoEncodedFrameInfo)
        {
            Debug.Log("OnEncodedVideoFrameReceived: " + userAccount);
            return true;
        }
    }

    #endregion
}


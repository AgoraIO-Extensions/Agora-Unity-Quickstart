﻿using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using UnityEngine.Serialization;
using Logger = agora.util.Logger;

namespace Agora_Plugin.API_Example.examples.advanced.ChannelMediaRelay
{

    public class ChannelMediaRelay : MonoBehaviour
    {


        [FormerlySerializedAs("AgoraBaseProfile")]
        [SerializeField]
        private AgoraBaseProfile agoraBaseProfile;

        [FormerlySerializedAs("AgoraBaseProfile2")]
        [SerializeField]
        private AgoraBaseProfile agoraBaseProfile2;

        [Header("_____________Basic Configuration_____________")]
        [FormerlySerializedAs("APP_ID")]
        [SerializeField]
        public string appID = "";

        [FormerlySerializedAs("TOKEN")]
        [SerializeField]
        public string token = "";

        [FormerlySerializedAs("CHANNEL_NAME")]
        [SerializeField]
        public string channelName = "";

        public Text logText;
        public Logger logger;
        internal IAgoraRtcEngine mRtcEngine = null;


        void Start()
        {
            LoadAssetData();
            CheckAppId();
            InitEngine();
            SetupUI();
            EnableUI(false);
            JoinChannel();
        }

        [ContextMenu("ShowAgoraBasicProfileData")]
        public void LoadAssetData()
        {
            if (agoraBaseProfile == null) return;
            appID = agoraBaseProfile.appID;
            token = agoraBaseProfile.token;
            channelName = agoraBaseProfile.channelName;
        }

        void CheckAppId()
        {
            logger = new Logger(logText);
            logger.DebugAssert(appID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
        }

        void InitEngine()
        {
            mRtcEngine = agora.rtc.AgoraRtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(null, appID, null, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            mRtcEngine.Initialize(context);
            mRtcEngine.InitEventHandler(handler);
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            mRtcEngine.EnableAudio();
            mRtcEngine.EnableVideo();
        }

        void JoinChannel()
        {
            mRtcEngine.JoinChannel(token, channelName, "");
        }

        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        public void SetupUI()
        {
            var ui = this.transform.Find("UI");

            var btn = ui.Find("StartButton").GetComponent<Button>();
            btn.onClick.AddListener(OnStartButtonClick);

            btn = ui.Find("UpdateButton").GetComponent<Button>();
            btn.onClick.AddListener(onUpdateButtonClick);

            btn = ui.Find("StopButton").GetComponent<Button>();
            btn.onClick.AddListener(OnStopButtonClick);

            btn = ui.Find("PauseAllButton").GetComponent<Button>();
            btn.onClick.AddListener(onPauseAllButtonClick);

            btn = ui.Find("ResumAllButton").GetComponent<Button>();
            btn.onClick.AddListener(OnResumeAllButtonClick);
        }


        public void EnableUI(bool visible)
        {
            var ui = this.transform.Find("UI");
            ui.gameObject.SetActive(visible);
        }


        void OnStartButtonClick()
        {
            if (this.agoraBaseProfile2 == null)
            {
                this.logger.UpdateLog("you must set second channel first!!");
                return;
            }

            ChannelMediaRelayConfiguration config = new ChannelMediaRelayConfiguration();
            config.srcInfo = new ChannelMediaInfo
            {
                channelName = this.agoraBaseProfile.channelName,
                uid = 0,
                token = this.agoraBaseProfile.token
            };

            //you can relay to another channels (limit max is 4)
            config.destInfos = new ChannelMediaInfo[1];
            config.destInfos[0] = new ChannelMediaInfo
            {
                channelName = this.agoraBaseProfile2.channelName,
                uid = 0,
                token = this.agoraBaseProfile2.token
            };
            config.destCount = 1;

            mRtcEngine.StartChannelMediaRelay(config);

        }

        void onUpdateButtonClick()
        {
            if (this.agoraBaseProfile2 == null)
            {
                this.logger.UpdateLog("you must set second channel first!!");
                return;
            }

            ChannelMediaRelayConfiguration config = new ChannelMediaRelayConfiguration();
            config.srcInfo = new ChannelMediaInfo
            {
                channelName = this.agoraBaseProfile.channelName,
                uid = 0,
                token = this.agoraBaseProfile.token
            };

            config.destInfos = new ChannelMediaInfo[1];
            config.destInfos[0] = new ChannelMediaInfo
            {
                channelName = this.agoraBaseProfile2.channelName,
                uid = 0,
                token = this.agoraBaseProfile2.token
            };
            config.destCount = 1;

            //after StartChannelMediaRelay you can use StartChannelMediaRelay to remove or relay to anthoner channel
            mRtcEngine.UpdateChannelMediaRelay(config);


        }

        void onPauseAllButtonClick()
        {
            mRtcEngine.PauseAllChannelMediaRelay();
        }

        void OnResumeAllButtonClick()
        {
            mRtcEngine.ResumeAllChannelMediaRelay();
        }

        void OnStopButtonClick()
        {
            mRtcEngine.StopChannelMediaRelay();
        }

        void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            if (mRtcEngine != null)
            {
                mRtcEngine.LeaveChannel();
                mRtcEngine.Dispose();
            }
        }

        internal static void MakeVideoView(uint uid, string channelId = "")
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            // create a GameObject and assign to this new user
            var videoSurface = MakePlaneSurface(uid.ToString());//MakeImageSurface(uid.ToString());
            if (ReferenceEquals(videoSurface, null)) return;
            // configure videoSurface
            if (uid == 0)
            {
                videoSurface.SetForUser(uid, channelId);
            }
            else
            {
                videoSurface.SetForUser(uid, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            }
            videoSurface.SetEnable(true);
        }

        // VIDEO TYPE 1: 3D Object
        private static AgoraVideoSurface MakePlaneSurface(string goName)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // set up transform
            go.transform.Rotate(-90.0f, 0.0f, 0.0f);
            var yPos = Random.Range(3.0f, 5.0f);
            var xPos = Random.Range(-2.0f, 2.0f);
            go.transform.position = new Vector3(xPos, yPos, 0f);
            go.transform.localScale = new Vector3(0.25f, 0.5f, 0.5f);

            // configure videoSurface
            var videoSurface = go.AddComponent<AgoraVideoSurface>();
            return videoSurface;
        }

        // Video TYPE 2: RawImage
        private static AgoraVideoSurface MakeImageSurface(string goName)
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
            //var xPos = Random.Range(Offset - Screen.width / 2f, Screen.width / 2f - Offset);
            //var yPos = Random.Range(Offset, Screen.height / 2f - Offset);
            //Debug.Log("position x " + xPos + " y: " + yPos);
            go.transform.localPosition = new Vector3(Screen.width / 2f - 100, Screen.height / 2f - 100, 0f);
            go.transform.localScale = new Vector3(2f, 3f, 1f);

            // configure videoSurface
            var videoSurface = go.AddComponent<AgoraVideoSurface>();
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

    internal class UserEventHandler : IAgoraRtcEngineEventHandler
    {
        private readonly ChannelMediaRelay _channelMediaRelay;

        internal UserEventHandler(ChannelMediaRelay videoSample)
        {
            _channelMediaRelay = videoSample;
        }

        public override void OnWarning(int warn, string msg)
        {
            _channelMediaRelay.logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _channelMediaRelay.logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _channelMediaRelay.logger.UpdateLog(string.Format("sdk version: ${0}",
                _channelMediaRelay.mRtcEngine.GetVersion()));
            _channelMediaRelay.logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));
            _channelMediaRelay.EnableUI(true);
            ChannelMediaRelay.MakeVideoView(0);

        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _channelMediaRelay.logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _channelMediaRelay.logger.UpdateLog("OnLeaveChannel");
            _channelMediaRelay.EnableUI(false);
            ChannelMediaRelay.DestroyVideoView(0);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _channelMediaRelay.logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _channelMediaRelay.logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            ChannelMediaRelay.MakeVideoView(uid, _channelMediaRelay.channelName);
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _channelMediaRelay.logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            ChannelMediaRelay.DestroyVideoView(uid);
        }

        public override void OnChannelMediaRelayEvent(int code)
        {
            _channelMediaRelay.logger.UpdateLog(string.Format("OnChannelMediaRelayEvent: {0}", code));
                
        }

        public override void OnChannelMediaRelayStateChanged(int state, int code)
        {
            _channelMediaRelay.logger.UpdateLog(string.Format("OnChannelMediaRelayStateChanged state: {0}, code: {1}", state,code));
        }
    }
}

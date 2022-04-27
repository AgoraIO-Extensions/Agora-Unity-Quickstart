using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using UnityEngine.Serialization;
using Logger = agora.util.Logger;
using System;

namespace Agora_Plugin.API_Example.examples.advanced.PushVideoImage
{
    public class PushVideoImage : MonoBehaviour
    {
        [FormerlySerializedAs("AgoraBaseProfile")]
        [SerializeField]
        private AgoraBaseProfile agoraBaseProfile;


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

        public GameObject rolePrefab;
        private GameObject roleLocal;

        public Text logText;
        public Logger logger;
        internal IAgoraRtcEngine mRtcEngine = null;



        void Start()
        {
            LoadAssetData();
            CheckAppId();
            InitEngine();
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
            mRtcEngine.RegisterVideoEncodedImageReceiver(new VideoEncodedImageReceiver(this));
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            mRtcEngine.EnableVideo();

            EncodedVideoTrackOptions encodedVideoTrackOptions = new EncodedVideoTrackOptions();
            mRtcEngine.SetExternalVideoSource(true, false, EXTERNAL_VIDEO_SOURCE_TYPE.ENCODED_VIDEO_FRAME, encodedVideoTrackOptions);



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

        void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            if (mRtcEngine != null)
            {
                mRtcEngine.LeaveChannel();
                mRtcEngine.Dispose();
            }
        }


        public void CreateRole(string uid, bool isLocal)
        {
            var role = Instantiate(this.rolePrefab);
            role.name = "Role" + uid;
            var text = role.transform.Find("Text").GetComponent<Text>();
            text.text = uid;

            if (isLocal)
            {
                text.text += "\n(Local)";
                role.AddComponent<UIElementDrag>();
                this.roleLocal = role;
            }

            role.transform.parent = this.gameObject.transform;
        }

        public void DestroyRole(string uid, bool isLocal)
        {
            var name = "Role" + uid;
            var role = this.gameObject.transform.Find(name).gameObject;
            if (role)
            {
                Destroy(role);
            }

            if (isLocal)
            {
                this.roleLocal = null;
            }
        }

        public void UpdateRolePositon(string uid, Vector3 pos)
        {
            var name = "Role" + uid;
            var role = this.gameObject.transform.Find(name);
            if (role)
            {
                role.transform.localPosition = pos;
            }
        }


        public void StartPushEncodeVideoImage()
        {
            this.Invoke("UpdateForPushEncodeVideoImage", 0);
            this.logger.UpdateLog("Start PushEncodeVideoImage in every frame");
        }

        public void StopPushEncodeVideoImage()
        {
            this.CancelInvoke("UpdateForPushEncodeVideoImage");
            this.logger.UpdateLog("Stop PushEncodeVideoImage");
        }

        void UpdateForPushEncodeVideoImage()
        {
            //you can send any data not just  video image byte
            if (this.roleLocal)
            {
                //in this case, we send pos byte 
                string json = JsonUtility.ToJson(this.roleLocal.transform.localPosition);
                byte[] data = System.Text.Encoding.Default.GetBytes(json);
                EncodedVideoFrameInfo encodedVideoFrameInfo = new EncodedVideoFrameInfo();
                this.mRtcEngine.PushEncodedVideoImage(data, Convert.ToUInt32(data.Length), encodedVideoFrameInfo);
            }
        }



    }




    internal class UserEventHandler : IAgoraRtcEngineEventHandler
    {
        private readonly PushVideoImage _pushVideoImage;

        internal UserEventHandler(PushVideoImage videoSample)
        {
            _pushVideoImage = videoSample;
        }

        public override void OnWarning(int warn, string msg)
        {
            _pushVideoImage.logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _pushVideoImage.logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _pushVideoImage.logger.UpdateLog(string.Format("sdk version: ${0}",
                _pushVideoImage.mRtcEngine.GetVersion()));
            _pushVideoImage.logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));


            _pushVideoImage.CreateRole(connection.localUid.ToString(), true);
            _pushVideoImage.logger.UpdateLog("you can drag your role to every where");
            _pushVideoImage.StartPushEncodeVideoImage();
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _pushVideoImage.logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _pushVideoImage.logger.UpdateLog("OnLeaveChannel");
            _pushVideoImage.DestroyRole(connection.localUid.ToString(), true);
            _pushVideoImage.StopPushEncodeVideoImage();
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _pushVideoImage.logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _pushVideoImage.logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            _pushVideoImage.CreateRole(uid.ToString(), false);


            //you must set options.encodedFrameOnly = true when you receive other 
            VideoSubscriptionOptions options = new VideoSubscriptionOptions
            {
                type = VIDEO_STREAM_TYPE.VIDEO_STREAM_HIGH,
                encodedFrameOnly = true
            };
            _pushVideoImage.mRtcEngine.SetRemoteVideoSubscriptionOptions(uid, options);

        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _pushVideoImage.logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            _pushVideoImage.DestroyRole(uid.ToString(), false);
        }

        public override void OnChannelMediaRelayEvent(int code)
        {
            _pushVideoImage.logger.UpdateLog(string.Format("OnChannelMediaRelayEvent: {0}", code));

        }

        public override void OnChannelMediaRelayStateChanged(int state, int code)
        {
            _pushVideoImage.logger.UpdateLog(string.Format("OnChannelMediaRelayStateChanged state: {0}, code: {1}", state, code));
        }
    }


    internal class VideoEncodedImageReceiver : IAgoraRtcVideoEncodedImageReceiver
    {

        private readonly PushVideoImage _pushVideoImage;

        internal VideoEncodedImageReceiver(PushVideoImage videoSample)
        {
            _pushVideoImage = videoSample;
        }

        public override bool OnEncodedVideoImageReceived(byte[] imageBuffer, UInt64 length, EncodedVideoFrameInfo videoEncodedFrameInfo)
        {
            string str = System.Text.Encoding.Default.GetString(imageBuffer);
            var pos = JsonUtility.FromJson<Vector3>(str);
            _pushVideoImage.UpdateRolePositon(videoEncodedFrameInfo.uid.ToString(), pos);

            return true;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using UnityEngine.Serialization;
using Logger = agora.util.Logger;
using System;
using System.Collections.Generic;

namespace Agora_Plugin.API_Example.examples.advanced.PushEncodedVideoImage
{
    public class PushEncodedVideoImage : MonoBehaviour
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
        internal IRtcEngine mRtcEngine = null;
        public Dictionary<string, Vector3> rolePositionDic = new Dictionary<string, Vector3>();


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
            logger.DebugAssert(appID.Length > 10, "Please fill in your appId in API-Example/profile/AgoraBaseProfile.asset");
        }

        void InitEngine()
        {
            mRtcEngine = RtcEngineImpl.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(appID, 0, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            mRtcEngine.Initialize(context);
            mRtcEngine.InitEventHandler(handler);
            mRtcEngine.RegisterVideoEncodedImageReceiver(new VideoEncodedImageReceiver(this),OBSERVER_MODE.RAW_DATA);
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            mRtcEngine.EnableVideo();

            mRtcEngine.SetExternalVideoSource(true, true, EXTERNAL_VIDEO_SOURCE_TYPE.ENCODED_VIDEO_FRAME);
        }

        void JoinChannel()
        {
            var option = new ChannelMediaOptions
            {
             
            };

            option.autoSubscribeVideo.SetValue(true);
            option.autoSubscribeAudio.SetValue(true);
            option.publishAudioTrack.SetValue(false);
            option.publishCameraTrack.SetValue(false);
            option.publishCustomVideoTrack.SetValue(false);
            option.publishEncodedVideoTrack.SetValue(true);
            option.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            option.channelProfile.SetValue(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);

            mRtcEngine.JoinChannel(token, channelName, 0, option);
        }

        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();

            lock (this.rolePositionDic)
            {
                foreach (var e in this.rolePositionDic)
                {
                    this.UpdateRolePositon(e.Key, e.Value);
                }
            }
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (mRtcEngine == null) return;
            mRtcEngine.InitEventHandler(null);
            mRtcEngine.LeaveChannel();
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
            var role = Instantiate(this.rolePrefab, this.transform);
            role.name = "Role" + uid;
            var text = role.transform.Find("Text").GetComponent<Text>();
            text.text = uid;

            if (isLocal)
            {
                text.text += "\n(Local)";
                role.AddComponent<UIElementDrag>();
                this.roleLocal = role;
            }

            role.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
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
            this.InvokeRepeating("UpdateForPushEncodeVideoImage", 0, 0.1f);
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
            if (roleLocal)
            {
                //in this case, we send pos byte 
                string json = JsonUtility.ToJson(this.roleLocal.transform.localPosition);
                byte[] data = System.Text.Encoding.Default.GetBytes(json);
                EncodedVideoFrameInfo encodedVideoFrameInfo = new EncodedVideoFrameInfo()
                {
                    framesPerSecond = 60,
                    codecType = VIDEO_CODEC_TYPE.VIDEO_CODEC_GENERIC,
                    frameType = VIDEO_FRAME_TYPE_NATIVE.VIDEO_FRAME_TYPE_KEY_FRAME
                };
                int nRet = this.mRtcEngine.PushEncodedVideoImage(data, Convert.ToUInt32(data.Length), encodedVideoFrameInfo);
                Debug.Log("PushEncodedVideoImage: " + nRet);
            }
        }
    }




    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly PushEncodedVideoImage _pushEncodedVideoImage;

        internal UserEventHandler(PushEncodedVideoImage videoSample)
        {
            _pushEncodedVideoImage = videoSample;
        }

        public override void OnWarning(int warn, string msg)
        {
            _pushEncodedVideoImage.logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _pushEncodedVideoImage.logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _pushEncodedVideoImage.logger.UpdateLog(string.Format("sdk version: ${0}",
                _pushEncodedVideoImage.mRtcEngine.GetVersion()));
            _pushEncodedVideoImage.logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));


            _pushEncodedVideoImage.CreateRole(connection.localUid.ToString(), true);
            _pushEncodedVideoImage.logger.UpdateLog("you can drag your role to every where");
            _pushEncodedVideoImage.StartPushEncodeVideoImage();
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _pushEncodedVideoImage.logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _pushEncodedVideoImage.logger.UpdateLog("OnLeaveChannel");
            _pushEncodedVideoImage.DestroyRole(connection.localUid.ToString(), true);
            _pushEncodedVideoImage.StopPushEncodeVideoImage();
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _pushEncodedVideoImage.logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _pushEncodedVideoImage.logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            _pushEncodedVideoImage.CreateRole(uid.ToString(), false);


            //you must set options.encodedFrameOnly = true when you receive other 
            // VideoSubscriptionOptions options = new VideoSubscriptionOptions
            // {
            //     type = VIDEO_STREAM_TYPE.VIDEO_STREAM_HIGH,
            //     encodedFrameOnly = true
            // };
            //int nRet = _pushEncodedVideoImage.mRtcEngine.SetRemoteVideoSubscriptionOptions(uid, options);
            //_pushEncodedVideoImage.logger.UpdateLog("SetRemoteVideoSubscriptionOptions nRet:" + nRet);
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _pushEncodedVideoImage.logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            _pushEncodedVideoImage.DestroyRole(uid.ToString(), false);
        }

        public override void OnChannelMediaRelayEvent(int code)
        {
            _pushEncodedVideoImage.logger.UpdateLog(string.Format("OnChannelMediaRelayEvent: {0}", code));

        }

        public override void OnChannelMediaRelayStateChanged(int state, int code)
        {
            _pushEncodedVideoImage.logger.UpdateLog(string.Format("OnChannelMediaRelayStateChanged state: {0}, code: {1}", state, code));
        }
    }


    internal class VideoEncodedImageReceiver : IVideoEncodedImageReceiver
    {

        private readonly PushEncodedVideoImage _pushEncodedVideoImage;

        internal VideoEncodedImageReceiver(PushEncodedVideoImage videoSample)
        {
            _pushEncodedVideoImage = videoSample;
        }

        public override bool OnEncodedVideoImageReceived(IntPtr imageBufferPtr, byte[] imageBuffer, UInt64 length, EncodedVideoFrameInfo videoEncodedFrameInfo)
        {
            string str = System.Text.Encoding.Default.GetString(imageBuffer);
            var pos = JsonUtility.FromJson<Vector3>(str);
            var uid = videoEncodedFrameInfo.uid.ToString();

            //this called is not in Unity MainThread.we need push data in this dic. And read it in Update()
            lock (_pushEncodedVideoImage.rolePositionDic)
            {
                if (_pushEncodedVideoImage.rolePositionDic.ContainsKey(uid))
                {
                    _pushEncodedVideoImage.rolePositionDic[uid] = pos;
                }
                else
                {
                    _pushEncodedVideoImage.rolePositionDic.Add(uid, pos);
                }
            }
            return true;
        }
    }
}

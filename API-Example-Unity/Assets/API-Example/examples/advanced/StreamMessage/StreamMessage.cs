using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using UnityEngine.Serialization;
using Logger = agora.util.Logger;
using System;

namespace Agora_Plugin.API_Example.examples.advanced.StreamMessage
{

    public class StreamMessage : MonoBehaviour
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

        int streamId = -1;
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
            RtcEngineContext context = new RtcEngineContext(appID, 0, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            mRtcEngine.Initialize(context);
            mRtcEngine.InitEventHandler(handler);
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            mRtcEngine.EnableAudio();
        }

        void JoinChannel()
        {
            mRtcEngine.JoinChannel(token, channelName, "");
        }

        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
        }

        public void SetupUI()
        {
            var ui = this.transform.Find("UI");

            var btn = ui.Find("SendButton").GetComponent<Button>();
            btn.onClick.AddListener(onSendButtonPress);
        }


        public void EnableUI(bool visible)
        {
            var ui = this.transform.Find("UI");
            ui.gameObject.SetActive(visible);
        }


        void onSendButtonPress()
        {
            var text = this.transform.Find("UI/InputField").GetComponent<InputField>();
            if (text.text == "")
            {
                logger.UpdateLog("Dont send empty message!");
            }

            int streamId = this.CreateDataStreamId();
            if (streamId < 0)
            {
                logger.UpdateLog("CreateDataStream failed!");
                return;
            }
            else
            {
                SendStreamMessage(streamId, text.text);
                text.text = "";
            }
        }

        int CreateDataStreamId()
        {
            if (this.streamId == -1)
            {
                var config = new DataStreamConfig();
                config.syncWithAudio = false;
                config.ordered = true;
                var nRet = mRtcEngine.CreateDataStream(ref this.streamId, config);
                this.logger.UpdateLog(string.Format("CreateDataStream: nRet{0}, streamId{1}", nRet, streamId));
            }
            return streamId;
        }


        void SendStreamMessage(int streamId, string message)
        {
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(message);
            mRtcEngine.SendStreamMessage(streamId, byteArray, Convert.ToUInt32(byteArray.Length));
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
    }


    internal class UserEventHandler : IAgoraRtcEngineEventHandler
    {
        private readonly StreamMessage _streamMessage;

        internal UserEventHandler(StreamMessage videoSample)
        {
            _streamMessage = videoSample;
        }

        public override void OnWarning(int warn, string msg)
        {
            _streamMessage.logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _streamMessage.logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _streamMessage.logger.UpdateLog(string.Format("sdk version: ${0}",
                _streamMessage.mRtcEngine.GetVersion()));
            _streamMessage.logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));
            _streamMessage.EnableUI(true);
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _streamMessage.logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _streamMessage.logger.UpdateLog("OnLeaveChannel");
            _streamMessage.EnableUI(false);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _streamMessage.logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _streamMessage.logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));

        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _streamMessage.logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));

        }

        public override void OnStreamMessage(RtcConnection connection, uint remoteUid, int streamId, byte[] data, uint length, ulong sentTs)
        {
            string streamMessage = System.Text.Encoding.Default.GetString(data);
            _streamMessage.logger.UpdateLog(string.Format("OnStreamMessage remoteUid: {0}, stream message: {1}", remoteUid, streamMessage));
        }

        public override void OnStreamMessageError(RtcConnection connection, uint remoteUid, int streamId, int code, int missed, int cached)
        {
            _streamMessage.logger.UpdateLog(string.Format("OnStreamMessageError remoteUid: {0}, streamId: {1}, code: {2}, missed: {3}, cached: {4}", remoteUid, streamId, code, missed, cached));
        }

    }

}

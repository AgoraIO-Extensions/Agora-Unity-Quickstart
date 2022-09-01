using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using Agora.Util;
using Logger = Agora.Util.Logger;
using System;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.StreamMessage
{
    public class StreamMessage : MonoBehaviour
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


        public Text LogText;
        internal Logger Log;
        internal IRtcEngine RtcEngine;

        private int _streamId = -1;

        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
                SetupUI();
                EnableUI(false);
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
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
        }

        private void JoinChannel()
        {
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableAudio();
            RtcEngine.JoinChannel(_token, _channelName, "");
        }

        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
        }

        private void SetupUI()
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

        private void onSendButtonPress()
        {
            var text = this.transform.Find("UI/InputField").GetComponent<InputField>();
            if (text.text == "")
            {
                Log.UpdateLog("Dont send empty message!");
            }

            int streamId = this.CreateDataStreamId();
            if (streamId < 0)
            {
                Log.UpdateLog("CreateDataStream failed!");
                return;
            }
            else
            {
                SendStreamMessage(streamId, text.text);
                text.text = "";
            }
        }

        private int CreateDataStreamId()
        {
            if (this._streamId == -1)
            {
                var config = new DataStreamConfig();
                config.syncWithAudio = false;
                config.ordered = true;
                var nRet = RtcEngine.CreateDataStream(ref this._streamId, config);
                this.Log.UpdateLog(string.Format("CreateDataStream: nRet{0}, streamId{1}", nRet, _streamId));
            }
            return _streamId;
        }

        private void SendStreamMessage(int streamId, string message)
        {
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(message);
            var nRet = RtcEngine.SendStreamMessage(streamId, byteArray, Convert.ToUInt32(byteArray.Length));
            this.Log.UpdateLog("SendStreamMessage :" + nRet);
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly StreamMessage _streamMessage;

        internal UserEventHandler(StreamMessage videoSample)
        {
            _streamMessage = videoSample;
        }

        public override void OnError(int err, string msg)
        {
            _streamMessage.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _streamMessage.Log.UpdateLog(string.Format("sdk version: ${0}",
                _streamMessage.RtcEngine.GetVersion(ref build)));
            _streamMessage.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));
            _streamMessage.EnableUI(true);
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _streamMessage.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _streamMessage.Log.UpdateLog("OnLeaveChannel");
            _streamMessage.EnableUI(false);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _streamMessage.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _streamMessage.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));

        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _streamMessage.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));

        }

        public override void OnStreamMessage(RtcConnection connection, uint remoteUid, int streamId, byte[] data, uint length, ulong sentTs)
        {
            string streamMessage = System.Text.Encoding.Default.GetString(data);
            _streamMessage.Log.UpdateLog(string.Format("OnStreamMessage remoteUid: {0}, stream message: {1}", remoteUid, streamMessage));
        }

        public override void OnStreamMessageError(RtcConnection connection, uint remoteUid, int streamId, int code, int missed, int cached)
        {
            _streamMessage.Log.UpdateLog(string.Format("OnStreamMessageError remoteUid: {0}, streamId: {1}, code: {2}, missed: {3}, cached: {4}", remoteUid, streamId, code, missed, cached));
        }
    }

    #endregion
}

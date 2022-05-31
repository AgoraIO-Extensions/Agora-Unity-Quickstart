using System.Text;
using agora.rtc;
using agora.util;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Logger = agora.util.Logger;

namespace Agora_Plugin.API_Example.examples.advanced.SetEncryption
{
    public class EncryptionSample : MonoBehaviour
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

        [SerializeField]
        public ENCRYPTION_MODE EncrytionMode = ENCRYPTION_MODE.AES_128_GCM2;

        [SerializeField]
        public string Secret = "";

        public Text LogText;
        internal Logger Log;
        internal IRtcEngine RtcEngine;

        // Start is called before the first frame update
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitRtcEngine();
                SetEncryption();
                JoinChannel();
            }
        }

        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
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

        private void InitRtcEngine()
        {
            RtcEngine = agora.rtc.RtcEngine.CreateAgoraRtcEngine();
            RtcEngineContext context = new RtcEngineContext(_appID, 0, false,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(new UserEventHandler(this));
        }

        private byte[] GetEncryptionSaltFromServer()
        {
            return Encoding.UTF8.GetBytes("EncryptionKdfSaltInBase64Strings");
        }

        private void SetEncryption()
        {
            var config = new EncryptionConfig
            {
                encryptionMode = EncrytionMode,
                encryptionKey = Secret,
                encryptionKdfSalt = GetEncryptionSaltFromServer()
            };
            Log.UpdateLog(string.Format("encryption mode: {0} secret: {1}", EncrytionMode, Secret));
            var nRet= RtcEngine.EnableEncryption(true, config);
            this.Log.UpdateLog("EnableEncryption: " + nRet);
        }

        private void JoinChannel()
        {
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.JoinChannel(_token, _channelName, "", 0);
        }

        private void OnLeaveBtnClick()
        {
            RtcEngine.LeaveChannel();
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }

        internal class UserEventHandler : IRtcEngineEventHandler
        {
            private readonly EncryptionSample _encryptionSample;

            internal UserEventHandler(EncryptionSample encryptionSample)
            {
                _encryptionSample = encryptionSample;
            }

            public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _encryptionSample.Log.UpdateLog(string.Format("sdk version: {0}",
                    _encryptionSample.RtcEngine.GetVersion()));
                _encryptionSample.Log.UpdateLog(string.Format(
                    "onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", connection.channelId,
                    connection.localUid, elapsed));
            }

            public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
            {
                _encryptionSample.Log.UpdateLog("OnLeaveChannelSuccess");
            }

            public override void OnWarning(int warn, string msg)
            {
                _encryptionSample.Log.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
            }

            public override void OnError(int error, string msg)
            {
                _encryptionSample.Log.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
            }

            public override void OnConnectionLost(RtcConnection connection)
            {
                _encryptionSample.Log.UpdateLog(string.Format("OnConnectionLost "));
            }
        }
    }
}
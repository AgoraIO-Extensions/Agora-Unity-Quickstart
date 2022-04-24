using System.Text;
using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using Logger = agora.util.Logger;
using agora.util;

namespace Agora_Plugin.API_Example.examples.advanced.SetEncryption
{
    public class EncryptionSample : MonoBehaviour
    {
        [SerializeField] private string APP_ID = "YOUR_APPID";

        [SerializeField] private string TOKEN = "";

        [SerializeField] private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

        [SerializeField] private ENCRYPTION_MODE ENCRYPTION_MODE = ENCRYPTION_MODE.AES_128_GCM2;

        [SerializeField] private string SECRET = "";

        public Text logText;
        private Logger logger;
        private IRtcEngine mRtcEngine = null;

        // Start is called before the first frame update
        void Start()
        {
            CheckAppId();
            InitRtcEngine();
            SetEncryption();
            JoinChannel();
        }

        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
        }

        void CheckAppId()
        {
            logger = new Logger(logText);
            logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in Canvas!!!!!");
        }

        void InitRtcEngine()
        {
            mRtcEngine = agora.rtc.AgoraRtcEngine.CreateAgoraRtcEngine();
            RtcEngineContext context = new RtcEngineContext(null, APP_ID, null, false,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            mRtcEngine.Initialize(context);
            mRtcEngine.InitEventHandler(new UserEventHandler(this));
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            mRtcEngine.EnableAudio();
            mRtcEngine.EnableVideo();
        }

        byte[] GetEncryptionSaltFromServer()
        {
            return Encoding.UTF8.GetBytes("EncryptionKdfSaltInBase64Strings");
        }

        void SetEncryption()
        {
            var config = new EncryptionConfig
            {
                encryptionMode = ENCRYPTION_MODE,
                encryptionKey = SECRET,
                encryptionKdfSalt = GetEncryptionSaltFromServer()
            };
            logger.UpdateLog(string.Format("encryption mode: {0} secret: {1}", ENCRYPTION_MODE, SECRET));
            mRtcEngine.EnableEncryption(true, config);
        }

        void JoinChannel()
        {
            mRtcEngine.JoinChannel(TOKEN, CHANNEL_NAME, "", 0);
        }

        void OnLeaveBtnClick()
        {
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


        internal class UserEventHandler : IAgoraRtcEngineEventHandler
        {
            private readonly EncryptionSample _encryptionSample;

            internal UserEventHandler(EncryptionSample encryptionSample)
            {
                _encryptionSample = encryptionSample;
            }

            public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                _encryptionSample.logger.UpdateLog(string.Format("sdk version: {0}",
                    _encryptionSample.mRtcEngine.GetVersion()));
                _encryptionSample.logger.UpdateLog(string.Format(
                    "onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", connection.channelId,
                    connection.localUid, elapsed));
            }

            public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
            {
                _encryptionSample.logger.UpdateLog("OnLeaveChannelSuccess");
            }

            public override void OnWarning(int warn, string msg)
            {
                _encryptionSample.logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
            }

            public override void OnError(int error, string msg)
            {
                _encryptionSample.logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
            }

            public override void OnConnectionLost(RtcConnection connection)
            {
                _encryptionSample.logger.UpdateLog(string.Format("OnConnectionLost "));
            }
        }
    }
}
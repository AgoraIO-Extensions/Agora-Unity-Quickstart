using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using UnityEngine.Serialization;
using Logger = agora.util.Logger;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Agora_Plugin.API_Example.examples.advanced.PushEncodedVideoImage
{
    public class PushEncodedVideoImage : MonoBehaviour
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
        private GameObject _roleLocal;

        public Text LogText;
        internal Logger Log;
        internal IRtcEngine RtcEngine = null;

        public Dictionary<string, Vector3> RolePositionDic = new Dictionary<string, Vector3>();

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
            RtcEngine = agora.rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
            RtcEngine.RegisterVideoEncodedImageReceiver(new VideoEncodedImageReceiver(this), OBSERVER_MODE.RAW_DATA);
        }

        private void JoinChannel()
        {
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableVideo();
            RtcEngine.SetExternalVideoSource(true, true, EXTERNAL_VIDEO_SOURCE_TYPE.ENCODED_VIDEO_FRAME);

            var option = new ChannelMediaOptions();
            option.autoSubscribeVideo.SetValue(true);
            option.autoSubscribeAudio.SetValue(true);
            option.publishAudioTrack.SetValue(false);
            option.publishCameraTrack.SetValue(false);
            option.publishCustomVideoTrack.SetValue(false);
            option.publishEncodedVideoTrack.SetValue(true);
            option.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            option.channelProfile.SetValue(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);

            RtcEngine.JoinChannel(_token, _channelName, 0, option);
        }

        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();

            lock (this.RolePositionDic)
            {
                foreach (var e in this.RolePositionDic)
                {
                    this.UpdateRolePositon(e.Key, e.Value);
                }
            }
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }


        public void CreateRole(string uid, bool isLocal)
        {
            var role = Instantiate(this.RolePrefab, this.transform);
            role.name = "Role" + uid;
            var text = role.transform.Find("Text").GetComponent<Text>();
            text.text = uid;

            if (isLocal)
            {
                text.text += "\n(Local)";
                role.AddComponent<UIElementDrag>();
                this._roleLocal = role;
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
                this._roleLocal = null;
            }
        }

        private void UpdateRolePositon(string uid, Vector3 pos)
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
            this.Log.UpdateLog("Start PushEncodeVideoImage in every frame");
        }


        public void StopPushEncodeVideoImage()
        {
            this.CancelInvoke("UpdateForPushEncodeVideoImage");
            this.Log.UpdateLog("Stop PushEncodeVideoImage");
        }

        private void UpdateForPushEncodeVideoImage()
        {
            //you can send any data not just  video image byte
            if (_roleLocal)
            {
                //in this case, we send pos byte 
                string json = JsonUtility.ToJson(this._roleLocal.transform.localPosition);
                byte[] data = System.Text.Encoding.Default.GetBytes(json);
                EncodedVideoFrameInfo encodedVideoFrameInfo = new EncodedVideoFrameInfo()
                {
                    framesPerSecond = 60,
                    //dont set codecType = VIDEO_CODEC_GENERIC will crash
                    codecType = VIDEO_CODEC_TYPE.VIDEO_CODEC_GENERIC_H264,
                    frameType = VIDEO_FRAME_TYPE_NATIVE.VIDEO_FRAME_TYPE_KEY_FRAME
                };
                int nRet = this.RtcEngine.PushEncodedVideoImage(data, Convert.ToUInt32(data.Length), encodedVideoFrameInfo);
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
            _pushEncodedVideoImage.Log.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _pushEncodedVideoImage.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _pushEncodedVideoImage.Log.UpdateLog(string.Format("sdk version: ${0}",
                _pushEncodedVideoImage.RtcEngine.GetVersion()));
            _pushEncodedVideoImage.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));


            _pushEncodedVideoImage.CreateRole(connection.localUid.ToString(), true);
            _pushEncodedVideoImage.Log.UpdateLog("you can drag your role to every where");
            _pushEncodedVideoImage.StartPushEncodeVideoImage();
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _pushEncodedVideoImage.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _pushEncodedVideoImage.Log.UpdateLog("OnLeaveChannel");
            _pushEncodedVideoImage.DestroyRole(connection.localUid.ToString(), true);
            _pushEncodedVideoImage.StopPushEncodeVideoImage();
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _pushEncodedVideoImage.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _pushEncodedVideoImage.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
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
            _pushEncodedVideoImage.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            _pushEncodedVideoImage.DestroyRole(uid.ToString(), false);
        }

        public override void OnChannelMediaRelayEvent(int code)
        {
            _pushEncodedVideoImage.Log.UpdateLog(string.Format("OnChannelMediaRelayEvent: {0}", code));

        }

        public override void OnChannelMediaRelayStateChanged(int state, int code)
        {
            _pushEncodedVideoImage.Log.UpdateLog(string.Format("OnChannelMediaRelayStateChanged state: {0}, code: {1}", state, code));
        }
    }


    internal class VideoEncodedImageReceiver : IVideoEncodedImageReceiver
    {

        private readonly PushEncodedVideoImage _pushEncodedVideoImage;

        internal VideoEncodedImageReceiver(PushEncodedVideoImage videoSample)
        {
            _pushEncodedVideoImage = videoSample;
        }

        public override bool OnEncodedVideoImageReceived(IntPtr imageBufferPtr, UInt64 length, EncodedVideoFrameInfo videoEncodedFrameInfo)
        {
            byte[] imageBuffer = new byte[length];
            Marshal.Copy(imageBuffer, 0, imageBufferPtr, (int)length);
            string str = System.Text.Encoding.Default.GetString(imageBuffer);
            var pos = JsonUtility.FromJson<Vector3>(str);
            var uid = videoEncodedFrameInfo.uid.ToString();

            //this called is not in Unity MainThread.we need push data in this dic. And read it in Update()
            lock (_pushEncodedVideoImage.RolePositionDic)
            {
                if (_pushEncodedVideoImage.RolePositionDic.ContainsKey(uid))
                {
                    _pushEncodedVideoImage.RolePositionDic[uid] = pos;
                }
                else
                {
                    _pushEncodedVideoImage.RolePositionDic.Add(uid, pos);
                }
            }
            return true;
        }
    }
}

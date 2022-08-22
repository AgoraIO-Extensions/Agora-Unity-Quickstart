using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Agora.Rtc;
using Agora.Util;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Logger = Agora.Util.Logger;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.PushEncodedVideoImage
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
        internal IRtcEngineEx RtcEngine = null;

        public Dictionary<string, Vector3> RolePositionDic = new Dictionary<string, Vector3>();



        internal uint Uid1;
        internal uint Uid2;
        private System.Random _random = new System.Random();

        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                Uid1 = (uint)(_random.Next() % 1000);
                Uid2 = (uint)(_random.Next() % 1000 + 1000);
                InitEngine();
                JoinChannel1();
                JoinChannel2();
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
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngineEx();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
            RtcEngine.RegisterVideoEncodedFrameObserver(new VideoEncodedImageReceiver(this), OBSERVER_MODE.INTPTR);
        }

        private void JoinChannel1()
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


            var connection = new RtcConnection();
            connection.channelId = _channelName;
            connection.localUid = Uid1;

            var nRet = RtcEngine.JoinChannelEx(_token, connection, option);
            this.Log.UpdateLog("joinChanne1: nRet" + nRet + " uid1:" + Uid1);
        }

        private void JoinChannel2()
        {
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableVideo();

            SenderOptions senderOptions = new SenderOptions();
            senderOptions.codecType = VIDEO_CODEC_TYPE.VIDEO_CODEC_GENERIC;
            RtcEngine.SetExternalVideoSource(true, true, EXTERNAL_VIDEO_SOURCE_TYPE.ENCODED_VIDEO_FRAME, senderOptions);

            var option = new ChannelMediaOptions();
            option.autoSubscribeVideo.SetValue(true);
            option.autoSubscribeAudio.SetValue(true);
            option.publishCustomAudioTrack.SetValue(false);
            option.publishCameraTrack.SetValue(false);
            option.publishCustomVideoTrack.SetValue(false);
            option.publishEncodedVideoTrack.SetValue(true);
            option.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            option.channelProfile.SetValue(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);


            var connection = new RtcConnection();
            connection.channelId = _channelName;
            connection.localUid = Uid2;

            var nRet = RtcEngine.JoinChannelEx(_token, connection, option);
            this.Log.UpdateLog("joinChanne1: nRet" + nRet + " uid2:" + Uid2);
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

            RtcEngine.UnRegisterVideoEncodedFrameObserver();

            var connection = new RtcConnection();
            connection.channelId = _channelName;

            connection.localUid = Uid1;
            RtcEngine.LeaveChannelEx(connection);

            connection.localUid = Uid2;
            RtcEngine.LeaveChannelEx(connection);

            RtcEngine.Dispose();
        }

        public void CreateRole(string uid, bool isLocal)
        {
            if (GameObject.Find("Role" + uid) != null)
            {
                return;
            }

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
            else if (this._roleLocal != null)
            {
                var count = this.transform.childCount;
                this._roleLocal.transform.SetSiblingIndex(count - 1);
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

        internal string GetChannelName()
        {
            return _channelName;
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
                    codecType = VIDEO_CODEC_TYPE.VIDEO_CODEC_GENERIC,
                    frameType = VIDEO_FRAME_TYPE_NATIVE.VIDEO_FRAME_TYPE_KEY_FRAME
                };
                int nRet = this.RtcEngine.PushEncodedVideoImage(data, Convert.ToUInt32(data.Length), encodedVideoFrameInfo);
                Debug.Log("PushEncodedVideoImage: " + nRet);
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
            var videoSurface = MakeImageSurface(uid.ToString());
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
            // set up transform
            go.transform.Rotate(-90.0f, 0.0f, 0.0f);
            var yPos = UnityEngine.Random.Range(3.0f, 5.0f);
            var xPos = UnityEngine.Random.Range(-2.0f, 2.0f);
            go.transform.position = Vector3.zero;
            go.transform.localScale = new Vector3(0.25f, 0.5f, 0.5f);

            // configure videoSurface
            var videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

        // Video TYPE 2: RawImage
        private static VideoSurface MakeImageSurface(string goName)
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
            var videoSurface = go.AddComponent<VideoSurface>();
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

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly PushEncodedVideoImage _pushEncodedVideoImage;

        internal UserEventHandler(PushEncodedVideoImage videoSample)
        {
            _pushEncodedVideoImage = videoSample;
        }

        public override void OnError(int err, string msg)
        {
            _pushEncodedVideoImage.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _pushEncodedVideoImage.Log.UpdateLog(string.Format("sdk version: ${0}",
                _pushEncodedVideoImage.RtcEngine.GetVersion(ref build)));
            _pushEncodedVideoImage.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));



            if (connection.localUid >= 1000)
            {
                _pushEncodedVideoImage.CreateRole(connection.localUid.ToString(), true);
                _pushEncodedVideoImage.Log.UpdateLog("you can drag your role to every where");
                _pushEncodedVideoImage.StartPushEncodeVideoImage();
            }
            else
            {
                PushEncodedVideoImage.MakeVideoView(0);
            }
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _pushEncodedVideoImage.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _pushEncodedVideoImage.Log.UpdateLog("OnLeaveChannel");

            if (connection.localUid >= 1000)
            {
                _pushEncodedVideoImage.DestroyRole(connection.localUid.ToString(), true);
                _pushEncodedVideoImage.StopPushEncodeVideoImage();
            }
            else
            {
                PushEncodedVideoImage.DestroyVideoView(0);
            }
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _pushEncodedVideoImage.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _pushEncodedVideoImage.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));


            if (uid == _pushEncodedVideoImage.Uid1 || uid == _pushEncodedVideoImage.Uid2)
                return;

            if (uid >= 1000)
            {
                _pushEncodedVideoImage.CreateRole(uid.ToString(), false);
                //you must set options.encodedFrameOnly = true when you receive other 
                VideoSubscriptionOptions options = new VideoSubscriptionOptions();
                options.encodedFrameOnly.SetValue(true);
                int nRet = _pushEncodedVideoImage.RtcEngine.SetRemoteVideoSubscriptionOptionsEx(uid, options, connection);
                _pushEncodedVideoImage.Log.UpdateLog("SetRemoteVideoSubscriptionOptions nRet:" + nRet);
            }
            else
            {
                PushEncodedVideoImage.MakeVideoView(uid, _pushEncodedVideoImage.GetChannelName());
            }


        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _pushEncodedVideoImage.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));

            if (uid == _pushEncodedVideoImage.Uid1 || uid == _pushEncodedVideoImage.Uid2)
                return;

            if (uid >= 1000)
            {
                _pushEncodedVideoImage.DestroyRole(uid.ToString(), false);
            }
            else
            {
                PushEncodedVideoImage.DestroyVideoView(uid);
            }
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


    internal class VideoEncodedImageReceiver : IVideoEncodedFrameObserver
    {
        private readonly PushEncodedVideoImage _pushEncodedVideoImage;

        internal VideoEncodedImageReceiver(PushEncodedVideoImage videoSample)
        {
            _pushEncodedVideoImage = videoSample;
        }

        public override bool OnEncodedVideoFrameReceived(uint uid, IntPtr imageBufferPtr, UInt64 length, EncodedVideoFrameInfo videoEncodedFrameInfo)
        {
            byte[] imageBuffer = new byte[length];
            Marshal.Copy(imageBufferPtr, imageBuffer, 0, (int)length);
            string str = System.Text.Encoding.Default.GetString(imageBuffer);
            var pos = JsonUtility.FromJson<Vector3>(str);
            var uidStr = uid.ToString();
            Debug.Log("OnEncodedVideoImageReceived" + uid + " pos" + str);
            //this called is not in Unity MainThread.we need push data in this dic. And read it in Update()
            lock (_pushEncodedVideoImage.RolePositionDic)
            {
                if (_pushEncodedVideoImage.RolePositionDic.ContainsKey(uidStr))
                {
                    _pushEncodedVideoImage.RolePositionDic[uidStr] = pos;
                }
                else
                {
                    _pushEncodedVideoImage.RolePositionDic.Add(uidStr, pos);
                }
            }
            return true;
        }
    }

    #endregion
}


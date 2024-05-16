using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using io.agora.rtc.demo;


namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.MetadataSample
{
    public class MetadataSample : MonoBehaviour
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
        public InputField InputField;
        public Button SetMaxSizeButton;
        public Button SendButton;
        internal Logger Log;
        internal IRtcEngine RtcEngine;

        internal Queue<String> MetadataQueue = new Queue<string>();


        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
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
            RtcEngineContext context = new RtcEngineContext();
            context.appId = _appID;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
            context.areaCode = AREA_CODE.AREA_CODE_GLOB;
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
        }

        private void SetupUI()
        {

        }

        private void JoinChannel()
        {
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.JoinChannel(_token, _channelName, "",0);
        }

        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            lock (MetadataQueue)
            {
                while (MetadataQueue.Count > 0)
                {
                    string metadataString = MetadataQueue.Dequeue();
                    this.Log.UpdateLog(metadataString);
                }
            }
        }


        public void OnSetMaxSizeButtonPress()
        {
            int size = 128;
            try
            {
                size = int.Parse(this.InputField.text);
            }
            catch (Exception e)
            {
                this.Log.UpdateLog("Invalid input, please enter an int string");
                return;
            }

            int nRet = this.RtcEngine.SetMaxMetadataSize(size);
            this.Log.UpdateLog("SetMaxMetadataSize:" + nRet);
            //need RegisterMediaMetadataObserver before JoinChannel
            UserMetadataObserver metadataObserver = new UserMetadataObserver(this);
            RtcEngine.RegisterMediaMetadataObserver(metadataObserver, METADATA_TYPE.VIDEO_METADATA);
            this.Log.UpdateLog("You can send metadata now");
            this.InputField.text = "";
            this.SetMaxSizeButton.gameObject.SetActive(false);
            this.SendButton.gameObject.SetActive(true);

            JoinChannel();
        }

        public void OnSendButtonPress()
        {
            var str = this.InputField.text;
            if (str != "")
            {
                Metadata metadata = new Metadata();

                byte[] strByte = System.Text.Encoding.UTF8.GetBytes(str);
                metadata.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(strByte, 0);
                metadata.size = (uint)strByte.Length;
                int nRet = RtcEngine.SendMetadata(metadata, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA);
                this.Log.UpdateLog("SendMetadata:" + nRet);
                this.InputField.text = "";
            }
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.UnregisterMediaMetadataObserver();
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }

        public string GetChannelName()
        {
            return this._channelName;
        }

        #region -- Video Render UI Logic ---

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
                var transform = videoSurface.GetComponent<RectTransform>();
                if (transform)
                {
                    //If render in RawImage. just set rawImage size.
                    transform.sizeDelta = new Vector2(width / 2, height / 2);
                    transform.localScale = Vector3.one;
                }
                else
                {
                    //If render in MeshRenderer, just set localSize with MeshRenderer
                    float scale = (float)height / (float)width;
                    videoSurface.transform.localScale = new Vector3(-1, 1, scale);
                }
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

        #endregion
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly MetadataSample _sample;

        internal UserEventHandler(MetadataSample sample)
        {
            _sample = sample;
        }

        public override void OnError(int err, string msg)
        {
            _sample.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _sample.Log.UpdateLog(string.Format("sdk version: ${0}",
                _sample.RtcEngine.GetVersion(ref build)));
            _sample.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));


            MetadataSample.MakeVideoView(0);
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _sample.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _sample.Log.UpdateLog("OnLeaveChannel");
            MetadataSample.DestroyVideoView(0);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _sample.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnStreamMessageError(RtcConnection connection, uint remoteUid, int streamId, int code, int missed, int cached)
        {
            _sample.Log.UpdateLog(string.Format("OnStreamMessageError remoteUid: {0}, streamId: {1}, code: {2}, missed: {3}, cached: {4}", remoteUid, streamId, code, missed, cached));
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _sample.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            MetadataSample.MakeVideoView(uid, _sample.GetChannelName());
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _sample.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            MetadataSample.DestroyVideoView(uid);
        }

    }

    internal class UserMetadataObserver : IMetadataObserver
    {
        MetadataSample _sample;
        private int tick = 0;

        internal UserMetadataObserver(MetadataSample sample)
        {
            _sample = sample;
        }

        public override void OnMetadataReceived(Metadata data)
        {
            //this callback not trigger in unity main thread
            byte[] strByte = new byte[data.size];
            Marshal.Copy(data.buffer, strByte, 0, (int)data.size);
            string str = System.Text.Encoding.UTF8.GetString(strByte);
            var str2 = string.Format("OnMetadataReceived uid:{0} buffer:{1}", data.uid, str);
            lock (this._sample.MetadataQueue)
            {
                this._sample.MetadataQueue.Enqueue(str2);
            }
        }
    }

    #endregion
}

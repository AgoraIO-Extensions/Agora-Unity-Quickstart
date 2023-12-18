using System.Collections.Generic;
using Agora.Rtc;
using Agora.Util;
using UnityEngine;
using UnityEngine.UI;


namespace Agora_RTC_Plugin.API_Example.Examples.ExtensionsMarket.HTEffectARSceneSample
{
    public class HTEffectARSceneSample : MonoBehaviour
    {
        public Text LogText;
        internal Agora.Util.Logger Log;
        internal IRtcEngine RtcEngine = null;

        public InputField RtcAppIdField;
        public InputField RtcTokenField;
        public InputField ChannelNameField;


        public void Start()
        {
            Log = new Agora.Util.Logger(LogText);
        }

       
        public void OnInitRtcEngineButtonClick()
        {
            PermissionHelper.RequestCameraPermission();
            PermissionHelper.RequestMicrophontPermission();
           
            string appid = this.RtcAppIdField.text;
            if (appid.Length < 2)
            {
                Log.UpdateLog("Rtc App ID is required");
                return;
            }

            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(appid, 0,
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT, AREA_CODE.AREA_CODE_GLOB);
            var ret = RtcEngine.Initialize(context);
            Log.UpdateLog("Initialize: " + ret);
            RtcEngine.InitEventHandler(handler);
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            int build = 0;
            Log.UpdateLog(string.Format("sdk version: ${0}",
              RtcEngine.GetVersion(ref build)));
            Log.UpdateLog(string.Format("sdk build: ${0}",
              build));
        }

        public void OnInitExtensionButtonClick()
        {
            int ret = -10086;


#if UNITY_ANDROID
            ret = RtcEngine.LoadExtensionProvider("AgoraTexelJoyExtension", false);
            Log.UpdateLog("LoadExtensionProvider  AgoraTexelJoyExtension" + ret);
#endif


            ret = RtcEngine.EnableExtension("Texeljoy", "HTEffect", true);
            Log.UpdateLog("EnableExtension " + ret);

            Dictionary<string, object> jsonDic = new Dictionary<string, object>();
            string json = null;
            //jsonDic.Clear();
            //jsonDic.Add("key", "Online license");
            //json = Agora.Rtc.AgoraJson.ToJson(jsonDic);
            //ret = RtcEngine.SetExtensionProperty("Texeljoy", "HTEffect", "htInitHTEffectOnline", json, MEDIA_SOURCE_TYPE.PRIMARY_CAMERA_SOURCE);
            //Log.UpdateLog("SetExtensionProperty htInitHTEffectOnline:  " + ret);

            jsonDic.Clear();
#if UNITY_IOS
            jsonDic.Add("key", "Offline license");
#elif UNITY_ANDROID
            jsonDic.Add("license", "Offline license");
#endif
            json = Agora.Rtc.AgoraJson.ToJson(jsonDic);
            ret = RtcEngine.SetExtensionProperty("Texeljoy", "HTEffect", "htInitHTEffectOffline", json, MEDIA_SOURCE_TYPE.PRIMARY_CAMERA_SOURCE);
            Log.UpdateLog("SetExtensionProperty htInitHTEffectOffline:  " + ret);
        }

        //will trigger by OnExtensionStart 
        public void OnExtensionStartSucess()
        {
            
        }


        public void StartEffectSetFilter() {
            int ret = -10086;
            Dictionary<string, object> jsonDic = new Dictionary<string, object>();
            jsonDic.Clear();
            jsonDic.Add("type", 0);
            jsonDic.Add("name", "jiuzhao");
            string json = Agora.Rtc.AgoraJson.ToJson(jsonDic);
            ret = RtcEngine.SetExtensionProperty("Texeljoy", "HTEffect", "htSetFilter", json, MEDIA_SOURCE_TYPE.PRIMARY_CAMERA_SOURCE);
            Log.UpdateLog("SetExtensionProperty htSetFilter :  " + ret);
        }

        public void OnJoinChannelButtonClick()
        {
            string channelName = this.ChannelNameField.text;
            if (channelName == "")
            {
                Log.UpdateLog("channel name is required");
                return;
            }
            string token = this.RtcTokenField.text;

            RtcEngine.StartPreview();
            RtcEngine.JoinChannel(token, channelName);
        }


        public string GetChannelName()
        {
            return this.ChannelNameField.text;
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
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
            var mesh = go.GetComponent<MeshRenderer>();
            if (mesh != null)
            {
                Debug.LogWarning("VideoSureface update shader");
                mesh.material = new Material(Shader.Find("Unlit/Texture"));
            }
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
        private readonly HTEffectARSceneSample _videoSample;

        internal UserEventHandler(HTEffectARSceneSample videoSample)
        {
            _videoSample = videoSample;
        }

        public override void OnError(int err, string msg)
        {
            _videoSample.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("Agora: OnJoinChannelSuccess ");

            _videoSample.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));
            HTEffectARSceneSample.MakeVideoView(0);
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _videoSample.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _videoSample.Log.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _videoSample.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _videoSample.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            HTEffectARSceneSample.MakeVideoView(uid, _videoSample.GetChannelName());
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _videoSample.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            HTEffectARSceneSample.DestroyVideoView(uid);
        }

        public override void OnExtensionError(string provider, string extension, int error, string message)
        {
            string info = string.Format("OnExtensionError {0}, {1}, {2}, {3}", provider, extension, error, message);
            _videoSample.Log.UpdateLog(info);
        }

        public override void OnExtensionEvent(string provider, string extension, string key, string value)
        {
            if (key == "htIsTracking")
            {
                return;
            }
            string info = string.Format("OnExtensionEvent {0}, {1}, {2}, {3}", provider, extension, key, value);
            _videoSample.Log.UpdateLog(info);
        }

        public override void OnExtensionStarted(string provider, string extension)
        {
            string info = string.Format("OnExtensionStarted {0}, {1}", provider, extension);
            _videoSample.Log.UpdateLog(info);

            if (provider == "Texeljoy" && extension == "HTEffect")
            {
                _videoSample.OnExtensionStartSucess();
            }
        }

        public override void OnExtensionStopped(string provider, string extension)
        {
            string info = string.Format("OnExtensionStopped {0}, {1}", provider, extension);
            _videoSample.Log.UpdateLog(info);
        }
    }
    #endregion
}

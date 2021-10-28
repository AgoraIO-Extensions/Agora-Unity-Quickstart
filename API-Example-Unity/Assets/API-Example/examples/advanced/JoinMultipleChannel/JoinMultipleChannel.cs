using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using Logger = agora.util.Logger;
using UnityEngine.Serialization;

public class JoinMultipleChannel : MonoBehaviour
{
     [FormerlySerializedAs("APP_ID")] [SerializeField]
     private string appID = "YOUR_APPID";

     [FormerlySerializedAs("TOKEN_1")] [SerializeField]
     private string token1 = "";

     [FormerlySerializedAs("CHANNEL_NAME_1")] [SerializeField]
     private string channelName1 = "YOUR_CHANNEL_NAME_1";

     [FormerlySerializedAs("TOKEN_2")] [SerializeField]
     private string token2 = "";

     [FormerlySerializedAs("CHANNEL_NAME_2")] [SerializeField]
     private string channelName2 = "YOUR_CHANNEL_NAME_2";

     public Text logText;
     internal Logger Logger;
     internal IAgoraRtcEngine AgoraRtcEngine = null;
     internal IAgoraRtcChannel _channel1 = null;
     internal IAgoraRtcChannel _channel2 = null;
     private const float Offset = 100;

     // Use this for initialization
     private void Start()
     {
         CheckAppId();
         InitEngine();
         JoinChannel1();
         JoinChannel2();
     }

     private void Update()
     {
         PermissionHelper.RequestMicrophontPermission();
         PermissionHelper.RequestCameraPermission();
     }

     private void CheckAppId()
     {
         Logger = new Logger(logText);
         Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
     }

     private void InitEngine()
     {
         AgoraRtcEngine = agora.rtc.AgoraRtcEngine.CreateAgoraRtcEngine();
         AgoraRtcEngine.Initialize(new RtcEngineContext(appID, AREA_CODE.AREA_CODE_GLOB, new LogConfig("log.txt")));

         AgoraRtcEngine.EnableAudio();
         AgoraRtcEngine.EnableVideo();
         AgoraRtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
     }

     private void JoinChannel1()
     {
         _channel1 = AgoraRtcEngine.CreateChannel(channelName1);
         _channel1.InitEventHandler(new UserEventHandler1(this));
         //AgoraRtcEngine.SetChannelEventHandler();
         _channel1.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
         _channel1.JoinChannel(token1, "", 0, new ChannelMediaOptions(true, true, true, true));
     }

     private void JoinChannel2()
     {
         _channel2 = AgoraRtcEngine.CreateChannel(channelName2);
         _channel2.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE);
         _channel2.JoinChannel(token2, "", 0, new ChannelMediaOptions(true, true, false, false));
         _channel2.InitEventHandler(new UserEventHandler2(this));
         _channel2.MuteLocalAudioStream(true);
     }

     private void OnApplicationQuit()
     {
         Debug.Log("OnApplicationQuit");
         if (AgoraRtcEngine == null) return;
         _channel2.LeaveChannel();
         _channel2.Dispose();
         _channel1.LeaveChannel();
         _channel1.Dispose();
         AgoraRtcEngine.Dispose();
     }

     public void MakeVideoView(uint uid, string channelId)
     {
         string objName = channelId + "_" + uid.ToString();
         GameObject go = GameObject.Find(objName);
         if (!ReferenceEquals(go, null))
         {
             return; // reuse
         }

         // create a GameObject and assign to this new user
         VideoSurface videoSurface = makeImageSurface(objName);
         if (!ReferenceEquals(videoSurface, null))
         {
             // configure videoSurface
             videoSurface.SetForUser(uid, channelId);
             videoSurface.SetEnable(true);
         }
     }

     // VIDEO TYPE 1: 3D Object
     public VideoSurface makePlaneSurface(string goName)
     {
         GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);

         if (go == null)
         {
             return null;
         }

         go.name = goName;
         // set up transform
         go.transform.Rotate(-90.0f, 0.0f, 0.0f);
         float yPos = Random.Range(3.0f, 5.0f);
         float xPos = Random.Range(-2.0f, 2.0f);
         go.transform.position = new Vector3(xPos, yPos, 0f);
         go.transform.localScale = new Vector3(0.25f, 0.5f, .5f);

         // configure videoSurface
         VideoSurface videoSurface = go.AddComponent<VideoSurface>();
         return videoSurface;
     }

     // Video TYPE 2: RawImage
     public VideoSurface makeImageSurface(string goName)
     {
         GameObject go = new GameObject();

         if (go == null)
         {
             return null;
         }

         go.name = goName;
         // make the object draggable
         go.AddComponent<UIElementDrag>();
         // to be renderered onto
         go.AddComponent<RawImage>();

         GameObject canvas = GameObject.Find("VideoCanvas");
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
         float xPos = Random.Range(Offset - Screen.width / 2f, Screen.width / 2f - Offset);
         float yPos = Random.Range(Offset, Screen.height / 2f - Offset);
         Debug.Log("position x " + xPos + " y: " + yPos);
         go.transform.localPosition = new Vector3(xPos, yPos, 0f);
         go.transform.localScale = new Vector3(3f, 4f, 1f);

         // configure videoSurface
         VideoSurface videoSurface = go.AddComponent<VideoSurface>();
         return videoSurface;
     }

     private void DestroyVideoView(uint uid)
     {
         GameObject go = GameObject.Find(uid.ToString());
         if (!ReferenceEquals(go, null))
         {
             Object.Destroy(go);
         }
     }
     
     internal class UserEventHandler1 : IAgoraRtcChannelEventHandler
     {
         private readonly JoinMultipleChannel _multiChannelSample;
         
         internal UserEventHandler1(JoinMultipleChannel multiChannelSample)
         {
             _multiChannelSample = multiChannelSample;
         }
         
         public override void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
         {
             _multiChannelSample.Logger.UpdateLog(string.Format("sdk version: {0}", _multiChannelSample.AgoraRtcEngine.GetVersion()));
             _multiChannelSample.Logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName,
                 uid, elapsed));
             _multiChannelSample.MakeVideoView(0, "");
         }

         public override void OnLeaveChannel(string channelId, RtcStats stats)
         {
             _multiChannelSample.Logger.UpdateLog(string.Format("onLeaveChannelSuccess channelName: {0}", channelId));
             _multiChannelSample.DestroyVideoView(0);
         }

         public override void OnChannelError(string channelId, int err, string msg)
         {
             _multiChannelSample.Logger.UpdateLog(string.Format("Channel1OnErrorHandler channelId: {0}, err: {1}, message: {2}", channelId, err,
                 msg));
         }

         public override void OnUserJoined(string channelId, uint uid, int elapsed)
         {
             _multiChannelSample.Logger.UpdateLog(string.Format("Channel1OnUserJoinedHandler channelId: {0} uid: ${1} elapsed: ${2}", channelId,
                 uid, elapsed));
             _multiChannelSample.MakeVideoView(uid, channelId);
         }
         
         public override void OnUserOffline(string channelId, uint uid, USER_OFFLINE_REASON_TYPE reason)
         {
             _multiChannelSample.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid, (int) reason));
             _multiChannelSample.DestroyVideoView(uid);
         }
     }
     
     internal class UserEventHandler2 : IAgoraRtcChannelEventHandler
     {
         private readonly JoinMultipleChannel _multiChannelSample;
         
         internal UserEventHandler2(JoinMultipleChannel multiChannelSample)
         {
             _multiChannelSample = multiChannelSample;
         }
         
         public override void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
         {
             _multiChannelSample.Logger.UpdateLog(string.Format("sdk version: {0}", _multiChannelSample.AgoraRtcEngine.GetVersion()));
             _multiChannelSample.Logger.UpdateLog(string.Format("onJoinChannel2Success channelName: {0}, uid: {1}, elapsed: {2}", channelName,
                 uid, elapsed));
             _multiChannelSample.MakeVideoView(0, "");
         }

         public override void OnLeaveChannel(string channelId, RtcStats stats)
         {
             _multiChannelSample.Logger.UpdateLog(string.Format("onLeaveChannel2Success channelName: {0}", channelId));
             _multiChannelSample.DestroyVideoView(0);
         }


         public override void OnChannelError(string channelId, int err, string msg)
         {
             _multiChannelSample.Logger.UpdateLog(string.Format("Channel2OnErrorHandler channelId: {0}, err: {1}, message: {2}", channelId, err,
                 msg));
         }

         public override void OnUserJoined(string channelId, uint uid, int elapsed)
         {
             _multiChannelSample.Logger.UpdateLog(string.Format("Channel2OnUserJoinedHandler channelId: {0} uid: ${1} elapsed: ${2}", channelId,
                 uid, elapsed));
             _multiChannelSample.MakeVideoView(uid, channelId);
         }
         
         public override void OnUserOffline(string channelId, uint uid, USER_OFFLINE_REASON_TYPE reason)
         {
             _multiChannelSample.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid, (int) reason));
             _multiChannelSample.DestroyVideoView(uid);
         }
     }
}
using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using UnityEngine.Serialization;
using Logger = agora.util.Logger;

public class HelloVideoTokenAgora : MonoBehaviour {

  [SerializeField]
  private string APP_ID = "YOUR_APPID";
  
  [FormerlySerializedAs("CHANNEL_NAME")] [SerializeField]
  private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";
  
  public Text logText;
  private Logger logger;
  private const float Offset = 100;
  //private static string channelName = "Agora_Channel";
  private static string channelToken = "0064e48fd216cba47dfbd4811d94dad4f4cIADKQS8gBDod1zihizZptTd07NtfzeC3jsmo3ZBIxkPyf9JjSIgAAAAAEAALrnX4FwsnYQEAAQAWCydh";
  private static string tokenBase = "http://localhost:8080";
  private CONNECTION_STATE_TYPE state = CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED;
  
  internal IAgoraRtcEngine mRtcEngine = null;

	// Use this for initialization
	void Start () {
		CheckAppId();
		InitEngine();
		JoinChannel();
	}

  void RenewOrJoinToken(string newToken) {
    HelloVideoTokenAgora.channelToken = newToken;
    if (state == CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED
        || state == CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED
        || state == CONNECTION_STATE_TYPE.CONNECTION_STATE_FAILED
    ) {
      // If we are not connected yet, connect to the channel as normal
      JoinChannel();
    } else {
      // If we are already connected, we should just update the token
      UpdateToken();
    }
  }

	// Update is called once per frame
	void Update () {
		PermissionHelper.RequestMicrophontPermission();
		PermissionHelper.RequestCameraPermission();
	}

  void UpdateToken()
  {
    mRtcEngine.RenewToken(HelloVideoTokenAgora.channelToken);
  }

	void CheckAppId()
  {
	logger = new Logger(logText);
	logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
  }

  void InitEngine()
  {
      mRtcEngine = agora.rtc.AgoraRtcEngine.CreateAgoraRtcEngine();
      mRtcEngine.Initialize(new RtcEngineContext(APP_ID, AREA_CODE.AREA_CODE_GLOB, new LogConfig("log.txt")));
      mRtcEngine.InitEventHandler(new UserEventHandler(this));
      mRtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
      mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
      mRtcEngine.EnableAudio();
      mRtcEngine.EnableVideo();
  }

  void JoinChannel()
  {
    if (channelToken.Length == 0) {
      StartCoroutine(HelperClass.FetchToken(tokenBase, CHANNEL_NAME, 0, this.RenewOrJoinToken));
      return;
    }
    mRtcEngine.JoinChannel(channelToken, CHANNEL_NAME, "");
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

    internal string GetChannelName()
    {
        return CHANNEL_NAME;
    }

    private void DestroyVideoView(uint uid)
    {
        GameObject go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            Object.Destroy(go);
        }
    }

    private void makeVideoView(uint uid, string channelId = "")
    {
        GameObject go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            return; // reuse
        }

        // create a GameObject and assign to this new user
        VideoSurface videoSurface = makeImageSurface(uid.ToString());
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
        // to be renderered onto
        go.AddComponent<RawImage>();
        // make the object draggable
        go.AddComponent<UIElementDrag>();
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
    
    internal class UserEventHandler : IAgoraRtcEngineEventHandler
    {
        private readonly HelloVideoTokenAgora _helloVideoTokenAgora;

        internal UserEventHandler(HelloVideoTokenAgora helloVideoTokenAgora)
        {
            _helloVideoTokenAgora = helloVideoTokenAgora;
        }
        
        public override void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
        {
            _helloVideoTokenAgora.logger.UpdateLog(string.Format("sdk version: ${0}", _helloVideoTokenAgora.mRtcEngine.GetVersion()));
            _helloVideoTokenAgora.logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
            _helloVideoTokenAgora.logger.UpdateLog(string.Format("New Token: {0}", HelloVideoTokenAgora.channelToken));
            // HelperClass.FetchToken(tokenBase, channelName, 0, this.RenewOrJoinToken);
            _helloVideoTokenAgora.makeVideoView(0);
        }

        public override void OnLeaveChannel(RtcStats stats)
        {
            _helloVideoTokenAgora.logger.UpdateLog("OnLeaveChannelSuccess");
            _helloVideoTokenAgora.DestroyVideoView(0);
        }

        public override void OnUserJoined(uint uid, int elapsed)
        {
            _helloVideoTokenAgora.logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            _helloVideoTokenAgora.makeVideoView(uid, _helloVideoTokenAgora.GetChannelName());
        }

        public override void OnUserOffline(uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _helloVideoTokenAgora.logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid, (int)reason));
            _helloVideoTokenAgora.DestroyVideoView(uid);
        }

        public override void OnTokenPrivilegeWillExpire(string token)
        {
            _helloVideoTokenAgora.StartCoroutine(HelperClass.FetchToken(tokenBase, _helloVideoTokenAgora.CHANNEL_NAME, 0, _helloVideoTokenAgora.RenewOrJoinToken));
        }

        public override void OnConnectionStateChanged(CONNECTION_STATE_TYPE state, CONNECTION_CHANGED_REASON_TYPE reason)
        {
            _helloVideoTokenAgora.state = state;
        }

        public override void OnWarning(int warn, string msg)
        {
            _helloVideoTokenAgora.logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int error, string msg)
        {
            _helloVideoTokenAgora.logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
        }

        public override void OnConnectionLost()
        {
            _helloVideoTokenAgora.logger.UpdateLog(string.Format("OnConnectionLost "));
        }
    }
}

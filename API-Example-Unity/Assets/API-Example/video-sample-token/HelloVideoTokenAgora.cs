using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using agora_utilities;

public class HelloVideoTokenAgora : MonoBehaviour {

  [SerializeField]
  private string APP_ID = "YOUR_APPID";
  public Text logText;
  private Logger logger;
  private IRtcEngine mRtcEngine = null;
  private const float Offset = 100;
  private static string channelName = "Agora_Channel";
  private static string channelToken = "";
  private static string tokenBase = "http://localhost:8080";
  private CONNECTION_STATE_TYPE state = CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED;

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
    mRtcEngine = IRtcEngine.GetEngine(APP_ID);
    mRtcEngine.SetLogFile("log.txt");
    mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
    mRtcEngine.SetClientRole(CLIENT_ROLE.BROADCASTER);
    mRtcEngine.EnableAudio();
    mRtcEngine.EnableVideo();
    mRtcEngine.EnableVideoObserver();
    mRtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
    mRtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
    mRtcEngine.OnWarning += OnSDKWarningHandler;
    mRtcEngine.OnError += OnSDKErrorHandler;
    mRtcEngine.OnConnectionLost += OnConnectionLostHandler;
    mRtcEngine.OnUserJoined += OnUserJoinedHandler;
    mRtcEngine.OnUserOffline += OnUserOfflineHandler;
    mRtcEngine.OnTokenPrivilegeWillExpire += OnTokenPrivilegeWillExpireHandler;
  }

  void JoinChannel()
  {
    if (channelToken.Length == 0) {
      StartCoroutine(HelperClass.FetchToken(tokenBase, channelName, 0, this.RenewOrJoinToken));
      return;
    }
    mRtcEngine.JoinChannelByKey(channelToken, channelName, "", 0);
  }

	void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
  {
    logger.UpdateLog(string.Format("sdk version: ${0}", IRtcEngine.GetSdkVersion()));
    logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
    logger.UpdateLog(string.Format("New Token: {0}", HelloVideoTokenAgora.channelToken));
    // HelperClass.FetchToken(tokenBase, channelName, 0, this.RenewOrJoinToken);
    makeVideoView(0);
  }

  void OnLeaveChannelHandler(RtcStats stats)
  {
    logger.UpdateLog("OnLeaveChannelSuccess");
    DestroyVideoView(0);
  }

  void OnUserJoinedHandler(uint uid, int elapsed)
  {
    logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
    makeVideoView(uid);
  }

  void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
  {
    logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid, (int)reason));
    DestroyVideoView(uid);
  }

  void OnTokenPrivilegeWillExpireHandler(string token)
  {
    StartCoroutine(HelperClass.FetchToken(tokenBase, channelName, 0, this.RenewOrJoinToken));
  }

  void OnConnectionStateChangedHandler(CONNECTION_STATE_TYPE state, CONNECTION_CHANGED_REASON_TYPE reason)
  {
        this.state = state;
  }

  void OnSDKWarningHandler(int warn, string msg)
  {
    logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
  }

  void OnSDKErrorHandler(int error, string msg)
  {
    logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
  }

  void OnConnectionLostHandler()
  {
    logger.UpdateLog(string.Format("OnConnectionLost "));
  }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        if (mRtcEngine != null)
        {
			mRtcEngine.LeaveChannel();
			mRtcEngine.DisableVideoObserver();
            IRtcEngine.Destroy();
        }
    }

    private void DestroyVideoView(uint uid)
    {
        GameObject go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            Object.Destroy(go);
        }
    }

    private void makeVideoView(uint uid)
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
            videoSurface.SetForUser(uid);
            videoSurface.SetEnable(true);
            videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
            videoSurface.SetGameFps(30);
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
}

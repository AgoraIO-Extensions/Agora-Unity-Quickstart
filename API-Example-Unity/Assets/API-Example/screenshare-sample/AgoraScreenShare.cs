using System.Collections;
using UnityEngine;
using agora.rtc;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using agora.util;
using Logger = agora.util.Logger;

public class AgoraScreenShare : MonoBehaviour 
{

    [SerializeField]
    private string APP_ID = "YOUR_APPID";

    [SerializeField]
    private string TOKEN = "";

    [SerializeField]
    private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";
   	public Text logText;
    private Logger logger;
    private const float Offset = 100;
	private Texture2D mTexture;
    private UnityEngine.Rect mRect;	
	private int i = 0;
    private WebCamTexture webCameraTexture;
    public RawImage rawImage;
	public Vector2 cameraSize = new Vector2(640, 480);
	public int cameraFPS = 15;
    internal IAgoraRtcEngine mRtcEngine = null;

	// Use this for initialization
	void Start () 
	{
        InitCameraDevice();
        InitTexture();
		CheckAppId();	
		InitEngine();
		JoinChannel();
	}

    void Update() 
    {
        PermissionHelper.RequestMicrophontPermission();
		StartCoroutine(ShareScreen());
    }

    private IEnumerator ShareScreen()
    {
        yield return new WaitForEndOfFrame();
        IAgoraRtcEngine rtc = AgoraRtcEngine.Get();
        if (rtc != null)
        {
            mTexture.ReadPixels(mRect, 0, 0);
            mTexture.Apply();
            byte[] bytes = mTexture.GetRawTextureData();
            var size = Marshal.SizeOf(bytes[0]) * bytes.Length;
            ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
            externalVideoFrame.type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
            externalVideoFrame.format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;
            externalVideoFrame.buffer = bytes;
            externalVideoFrame.stride = (int)mRect.width;
            externalVideoFrame.height = (int)mRect.height;
            externalVideoFrame.cropLeft = 10;
            externalVideoFrame.cropTop = 10;
            externalVideoFrame.cropRight = 10;
            externalVideoFrame.cropBottom = 10;
            externalVideoFrame.rotation = 180;
            externalVideoFrame.timestamp = i++;
            var a = rtc.PushVideoFrame(externalVideoFrame);
            Debug.Log("PushVideoFrame ret = " + a);
        }
    }

	private void InitEngine()
    {
        mRtcEngine = agora.rtc.AgoraRtcEngine.CreateAgoraRtcEngine();
        mRtcEngine.Initialize(new RtcEngineContext(APP_ID, AREA_CODE.AREA_CODE_GLOB, new LogConfig("log.txt")));
        mRtcEngine.InitEventHandler(new UserEventHandler(this));
        mRtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
		mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
		mRtcEngine.EnableAudio();
		mRtcEngine.EnableVideo();
        mRtcEngine.SetExternalVideoSource(true);
    }

	private void JoinChannel()
	{
        int ret = mRtcEngine.JoinChannel(TOKEN, CHANNEL_NAME, "");
        Debug.Log(string.Format("JoinChannel ret: ${0}", ret));
	}

	private void CheckAppId()
    {
        logger = new Logger(logText);
        logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in Canvas!!!!");
    }

    private void InitTexture()
    {
        mRect = new UnityEngine.Rect(0, 0, Screen.width, Screen.height);
        mTexture = new Texture2D((int)mRect.width, (int)mRect.height, TextureFormat.RGBA32, false);
    }

    private void InitCameraDevice()
    {   

        WebCamDevice[] devices = WebCamTexture.devices;
        webCameraTexture = new WebCamTexture(devices[0].name, (int)cameraSize.x, (int)cameraSize.y, cameraFPS);
        rawImage.texture = webCameraTexture;
        webCameraTexture.Play();
    }

    private void OnApplicationQuit()
    {
        if (webCameraTexture)
        {
            webCameraTexture.Stop();
        }

        if (mRtcEngine != null)
        {
			mRtcEngine.LeaveChannel();
            mRtcEngine.Dispose();
            mRtcEngine = null;
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
            Destroy(go);
        }
    }

    private void MakeVideoView(uint uid, string channelId = "")
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
    public VideoSurface MakePlaneSurface(string goName)
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
        private readonly AgoraScreenShare _agoraScreenShare;
        
        internal UserEventHandler(AgoraScreenShare agoraScreenShare)
        {
            _agoraScreenShare = agoraScreenShare;
        }
        
        public override void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
        {
            _agoraScreenShare.logger.UpdateLog(string.Format("sdk version: ${0}", _agoraScreenShare.mRtcEngine.GetVersion()));
            _agoraScreenShare.logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
        }

        public override void OnLeaveChannel(RtcStats stats)
        {
            _agoraScreenShare.logger.UpdateLog("OnLeaveChannelSuccess");
        }

        public override void OnUserJoined(uint uid, int elapsed)
        {
            _agoraScreenShare.logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            _agoraScreenShare.MakeVideoView(uid, _agoraScreenShare.GetChannelName());
        }

        public override void OnUserOffline(uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _agoraScreenShare.logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid, (int)reason));
            _agoraScreenShare.DestroyVideoView(uid);
        }

        public override void OnWarning(int warn, string msg)
        {
            _agoraScreenShare.logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
        }
    
        public override void OnError(int error, string msg)
        {
            _agoraScreenShare.logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
        }
    
        public override void OnConnectionLost()
        {
            _agoraScreenShare.logger.UpdateLog(string.Format("OnConnectionLost "));
        }
        
    }
}

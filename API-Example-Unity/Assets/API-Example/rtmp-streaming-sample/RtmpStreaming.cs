using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using Logger = agora.util.Logger;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class RtmpStreaming : MonoBehaviour
{
    [SerializeField] private string APP_ID = "";

    [SerializeField] private string TOKEN = "";

    [SerializeField] private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

    [SerializeField] private string RTMP_URL = "";

    public Text logText;
    internal Logger logger;
    internal IAgoraRtcEngine mRtcEngine = null;
    private const float Offset = 100;
    private uint remoteUid = 0;
    private bool isStreaming = false;

    // Use this for initialization
    private void Start()
    {
        CheckAppId();
        InitEngine();
        JoinChannel();
    }

    // Update is called once per frame
    private void Update()
    {
        PermissionHelper.RequestMicrophontPermission();
        PermissionHelper.RequestCameraPermission();
    }

    private void CheckAppId()
    {
        logger = new Logger(logText);
        logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
    }

    private void InitEngine()
    {
        mRtcEngine = agora.rtc.AgoraRtcEngine.CreateAgoraRtcEngine();
        mRtcEngine.Initialize(new RtcEngineContext(APP_ID, AREA_CODE.AREA_CODE_GLOB, new LogConfig("log.txt")));
        mRtcEngine.InitEventHandler(new UserEventHandler(this));
        
        mRtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        mRtcEngine.SetVideoEncoderConfiguration(new VideoEncoderConfiguration
        {
            dimensions = new VideoDimensions {width = 720, height = 640},
            frameRate = FRAME_RATE.FRAME_RATE_FPS_24
        });
        mRtcEngine.EnableAudio();
        mRtcEngine.EnableVideo();
    }

    private void StartTranscoding(bool ifRemoteUser = false)
    {
        if (isStreaming && !ifRemoteUser) return;
        if (isStreaming && ifRemoteUser)
        {
            mRtcEngine.RemovePublishStreamUrl(RTMP_URL);
        }
        
        var lt = new LiveTranscoding();
        lt.videoBitrate = 400;
        lt.videoCodecProfile = VIDEO_CODEC_PROFILE_TYPE.VIDEO_CODEC_PROFILE_HIGH;
        lt.videoGop = 30;
        lt.videoFramerate = 24;
        lt.lowLatency = false;
        lt.audioSampleRate = AUDIO_SAMPLE_RATE_TYPE.AUDIO_SAMPLE_RATE_44100;
        lt.audioBitrate = 48;
        lt.audioChannels = 1;
        lt.audioCodecProfile = AUDIO_CODEC_PROFILE_TYPE.AUDIO_CODEC_PROFILE_LC_AAC;
        //lt.liveStreamAdvancedFeatures = new LiveStreamAdvancedFeature[0];
        
        var localUesr = new TranscodingUser()
        {
            uid = 0,
            x = 0,
            y = 0,
            width = 360,
            height = 640,
            audioChannel = 0,
            alpha = 1.0,
        };
        
        if (ifRemoteUser)
        {
            var remoteUser = new TranscodingUser()
            {
                uid = remoteUid,
                x = 360,
                y = 0,
                width = 360,
                height = 640,
                audioChannel = 0,
                alpha = 1.0,
            };
            lt.userCount = 2;
            lt.width = 720;
            lt.height = 640;
            lt.transcodingUsers = new[] {localUesr, remoteUser};
        }
        else
        {
            lt.userCount = 1;
            lt.width = 360;
            lt.height = 640;
            lt.transcodingUsers = new[] {localUesr};
        }
        
        mRtcEngine.SetLiveTranscoding(lt);

        var rc = mRtcEngine.AddPublishStreamUrl(RTMP_URL, true);
        if (rc == 0) logger.UpdateLog(string.Format("Error in AddPublishStreamUrl: {0}", RTMP_URL));
    }

    private void JoinChannel()
    {
        mRtcEngine.JoinChannel(TOKEN, CHANNEL_NAME, "");
    }

    private void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        if (mRtcEngine != null)
        {
            mRtcEngine.RemovePublishStreamUrl(RTMP_URL);
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
        private readonly RtmpStreaming _rtmpStreaming;

        internal UserEventHandler(RtmpStreaming rtmpStreaming)
        {
            _rtmpStreaming = rtmpStreaming;
        }

        public override void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
        {
            _rtmpStreaming.logger.UpdateLog(string.Format("sdk version: ${0}", _rtmpStreaming.mRtcEngine.GetVersion()));
            _rtmpStreaming.logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName,
                uid, elapsed));
            _rtmpStreaming.MakeVideoView(0, _rtmpStreaming.GetChannelName());
            _rtmpStreaming.StartTranscoding();
        }

        public override void OnLeaveChannel(RtcStats stats)
        {
            _rtmpStreaming.logger.UpdateLog("OnLeaveChannelSuccess");
            _rtmpStreaming.DestroyVideoView(0);
        }

        public override void OnUserJoined(uint uid, int elapsed)
        {
            if (_rtmpStreaming.remoteUid == 0) _rtmpStreaming.remoteUid = uid;
            _rtmpStreaming.StartTranscoding(true);
            _rtmpStreaming.logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            _rtmpStreaming.MakeVideoView(uid, _rtmpStreaming.GetChannelName());
        }

        public override void OnUserOffline(uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _rtmpStreaming.remoteUid = 0;
            _rtmpStreaming.logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid, (int) reason));
            _rtmpStreaming.DestroyVideoView(uid);
        }

        public override void OnWarning(int warn, string msg)
        {
            _rtmpStreaming.logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int error, string msg)
        {
            _rtmpStreaming.logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
        }

        public override void OnConnectionLost()
        {
            _rtmpStreaming.logger.UpdateLog("OnConnectionLost ");
        }

        public override void OnStreamPublished(string url, int error)
        {
            _rtmpStreaming.logger.UpdateLog(string.Format("OnStreamPublished url: {0}, error : {1}", url, error));
        }

        public override void OnRtmpStreamingStateChanged(string url, RTMP_STREAM_PUBLISH_STATE state, RTMP_STREAM_PUBLISH_ERROR code)
        {
            _rtmpStreaming.logger.UpdateLog(string.Format("OnRtmpStreamingStateChanged url: {0}, state: {1}, code: {2}", url, state,
                code));
        }

        public override void OnRtmpStreamingEvent(string url, RTMP_STREAMING_EVENT code)
        {
            _rtmpStreaming.logger.UpdateLog(string.Format("OnRtmpStreamingEvent url: {0}, code: {1}", url, code));
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;

public class AgoraTest : MonoBehaviour
{
    public string AppID;
    public string ChannelName;

    VideoSurface myView;
    VideoSurface remoteView;

    IRtcEngine mRtcEngine;

    private void Awake()
    {
        SetupUI();
    }

    private void Start()
    {
        SetupAgora();
    }

    void SetupUI()
    {
        GameObject go = GameObject.Find("MyView");
        myView = go.AddComponent<VideoSurface>();

        go = GameObject.Find("LeaveButton");
        go.GetComponent<Button>().onClick.AddListener(() =>
        {
            mRtcEngine.LeaveChannel();
            mRtcEngine.DisableVideo();
            mRtcEngine.DisableVideoObserver();
        });

        go = GameObject.Find("JoinButton");
        go.GetComponent<Button>().onClick.AddListener(() =>
        {
            mRtcEngine.EnableVideo();
            mRtcEngine.EnableVideoObserver();
            myView.SetEnable(true);
            mRtcEngine.JoinChannel(ChannelName, "", 0);
        });
    }

    void SetupAgora()
    {
        mRtcEngine = IRtcEngine.GetEngine(AppID);

        mRtcEngine.OnUserJoined = OnUserJoined;
        mRtcEngine.OnUserOffline = OnUserOffline;
        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccessHandler;
        mRtcEngine.OnLeaveChannel = OnLeaveChannelHandler;

    }

    string callId;
    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {

        callId = mRtcEngine.GetCallId();
        Debug.Log("Call ID = " + callId);
    }

    void OnLeaveChannelHandler(RtcStats stats)
    {
        myView.SetEnable(false);
        if (remoteView != null)
        {
            remoteView.SetEnable(false);
        }
        Debug.Log("Sending Rating:" + mRtcEngine.Rate(callId, 4, "Rating"));
    }

    void OnUserJoined(uint uid, int elapsed)
    {
        GameObject go = GameObject.Find("RemoteView");

        if (remoteView == null)
        {
            remoteView = go.AddComponent<VideoSurface>();
        }

        remoteView.SetForUser(uid);
        remoteView.SetEnable(true);
        remoteView.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
        remoteView.SetGameFps(30);
    }

    void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        remoteView.SetEnable(false);
    }

    void UnloadEngine()
    {
        Debug.Log("calling unloadEngine");

        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();  
            mRtcEngine = null;
        }
    }

    void OnApplicationQuit()
    {
        UnloadEngine();
    }
}

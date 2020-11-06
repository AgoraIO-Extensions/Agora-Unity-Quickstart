using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;

public class AgoraDeviceManager : MonoBehaviour
{
    [SerializeField]
    private string APP_ID = "YOUR_APPID";
    [SerializeField]
    private string TOKEN = "YOUR_TOKEN";
    [SerializeField]
    private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

    public Text logText;
    private Logger logger;
    private IRtcEngine mRtcEngine = null;
    private AudioRecordingDeviceManager audioRecordingDeviceManager = null;
    private AudioPlaybackDeviceManager audioPlaybackDeviceManager = null;
    private VideoDeviceManager videoDeviceManager = null;
    private Dictionary<int, string> audioRecordingDeviceDic = new Dictionary<int, string>();
    private Dictionary<int, string> audioPlaybackDeviceDic = new Dictionary<int, string>();
    private Dictionary<int, string> videoDeviceManagerDic = new Dictionary<int, string>();
    private int deviceIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        CheckAppId();
        InitRtcEngine();
		JoinChannel();
    }

    void Update() 
    {
        PermissionHelper.RequestMicrophontPermission();
    }

    void CheckAppId()
    {
        logger = new Logger(logText);
        logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in Canvas!!!!!");
    }

    void InitRtcEngine()
    {
        mRtcEngine = IRtcEngine.GetEngine(APP_ID);
        mRtcEngine.SetLogFile("log.txt");
        mRtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
        mRtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        mRtcEngine.OnWarning += OnSDKWarningHandler;
        mRtcEngine.OnError += OnSDKErrorHandler;
        mRtcEngine.OnConnectionLost += OnConnectionLostHandler;
        GetAudioRecordingDevice();
        GetAudioPlaybackDevice();
        GetVideoDeviceManager();
        SetCurrentDevice();
        SetCurrentDeviceVolume();
        ReleaseDeviceManager();
    }

    void GetAudioRecordingDevice() 
    {
        string audioRecordingDeviceName = "";
        string audioRecordingDeviceId = "";
        audioRecordingDeviceManager = (AudioRecordingDeviceManager)mRtcEngine.GetAudioRecordingDeviceManager();
        audioRecordingDeviceManager.CreateAAudioRecordingDeviceManager();
        int count = audioRecordingDeviceManager.GetAudioRecordingDeviceCount();
        logger.UpdateLog(string.Format("AudioRecordingDevice count: {0}", count));
        for(int i = 0; i < count ; i ++) {
            audioRecordingDeviceManager.GetAudioRecordingDevice(i, ref audioRecordingDeviceName, ref audioRecordingDeviceId);
            audioRecordingDeviceDic.Add(i, audioRecordingDeviceId);
            logger.UpdateLog(string.Format("AudioRecordingDevice device index: {0}, name: {1}, id: {2}", i, audioRecordingDeviceName, audioRecordingDeviceId));
        }
    }

    void GetAudioPlaybackDevice()
    {
        string audioPlaybackDeviceName = "";
        string audioPlaybackDeviceId = "";
        audioPlaybackDeviceManager = (AudioPlaybackDeviceManager)mRtcEngine.GetAudioPlaybackDeviceManager();
        audioPlaybackDeviceManager.CreateAAudioPlaybackDeviceManager();
        int count = audioPlaybackDeviceManager.GetAudioPlaybackDeviceCount();
        logger.UpdateLog(string.Format("AudioPlaybackDeviceManager count: {0}", count));
        for(int i = 0; i < count ; i ++) {
            audioPlaybackDeviceManager.GetAudioPlaybackDevice(i, ref audioPlaybackDeviceName, ref audioPlaybackDeviceId);
            audioPlaybackDeviceDic.Add(i, audioPlaybackDeviceId);
            logger.UpdateLog(string.Format("AudioPlaybackDevice device index: {0}, name: {1}, id: {2}", i, audioPlaybackDeviceName, audioPlaybackDeviceId));
        }
    }

    void GetVideoDeviceManager()
    {
        string videoDeviceName = "";
        string videoDeviceId = "";
		/// If you want to getVideoDeviceManager, you need to call startPreview() first;
		mRtcEngine.StartPreview();
        videoDeviceManager = (VideoDeviceManager)mRtcEngine.GetVideoDeviceManager();
        videoDeviceManager.CreateAVideoDeviceManager();
        int count = videoDeviceManager.GetVideoDeviceCount();
        logger.UpdateLog(string.Format("VideoDeviceManager count: {0}", count));
        for(int i = 0; i < count ; i ++) {
            videoDeviceManager.GetVideoDevice(i, ref videoDeviceName, ref videoDeviceId);
            videoDeviceManagerDic.Add(i, videoDeviceId);
            logger.UpdateLog(string.Format("VideoDeviceManager device index: {0}, name: {1}, id: {2}", i, videoDeviceName, videoDeviceId));
        }
    }

    void SetCurrentDevice()
    {
        audioRecordingDeviceManager.SetAudioRecordingDevice(audioRecordingDeviceDic[deviceIndex]);
        audioPlaybackDeviceManager.SetAudioPlaybackDevice(audioPlaybackDeviceDic[deviceIndex]);
        videoDeviceManager.SetVideoDevice(videoDeviceManagerDic[deviceIndex]);
    }

    void SetCurrentDeviceVolume()
    {
        audioRecordingDeviceManager.SetAudioRecordingDeviceVolume(100);
        audioPlaybackDeviceManager.SetAudioPlaybackDeviceVolume(100);
    }

    void ReleaseDeviceManager()
    {
        audioPlaybackDeviceManager.ReleaseAAudioPlaybackDeviceManager();
        audioRecordingDeviceManager.ReleaseAAudioRecordingDeviceManager();
        videoDeviceManager.ReleaseAVideoDeviceManager();
    }

    void JoinChannel()
    {
        mRtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();
        }
    }

    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        logger.UpdateLog(string.Format("sdk version: {0}", IRtcEngine.GetSdkVersion()));
        logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
    }

    void OnLeaveChannelHandler(RtcStats stats)
    {
        logger.UpdateLog("OnLeaveChannelSuccess");
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
}


using agora_gaming_rtc;
using UnityEngine;
using UnityEngine.UI;

using Logger = agora_utilities.Logger;

public class TranscodingApp : PlayerViewControllerBase
{
    const string DOCUMENTATION_URL = "https://docs.agora.io/en/Interactive%20Broadcast/cdn_streaming_unity?platform=Unity";
    const int HOSTVIEW_WIDTH = 360;
    const int HOSTVIEW_HEIGHT = 640;


    //!!! ------------ fill in your stream key here !!!----------//
    string TWURL = "rtmp://live-sjc.twitch.tv/app/<YOUR STREAM KEY>";
    string FBURL = "rtmps://live-api-s.facebook.com:443/rtmp/<YOUR STREAM KEY>";
    string YTURL = "rtmp://a.rtmp.youtube.com/live2/<YOUR STREAM KEY>";


    VideoSurface host1;
    VideoSurface host2;
    int hostCount = 0;
    uint MyUID { get; set; }
    uint RemoteUID { get; set; }
    bool IsStreamingLive { get; set; }
    Logger logger = null;

    protected override void PrepareToJoin()
    {
        base.PrepareToJoin();
        VideoEncoderConfiguration configuration = new VideoEncoderConfiguration
        {
            dimensions = new VideoDimensions { width = HOSTVIEW_WIDTH * 2, height = HOSTVIEW_HEIGHT },
            frameRate = FRAME_RATE.FRAME_RATE_FPS_24
            // mirrorMode = VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_ENABLED
        };
        mRtcEngine.SetVideoEncoderConfiguration(configuration);

        mRtcEngine.OnFirstLocalVideoFrame = delegate (int width, int height, int elapsed)
        {
            Debug.LogFormat("OnFirstLocalVideoFrame => width:{0} height:{1} elapsed:{2}", width, height, elapsed);
        };
        mRtcEngine.OnFirstRemoteVideoFrame = delegate (uint uid, int width, int height, int elapsed)
        {
            Debug.LogFormat("OnFirstRemoteVideoFrame => width:{0} height:{1} elapsed:{2} uid:{3}", width, height, elapsed, uid);
        };

        mRtcEngine.OnStreamPublished = OnStreamPublished;
    }

    protected override void SetupUI()
    {
        base.SetupUI();
        Button titleBtn = GameObject.Find("TitleButton").GetComponent<Button>();
        titleBtn.onClick.AddListener(() => { Application.OpenURL(DOCUMENTATION_URL); });

        Button startButton = GameObject.Find("StartButton").GetComponent<Button>();
        startButton.onClick.AddListener(() => { HandleStartButtonClick(startButton); });

        host1 = GameObject.Find("Host1").AddComponent<VideoSurface>();
        host2 = GameObject.Find("Host2").AddComponent<VideoSurface>();
        host1.SetEnable(false);
        host2.SetEnable(false);

        GameObject loggerObj = GameObject.Find("LoggerText");
        if (loggerObj != null)
        {
            Text text = loggerObj.GetComponent<Text>();
            if (text != null)
            {
                logger = new Logger(text);
                logger.Clear();
            }
        }
    }

    void HandleStartButtonClick(Button button)
    {
        if (IsStreamingLive)
        {
            StopTranscoding();
            button.GetComponentInChildren<Text>().text = "Start";
        }
        else
        {
            if (logger != null)
            {
                logger.DebugAssert(IsCDNAddressReady(), "You may need to fill in your Stream Key in the TranscodingApp source file!");
            }
            button.GetComponentInChildren<Text>().text = "Stop";
            StartTranscoding(RemoteUID);
        }
        IsStreamingLive = !IsStreamingLive;
    }

    protected override void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        base.OnJoinChannelSuccess(channelName, uid, elapsed);
        host1.SetEnable(true);
        hostCount++;
        MyUID = uid;
    }

    protected override void OnUserJoined(uint uid, int elapsed)
    {
        hostCount++;

        if (hostCount == 2)
        {
            host2.SetForUser(uid);
            host2.SetEnable(true);
            RemoteUID = uid;
        }
        else
        {
            return;
        }
    }

    protected override void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        base.OnUserOffline(uid, reason);
        if (RemoteUID == uid)
        {
            host2.SetEnable(false);
            hostCount--;
            RemoteUID = 0;
        }
    }

    void StopTranscoding()
    {
        mRtcEngine.RemovePublishStreamUrl(TWURL);
        mRtcEngine.RemovePublishStreamUrl(FBURL);
        mRtcEngine.RemovePublishStreamUrl(YTURL);
        IsStreamingLive = false;
    }

    void StartTranscoding(uint uid)
    {
        LiveTranscoding live = new LiveTranscoding();

        TranscodingUser user = new TranscodingUser();
        user.uid = uid;
        user.x = 0;
        user.y = 0;
        user.width = HOSTVIEW_WIDTH;
        user.height = HOSTVIEW_HEIGHT;
        user.audioChannel = 0;
        user.alpha = 1;

        TranscodingUser me = user;
        me.uid = MyUID;
        me.x = me.width;

        live.transcodingUsers = new TranscodingUser[] { me, user };
        live.userCount = 2;

        live.width = 2 * HOSTVIEW_WIDTH;
        live.height = HOSTVIEW_HEIGHT;
        live.videoBitrate = 400;
        live.videoCodecProfile = VIDEO_CODEC_PROFILE_TYPE.VIDEO_CODEC_PROFILE_HIGH;
        live.videoGop = 30;
        live.videoFramerate = 24;
        live.lowLatency = false;

        live.audioSampleRate = AUDIO_SAMPLE_RATE_TYPE.AUDIO_SAMPLE_RATE_44100;
        live.audioBitrate = 48;
        live.audioChannels = 1;
        live.audioCodecProfile = AUDIO_CODEC_PROFILE_TYPE.AUDIO_CODEC_PROFILE_LC_AAC;

        mRtcEngine.SetLiveTranscoding(live);

        int rc = mRtcEngine.AddPublishStreamUrl(url: YTURL, transcodingEnabled: true);
        Debug.Assert(rc == 0, " error in adding " + YTURL);
        rc = mRtcEngine.AddPublishStreamUrl(url: FBURL, transcodingEnabled: true);
        Debug.Assert(rc == 0, " error in adding " + FBURL);
        rc = mRtcEngine.AddPublishStreamUrl(url: TWURL, transcodingEnabled: hostCount > 1 ? true : false);
        Debug.Assert(rc == 0, " error in adding " + TWURL);
    }

    private void OnStreamPublished(string url, int errorCode)
    {
        /** Reports the result of calling the {@link agora_gaming_rtc.IRtcEngine.AddPublishStreamUrl AddPublishStreamUrl} method. (CDN live only.)
		* 
		* @param url The RTMP URL address.
		* @param error Error code: Main errors include:
		* - ERR_OK(0): The publishing succeeds.
		* - ERR_FAILED(1): The publishing fails.
		* - ERR_INVALID_ARGUMENT(2): Invalid argument used. If, for example, you did not call {@link agora_gaming_rtc.IRtcEngine.SetLiveTranscoding SetLiveTranscoding} to configure LiveTranscoding before calling `AddPublishStreamUrl`, the SDK reports `ERR_INVALID_ARGUMENT(2)`.
		* - ERR_TIMEDOUT(10): The publishing timed out.
		* - ERR_ALREADY_IN_USE(19): The chosen URL address is already in use for CDN live streaming.
		* - ERR_RESOURCE_LIMITED(22): The backend system does not have enough resources for the CDN live streaming.
		* - ERR_ENCRYPTED_STREAM_NOT_ALLOWED_PUBLISH(130): You cannot publish an encrypted stream.
		* - ERR_PUBLISH_STREAM_CDN_ERROR(151)
		* - ERR_PUBLISH_STREAM_NUM_REACH_LIMIT(152)
		* - ERR_PUBLISH_STREAM_NOT_AUTHORIZED(153)
		* - ERR_PUBLISH_STREAM_INTERNAL_SERVER_ERROR(154)
		* - ERR_PUBLISH_STREAM_FORMAT_NOT_SUPPORTED(156)
		*/
        Debug.Log("\n\n");
        Debug.Log("---------------OnStreamPublished called----------------");
        Debug.Log("OnStreamPublished url===" + url);
        Debug.Log("OnStreamPublished errorCode===" + errorCode + " = " + IRtcEngine.GetErrorDescription(errorCode));
    }

    const string STREAMKEY_PLACEHOLDER = "<YOUR STREAM KEY>";
    bool IsCDNAddressReady()
    {
        if (YTURL.Contains(STREAMKEY_PLACEHOLDER) || FBURL.Contains(STREAMKEY_PLACEHOLDER) || TWURL.Contains(STREAMKEY_PLACEHOLDER))
        {
            return false;
        }

        return true;
    }
}


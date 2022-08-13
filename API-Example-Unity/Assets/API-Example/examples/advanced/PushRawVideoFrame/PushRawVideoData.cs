using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using agora_gaming_rtc;

public class PushRawVideoData : MonoBehaviour
{
    public RawImage textureImage;
    private Texture2D mTexture;
    private Rect mRect;
    bool running = false;

    [SerializeField]
    public string APP_ID = "YOUR_APPID";

    [SerializeField]
    public string TOKEN = "";

    [SerializeField]
    public string CHANNEL_NAME = "YOUR_CHANNEL_NAME";
    private IRtcEngine _rtcEngine = null;

    public bool usePointer = false;

    private void Start()
    {
        InitEngine();
    }

    void StartPushVideo()
    {
        StartCoroutine(CoVideoPush());
    }

    void InitEngine()
    {
        _rtcEngine = IRtcEngine.GetEngine(APP_ID);
        _rtcEngine.SetLogFile("log.txt");
        _rtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        _rtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        _rtcEngine.SetExternalVideoSource(true);
        _rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;

        mRect = new Rect(0, 0, textureImage.texture.width, textureImage.texture.height);
        mTexture = textureImage.texture as Texture2D;

        JoinChannel();
        StartPushVideo();
    }

    void JoinChannel()
    {
        _rtcEngine.DisableAudio();
        _rtcEngine.EnableVideo();
        _rtcEngine.EnableVideoObserver();

        VideoEncoderConfiguration config = new VideoEncoderConfiguration
        {
            dimensions = new VideoDimensions() { width = (int)mRect.width, height = (int)mRect.height },
            frameRate = FRAME_RATE.FRAME_RATE_FPS_15,
            minFrameRate = -1,
            bitrate = 0,
            minBitrate = 1,
            orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_ADAPTIVE,
            degradationPreference = DEGRADATION_PREFERENCE.MAINTAIN_FRAMERATE,
            mirrorMode = VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_ENABLED
        };
        _rtcEngine.SetVideoEncoderConfiguration(config);

        int ret = _rtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
        Debug.Log(string.Format("JoinChannel ret: {0}", ret));
    }


    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        Debug.Log(string.Format("sdk version: {0}", IRtcEngine.GetSdkVersion()));
        Debug.Log(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
        running = true;
    }

    IEnumerator CoVideoPush()
    {
        yield return new WaitUntil(() => running);
        // Get the Raw Texture data from the the from the texture and apply it to an array of bytes
        IntPtr pointer = mTexture.GetNativeTexturePtr();
        int len = mTexture.GetRawTextureData().Length;
        Debug.LogWarningFormat("Image rect = {0} len={1}", mRect, len);
        while (running)
        {
            yield return new WaitForSeconds(0.03f);

            IRtcEngine rtc = IRtcEngine.QueryEngine();
            if (rtc != null)
            {
                ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
                externalVideoFrame.type = ExternalVideoFrame.VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
                externalVideoFrame.format = ExternalVideoFrame.VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;

                if (usePointer)
                {
                    pointer = mTexture.GetNativeTexturePtr();
                    externalVideoFrame.bufferPtr = pointer;
                }
                else
                {
                    byte[] bytes = mTexture.GetRawTextureData();
                    externalVideoFrame.buffer = bytes;
                }

                externalVideoFrame.stride = (int)mRect.width;
                externalVideoFrame.height = (int)mRect.height;
                //externalVideoFrame.cropLeft = 10;
                //externalVideoFrame.cropTop = 10;
                //externalVideoFrame.cropRight = 10;
                //externalVideoFrame.cropBottom = 10;
                //externalVideoFrame.rotation = 180;
                //externalVideoFrame.timestamp = System.DateTime.Now.Ticks / 10000;
                externalVideoFrame.timestamp = timestamp++;
                int a = rtc.PushVideoFrame(externalVideoFrame);
                if (timestamp % 100 == 0) Debug.Log("PushVideoFrame " + timestamp + " ret = " + a);

            }
        }

    }

    static int timestamp = 0;
    void OnApplicationQuit()
    {
        running = false;
        if (_rtcEngine != null)
        {
            _rtcEngine.LeaveChannel();
            _rtcEngine.DisableVideoObserver();
            IRtcEngine.Destroy();
            _rtcEngine = null;
        }
    }

}


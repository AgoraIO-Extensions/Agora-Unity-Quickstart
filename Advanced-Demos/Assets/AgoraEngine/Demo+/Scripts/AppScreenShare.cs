using UnityEngine;
using agora_gaming_rtc;
using System.Collections;
using UnityEngine.UI;

/// <summary>
///   This script shows how to do Screen Sharing the Unity Application screen.
///   The Sender/Receiver are operating in LiveBroadcasting mode.
///   Two important APIs are center of this feature:
///     1. SetExternalVideoSource - sets up the stream to use user defined data
///     2. PushVideoFrame - sends the raw data to the receipants. 
///   Note:  VIDEO_PIXEL_BGRA is the current Pixel format that the API supports
///     So the receipant side should instantiate Textures with BGRA32 format to
///   decode the color properly.
/// </summary>
public class TestAppScreenShare : PlayerViewControllerBase
{
    Texture2D mTexture;
    Rect mRect;
    bool running = false;
    int timestamp = 0;
    MonoBehaviour monoProxy;
    GameObject particleEffect;

    int EncodeWidth;
    int EncodeHeight;

    protected override void PrepareToJoin()
    {
        base.PrepareToJoin();
        EnableShareScreen();

        // Live Broadcasting mode to allow many view only audience 
        mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        EncodeWidth = Screen.width;
        EncodeHeight = Screen.height;

        // do not exceed 1080p
        if ( System.Math.Min(EncodeWidth, EncodeHeight) > 1080) { 
            Vector2 vector2 = agora_utilities.AgoraUIUtils.GetScaledDimension(EncodeWidth, EncodeHeight, 720);
            EncodeHeight = (int)vector2.y;
            EncodeWidth = (int)vector2.x;
	    }

        mRtcEngine.SetVideoEncoderConfiguration(new VideoEncoderConfiguration()
        {
            bitrate = 1130,
            frameRate = FRAME_RATE.FRAME_RATE_FPS_15,
            dimensions = new VideoDimensions() { width = EncodeWidth, height = EncodeHeight },

            // Note if your remote user video surface to set to flip Horizontal, then we should flip it before sending
            mirrorMode = VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_ENABLED
        });

        Debug.LogFormat("Sharing Screen with width = {0} height = {1}, encoding at {2},{3}", Screen.width, Screen.height, EncodeWidth, EncodeHeight);
    }


    protected override void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        base.OnJoinChannelSuccess(channelName, uid, elapsed);
        StartSharing();
    }

    protected override void SetupUI()
    {
        base.SetupUI();
        monoProxy = GameObject.Find("Canvas").GetComponent<MonoBehaviour>();

        Button button = GameObject.Find("StopButton").GetComponent<Button>();
        button.onClick.AddListener(() => { DisableShareScreen(); });

        particleEffect = GameObject.Find("ParticleEffect");
    }

    protected void StartSharing()
    {
        if (running == false)
        {
            // Create a rectangle width and height of the screen
            mRect = new Rect(0, 0, Screen.width, Screen.height);
            // Create a texture the size of the rectangle you just created
            mTexture = new Texture2D((int)mRect.width, (int)mRect.height, TextureFormat.RGBA32, false);
            // get the rtc engine instance, assume it has been created before this script starts
            running = true;
            monoProxy.StartCoroutine(shareScreen());
        }
    }

    void EnableShareScreen()
    {
        // Very Important to make this app work
        mRtcEngine.SetExternalVideoSource(true, false);
    }

    void DisableShareScreen()
    {
        StopSharing();
        Debug.Log("ScreenShare Deactivated");
        particleEffect.SetActive(false);
    }

    IEnumerator shareScreen()
    {
        while (running)
        {
            yield return new WaitForEndOfFrame();
            //Read the Pixels inside the Rectangle
            mTexture.ReadPixels(mRect, 0, 0);
            //Apply the Pixels read from the rectangle to the texture
            mTexture.Apply();
            // Get the Raw Texture data from the the from the texture and apply it to an array of bytes
            byte[] bytes = mTexture.GetRawTextureData();
            // int size = Marshal.SizeOf(bytes[0]) * bytes.Length;
            // Check to see if there is an engine instance already created
            //if the engine is present
            if (mRtcEngine != null)
            {
                //Create a new external video frame
                ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
                //Set the buffer type of the video frame
                externalVideoFrame.type = ExternalVideoFrame.VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
                // Set the video pixel format
                //externalVideoFrame.format = ExternalVideoFrame.VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_BGRA;  // V.2.9.x
                externalVideoFrame.format = ExternalVideoFrame.VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;  // V.3.x.x
                //apply raw data you are pulling from the rectangle you created earlier to the video frame
                externalVideoFrame.buffer = bytes;
                //Set the width of the video frame (in pixels)
                externalVideoFrame.stride = (int)mRect.width;
                //Set the height of the video frame
                externalVideoFrame.height = (int)mRect.height;
                //Remove pixels from the sides of the frame
                externalVideoFrame.cropLeft = 10;
                externalVideoFrame.cropTop = 10;
                externalVideoFrame.cropRight = 10;
                externalVideoFrame.cropBottom = 10;
                //Rotate the video frame (0, 90, 180, or 270)
                externalVideoFrame.rotation = 180;
                externalVideoFrame.timestamp = timestamp++;
                //Push the external video frame with the frame we just created
                mRtcEngine.PushVideoFrame(externalVideoFrame);
                if (timestamp % 100 == 0)
                {
                    Debug.LogWarning("Pushed frame = " + timestamp);
                }

            }
        }
    }

    void StopSharing()
    {
        // set the boolean false will cause the shareScreen coRoutine to exit
        running = false;
    }
}

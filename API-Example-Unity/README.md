# API-Example-Unity

*__其他语言版本：__  [__简体中文__](README.zh.md)*

The API-Example-Unity project is an open-source demo that will show you different scenes on how to integrate Agora SDK APIs into your project.

Any scene of this project can run successfully alone.

This project contains these scenes:

* **Basic demos:**

| Demo             | Description                                        | APIs                                                         |
| ---------------- | -------------------------------------------------- | ------------------------------------------------------------ |
| JoinChannelAudio | basic demo to show audio call                      | GetEngine, JoinChannelByKey, LeaveChannel                    |
| JoinChannelVideo | video demo with role selection in Editor Inspector | SetChannelProfile,SetClientRole,EnableVideo,EnableVideoObserver, JoinChannelByKey, VideoSurface |

* **Advanced demos:**

| Demo                         | Description                                                  | APIs                                                         |
| ---------------------------- | ------------------------------------------------------------ | ------------------------------------------------------------ |
| ScreenShareOnMobile          | sharing application screen view from Unity camera            | PushVideoFrame, SetExternalVideoSource                       |
| ScreenShareOnDesktop         | sharing desktop screen or individual windows                 | StartScreenCaptureByWindowId, StartScreenCaptureByDisplayId, StartScreenCaptureByScreenRect |
| AudioMixing                  | audioMixing and play audio effect in the channel             | StartAudioMixing, PlayEffect                                 |
| CustomRenderAudio            | use AudioSource to play raw data received in the Agora channel | PullAudioFrame                                               |
| CustomCaptureAudio           | Sending raw data from AudioSource into the Agora channel     | PushAudioFrame                                               |
| CustomCaptureFile            | feeding audio raw data using a file                          | PushAudioFrame                                               |
| DeviceManager                | show how to get and set Device on the desktop platforms      | GetAudioRecordingDeviceManager, CreateAAudioRecordingDeviceManager,   GetAudioRecordingDeviceCount, GetAudioRecordingDevice, GetVideoDevice, SetAudioRecordingDevice,  SetAudioPlaybackDevice, SetAudioRecordingDeviceVolume, SetAudioPlaybackDeviceVolume, ReleaseAAudioRecordingDeviceManager, ReleaseAAudioPlaybackDeviceManager, ReleaseAVideoDeviceManager |
| SetEncryption                | sending video with encryption                                | EnableEncryption                                             |
| JoinMultipleChannel          | multi-channel video call with AgoraChannel class             | CreateChannel, SetClientRole, EnableEncryption, LeaveChannel, ReleaseChannel |
| ProcessVideoRawData          | show how to setup raw video capture and render               | SetOnCaptureVideoFrameCallback, SetOnRenderVideoFrameCallback,  OnRenderVideoFrameHandler, OnCaptureVideoFrameHandler |
| ProcessAudioRawData          | playback audio frames from the channel on an AudioSource object | RegisterAudioRawDataObserver, SetOnPlaybackAudioFrameCallback, OnPlaybackAudioFrameHandler |
| RtmpStreaming                | stream video by RTMP Push to a CDN                           | SetVideoEncoderConfiguration, SetLiveTranscoding, AddPublishStreamUrl, RemovePublishStreamUrl |
| JoinChannelVideoToken        | demo on how to run Agora app with a token                    | RenewToken                                                   |
| PlaybackAudioFrame           | playback single user's audio frame on an AudioSource object  | RegisterAudioRawDataObserver, SetOnPlaybackAudioFrameBeforeMixingCallback, OnPlaybackAudioFrameBeforeMixingHandler |
| SetVideoEncoderConfiguration | video demo with multiple encoding dimension choice           | SetVideoEncoderConfiguration                                 |


## Running the App

First, create a developer account at [Agora.io](https://dashboard.agora.io/signin/), and obtain an App ID.

Then do the following:

1. Clone this repository.

2. Open the project in Unity Editor. Note that you will see compiler errors before you download the SDK package.

3. You may download the SDK package in either of the following two ways:

  

    a. From Unity Asset Store download and import [the Agora Video SDK](https://assetstore.unity.com/packages/tools/video/agora-video-chat-sdk-for-unity-134502)
    
    b. Download the ******Agora Video SDK****** from [Agora.io SDK](https://docs.agora.io/en/Video/downloads?platform=Unity). Unzip the downloaded SDK package and copy the files from ******samples/Hello-Video-Unity-Agora/Assets/AgoraEngine/****** in SDK to ******API-Example-Unity/Assets/****** in project

4.  Choose one of the scene that you want to run

5.  Fill your App ID into the scene on the game controller (attached on VideoCanvas most likely), like the following example:
    ![api-sample-appid](https://user-images.githubusercontent.com/1261195/89360166-652da380-d67c-11ea-9e67-1e02bbe94fc5.png)

Run the game and you are now good to go!

## Developer Environment Requirements

* Unity 2017 LTS and up

## Help Resources

- You can find full API document at [Document Center]([https://docs.agora.io/en/Video/API%20Reference/unity/index.html](https://docs.agora.io/en/Video/API%20Reference/unity/index.html))

- You can file issues about this demo at [issue](https://github.com/AgoraIO/Voice-Call-for-Mobile-Gaming/issues)

- For advanced features such as screensharing and transcoding, please refer to [this repo](https://bit.ly/2RRP5tK), which implements [advanced guides](https://docs.agora.io/en/Interactive%20Broadcast/media_relay_unity?platform=Unity) in the documentation.

## License
The MIT License (MIT).

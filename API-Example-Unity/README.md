# API-Example-Unity

**其他语言版本：** [**简体中文**](https://github.com/AgoraIO/Agora-Unity-Quickstart/blob/release/3.4.6_iris/API-Example-Unity/README.zh.md)

## Overview

The API-Example-Unity project is an open-source demo that will show you different scenes on how to integrate Agora SDK APIs into your project.

Any scene of this project can run successfully alone.

## Project structure

* **Basic demos:**

| Demo                                                         | Description                                        | APIs                                                         |
| ------------------------------------------------------------ | -------------------------------------------------- | ------------------------------------------------------------ |
| [JoinChannelAudio](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/release/3.4.6_iris/API-Example-Unity/Assets/API-Example/examples/basic/JoinChannelAudio) | basic demo to show audio call                      | GetEngine, JoinChannelByKey, LeaveChannel                    |
| [JoinChannelVideo](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/release/3.4.6_iris/API-Example-Unity/Assets/API-Example/examples/basic/JoinChannelVideo) | video demo with role selection in Editor Inspector | SetChannelProfile,SetClientRole,EnableVideo,EnableVideoObserver, JoinChannelByKey, VideoSurface |

* **Advanced demos:**

| Demo                             | Description                                                  | APIs                                                         |
| -------------------------------- | ------------------------------------------------------------ | ------------------------------------------------------------ |
| [ScreenShareOnMobile]()          | sharing application screen view from Unity camera            | PushVideoFrame, SetExternalVideoSource                       |
| [ScreenShareWhileVideoCall]()    | sharing desktop screen or individual windows while video call | StartScreenCaptureByWindowId, StartScreenCaptureByDisplayId, StartScreenCaptureByScreenRect |
| [AudioMixing]()                  | audioMixing and play audio effect in the channel             | StartAudioMixing, PlayEffect                                 |
| [CustomRenderAudio]()            | use AudioSource to play raw data received in the Agora channel | PullAudioFrame                                               |
| [CustomCaptureAudio]()           | Sending raw data from AudioSource into the Agora channel     | PushAudioFrame                                               |
| [CustomCaptureFile]()            | feeding audio raw data using a file                          | PushAudioFrame                                               |
| [DeviceManager]()                | show how to get and set Device on the desktop platforms      | GetAudioRecordingDeviceManager, CreateAAudioRecordingDeviceManager,   GetAudioRecordingDeviceCount, GetAudioRecordingDevice, GetVideoDevice, SetAudioRecordingDevice,  SetAudioPlaybackDevice, SetAudioRecordingDeviceVolume, SetAudioPlaybackDeviceVolume, ReleaseAAudioRecordingDeviceManager, ReleaseAAudioPlaybackDeviceManager, ReleaseAVideoDeviceManager |
| [SetEncryption]()                | sending video with encryption                                | EnableEncryption                                             |
| [JoinMultipleChannel]()          | multi-channel video call with AgoraChannel class             | CreateChannel, SetClientRole, EnableEncryption, LeaveChannel, ReleaseChannel |
| [ProcessVideoRawData]()          | show how to setup raw video capture and render               | SetOnCaptureVideoFrameCallback, SetOnRenderVideoFrameCallback,  OnRenderVideoFrameHandler, OnCaptureVideoFrameHandler |
| [ProcessAudioRawData]()          | playback audio frames from the channel on an AudioSource object | RegisterAudioRawDataObserver, SetOnPlaybackAudioFrameCallback, OnPlaybackAudioFrameHandler |
| [RtmpStreaming]()                | stream video by RTMP Push to a CDN                           | SetVideoEncoderConfiguration, SetLiveTranscoding, AddPublishStreamUrl, RemovePublishStreamUrl |
| [JoinChannelVideoToken]()        | demo on how to run Agora app with a token                    | RenewToken                                                   |
| [PlaybackAudioFrame]()           | playback single user's audio frame on an AudioSource object  | RegisterAudioRawDataObserver, SetOnPlaybackAudioFrameBeforeMixingCallback, OnPlaybackAudioFrameBeforeMixingHandler |
| [SetVideoEncoderConfiguration]() | video demo with multiple encoding dimension choice           | SetVideoEncoderConfiguration                                 |



## How to run the sample project

#### Developer Environment Requirements

- Unity 2017 LTS and up

#### Steps to run

First, create a developer account at [Agora.io](https://dashboard.agora.io/signin/), and obtain an App ID.

Then do the following:

1. Clone this repository.

2. Open the project in Unity Editor. Note that you will see compiler errors before you download the SDK package.

3. You may download the SDK package in either of the following two ways:

   a. From Unity Asset Store download and import [the Agora Video SDK](https://assetstore.unity.com/packages/tools/video/agora-video-chat-sdk-for-unity-134502)

   b. Download the **Agora Video SDK** from [Agora.io SDK](https://docs.agora.io/en/Video/downloads?platform=Unity). Unzip the downloaded SDK package and copy the files from **samples/Hello-Video-Unity-Agora/Assets/AgoraEngine/** in SDK to **API-Example-Unity/Assets/** in project

4. Choose one of the scene that you want to run

5. Fill your App ID into the scene on the game controller (attached on VideoCanvas most likely), like the following example: [![api-sample-appid](https://user-images.githubusercontent.com/1261195/89360166-652da380-d67c-11ea-9e67-1e02bbe94fc5.png)](https://user-images.githubusercontent.com/1261195/89360166-652da380-d67c-11ea-9e67-1e02bbe94fc5.png)

Run the game and you are now good to go!

## Feedback

If you have any problems or suggestions regarding the sample projects, feel free to file an issue.

## Reference

- You can find full API document at [Document Center](https://docs-preprod.agora.io/en/Video/API%20Reference/unity/v3.5.0.3/API/rtc_api_overview.html)
- You can file issues about this demo at [issue](https://github.com/AgoraIO/Voice-Call-for-Mobile-Gaming/issues)
- For advanced features such as screensharing and transcoding, please refer to [this repo](https://bit.ly/2RRP5tK), which implements [advanced guides](https://docs.agora.io/en/Interactive Broadcast/media_relay_unity?platform=Unity) in the documentation.

## Related resources

- Check our [FAQ](https://docs.agora.io/en/faq) to see if your issue has been recorded.
- Dive into [Agora SDK Samples](https://github.com/AgoraIO) to see more tutorials
- Take a look at [Agora Use Case](https://github.com/AgoraIO-usecase) for more complicated real use case
- Repositories managed by developer communities can be found at [Agora Community](https://github.com/AgoraIO-Community)
- If you encounter problems during integration, feel free to ask questions in [Stack Overflow](https://stackoverflow.com/questions/tagged/agora.io)

## License

The sample projects are under the MIT license.

# API-Example-Unity

*__其他语言版本：__  [__简体中文__](README.zh.md)*

## Overview

The API-Example-Unity project is an open-source demo that will show you different scenes on how to integrate Agora SDK APIs into your project.

Any scene of this project can run successfully alone.

## Project structure

* **Basic demos:**

| Demo                                                         | Description                                        | APIs                                                         |
| ------------------------------------------------------------ | -------------------------------------------------- | ------------------------------------------------------------ |
| [JoinChannelAudio](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Basic/JoinChannelAudio) | basic demo to show audio call                      | CreateAgoraRtcEngine, JoinChannel, LeaveChannel              |
| [JoinChannelVideo](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Basic/JoinChannelVideo) | video demo with role selection in Editor Inspector | SetChannelProfile,SetClientRole,EnableVideo,EnableVideo, JoinChannel, VideoSurface |

* **Advanced demos:**

| Demo                                                         | Description                                                  | APIs                                                         |
| ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ |
| [AudioMixing](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/AudioMixing) | audioMixing and play audio effect in the channel             | StartAudioMixing, PlayEffect                                 |
| [AudioSpectrum](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/AudioSpectrum) | audio spectrum             | IAudioSpectrumObserver, RegisterMediaPlayerAudioSpectrumObserver                                 |
| [ChannelMediaRelay](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/ChannelMediaRelay) | start cross channel media streaming forwarding. This method can be used to realize cross channel wheat connection and other scenes. | StartChannelMediaRelay, UpdateChannelMediaRelay, PauseAllChannelMediaRelay, ResumeAllChannelMediaRelay, StopChannelMediaRelay |
| [ContentInspect](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/ContentInspect) | content inspect | SetContentInspect |
| [CustomCaptureAudio](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/CustomCaptureAudio) | Sending raw data from AudioSource into the Agora channel     | PushAudioFrame                                               |
| [CustomCaptureVideo](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/CustomCaptureVideo) | Sending raw data from VideoSource into the Agora channel     | PushVideoFrame 
| [CustomCaptureVideo/StaticImagePush](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/CustomCaptureVideo/StaticImagePush) | Push a static texture as custom video input | PushVideoFrame, CreateCustomVideoTrack                                          |
| [CustomCaptureVideo/MultChannelsPush](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/CustomCaptureVideo/MultChannelsPush) | push two distinct textures as custom video sources  | PushVideoFrame, CreateCustomVideoTrack, JoinChannelEx                                       |
| [CustomCaptureVideo/WebCamWithVirtualCamPush](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/CustomCaptureVideo/WebCamWithVirtualCamPush) |Main webcam and virtual camera (render texture) video push| PushVideoFrame, CreateCustomVideoTrack, JoinChannelEx                                          |
| [CustomRenderAudio](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/CustomRenderAudio) | use AudioSource to play raw data received in the Agora channel | PullAudioFrame                                               |
| [DeviceManager](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/DeviceManager) | show how to get and set Device on the desktop platforms      | GetAudioDeviceManager, GetVideoDeviceManager                 |
| [DualCamera](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/DualCamera) | show how to use dual camera to capture data                  | StartPrimaryCameraCapture, StartSecondaryCameraCapture       |
| [JoinChannelVideoToken](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/JoinChannelVideoToken) | demo on how to run Agora app with a token                    | RenewToken                                                   |
| [JoinChannelWithUserAccount](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/JoinChannelWithUserAccount) | demo on how to join channel with user account                | JoinChannelWithUserAccount,   GetUserInfoByUserAccount       |
| [MediaPlayer](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/MediaPlayer) | how to  play media                                           | CreateMediaPlayer,  Play, Stop                               |
| [MediaRecorder](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/MediaRecorder) | Media recording                  | StartRecording,  StopRecording,                                                |
| [Metadata](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/Metadata) | send meta data                  | IMetadataObserver                                                |
| [ProcessAudioRawData](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/ProcessAudioRawData) | process raw Audio data in rtc engine and play with audioclip                       | RegisterAudioFrameObserver, OnPlaybackAudioFrame, OnRceordAudioFrame |
| [ProcessVideoRawData](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/ProcessVideoRawData) | process raw Video data in rtc engine and render with texture                       | RegisterVideoFrameObserver, OnCaptureVideoFrame, OnRenderVideoFrame |
| [PushEncodedVideoImage](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/PushEncodedVideoImage) | push encoded data in rtc engine                              | PushEncodedVideoImage                                        |
| [ScreenShare](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/ScreenShare) | sharing screen view with rtc engine                          | StartScreenCaptureByWindowId, StartScreenCaptureByDisplayId  |
| [ScreenShareWhileVideoCall](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/ScreenShareWhileVideoCall) | sharing screen view while with a video call                  | StartScreenCaptureByWindowId, StartScreenCaptureByDisplayId  |
| [SetBeautyEffectOptions](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/SetBeautyEffectOptions) | Turn on beauty during video call                             | SetBeautyEffectOptions                                       |
| [SetEncryption](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/SetEncryption) | sending video with encryption                                | EnableEncryption                                             |
| [SetVideoEncoderConfiguration](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/SetVideoEncoderConfiguration) | video demo with multiple encoding dimension choice           | SetVideoEncoderConfiguration                                 |
| [SpatialAudioWithMediaPlayer](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/SpatialAudioWithMediaPlayer) | play media with spatial audio                                | GetLocalSpatialAudioEngine, UpdateRemotePositionEx           |
| [SpatialAudioWithUsers](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/SpatialAudioWithUsers) | experience remote user spatial audio based on position change                                | GetLocalSpatialAudioEngine, UpdateSelfPosition, UpdateRemotePosition           |
| [StartLocalVideoTranscoder](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/StartLocalVideoTranscoder) | Merging of multiple video sources, options are jpg,png,gif,medai,etc | StartLocalVideoTranscoder                                    |
| [StartDirectCdnStreaming](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/StartDirectCdnStreaming) | stream video by RTMP Push to a CDN          | StartDirectCdnStreaming, SetDirectCdnStreamingVideoConfiguration, StopDirectCdnStreaming                               |
| [StartRhythmPlayer](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/StartRhythmPlayer) | use phythm player in rtc engine                              | StartRhythmPlayer                                            |
| [StartRtmpStreamWithTranscoding](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/StartRtmpStreamWithTranscoding) | stream video by RTMP Push to a CDN                           | StartRtmpStreamWithTranscoding, UpdateRtmpTranscoding, StopRtmpStream |
| [StreamMessage](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/StreamMessage) | how to send stream message                                   | CreateDataStream, SendStreamMessage                          |
| [TakeSnapshot](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/TakeSnapshot) | how to screen shot                                           | TakeSnapshot                                                 |
| [VirtualBackground](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/VirtualBackground) | enable virtual background                                    | EnableVirtualBackground                                      |
| [VoiceChanger](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/VoiceChanger) | how to modify your voice                                     | SetVoiceBeautifierPreset, SetAudioEffectPreset,SetVoiceConversionPreset,SetLocalVoicePitch, SetLocalVoiceEqualization,   SetLocalVoiceReverb |

* **Rtm demos:**

| Demo                                                         | Description                                                  | APIs                                                         |
| ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ |
| [IRTMClient](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Rtm/RtmScene/RTMClientScene.unity) | Use IRtmClient to log in, and send or receive messages            | LoginAsync, PublishAsync, OnMessageEvent                          |
| [IStreamChannel](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Rtm/RtmScene/RTMLockScene.unity) | Create and join stream channel to send or receive topic messages           | JoinAsync, JoinTopicAsync, PublishTopicMessageAsync,                                 |
| [IRtmLock](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Rtm/RtmScene/RTMLockScene.unity) | Lock or unlock specified attributes within the RTM channel.             | SetLockAsync, GetLocksAsync, RemoveLockAsync                                |
| [IRtmPresence](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Rtm/RtmScene) | Set channel attribute and query user status | GetOnlineUsersAsync, GetUserChannelsAsync, SetStateAsync |
| [IRtmStorage](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Rtm/RtmScene) | Store unique information in the channel or in user | SetChannelMetadataAsync, UpdateChannelMetadataAsync, RemoveChannelMetadataAsync, GetChannelMetadataAsync  |


## How to run the sample project

#### Developer Environment Requirements

* Unity 2017 LTS and up

#### Steps to run

First, create a developer account at [Agora.io](https://dashboard.agora.io/signin/), and obtain an App ID.

Then do the following:

1. Clone this repository.

2. Open the project in Unity Editor. Note that you will see compiler errors before you download the SDK package.

3. You may download the SDK package in either of the following two ways:

    a. From Unity Asset Store download and import [the Agora Video SDK](https://assetstore.unity.com/packages/tools/video/agora-video-chat-sdk-for-unity-134502)
    
    b. Download the ******Agora Video SDK****** from [Agora.io SDK](https://docs.agora.io/en/Video/downloads?platform=Unity). Unzip the downloaded SDK package and import into Unity project

4.  Fill your App ID into the ******API-Example-Unity/Assets/API-Example/appIdInput/AppIdInput.asset****** 

5.  Choose one of the scene that you want to run

Run the game and you are now good to go!



## Feedback

If you have any problems or suggestions regarding the sample projects, feel free to file an issue.

## Reference

- You can find full API document at [Document Center](https://api-ref.agora.io/en/video-sdk/unity/4.x/API/rtc_api_overview_ng.html)
- You can find full release note at [Release Note](https://docs.agora.io/en/video-calling/reference/release-notes?platform=unity)
- You can file issues about this demo at [issue](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/issues)

## Related resources

- Check our [FAQ](https://docs.agora.io/en/faq) to see if your issue has been recorded.
- Dive into [Agora SDK Samples](https://github.com/AgoraIO) to see more tutorials
- Take a look at [Agora Use Case](https://github.com/AgoraIO-usecase) for more complicated real use case
- Take a look at [AgoraIO-Extensions](https://github.com/AgoraIO-Extensions) for Crossplatform and Marketing-place projects
- Repositories managed by developer communities can be found at [Agora Community](https://github.com/AgoraIO-Community)
- If you encounter problems during integration, feel free to ask questions in [Stack Overflow](https://stackoverflow.com/questions/tagged/agora.io)

## License
The sample projects are under the MIT license.



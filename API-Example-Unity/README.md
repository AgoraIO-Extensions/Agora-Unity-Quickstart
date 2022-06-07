# API-Example-Unity

*__其他语言版本：__  [__简体中文__](README.zh.md)*

## Overview

The API-Example-Unity project is an open-source demo that will show you different scenes on how to integrate Agora SDK APIs into your project.

Any scene of this project can run successfully alone.

## Project structure

* **Basic demos:**

| Demo                                                         | Description                                        | APIs                                                         |
| ------------------------------------------------------------ | -------------------------------------------------- | ------------------------------------------------------------ |
| [JoinChannelAudio](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/basic/JoinChannelAudio) | basic demo to show audio call                      | CreateAgoraRtcEngine, JoinChannel, LeaveChannel                    |
| [JoinChannelVideo](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/basic/JoinChannelVideo) | video demo with role selection in Editor Inspector | SetChannelProfile,SetClientRole,EnableVideo,EnableVideo, JoinChannel, VideoSurface |

* **Advanced demos:**

| Demo                                                         | Description                                                  | APIs                                                         |
| ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ |
| [AudioMixing](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/AudioMixing) | audioMixing and play audio effect in the channel             | StartAudioMixing, PlayEffect                                 |
| [ChannelMediaRelay](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/ChannelMediaRelay) | start cross channel media streaming forwarding. This method can be used to realize cross channel wheat connection and other scenes.             | StartChannelMediaRelay, UpdateChannelMediaRelay, PauseAllChannelMediaRelay, ResumeAllChannelMediaRelay, StopChannelMediaRelay                      |
| [CustomCaptureAudio](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/CustomCaptureAudio) | Sending raw data from AudioSource into the Agora channel     | PushAudioFrame                                               |
| [CustomCaptureVideo](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/CustomCaptureVideo) | Sending raw data from VideoSource into the Agora channel     | PushVideoFrame                                               |
| [CustomRenderAudio](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/CustomRenderAudio) | use AudioSource to play raw data received in the Agora channel | PullAudioFrame                                               |
| [DeviceManager](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/DeviceManager) | show how to get and set Device on the desktop platforms      | GetAudioDeviceManager, GetVideoDeviceManager |
| [DualCamera](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/DualCamera) | show how to use dual camera to capture data  | StartPrimaryCameraCapture, StartSecondaryCameraCapture |
| [JoinChannelVideoToken](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/JoinChannelVideoToken) | demo on how to run Agora app with a token                    | RenewToken                                                   |
| [JoinChannelWithUserAccount](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/JoinChannelWithUserAccount) | demo on how to join channel with user account                   | JoinChannelWithUserAccount,   GetUserInfoByUserAccount                                                 |
| [MediaPlayer](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/MediaPlayer) | how to  play media                   | CreateMediaPlayer,  Play, Stop                                               |
| [ProcessRawData](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/ProcessRawData) | process raw video data in rtc engine | RegisterVideoFrameObserver, OnCaptureVideoFrame, OnRenderVideoFrame |
| [PushEncodedVideoImage](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/PushEncodedVideoImage) | push encoded data in rtc engine | PushEncodedVideoImage |
| [RtmpStreaming](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/RtmpStreaming) | stream video by RTMP Push to a CDN                           | SetVideoEncoderConfiguration, SetLiveTranscoding, AddPublishStreamUrl, RemovePublishStreamUrl |
| [ScreenShare](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/ScreenShare) | sharing screen view with rtc engine            | StartScreenCaptureByWindowId, StartScreenCaptureByDisplayId                       |
| [ScreenShareWhileVideoCall](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/ScreenShareWhileVideoCall) | sharing screen view while with a video call            | StartScreenCaptureByWindowId, StartScreenCaptureByDisplayId                       |
| [SetBeautyEffectOptions](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/SetBeautyEffectOptions) | Turn on beauty during video call            | SetBeautyEffectOptions                   |
| [SetEncryption](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/SetEncryption) | sending video with encryption                                | EnableEncryption                                             |
| [SetVideoEncoderConfiguration](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/SetVideoEncoderConfiguration) | video demo with multiple encoding dimension choice           | SetVideoEncoderConfiguration                                 |
| [SpatialAudioWithMediaPlayer](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/SpatialAudioWithMediaPlayer) | play media with spatial audio         | GetLocalSpatialAudioEngine, UpdateRemotePositionEx                                 |
| [StartLocalVideoTranscoder](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/StartLocalVideoTranscoder) | Merging of multiple video sources, options are jpg,png,gif,medai,etc          | StartLocalVideoTranscoder                        |
| [StartRhythmPlayer](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/StartRhythmPlayer) | use phythm player in rtc engine        | StartRhythmPlayer                        |
| [StartRtmpStreamWithTranscoding](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/StartRtmpStreamWithTranscoding) | stream video by RTMP Push to a CDN        | StartRtmpStreamWithTranscoding, UpdateRtmpTranscoding, StopRtmpStream                        |
| [StreamMessage](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/StreamMessage) | how to send stream message        | CreateDataStream, SendStreamMessage                        |
| [TakeSnapshot](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/TakeSnapshot) | how to screen shot      | TakeSnapshot                        |
| [VirtualBackground](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/VirtualBackground) | enable virtual background   | EnableVirtualBackground                 |
| [VoiceChanger](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/VoiceChanger) | how to modify your voice   | SetVoiceBeautifierPreset, SetAudioEffectPreset,SetVoiceConversionPreset,SetLocalVoicePitch, SetLocalVoiceEqualization,   SetLocalVoiceReverb                  |

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

- You can find full API document at [Document Center](https://docs.agora.io/en/Video/API%20Reference/unity/index.html)
- You can file issues about this demo at [issue](https://github.com/AgoraIO/Voice-Call-for-Mobile-Gaming/issues)
- For advanced features such as screensharing and transcoding, please refer to [this repo](https://bit.ly/2RRP5tK), which implements [advanced guides](https://docs.agora.io/en/Interactive%20Broadcast/media_relay_unity?platform=Unity) in the documentation.

## Related resources

- Check our [FAQ](https://docs.agora.io/en/faq) to see if your issue has been recorded.
- Dive into [Agora SDK Samples](https://github.com/AgoraIO) to see more tutorials
- Take a look at [Agora Use Case](https://github.com/AgoraIO-usecase) for more complicated real use case
- Repositories managed by developer communities can be found at [Agora Community](https://github.com/AgoraIO-Community)
- If you encounter problems during integration, feel free to ask questions in [Stack Overflow](https://stackoverflow.com/questions/tagged/agora.io)

## License
The sample projects are under the MIT license.

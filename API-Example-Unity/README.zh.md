# API-Example-Unity

*Read this in other languages: [English](README.md)*

## 简介

这个开源示例项目演示了不同场景下，Agora SDK 的基本集成逻辑。 项目中每个 Scene 都是一个独立的场景，都可以成功独立运行。

在这个示例项目中包含的所有场景都可以独立运行：

## 项目结构

* **基础案例:**
| Demo                                                         | Description                                        | APIs                                                         |
| ------------------------------------------------------------ | -------------------------------------------------- | ------------------------------------------------------------ |
| [JoinChannelAudio](Assets/API-Example/examples/basic/JoinChannelAudio) | basic demo to show audio call                      | GetEngine, JoinChannelByKey, LeaveChannel                    |
| [JoinChannelVideo](Assets/API-Example/examples/basic/JoinChannelVideo) | video demo with role selection in Editor Inspector | SetChannelProfile,SetClientRole,EnableVideo,EnableVideoObserver, JoinChannelByKey, VideoSurface |

* **进阶案例:**

| Demo                                                         | Description                                                  | APIs                                                         |
| ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ |
| [ScreenShareOnMobile](Assets/API-Example/examples/advanced/ScreenShareOnMobile) | sharing application screen view from Unity camera            | PushVideoFrame, SetExternalVideoSource                       |
| [ScreenShareOnDesktop](Assets/API-Example/examples/advanced/ScreenShareOnDesktop) | sharing desktop screen or individual windows                 | StartScreenCaptureByWindowId, StartScreenCaptureByDisplayId, StartScreenCaptureByScreenRect |
| [AudioMixing](Assets/API-Example/examples/advanced/AudioMixing) | audioMixing and play audio effect in the channel             | StartAudioMixing, PlayEffect                                 |
| [CustomRenderAudio](Assets/API-Example/examples/advanced/CustomRenderAudio) | use AudioSource to play raw data received in the Agora channel | PullAudioFrame                                               |
| [CustomCaptureAudio](Assets/API-Example/examples/advanced/CustomCaptureAudio) | Sending raw data from AudioSource into the Agora channel     | PushAudioFrame                                               |
| [CustomCaptureAudioFile](Assets/API-Example/examples/advanced/CustomCaptureAudioFile) | feeding audio raw data using a file                          | PushAudioFrame                                               |
| [DeviceManager](Assets/API-Example/examples/advanced/DeviceManager) | show how to get and set Device on the desktop platforms      | GetAudioRecordingDeviceManager, CreateAAudioRecordingDeviceManager,   GetAudioRecordingDeviceCount, GetAudioRecordingDevice, GetVideoDevice, SetAudioRecordingDevice,  SetAudioPlaybackDevice, SetAudioRecordingDeviceVolume, SetAudioPlaybackDeviceVolume, ReleaseAAudioRecordingDeviceManager, ReleaseAAudioPlaybackDeviceManager, ReleaseAVideoDeviceManager |
| [SetEncryption](Assets/API-Example/examples/advanced/SetEncryption) | sending video with encryption                                | EnableEncryption                                             |
| [JoinMultipleChannel](Assets/API-Example/examples/advanced/JoinMultipleChannel) | multi-channel video call with AgoraChannel class             | CreateChannel, SetClientRole, EnableEncryption, LeaveChannel, ReleaseChannel |
| [ProcessVideoRawData](Assets/API-Example/examples/advanced/ProcessVideoRawData) | show how to setup raw video capture and render               | SetOnCaptureVideoFrameCallback, SetOnRenderVideoFrameCallback,  OnRenderVideoFrameHandler, OnCaptureVideoFrameHandler |
| [ProcessAudioRawData](Assets/API-Example/examples/advanced/ProcessAudioRawData) | playback audio frames from the channel on an AudioSource object | RegisterAudioRawDataObserver, SetOnPlaybackAudioFrameCallback, OnPlaybackAudioFrameHandler |
| [RtmpStreaming](Assets/API-Example/examples/advanced/RtmpStreaming) | stream video by RTMP Push to a CDN                           | SetVideoEncoderConfiguration, SetLiveTranscoding, AddPublishStreamUrl, RemovePublishStreamUrl |
| [JoinChannelVideoToken](Assets/API-Example/examples/advanced/JoinChannelVideoToken) | demo on how to run Agora app with a token                    | RenewToken                                                   |
| [PlaybackAudioFrame](Assets/API-Example/examples/advanced/PlaybackAudioFrame) | playback single user's audio frame on an AudioSource object  | RegisterAudioRawDataObserver, SetOnPlaybackAudioFrameBeforeMixingCallback, OnPlaybackAudioFrameBeforeMixingHandler |
| [SetVideoEncoderConfiguration](Assets/API-Example/examples/advanced/SetVideoEncoderConfiguration) | video demo with multiple encoding dimension choice           | SetVideoEncoderConfiguration                                 |
| [SendStreamMessage](Assets/API-Example/examples/advanced/SendStreamMessage) | Send Messages to other users in channel          | SendStreamMessage                                 |
|[Echo Test](Assets/API-Example/examples/advanced/EchoTest)| Speak and get echo back | StartEchoTest, StopEchoTest|

## 如何运行示例程序

#### 运行环境

* Unity 2017 LTS 或以上

#### 运行步骤

* 首先在 [Agora.io 注册](https://dashboard.agora.io/cn/signup/) 注册账号，并创建自己的测试项目，获取到 AppID。

* 然后在 [Agora.io SDK](https://docs.agora.io/cn/Agora%20Platform/downloads) 下载 **Unity SDK**，解压后把 sdk 包中的 samples/Hello-Video-Unity-Agora/Assets/AgoraEngine 文件夹拷贝到 API-Example-Unity/Assets 目录下。

* 最后使用 Unity 打开本项目, 选择想要运行的 Scene, 根据提示将 APPID 填入，然后运行程序。

* 一切就绪。你可以自由探索示例项目，体验 SDK 的丰富功能。



## 反馈

如果您对示例项目有任何问题或建议，请随时提交问题。

## 参考文档

- 您可以在 [文档中心](https://docs.agora.io/cn/Video/API%20Reference/unity/index.html)找到完整的API文档

- 有关屏幕共享和转码等高级功能，请参阅 [this repo](https://bit.ly/2RRP5tK), 文档 [advanced guides](https://docs.agora.io/en/Interactive%20Broadcast/media_relay_unity?platform=Unity)

## 相关资源

- 你可以先参阅[常见问题](https://docs.agora.io/cn/faq)
- 如果你想了解更多官方示例，可以参考[官方 SDK 示例](https://github.com/AgoraIO)
- 如果你想了解声网 SDK 在复杂场景下的应用，可以参考[官方场景案例](https://github.com/AgoraIO-usecase)
- 如果你想了解声网的一些社区开发者维护的项目，可以查看[社区](https://github.com/AgoraIO-Community)
- 若遇到问题需要开发者帮助，你可以到[开发者社区](https://rtcdeveloper.com/)提问
- 如果需要售后技术支持, 你可以在[Agora Dashboard](https://dashboard.agora.io/)提交工单

## 代码许可

示例项目遵守 MIT 许可证。

# API-Example_Unity

*Read this in other languages: [English](README.md)*

## 概述

这个开源示例项目演示了不同场景下，Agora SDK 的基本集成逻辑。 项目中每个 Scene 都是一个独立的场景，都可以成功独立运行。

在这个示例项目中包含的所有场景都可以独立运行：

## 项目结构

* **基础案例:**

| Demo                                                         | Description                                        | APIs                                                         |
| ------------------------------------------------------------ | -------------------------------------------------- | ------------------------------------------------------------ |
| [JoinChannelAudio](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/basic/JoinChannelAudio) | basic demo to show audio call                      | GetEngine, JoinChannelByKey, LeaveChannel                    |
| [JoinChannelVideo](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/basic/JoinChannelVideo) | video demo with role selection in Editor Inspector | SetChannelProfile,SetClientRole,EnableVideo,EnableVideoObserver, JoinChannelByKey, VideoSurface |

* **进阶案例:**

| Demo                                                         | Description                                                  | APIs                                                         |
| ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ |
| [ScreenShareOnMobile](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/ScreenShareOnMobile) | sharing application screen view from Unity camera            | PushVideoFrame, SetExternalVideoSource                       |
| [ScreenShareOnDesktop](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/ScreenShareOnDesktop) | sharing desktop screen or individual windows                 | StartScreenCaptureByWindowId, StartScreenCaptureByDisplayId, StartScreenCaptureByScreenRect |
| [AudioMixing](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/AudioMixing) | audioMixing and play audio effect in the channel             | StartAudioMixing, PlayEffect                                 |
| [CustomRenderAudio](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/CustomRenderAudio) | use AudioSource to play raw data received in the Agora channel | PullAudioFrame                                               |
| [CustomCaptureAudio](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/CustomCaptureAudio) | Sending raw data from AudioSource into the Agora channel     | PushAudioFrame                                               |
| [CustomCaptureAudioFile](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/CustomCaptureAudioFile) | feeding audio raw data using a file                          | PushAudioFrame                                               |
| [DeviceManager](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/DeviceManager) | show how to get and set Device on the desktop platforms      | GetAudioRecordingDeviceManager, CreateAAudioRecordingDeviceManager,   GetAudioRecordingDeviceCount, GetAudioRecordingDevice, GetVideoDevice, SetAudioRecordingDevice,  SetAudioPlaybackDevice, SetAudioRecordingDeviceVolume, SetAudioPlaybackDeviceVolume, ReleaseAAudioRecordingDeviceManager, ReleaseAAudioPlaybackDeviceManager, ReleaseAVideoDeviceManager |
| [SetEncryption](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/SetEncryption) | sending video with encryption                                | EnableEncryption                                             |
| [JoinMultipleChannel](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/JoinMultipleChannel) | multi-channel video call with AgoraChannel class             | CreateChannel, SetClientRole, EnableEncryption, LeaveChannel, ReleaseChannel |
| [ProcessVideoRawData](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/ProcessVideoRawData) | show how to setup raw video capture and render               | SetOnCaptureVideoFrameCallback, SetOnRenderVideoFrameCallback,  OnRenderVideoFrameHandler, OnCaptureVideoFrameHandler |
| [ProcessAudioRawData](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/ProcessAudioRawData) | playback audio frames from the channel on an AudioSource object | RegisterAudioRawDataObserver, SetOnPlaybackAudioFrameCallback, OnPlaybackAudioFrameHandler |
| [RtmpStreaming](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/RtmpStreaming) | stream video by RTMP Push to a CDN                           | SetVideoEncoderConfiguration, SetLiveTranscoding, AddPublishStreamUrl, RemovePublishStreamUrl |
| [JoinChannelVideoToken](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/JoinChannelVideoToken) | demo on how to run Agora app with a token                    | RenewToken                                                   |
| [PlaybackAudioFrame](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/PlaybackAudioFrame) | playback single user's audio frame on an AudioSource object  | RegisterAudioRawDataObserver, SetOnPlaybackAudioFrameBeforeMixingCallback, OnPlaybackAudioFrameBeforeMixingHandler |
| [SetVideoEncoderConfiguration](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/SetVideoEncoderConfiguration) | video demo with multiple encoding dimension choice           | SetVideoEncoderConfiguration                                 |



## 如何运行示例程序

#### 运行环境

* Unity 2017 LTS 或以上

#### 步骤

首先在 [Agora.io 注册](https://dashboard.agora.io/cn/signup/) 注册账号，并创建自己的测试项目，获取到 AppID。

然后在 [Agora.io SDK](https://docs.agora.io/cn/Agora%20Platform/downloads) 下载 **Unity SDK**，解压后

- 把 sdk 包中的 samples/Hello-Video-Unity-Agora/Assets/AgoraEngine 文件夹拷贝到 API-Example-Unity/Assets 目录下。

最后使用 Unity 打开本项目, 选择想要运行的 Scene, 根据提示将 APPID 填入，然后运行程序。



## 反馈

如果您对示例项目有任何问题或建议，请随时提交问题。

- 如果在集成中遇到问题, 你可以到 [开发者社区](https://dev.agora.io/cn/) 提问
- 如果有售前咨询问题, 可以拨打 400 632 6626，或加入官方Q群 12742516 提问
- 如果需要售后技术支持, 你可以在 [Agora Dashboard](https://dashboard.agora.io) 提交工单
- 如果发现了示例代码的bug, 欢迎提交 [issue](https://github.com/AgoraIO/Hello-Unity3D-Agora/issues)

## 参考

- 您可以在 [文档中心](https://docs.agora.io/cn/Video/API%20Reference/unity/index.html)找到完整的API文档

- 有关屏幕共享和转码等高级功能，请参阅 [this repo](https://bit.ly/2RRP5tK), 文档 [advanced guides](https://docs.agora.io/en/Interactive%20Broadcast/media_relay_unity?platform=Unity) 

## 相关资源

- 查看[FAQ](https://docs.agora.io/en/faq) 查看您的问题是否已被记录
- 更多教程可以在 [Agora SDK Samples](https://github.com/AgoraIO) 找到
- 参考 [Agora Use Case](https://github.com/AgoraIO-usecase) 对于更复杂的实际用例
- 开发者社区管理的文档库可在 [Agora Community](https://github.com/AgoraIO-Community)中找到

- 如果您在集成过程中遇到问题，请在 [Stack Overflow](https://stackoverflow.com/questions/tagged/agora.io)中自由提问

## 代码许可

The MIT License (MIT).

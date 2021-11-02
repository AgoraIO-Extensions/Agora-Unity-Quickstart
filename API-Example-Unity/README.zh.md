# API-Example_Unity

*Read this in other languages: [English](README.md)*

这个开源示例项目演示了不同场景下，Agora SDK 的基本集成逻辑。 项目中每个 Scene 都是一个独立的场景，都可以成功独立运行。

在这个示例项目中包含了以下场景：

- **基本功能案例:**

| Demo             | Description                                        | APIs                                                         |
| ---------------- | -------------------------------------------------- | ------------------------------------------------------------ |
| JoinChannelAudio | basic demo to show audio call                      | GetEngine, JoinChannelByKey, LeaveChannel                    |
| JoinChannelVideo | video demo with role selection in Editor Inspector | SetChannelProfile,SetClientRole,EnableVideo,EnableVideoObserver, JoinChannelByKey, VideoSurface |

* **进阶功能案例:**

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

## 运行示例程序
首先在 [Agora.io 注册](https://dashboard.agora.io/cn/signup/) 注册账号，并创建自己的测试项目，获取到 AppID。

然后在 [Agora.io SDK](https://docs.agora.io/cn/Agora%20Platform/downloads) 下载 **Unity SDK**，解压后

- 把 sdk 包中的 samples/Hello-Video-Unity-Agora/Assets/AgoraEngine 文件夹拷贝到 API-Example-Unity/Assets 目录下。

最后使用 Unity 打开本项目, 选择想要运行的 Scene, 根据提示将 APPID 填入，然后运行程序。

## 运行环境
* Unity 2017 LTS 或以上

## 联系我们

- 完整的 API 文档见 [文档中心](https://docs.agora.io/cn/)
- 如果在集成中遇到问题, 你可以到 [开发者社区](https://dev.agora.io/cn/) 提问
- 如果有售前咨询问题, 可以拨打 400 632 6626，或加入官方Q群 12742516 提问
- 如果需要售后技术支持, 你可以在 [Agora Dashboard](https://dashboard.agora.io) 提交工单
- 如果发现了示例代码的bug, 欢迎提交 [issue](https://github.com/AgoraIO/Hello-Unity3D-Agora/issues)

## 代码许可

The MIT License (MIT).

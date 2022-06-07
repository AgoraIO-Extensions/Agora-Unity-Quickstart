# API-Example-Unity

*Read this in other languages: [English](README.md)*

## 简介

这个开源示例项目演示了不同场景下，Agora SDK 的基本集成逻辑。 项目中每个 Scene 都是一个独立的场景，都可以成功独立运行。

在这个示例项目中包含的所有场景都可以独立运行：

## 项目结构

* **基础案例:**

| Demo                                                         | Description                                        | APIs                                                         |
| ------------------------------------------------------------ | -------------------------------------------------- | ------------------------------------------------------------ |
| [JoinChannelAudio](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/basic/JoinChannelAudio) | 基础音频通话                      | CreateAgoraRtcEngine, JoinChannelByKey, LeaveChannel                    |
| [JoinChannelVideo](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/basic/JoinChannelVideo) | 基础视频通话 | SetChannelProfile,SetClientRole,EnableVideo,EnableVideo, JoinChannelByKey, VideoSurface |

* **进阶案例:**

| Demo                                                         | Description                                                  | APIs                                                         |
| ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ |
| [AudioMixing](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/AudioMixing) | 在频道内播放混音和已音效             | StartAudioMixing, PlayEffect                                 |
| [ChannelMediaRelay](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/ChannelMediaRelay) | 启动跨通道媒体流转发。该方法可用于实现跨通道连麦等场景。            | StartChannelMediaRelay, UpdateChannelMediaRelay, PauseAllChannelMediaRelay, ResumeAllChannelMediaRelay, StopChannelMediaRelay                      |
| [CustomCaptureAudio](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/CustomCaptureAudio) | 推送外部音频帧     | PushAudioFrame                                               |
| [CustomCaptureVideo](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/CustomCaptureVideo) | 推送外部视频帧     | PushVideoFrame                                               |
| [CustomRenderAudio](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/CustomRenderAudio) |  拉取远端音频数据 | PullAudioFrame                                               |
| [DeviceManager](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/DeviceManager) | 获取当前的音视频设备信息      | GetAudioDeviceManager, GetVideoDeviceManager |
| [DualCamera](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/DualCamera) | 双摄像头工作  | StartPrimaryCameraCapture, StartSecondaryCameraCapture |
| [JoinChannelVideoToken](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/JoinChannelVideoToken) | 使用Token加入频道和更新Token                    | RenewToken                                                   |
| [JoinChannelWithUserAccount](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/JoinChannelWithUserAccount) | 使用用户账号加入频道                  | JoinChannelWithUserAccount,   GetUserInfoByUserAccount                                                 |
| [MediaPlayer](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/MediaPlayer) | 播放媒体文件                  | CreateMediaPlayer,  Play, Stop                                               |
| [ProcessRawData](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/ProcessRawData) | 注册视频观测器对象 | RegisterVideoFrameObserver, OnCaptureVideoFrame, OnRenderVideoFrame |
| [PushEncodedVideoImage](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/PushEncodedVideoImage) | 发送结构化数据 | PushEncodedVideoImage |
| [RtmpStreaming](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/RtmpStreaming) | 推流到CDN                           | SetVideoEncoderConfiguration, SetLiveTranscoding, AddPublishStreamUrl, RemovePublishStreamUrl |
| [ScreenShare](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/ScreenShare) | 屏幕共享            | StartScreenCaptureByWindowId, StartScreenCaptureByDisplayId                       |
| [ScreenShareWhileVideoCall](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/ScreenShareWhileVideoCall) | 在视频通话时进行屏幕共享            | StartScreenCaptureByWindowId, StartScreenCaptureByDisplayId                       |
| [SetBeautyEffectOptions](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/SetBeautyEffectOptions) | 开启美颜效果            | SetBeautyEffectOptions                   |
| [SetEncryption](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/SetEncryption) | 开启或关闭内置加密                                | EnableEncryption                                             |
| [SetVideoEncoderConfiguration](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/SetVideoEncoderConfiguration) | 设置视频编码属性。           | SetVideoEncoderConfiguration                                 |
| [SpatialAudioWithMediaPlayer](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/SpatialAudioWithMediaPlayer) | 使用空间音效播放媒体文件        | GetLocalSpatialAudioEngine, UpdateRemotePositionEx                                 |
| [StartLocalVideoTranscoder](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/StartLocalVideoTranscoder) | 多路视频合成一路，可以合成png,jpg,jif等等         | StartLocalVideoTranscoder                        |
| [StartRhythmPlayer](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/StartRhythmPlayer) | 开启节拍器        | StartRhythmPlayer                        |
| [StartRtmpStreamWithTranscoding](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/StartRtmpStreamWithTranscoding) | 推流到CDN        | StartRtmpStreamWithTranscoding, UpdateRtmpTranscoding, StopRtmpStream                        |
| [StreamMessage](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/StreamMessage) | 发送流数据        | CreateDataStream, SendStreamMessage                        |
| [TakeSnapshot](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/TakeSnapshot) | 屏幕截图      | TakeSnapshot                        |
| [VirtualBackground](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/VirtualBackground) | 开启虚拟背景   | EnableVirtualBackground                 |
| [VoiceChanger](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity/Assets/API-Example/examples/advanced/VoiceChanger) | 变声设置   | SetVoiceBeautifierPreset, SetAudioEffectPreset,SetVoiceConversionPreset,SetLocalVoicePitch, SetLocalVoiceEqualization,   SetLocalVoiceReverb                  |



## 如何运行示例程序

#### 运行环境

* Unity 2017 LTS 或以上

#### 运行步骤

* 首先在 [Agora.io 注册](https://dashboard.agora.io/cn/signup/) 注册账号，并创建自己的测试项目，获取到 AppID。

* 然后在 [Agora.io SDK](https://docs.agora.io/cn/Agora%20Platform/downloads) 下载 **Unity SDK**，解压后把 sdk 包中的 unitypackage文件导入工程

* 最后使用 Unity 打开本项目, 将获取到的AppID填入******API-Example-Unity/Assets/API-Example/appIdInput/AppIdInput.asset*****中

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

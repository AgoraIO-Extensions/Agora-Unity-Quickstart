# API-Example-Unity

*Read this in other languages: [English](README.md)*

## 简介

这个开源示例项目演示了不同场景下，Agora SDK 的基本集成逻辑。 项目中每个 Scene 都是一个独立的场景，都可以成功独立运行。

在这个示例项目中包含的所有场景都可以独立运行：

## 项目结构

* **基础案例:**

| Demo                                                         | Description                                        | APIs                                                         |
| ------------------------------------------------------------ | -------------------------------------------------- | ------------------------------------------------------------ |
| [JoinChannelAudio](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Basic/JoinChannelAudio) | 基础音频通话                      | CreateAgoraRtcEngine, JoinChannelByKey, LeaveChannel                    |
| [JoinChannelVideo](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Basic/JoinChannelVideo) | 基础视频通话 | SetChannelProfile,SetClientRole,EnableVideo,EnableVideo, JoinChannelByKey, VideoSurface |

* **进阶案例:**

| Demo                                                         | Description                                                  | APIs                                                         |
| ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ |
| [AudioMixing](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/AudioMixing) | 在频道内播放混音和已音效             | StartAudioMixing, PlayEffect                                 |
| [AudioSpectrum](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/AudioSpectrum) | 媒体音谱功能             | IAudioSpectrumObserver, RegisterMediaPlayerAudioSpectrumObserver                                 |
| [ChannelMediaRelay](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/ChannelMediaRelay) | 启动跨通道媒体流转发。该方法可用于实现跨通道连麦等场景。            | StartChannelMediaRelay, UpdateChannelMediaRelay, PauseAllChannelMediaRelay, ResumeAllChannelMediaRelay, StopChannelMediaRelay                      |
| [ContentInspect](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/ContentInspect) | 内容鉴定 | SetContentInspect |
| [CustomCaptureAudio](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/CustomCaptureAudio) | 推送外部音频帧     | PushAudioFrame                                               |
| [CustomCaptureVideo](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/CustomCaptureVideo) | 推送外部视频帧     | PushVideoFrame                                               |
| [CustomCaptureVideo/StaticImagePush](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/CustomCaptureVideo/StaticImagePush) | 推送一张静态图片作为外部视频帧 | PushVideoFrame, CreateCustomVideoTrack                                          |
| [CustomCaptureVideo/MultChannelsPush](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/CustomCaptureVideo/MultChannelsPush) | 利用多频道推送2张静态图片作为外部视频帧 | PushVideoFrame, CreateCustomVideoTrack, JoinChannelEx                                       |
| [CustomCaptureVideo/WebCamWithVirtualCamPush](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/CustomCaptureVideo/WebCamWithVirtualCamPush) | 同时推送摄像头和自采集视频（Unity虚拟镜头）| PushVideoFrame, CreateCustomVideoTrack, JoinChannelEx                                          |
| [CustomRenderAudio](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/CustomRenderAudio) |  拉取远端音频数据 | PullAudioFrame                                               |
| [DeviceManager](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/DeviceManager) | 获取当前的音视频设备信息      | GetAudioDeviceManager, GetVideoDeviceManager |
| [DualCamera](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/DualCamera) | 双摄像头工作  | StartPrimaryCameraCapture, StartSecondaryCameraCapture |
| [JoinChannelVideoToken](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/JoinChannelVideoToken) | 使用Token加入频道和更新Token                    | RenewToken                                                   |
| [JoinChannelWithUserAccount](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/JoinChannelWithUserAccount) | 使用用户账号加入频道                  | JoinChannelWithUserAccount,   GetUserInfoByUserAccount                                                 |
| [MediaPlayer](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/MediaPlayer) | 播放媒体文件                  | CreateMediaPlayer,  Play, Stop                                               |
| [MediaRecorder](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/MediaRecorder) | 媒体录制                  | StartRecording,  StopRecording,                                                |
| [Metadata](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/Metadata) | 发送元数据                  | IMetadataObserver                                                |
| [ProcessAudioRawData](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/ProcessAudioRawData) | 获注册音频裸数据观测器,并用audioclip播放获取到的音频数据                       | RegisterAudioFrameObserver, OnPlaybackAudioFrame, OnRceordAudioFrame |
| [ProcessVideoRawData](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/ProcessVideoRawData) | 注册视频裸数据观测器,并渲染在Texture上                       | RegisterVideoFrameObserver, OnCaptureVideoFrame, OnRenderVideoFrame |
| [PushEncodedVideoImage](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/PushEncodedVideoImage) | 发送结构化数据 | PushEncodedVideoImage |
| [ScreenShare](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/ScreenShare) | 屏幕共享            | StartScreenCaptureByWindowId, StartScreenCaptureByDisplayId                       |
| [ScreenShareWhileVideoCall](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/ScreenShareWhileVideoCall) | 在视频通话时进行屏幕共享            | StartScreenCaptureByWindowId, StartScreenCaptureByDisplayId                       |
| [SetBeautyEffectOptions](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/SetBeautyEffectOptions) | 开启美颜效果            | SetBeautyEffectOptions                   |
| [SetEncryption](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/SetEncryption) | 开启或关闭内置加密                                | EnableEncryption                                             |
| [SetVideoEncoderConfiguration](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/SetVideoEncoderConfiguration) | 设置视频编码属性。           | SetVideoEncoderConfiguration                                 |
| [SpatialAudioWithMediaPlayer](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/SpatialAudioWithMediaPlayer) | 使用空间音效播放媒体文件        | GetLocalSpatialAudioEngine, UpdateRemotePositionEx                                 |
| [SpatialAudioWithUsers](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/SpatialAudioWithUsers) | 远端用户的空间音频体现 | GetLocalSpatialAudioEngine, UpdateSelfPosition, UpdateRemotePosition           |
| [StartDirectCdnStreaming](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/StartDirectCdnStreaming) | cdn推流        | StartDirectCdnStreaming, SetDirectCdnStreamingVideoConfiguration, StopDirectCdnStreaming                               |
| [StartLocalVideoTranscoder](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/StartLocalVideoTranscoder) | 多路视频合成一路，可以合成png,jpg,jif等等         | StartLocalVideoTranscoder                        |
| [StartRhythmPlayer](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/StartRhythmPlayer) | 开启节拍器        | StartRhythmPlayer                        |
| [StartRtmpStreamWithTranscoding](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/StartRtmpStreamWithTranscoding) | 推流到CDN        | StartRtmpStreamWithTranscoding, UpdateRtmpTranscoding, StopRtmpStream                        |
| [StreamMessage](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/StreamMessage) | 发送流数据        | CreateDataStream, SendStreamMessage                        |
| [TakeSnapshot](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/TakeSnapshot) | 屏幕截图      | TakeSnapshot                        |
| [VirtualBackground](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/VirtualBackground) | 开启虚拟背景   | EnableVirtualBackground                 |
| [VoiceChanger](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/VoiceChanger) | 变声设置   | SetVoiceBeautifierPreset, SetAudioEffectPreset,SetVoiceConversionPreset,SetLocalVoicePitch, SetLocalVoiceEqualization,   SetLocalVoiceReverb                  |

* **Rtm demos:**

| Demo                                                         | Description                                                  | APIs                                                         |
| ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ |
| [IRTMClient](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Rtm/RtmScene/RTMClientScene.unity) | 使用IRtmClient登录并发送或接收消息           | LoginAsync, PublishAsync, OnMessageEvent                          |
| [IStreamChannel](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Rtm/RtmScene/RTMLockScene.unity) | 创建并加入流通道以发送或接收主题消息           | JoinAsync, JoinTopicAsync, PublishTopicMessageAsync,                                 |
| [IRtmLock](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Rtm/RtmScene/RTMLockScene.unity) | 锁定或解锁RTM通道中的指定属性。             | SetLockAsync, GetLocksAsync, RemoveLockAsync                                |
| [IRtmPresence](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Rtm/RtmScene) | 设置频道熟悉并查询用户状态 | GetOnlineUsersAsync, GetUserChannelsAsync, SetStateAsync |
| [IRtmStorage](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Rtm/RtmScene) | 在频道或用户中存储唯一信息 | SetChannelMetadataAsync, UpdateChannelMetadataAsync, RemoveChannelMetadataAsync, GetChannelMetadataAsync  |


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

- 您可以在 [文档中心](https://docs.agora.io/cn/All/API%20Reference/unity_ng/API/rtc_api_overview_ng.html)找到完整的API文档
- 您可以在 [发版说明](https://docs.agora.io/cn/video-call-4.x/release_unity_ng?platform=Unity)找到完整的发版说明
- 您可以在 [github issue](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/issues)提交你的issue

## 相关资源

- 你可以先参阅[常见问题](https://docs.agora.io/cn/faq)
- 如果你想了解更多官方示例，可以参考[官方 SDK 示例](https://github.com/AgoraIO)
- 如果你想了解声网 SDK 在复杂场景下的应用，可以参考[官方场景案例](https://github.com/AgoraIO-usecase)
- 如果你想了解声网的一些社区开发者维护的项目，可以查看[社区](https://github.com/AgoraIO-Community)
- 如果你想了解声网的跨平台框架和云市场相关的项目，可以查看[AgoraIO-Extensions](https://github.com/AgoraIO-Extensions)
- 若遇到问题需要开发者帮助，你可以到[开发者社区](https://rtcdeveloper.com/)提问
- 如果需要售后技术支持, 你可以在[Agora Dashboard](https://dashboard.agora.io/)提交工单

## 代码许可

示例项目遵守 MIT 许可证。


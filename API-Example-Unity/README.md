# API-Example-Unity

*__其他语言版本：__  [__简体中文__](README.zh.md)*

## Overview

The API-Example-Unity project is an open-source demo that will show you different scenes on how to integrate Agora SDK APIs into your project.

Any scene of this project can run successfully alone.


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
    
    b. Download the ******Agora Video SDK****** from [Agora.io SDK](https://docs.agora.io/en/Video/downloads?platform=Unity). Unzip the downloaded SDK package and copy the files from ******samples/Hello-Video-Unity-Agora/Assets/AgoraEngine/****** in SDK to ******API-Example-Unity/Assets/****** in project

4.  Choose one of the scene that you want to run

5.  Fill your App ID into the scene on the game controller (attached on VideoCanvas most likely), like the following example:
    ![api-sample-appid](https://user-images.githubusercontent.com/1261195/89360166-652da380-d67c-11ea-9e67-1e02bbe94fc5.png)

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

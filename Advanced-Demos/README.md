# Agora Unity Video SDK Demos

This project contains sample code to demonstrate advanced features provided by the Agora Video SDK.
 

## Main scene view

![Advanced Demo Home](https://user-images.githubusercontent.com/1261195/113460520-ed13ab00-93cd-11eb-8084-0f5928f2f68f.png)

  
## Feature Note

  

 **1. App Screen Share**
     This demo shows how to share the screen (or rather "current app window") by recording the current camera view and push as an external video frame.  Applicable to mobile and desktop platforms.<br>  Remote user show see the exact same screen the host user is sharing below.
     ![appshare](https://user-images.githubusercontent.com/1261195/113460842-edf90c80-93ce-11eb-8dca-31abc948fc5d.png)
 **2. Desktop Screen Share**
     This demo shows how to share the desktop/window of the current running OS.  Applicable to desktop platforms only (Windows/MacOS). Note SDK calls for Windows are still under improvement.  **Sharing a specific window on Windows is not working**.  Developer should provide a native plugin to pass the window Id to the app.  <br>
     ![Desktop Share](https://user-images.githubusercontent.com/1261195/113460887-1c76e780-93cf-11eb-9101-e89b1e8ed89a.png)
 **3. Transcoding**
     The demo shows the configuration to publish live streaming video to known CDNs, including Youtube, Facebook and Twitch.<br> You need to set up your own CDN provider account and set the required information in order to have this to work.  The button on the UI leads you to the corresponding dashboard area for that provider.
     ![Screen Shot 2021-04-02 at 4 14 10 PM](https://user-images.githubusercontent.com/1261195/113460984-6eb80880-93cf-11eb-8388-d302dd2fe7a7.png)
 **4. 1-to-1 Call**
     This demo show simple video chat capability with option to choose Communication mode (default) or LiveStreaming as AudienceRole.  Its main purpose is to serve as a helper App to test other feature like the ones above. <br>
    


  

## Developer Environment Requirements

  

- Unity3d 2017 or above

-  [Agora Video SDK from Unity Asset Store](https://assetstore.unity.com/packages/tools/video/agora-video-chat-sdk-for-unity-134502)

- Real devices (Windows, Android, iOS, and Mac supported)

  (Note some feature may require SDK version 3.0.1 and higher.)

  

## Quick Start

  

This section shows you how to prepare, build, and run the sample application.

  

### Obtain an App ID

Before you can build and run the project, you will need to add your AppID to the configuration. Go to your [developer account’s project console](https://console.agora.io/projects), create a new AppId or copy the AppId from an existing project. 

Note it is important that for a production ready project, you should always use an AppId with certificate enabled.  However, in this simple quick start demo, we will skip this part.  So you AppId should be created for testing mode.
![enter image description here](https://user-images.githubusercontent.com/1261195/110023464-11eb0480-7ce2-11eb-99d6-031af60715ab.png)

  

### [](https://github.com/AgoraIO-Community/Unity-RTM#run-the-application)Run the Application

  

1.  First clone this repository

2. From Asset Store window in the Unity Editor, download and import the Agora Video SDK

3.  [Mac only] Obtain the Mac ScreenShare library plugin [here](https://bit.ly/2AIFyjK)

4. [Mac only] import the downloaded plugin from Asset->Import Package->Custom Package

5.  From Project window, open Asset/AgoraEngine/Demo+/Main.scene

6. Next go into your Hierarchy window and select  ****GameController****, in the Inspector add your  ****App ID****  to to the  ****AppID****  Input field

  

****Note****

The library from Step 3/4 is **non-official**.  You may build your own Mac library in case this doesn't work for you.  Source code gist can be found [here](https://gist.github.com/icywind/0fd26481dd6884821d7f917944ec0042).

#### [](https://github.com/AgoraIO-Community/Unity-RTM#test-in-editor)Test in Editor

  

1.  Go to  ****File****  >  ****Builds****  >  ****Platform****  and select either Windows or Mac depending on the device you are working on.

2. [Mac] make sure you fill in Camera and Microphone usage description

3. Press the Unity Play button to run the example scene

  

#### [](https://github.com/AgoraIO-Community/Unity-RTM#deploy-to-windows-mac-android)Deploy to Windows, Mac, iOS, Android

  

1.  Deploy to Mac, Android, and Windows by simply changing the Platform in the  ****File****  >  ****Build Settings****, then switch to your prefered platform

2.  [Mac or iOS] make sure you fill in Camera and Microphone usage description

3.  Hit  ****Build and Run****

  

## [](https://github.com/AgoraIO-Community/Unity-RTM#resources)Resources

  

- For potential issues, take a look at our  [FAQ](https://docs.agora.io/en/faq)  first

- Dive into  [Agora SDK Samples](https://github.com/AgoraIO)  to see more tutorials, including [API demos](https://github.com/AgoraIO/Agora-Unity-Quickstart/tree/master/API-Example-Unity)

- Take a look at  [Agora Use Case](https://github.com/AgoraIO-usecase)  for more complicated real use case

- Repositories managed by developer communities can be found at  [Agora Community](https://github.com/AgoraIO-Community)




## License
The MIT License (MIT).  [See doc.](../LICENSE.md)


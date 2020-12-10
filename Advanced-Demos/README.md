# Agora Unity Video SDK Demos

This project contains sample code to demonstrate advanced features provided by the Agora Video SDK.

  

## Main scene view

![Screen Shot 2020-07-07 at 2 28 35 PM](https://user-images.githubusercontent.com/1261195/90931055-b2b24c00-e3b0-11ea-9cd7-f159f51c96b0.png)

  
## Feature Note

  

 **1. App Screen Share**
     This demo shows how to share the screen (or rather "current app window") by recording the current camera view and push as an external video frame.  Applicable to mobile and desktop platforms.<br>
 **2. Desktop Screen Share**
     This demo shows how to share the desktop/window of the current running OS.  Applicable to desktop platforms only (Windows/MacOS). Note SDK calls for Windows are still under improvement.  Sharing specific window is not working.<br>
 **3. Transcoding**
     The demo shows the configuration to publish live streaming video to known CDNs, including Youtube, Facebook and Twitch.<br>
 **4. Inject Stream**
     This demo show the configuration to inject real time video source to the video chat/broadcast. <br>
    

With this sample app, you can:

  

## Developer Environment Requirements

  

- Unity3d 2017 or above

-  [Agora Video SDK from Unity Asset Store](https://assetstore.unity.com/packages/tools/video/agora-video-chat-sdk-for-unity-134502)

- Real devices (Windows, Android, iOS, and Mac supported)

  (Note some feature may require SDK version 3.0.1 and higher.)

  

## Quick Start

  

This section shows you how to prepare, build, and run the sample application.

  

### Obtain an App ID

  

To build and run the sample application, get an App ID:

  

1.  Create a developer account at  [agora.io](https://dashboard.agora.io/signin/). Once you finish the signup process, you will be redirected to the Dashboard.

2.  Navigate in Agora Console on the left to  ****Projects****  >  ****More****  >  ****Create****  >  ****Create New Project****.

3.  Save the  ****App ID****  from the Dashboard for later use.

  

  

### [](https://github.com/AgoraIO-Community/Unity-RTM#run-the-application)Run the Application

  

1.  First clone this repository

2. From Asset Store window in the Unity Editor, download and import the Agora Video SDK

3.  [Mac only] Obtain the Mac ScreenShare library plugin [here](https://bit.ly/2AIFyjK)

4. [Mac only] import the downloaded plugin from Asset->Import Package->Custom Package

5.  From Project window, open Asset/AgoraEngine/Demo+/SceneHome2.scene

6. Next go into your Hierarchy window and select  ****GameController****, in the Inspector add your  ****App ID****  to to the  ****AppID****  Input field

  

****Note****

The library from Step 3/4 is non-official.  You may build your own Mac library in case this doesn't work for you.  Source code gist can be found [here](https://gist.github.com/icywind/0fd26481dd6884821d7f917944ec0042).

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

  

The MIT License (MIT).

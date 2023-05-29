
# Screen sharing

For screen sharing during video calls or interactive live broadcasts, the screen content of the speaker or the host can be shared with other speakers or viewers in the form of video to improve communication efficiency.

Screen sharing is widely used in the following scenarios：

- In the video conference scenario, screen sharing can share the speaker's local files, data, web pages, PPT and other pictures to other participants;
- In the online classroom scenario, screen sharing can display the teacher's courseware, notes, lecture content and other pictures to the students.

Agora provides C# API for screen sharing since 4.0.0. This article describes how to use screen sharing on Android and iOS platforms using Unity SDK version 4.0.0 and later.

## Preconditions

Before use screen sharing, make sure you have implemented basic real-time audio and video functionality in your project. For details, see [Start audio and video call](https://docs.agora.io/en/video-call-4.x/start_call_unity_ng?platform=Unity) or [Start interactive live broadcast](https://docs.agora.io/en/live-streaming-premium-4.x/start_live_unity_ng?platform=Unity).

## Android Platform

When use screen sharing on the Android platform, you only need to call `startScreenCapture` to enable screen sharing. You can refer to [agora-unity-example](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/ScreenShare), in which folder `ScreenShare.cs` implements screen sharing.

## iOS Platform

- Due to system limitations, screen sharing only supports iOS 12.0 or later.
- This feature requires high device performance, and Agora recommends that you use it on iPhone X (Nov, 2017) and later models.

### Technical Principle

Screen sharing on the iOS side is achieved by recording the screen using the iOS native ReplayKit framework in the Extension, and then adding the screen sharing stream as a user to the channel. Since Apple does not support capturing the screen in the main app process, you need to create a separate Extension for the screen sharing flow.

![img](https://web-cdn.agora.io/docs-files/1649660342845)

### Implementation Steps

1. Use Unity Editor to build iOS, export Xcode project.

2. Go to your project folder and open the `unity-iphone/.xcodeproj` folder with Xcode.

3. Create a Broadcast Upload Extension to start the process of screen sharing:

   a. In Xcode, click **File > New > Target...**, select **Broadcast Upload Extension** in the pop-up window, and click **Next**.

   ![img](https://web-cdn.agora.io/docs-files/1606368184836)

   b. Fill in the **Product Name** and other information in the pop-up window, uncheck **Include UI Extension**, and click **Finish**. Xcode automatically creates a folder for this Extension, which contains the `SampleHandler.h` file.

   c. Select the newly created Extension under **Target**, click **General**, and set the iOS version to 12.0 or later under **Deployment Info**. Make sure the app and extension have the same TARGETS/Deployment/iOS version.
   ![img](https://web-cdn.agora.io/docs-files/1652254668249)

   d. Modify the `SampleHandler.h` file to modify the code logic that implements screen sharing:

   - If you only need to use the functions in `AgoraReplayKitExtension.framework` provided by Agora, the modification method is: select `Target` as the newly created Extension, and in **Info**, set **NSExtension > NSExtensionPrincipalClass** to **AgoraReplayKitHandler**.

     ![img](https://web-cdn.agora.io/docs-files/1648112619203)

   - If you also need to customize some business logic, the modification method is: replace the following code in the `SampleHandler.h` file:

     ```objectivec
       // Objective-C
       #import "SampleHandler.h"
       #import "AgoraReplayKitExt.h"
       #import <sys/time.h>
     
       @interface SampleHandler ()<AgoraReplayKitExtDelegate>
     
       @end
     
       @implementation SampleHandler
     
       - (void)broadcastStartedWithSetupInfo:(NSDictionary<NSString *,NSObject *> *)setupInfo {
           // User has requested to start the broadcast. Setup info from the UI extension can be supplied but optional.
           [[AgoraReplayKitExt shareInstance] start:self];
     
       }
     
       - (void)broadcastPaused {
           // User has requested to pause the broadcast. Samples will stop being delivered.
           NSLog(@"broadcastPaused");
           [[AgoraReplayKitExt shareInstance] pause];
       }
     
       - (void)broadcastResumed {
           // User has requested to resume the broadcast. Samples delivery will resume.
           NSLog(@"broadcastResumed");
           [[AgoraReplayKitExt shareInstance] resume];
     
       }
     
       - (void)broadcastFinished {
           // User has requested to finish the broadcast.
           NSLog(@"broadcastFinished");
           [[AgoraReplayKitExt shareInstance] stop];
     
       }
     
       - (void)processSampleBuffer:(CMSampleBufferRef)sampleBuffer withType:(RPSampleBufferType)sampleBufferType {
           [[AgoraReplayKitExt shareInstance] pushSampleBuffer:sampleBuffer withType:sampleBufferType];
       }
     
       #pragma mark - AgoraReplayKitExtDelegate
     
       - (void)broadcastFinished:(AgoraReplayKitExt *_Nonnull)broadcast reason:(AgoraReplayKitExtReason)reason {
           switch (reason) {
               case AgoraReplayKitExtReasonInitiativeStop:
                   {
       //                NSDictionary *userInfo = @{NSLocalizedDescriptionKey : @"Host app stop srceen capture"};
       //                NSError *error = [NSError errorWithDomain:NSCocoaErrorDomain code:0 userInfo:userInfo];
       //                [self finishBroadcastWithError:error];
                       NSLog(@"AgoraReplayKitExtReasonInitiativeStop");
                   }
                   break;
               case AgoraReplayKitExtReasonConnectFail:
                   {
       //                NSDictionary *userInfo = @{NSLocalizedDescriptionKey : @"Connect host app fail need startScreenCapture in host app"};
       //                NSError *error = [NSError errorWithDomain:NSCocoaErrorDomain code:0 userInfo:userInfo];
       //                [self finishBroadcastWithError:error];
                       NSLog(@"AgoraReplayKitExReasonConnectFail");
                   }
                   break;
     
               case AgoraReplayKitExtReasonDisconnect:
                   {
       //                NSDictionary *userInfo = @{NSLocalizedDescriptionKey : @"disconnect with host app"};
       //                NSError *error = [NSError errorWithDomain:NSCocoaErrorDomain code:0 userInfo:userInfo];
       //               [self finishBroadcastWithError:error];
                       NSLog(@"AgoraReplayKitExReasonDisconnect");
                   }
                   break;
               default:
                   break;
           }
       }
     
       @end
     ```

4. Select the Extension you created in **TARGETS**, and add all frameworks under the **Frameworks/Agora-RTC-Plugin/Agora-Unity-RTC-SDK/Plugins/iOS/** path in **General/Frameworks and Libraries**.
![Unity-iPhone_—_Unity-iPhone_xcodeproj](https://user-images.githubusercontent.com/1261195/231263461-9ef70e04-798e-47f2-95ce-003b91bcf3ce.jpg)
5. Call `startScreenCapture` in Unity C#, combined with the user's [manual action](#manual-actions), to enable the app to start screen sharing.

#### Development Considerations

- Make sure the app and extension have the same TARGETS/Deployment/iOS version.

- The Broadcast Upload Extension's memory usage is limited to 50 MB, please make sure that the extension's memory usage for screen sharing does not exceed 50 MB.
- In the process of screen sharing, you need to call the `muteAllRemoteVideoStreams` and `muteAllRemoteAudioStreams` methods to cancel receiving streams from remote users to avoid repeated subscriptions.

## Manual Actions
Taking [the ShareScreen demo](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/main/API-Example-Unity/Assets/API-Example/Examples/Advanced/ScreenShare) as example, the steps to properly share the screen on iOS should follow the steps:
1. The order of buttons press should be: Join Channel -> Start Sharing -> Start Publish
![ss_step1](https://user-images.githubusercontent.com/1261195/231244602-0609459e-aa40-4d0a-a816-7607a15b449f.jpg)
2. You should see StartScreenCapture call return 0.  Here you may see a white box, since ScreenRecording has not started.
![ss_step2](https://user-images.githubusercontent.com/1261195/231244623-092d3d71-e198-477c-901a-fd362f2e08cc.jpg)
3. Access the iOS quick control panel, long press the Screen Recording button
![ss_step3](https://user-images.githubusercontent.com/1261195/231244635-ef61fa63-a1b9-4dd7-9c51-fc9e991c8528.jpg)
4. Select the App, in this example, we named it as "AgoraIOSScreenSharing".  And press "Start Broadcast"
![ss_step4](https://user-images.githubusercontent.com/1261195/231246651-b1c02bc2-bd46-4191-b053-79507aedcb10.jpg)
5. The "Screen Recording" title should be shown:
![ss_step5](https://user-images.githubusercontent.com/1261195/231244674-351d667a-7b69-44a1-8901-020df4b6136d.jpg)
6. Switch back into the App, you should see the local view now is showing the screen.
![IMG_0061](https://user-images.githubusercontent.com/1261195/231244714-3ae3b7e8-e964-4075-926a-ed23fff17ade.PNG)

 



## API Reference

There are currently some usage restrictions and precautions for the screen sharing function, and there could be future changes. Agora recommends that you read the following API reference before calling the API:

- [`startScreenCapture`](https://docs-preprod.agora.io/en/live-streaming-standard-4.x/API%20Reference/unity_ng/API/class_irtcengine.html#api_irtcengine_startscreencapture)
- [`stopScreenCapture`](https://docs-preprod.agora.io/en/live-streaming-standard-4.x/API%20Reference/unity_ng/API/class_irtcengine.html#api_irtcengine_stopscreencapture)
- [`updateScreenCaptureParameters`](https://docs-preprod.agora.io/en/live-streaming-standard-4.x/API%20Reference/unity_ng/API/class_irtcengine.html#api_irtcengine_updatescreencaptureparameters)

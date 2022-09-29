# Screen sharing

For screen sharing during video calls or interactive live broadcasts, the screen content of the speaker or the host can be shared with other speakers or viewers in the form of video to improve communication efficiency.

Screen sharing is widely used in the following scenarios：

- In the video conference scenario, screen sharing can share the speaker's local files, data, web pages, PPT and other pictures to other participants;
- In the online classroom scenario, screen sharing can display the teacher's courseware, notes, lecture content and other pictures to the students.

Agora provides C# API for screen sharing since 4.0.0. This article describes how to use screen sharing on Android and iOS platforms using Unity SDK version 4.0.0 and later.

## Preconditions

Before use screen sharing, make sure you have implemented basic real-time audio and video functionality in your project. For details, see [Start audio and video call](https://docs.agora.io/en/video-call-4.x/start_call_unity_ng?platform=Unity) or [Start interactive live broadcast](https://docs.agora.io/en/live-streaming-premium-4.x/start_live_unity_ng?platform=Unity).

## Android Platform

When use screen sharing on the Android platform, you only need to call `startScreenCapture` to enable screen sharing. You can refer to [agora-unity-example](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/release/4.0.0/API-Example-Unity/Assets/API-Example/Examples/Advanced/ScreenShare) `ScreenShare.cs` implements screen sharing.

## iOS Platform

- Due to system limitations, screen sharing only supports iOS 12.0 or later.
- This feature requires high device performance, and Agora recommends that you use it on iPhone X and later models.

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

   - If you only need to use the functions in `AgoraReplayKitExtension.framework` provided by Agora, the modification method is: select `Target` as the newly created Extension, and in **Info**, set **NSExtension > NSExtensionPrincipalClass** corresponding* *Value** changed from **SampleHandler** to **AgoraReplayKitHandler**.

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

4. Select the Extension you created in **TARGETS**, add **Frameworks/Agora-RTC-Plugin/Agora-Unity-RTC-SDK/Plugins/iOS/** path in **General/Frameworks and Libraries** Download all frameworks.

5. Call `startScreenCapture`, combined with the user's manual action, to enable the app to start screen sharing.

   - Method 1: Prompt the user to long press the **Screen Recording** button in the iOS control center, and choose to use the extension you created to start recording.

   

### Example Project

Agora in [agora-unity-example](https://github.com/AgoraIO-Extensions/Agora-Unity-Quickstart/tree/release/4.0.0/API-Example-Unity/Assets/API-Example/Examples/Advanced/ScreenShare) provides examples of screen sharing, you can refer to the following files to achieve screen sharing:

- `ScreenShare.cs`

### Development Considerations

- Make sure the app and extension have the same TARGETS/Deployment/iOS version.

- The Broadcast Upload Extension's memory usage is limited to 50 MB, please make sure that the extension's memory usage for screen sharing does not exceed 50 MB.
- In the process of screen sharing, you need to call the `muteAllRemoteVideoStreams` and `muteAllRemoteAudioStreams` methods to cancel receiving streams from remote users to avoid repeated subscriptions.

## API Reference

There are currently some usage restrictions and precautions for the screen sharing function, and there will be charges. Agora recommends that you read the following API reference before calling the API:

- [`startScreenCapture`]()
- [`stopScreenCapture`]()
- [`updateScreenCaptureParameters`]()
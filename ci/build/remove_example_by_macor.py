import sys
import os
import re
import shutil

number_example_list = [
    "API-Example/Examples/Basic/joinChannelAudio",
    "API-Example/Examples/Basic/JoinChannelVideo",
    "API-Example/Examples/Advanced/AudioMixing",
    "API-Example/Examples/Advanced/AudioSpectrum",
    "API-Example/Examples/Advanced/ChannelMediaRelay",
    "API-Example/Examples/Advanced/ContentInspect",
    "API-Example/Examples/Advanced/CustomCaptureAudio",
    "API-Example/Examples/Advanced/CustomCaptureVideo",
    "API-Example/Examples/Advanced/CustomRenderAudio",
    "API-Example/Examples/Advanced/DeviceManager",
    "API-Example/Examples/Advanced/DualCamera",
    "API-Example/Examples/Advanced/JoinChannelVideoToken",
    "API-Example/Examples/Advanced/JoinChannelWithUserAccount",
    "API-Example/Examples/Advanced/MediaPlayer",
    "API-Example/Examples/Advanced/MediaPlayerWithCustomDataProvider",
    "API-Example/Examples/Advanced/MediaRecorder",
    "API-Example/Examples/Advanced/Metadata",
    "API-Example/Examples/Advanced/MusicPlayer",
    "API-Example/Examples/Advanced/Plugin",
    "API-Example/Examples/Advanced/ProcessAudioRawData",
    "API-Example/Examples/Advanced/ProcessVideoRawData",
    "API-Example/Examples/Advanced/PushEncodedVideoImage",
    "API-Example/Examples/Advanced/RenderWithYUV",
    "API-Example/Examples/Advanced/ScreenShare",
    "API-Example/Examples/Advanced/ScreenShareWhileVideoCall",
    "API-Example/Examples/Advanced/SetBeautyEffectOptions",
    "API-Example/Examples/Advanced/SetEncryption",
    "API-Example/Examples/Advanced/SetVideoEncodeConfiguration",
    "API-Example/Examples/Advanced/SpatialAudioWithMediaPlayer",
    "API-Example/Examples/Advanced/SpatialAudioWithUsers",
    "API-Example/Examples/Advanced/StartDirectCdnStreaming",
    "API-Example/Examples/Advanced/StartLocalVideoTranscoder",
    "API-Example/Examples/Advanced/StartRhythmPlayer",
    "API-Example/Examples/Advanced/StartRtmpStreamWithTranscoding",
    "API-Example/Examples/Advanced/StreamMessage",
    "API-Example/Examples/Advanced/TakeSnapshot",
    "API-Example/Examples/Advanced/VirtualBackground",
    "API-Example/Examples/Advanced/VoiceChanger",
    "API-Example/Examples/Advanced/WriteBackVideoRawData",
]

string_example_list = [
    "API-Example/Examples/Basic/joinChannelAudioS",
    "API-Example/Examples/Basic/JoinChannelVideoS",
    "API-Example/Examples/Advanced/AudioMixingS",
    "API-Example/Examples/Advanced/AudioSpectrumS",
    "API-Example/Examples/Advanced/ChannelMediaRelayS",
    "API-Example/Examples/Advanced/ContentInspectS",
    "API-Example/Examples/Advanced/CustomCaptureAudioS",
    "API-Example/Examples/Advanced/CustomCaptureVideoS",
    "API-Example/Examples/Advanced/CustomRenderAudioS",
    "API-Example/Examples/Advanced/DeviceManagerS",
    "API-Example/Examples/Advanced/DualCameraS",
    "API-Example/Examples/Advanced/JoinChannelVideoTokenS",
    "API-Example/Examples/Advanced/MediaPlayerS",
    "API-Example/Examples/Advanced/MediaPlayerWithCustomDataProviderS",
    "API-Example/Examples/Advanced/MediaRecorderS",
    "API-Example/Examples/Advanced/MetadataS",
    "API-Example/Examples/Advanced/MusicPlayerS",
    "API-Example/Examples/Advanced/PluginS",
    "API-Example/Examples/Advanced/ProcessAudioRawDataS",
    "API-Example/Examples/Advanced/ProcessVideoRawDataS",
    "API-Example/Examples/Advanced/PushEncodedVideoImageS",
    "API-Example/Examples/Advanced/RenderWithYUVS",
    "API-Example/Examples/Advanced/ScreenShareS",
    "API-Example/Examples/Advanced/ScreenShareWhileVideoCallS",
    "API-Example/Examples/Advanced/SetBeautyEffectOptionsS",
    "API-Example/Examples/Advanced/SetEncryptionS",
    "API-Example/Examples/Advanced/SetVideoEncodeConfigurationS",
    "API-Example/Examples/Advanced/SpatialAudioWithMediaPlayerS",
    "API-Example/Examples/Advanced/SpatialAudioWithUsersS",
    "API-Example/Examples/Advanced/StartDirectCdnStreamingS",
    "API-Example/Examples/Advanced/StartLocalVideoTranscoderS",
    "API-Example/Examples/Advanced/StartRhythmPlayerS",
    "API-Example/Examples/Advanced/StartRtmpStreamWithTranscodingS",
    "API-Example/Examples/Advanced/StreamMessageS",
    "API-Example/Examples/Advanced/TakeSnapshotS",
    "API-Example/Examples/Advanced/VirtualBackgroundS",
    "API-Example/Examples/Advanced/VoiceChangerS",
    "API-Example/Examples/Advanced/WriteBackVideoRawDataS"
]

video_example_list = [
    {'dir_name': 'API-Example/Examples/Basic/JoinChannelVideo', 'scene_name': 'BasicVideoCallScene'},
    {'dir_name': 'API-Example/Examples/Basic/JoinChannelVideoS', 'scene_name': 'BasicVideoCallSceneS'},
    {'dir_name': 'API-Example/Examples/Advanced/ContentInspect', 'scene_name': 'ContentInspectScene'},
    {'dir_name': 'API-Example/Examples/Advanced/ContentInspectS', 'scene_name': 'ContentInspectSceneS'},
    {'dir_name': 'API-Example/Examples/Advanced/CustomCaptureVideo', 'scene_name': 'CustomCaptureVideoScene'},
    {'dir_name': 'API-Example/Examples/Advanced/CustomCaptureVideoS', 'scene_name': 'CustomCaptureVideoSceneS'},
    {'dir_name': 'API-Example/Examples/Advanced/DualCamera', 'scene_name': 'DualCameraScene'},
    {'dir_name': 'API-Example/Examples/Advanced/DualCameraS', 'scene_name': 'DualCameraSceneS'},
    {'dir_name': 'API-Example/Examples/Advanced/ProcessVideoRawData', 'scene_name': 'ProcessVideoRawDataScene'},
    {'dir_name': 'API-Example/Examples/Advanced/ProcessVideoRawDataS', 'scene_name': 'ProcessVideoRawDataSceneS'},
    {'dir_name': 'API-Example/Examples/Advanced/PushEncodedVideoImage', 'scene_name': 'PushEncodedVideoImageScene'},
    {'dir_name': 'API-Example/Examples/Advanced/PushEncodedVideoImageS', 'scene_name': 'PushEncodedVideoImageSceneS'},
    {'dir_name': 'API-Example/Examples/Advanced/RenderWithYUV', 'scene_name': 'RenderWithYUV'},
    {'dir_name': 'API-Example/Examples/Advanced/RenderWithYUVS', 'scene_name': 'RenderWithYUVS'},
    {'dir_name': 'API-Example/Examples/Advanced/ScreenShare', 'scene_name': 'ScreenShareScene'},
    {'dir_name': 'API-Example/Examples/Advanced/ScreenShareS', 'scene_name': 'ScreenShareSceneS'},
    {'dir_name': 'API-Example/Examples/Advanced/ScreenShareWhileVideoCall',
     'scene_name': 'ScreenShareWhileVideoCallScene'},
    {'dir_name': 'API-Example/Examples/Advanced/ScreenShareWhileVideoCallS',
     'scene_name': 'ScreenShareWhileVideoCallSceneS'},
    {'dir_name': 'API-Example/Examples/Advanced/SetBeautyEffectOptions', 'scene_name': 'SetBeautyEffectOptionsScene'},
    {'dir_name': 'API-Example/Examples/Advanced/SetBeautyEffectOptionsS', 'scene_name': 'SetBeautyEffectOptionsSceneS'},
    {'dir_name': 'API-Example/Examples/Advanced/SetVideoEncodeConfiguration',
     'scene_name': 'SetVideoEncodeConfigurationScene'},
    {'dir_name': 'API-Example/Examples/Advanced/SetVideoEncodeConfigurationS',
     'scene_name': 'SetVideoEncodeConfigurationSceneS'},
    {'dir_name': 'API-Example/Examples/Advanced/TakeSnapshot', 'scene_name': 'TakeSnapshotScene'},
    {'dir_name': 'API-Example/Examples/Advanced/TakeSnapshotS', 'scene_name': 'TakeSnapshotSceneS'},
    {'dir_name': 'API-Example/Examples/Advanced/VirtualBackground', 'scene_name': 'VirtualBackgroundScene'},
    {'dir_name': 'API-Example/Examples/Advanced/VirtualBackgroundS', 'scene_name': 'VirtualBackgroundSceneS'},
    {'dir_name': 'API-Example/Examples/Advanced/WriteBackVideoRawData', 'scene_name': 'WriteBackVideoRawDataScene'},
    {'dir_name': 'API-Example/Examples/Advanced/WriteBackVideoRawDataS', 'scene_name': 'WriteBackVideoRawDataSceneS'},
]
# xxx/xxxx/xxxx/Assets
assets_root = sys.argv[1]
RTC = sys.argv[2]
RTM = sys.argv[3]
NUMBER_UID = sys.argv[4]
STRING_UID = sys.argv[5]
FULL = sys.argv[6]
VOICE = sys.argv[7]
android_studio_temple = os.path.join(
    assets_root, "../../android_studio_template")

print('remove example by macor.py {0},{1},{2},{3},{4},{5},{6}'.format(
    assets_root, RTC, RTM, NUMBER_UID, STRING_UID, FULL, VOICE))


def get_all_files(target_dir):
    files = []
    list_files = os.listdir(target_dir)
    for i in range(0, len(list_files)):
        each_file = os.path.join(target_dir, list_files[i])
        if os.path.isdir(each_file):
            files.extend(get_all_files(each_file))
        elif os.path.isfile(each_file):
            files.append(each_file)
    return files


def remove_key_word_in_path(file_path, key_word, suffix=".cs"):
    files = get_all_files(file_path)
    for i in range(0, len(files)):
        file_name = files[i]
        if not file_name.endswith(suffix):
            continue
        f = open(file_name, 'r', encoding='UTF-8')
        content = f.read()
        f.close()
        content = content.replace(key_word, '')
        f = open(file_name, 'w')
        f.write(content)
        f.close()


def replace_key_word_in_file(file_name, key_word, replace_word):
    f = open(file_name, 'r', encoding='UTF-8')
    content = f.read()
    f.close()
    content = content.replace(key_word, replace_word)
    f = open(file_name, 'w')
    f.write(content)
    f.close()


# remove video example without full
if FULL == 'false':
    home_cs_file = open(os.path.join(assets_root, "API-Example/Home.cs"))
    home_cs_string = home_cs_file.read()
    home_cs_file.close()
    length = len(video_example_list)
    for i in range(length):
        e = video_example_list[i]
        # delete case files
        dir_name = e['dir_name']
        full_path = os.path.join(assets_root, dir_name)
        print("remove ing :" + full_path)
        if os.path.exists(full_path):
            shutil.rmtree(full_path)
            os.remove(full_path + ".meta")
        else:
            print(full_path + " not exists")

        # remove scene name from home.cs
        scene_name = e['scene_name']
        pa = re.compile(r'"' + scene_name + r'",{0,1}')
        home_cs_string = pa.sub("", home_cs_string)

    home_cs_file = open(os.path.join(assets_root, "API-Example/Home.cs"), 'w')
    home_cs_file.write(home_cs_string)
    home_cs_file.close()

## remove api example with rtc
if RTC == 'false':
    if os.path.isdir(assets_root + "/API-Example/Examples/Basic"):
        shutil.rmtree(assets_root + "/API-Example/Examples/Basic")
    if os.path.isdir(assets_root + "/API-Example/Examples/Advanced"):
        shutil.rmtree(assets_root + "/API-Example/Examples/Advanced")
    if os.path.isdir(assets_root + "/API-Example/Tools"):
        shutil.rmtree(assets_root + "/API-Example/Tools")

## remove api example with rtm
if RTM == 'false' and os.path.isdir(assets_root + '/API-Example/Examples/Rtm'):
    shutil.rmtree(assets_root + '/API-Example/Examples/Rtm')

# remove int example without int uid
if NUMBER_UID == 'false':
    for i in range(0, len(number_example_list)):
        full_path = os.path.join(assets_root, number_example_list[i])
        if os.path.exists(full_path):
            shutil.rmtree(full_path)

# remove string example without string uid
if STRING_UID == 'false':
    for i in range(0, len(string_example_list)):
        full_path = os.path.join(assets_root, string_example_list[i])
        if os.path.exists(full_path):
            shutil.rmtree(full_path)

## remove define without rtc
if RTC == 'false':
    remove_key_word_in_path(assets_root, '#define AGORA_RTC')

## remove define without rtm
if RTM == 'false':
    remove_key_word_in_path(assets_root, '#define AGORA_RTM')

## remove define without int uid
if NUMBER_UID == 'false':
    remove_key_word_in_path(assets_root, '#define AGORA_NUMBER_UID')

## remove define without string uid
if STRING_UID == 'false':
    remove_key_word_in_path(assets_root, '#define AGORA_STRING_UID')

## remove define without full
if FULL == 'false':
    remove_key_word_in_path(assets_root, '#define AGORA_FULL')

## remove define without voice
if VOICE == 'false':
    remove_key_word_in_path(assets_root, '#define AGORA_VOICE')

# android
if RTC == 'false':
    remove_key_word_in_path(android_studio_temple, "include 'unityLibrary:AgoraRtcEngineKit.plugin'", ".gradle")
    remove_key_word_in_path(android_studio_temple, "implementation project('AgoraRtcEngineKit.plugin')", ".gradle")

if RTM == 'false':
    remove_key_word_in_path(android_studio_temple, "include 'unityLibrary:AgoraRtmEngineKit.plugin'", ".gradle")
    remove_key_word_in_path(android_studio_temple, "implementation project('AgoraRtmEngineKit.plugin')", ".gradle")

# change meta file with out rtc. So rtm sdk can both with rtc sdk in the same unity project
if RTC == "false":
    os.remove(os.path.join(assets_root, "API-Example/Prefab.meta"))
    os.remove(os.path.join(assets_root, "API-Example.meta"))
    os.remove(os.path.join(assets_root, "API-Example/Examples.meta"))
    os.remove(os.path.join(assets_root, "API-Example/Editor.meta"))
    os.remove(os.path.join(assets_root, "API-Example/AppIdInput.meta"))

    os.rename(os.path.join(assets_root, "API-Example/HomeScene.unity"),
              os.path.join(assets_root, "API-Example/RtmHomeScene.unity"))
    os.rename(os.path.join(assets_root, "API-Example/HomeScene.unity.meta"),
              os.path.join(assets_root, "API-Example/RtmHomeScene.unity.meta"))
    os.rename(os.path.join(assets_root, "API-Example/Home.cs"),
              os.path.join(assets_root, "API-Example/RtmHome.cs"))
    os.rename(os.path.join(assets_root, "API-Example/Home.cs.meta"),
              os.path.join(assets_root, "API-Example/RtmHome.cs.meta"))
    replace_key_word_in_file(os.path.join(assets_root, "API-Example/RtmHome.cs"), "public class Home",
                             "public class RtmHome")

    os.rename(os.path.join(assets_root, "API-Example/Editor/CommandBuild.cs.meta"),
              os.path.join(assets_root, "API-Example/Editor/RtmCommandBuild.cs.meta"))
    os.rename(os.path.join(assets_root, "API-Example/Editor/CommandBuild.cs"),
              os.path.join(assets_root, "API-Example/Editor/RtmCommandBuild.cs"))
    replace_key_word_in_file(os.path.join(assets_root, "API-Example/Editor/RtmCommandBuild.cs"), "#define AGORA_RTC",
                             "")
    replace_key_word_in_file(os.path.join(assets_root, "API-Example/Editor/RtmCommandBuild.cs"),
                             "public class CommandBuild", "public class RtmCommandBuild")

    replace_key_word_in_file(os.path.join(assets_root, "API-Example/AppIdInput/AppIdInput.cs"),
                             "namespace io.agora.rtc.demo", "namespace io.agora.rtm.demo")

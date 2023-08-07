import sys
import os
import re
import shutil

# xxxx/xxxx/Assets
assets_root = sys.argv[1]
RTC = sys.argv[2]
RTM = sys.argv[3]
android_studio_temple = os.path.join(
    assets_root, "../../android_studio_template")

print('remove example by macor.py {0},{1},{2},{3}'.format(
    assets_root, RTC, RTM, android_studio_temple))


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
        if file_name.endswith(suffix) == False:
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


if RTC == 'false':
    if os.path.isdir(assets_root + "/API-Example/Examples/Basic"):
        shutil.rmtree(assets_root + "/API-Example/Examples/Basic")
    if os.path.isdir(assets_root + "/API-Example/Examples/Advanced"):
        shutil.rmtree(assets_root + "/API-Example/Examples/Advanced")
    if os.path.isdir(assets_root + "/API-Example/Tools"):
        shutil.rmtree(assets_root + "/API-Example/Tools")

if RTM == 'false' and os.path.isdir(assets_root + '/API-Example/Examples/Rtm'):
    shutil.rmtree(assets_root + '/API-Example/Examples/Rtm')

if RTC == 'false':
    remove_key_word_in_path(assets_root, '#define AGORA_RTC')

if RTM == 'false':
    remove_key_word_in_path(assets_root, '#define AGORA_RTM')

if RTC == 'false':
    remove_key_word_in_path(os.path.join(android_studio_temple, "launcher"),
                            "implementation files('../unityLibrary/libs/AgoraScreenShareExtension.aar')", ".gradle")

if RTC == "false":
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
    replace_key_word_in_file(os.path.join(assets_root, "API-Example/RtmHome.cs"),"public class Home","public class RtmHome")
    
    os.rename(os.path.join(assets_root, "API-Example/Editor/CommandBuild.cs.meta"),
              os.path.join(assets_root, "API-Example/Editor/RtmCommandBuild.cs.meta"))
    os.rename(os.path.join(assets_root, "API-Example/Editor/CommandBuild.cs"),
              os.path.join(assets_root, "API-Example/Editor/RtmCommandBuild.cs"))
    replace_key_word_in_file(os.path.join(assets_root, "API-Example/Editor/RtmCommandBuild.cs"),"#define AGORA_RTC","")
    replace_key_word_in_file(os.path.join(assets_root, "API-Example/Editor/RtmCommandBuild.cs"),"public class CommandBuild","public class RtmCommandBuild")
   

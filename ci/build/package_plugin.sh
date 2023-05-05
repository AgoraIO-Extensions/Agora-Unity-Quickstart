Plugin_Url=$1
WORKSPACE=$2
cd build_temp
python3 ${WORKSPACE}/artifactory_utils.py --action=download_file --file=$Plugin_Url
unzip -d ./ ./VideoObserver_Plugin_for_All_*.zip
ls ./

RTCPluginPath=../API-Example-Unity/Assets/Agora-RTC-Plugin/Agora-Unity-RTC-SDK/Plugins

#Android
AndroidLibPath=${RTCPluginPath}/Android/AgoraRtcEngineKit.plugin/lib
mv ./VideoObserver_Plugin_for_Android/Release/arm64-v8a/*.so ${AndroidLibPath}/arm64-v8a
mv ./VideoObserver_Plugin_for_Android/Release/armeabi-v7a/*.so ${AndroidLibPath}/armeabi-v7a
mv ./VideoObserver_Plugin_for_Android/Release/x86/*.so ${AndroidLibPath}/x86
mv ./VideoObserver_Plugin_for_Android/Release/x86_64/*.so ${AndroidLibPath}/x86_64

#iOS
mv ./VideoObserver_Plugin_for_iOS/AgoraRawDataPlugin.framework ${RTCPluginPath}/iOS

#x86
mv ./VideoObserver_Plugin_for_Windows/Win32/*.dll ${RTCPluginPath}/x86

#x86_64
mv ./VideoObserver_Plugin_for_Windows/x64/*.dll ${RTCPluginPath}/x86_64

#mac
mv ./VideoObserver_Plugin_for_macOS/*.bundle ${RTCPluginPath}/macOS

#meta file
cp -rf ../ci/RawDataPlugins/* ${RTCPluginPath}

#cs file
sed -i "" "s/\/\/#define USE_PLUGIN/#define USE_PLUGIN/g" ../API-Example-Unity/Assets/API-Example/Examples/Advanced/Plugin/PluginSceneSample.cs

cd ..
Plugin_Url=$1
WORKSPACE=$2
cd build_temp
python3 ${WORKSPACE}/artifactory_utils.py --action=download_file --file=$Plugin_Url || exit 1
unzip -d ./ ./VideoObserver_Plugin_*.zip || exit 1
ls ./

RTCPluginPath=../API-Example-Unity/Assets/Agora-RTC-Plugin/Agora-Unity-RTC-SDK/Plugins

#Android
AndroidLibPath=${RTCPluginPath}/Android/AgoraRtcEngineKit.plugin/lib
cp ./VideoObserver_Plugin_for_Android/Release/arm64-v8a/*.so ${AndroidLibPath}/arm64-v8a || exit 1
cp ./VideoObserver_Plugin_for_Android/Release/armeabi-v7a/*.so ${AndroidLibPath}/armeabi-v7a || exit 1
cp ./VideoObserver_Plugin_for_Android/Release/x86/*.so ${AndroidLibPath}/x86 || exit 1
cp ./VideoObserver_Plugin_for_Android/Release/x86_64/*.so ${AndroidLibPath}/x86_64 || exit 1

#iOS
cp ./VideoObserver_Plugin_for_iOS/AgoraRawDataPlugin.framework ${RTCPluginPath}/iOS || exit 1

#x86
cp ./VideoObserver_Plugin_for_Windows/Win32/*.dll ${RTCPluginPath}/x86 || exit 1

#x86_64
cp ./VideoObserver_Plugin_for_Windows/x64/*.dll ${RTCPluginPath}/x86_64 || exit 1

#mac
cp ./VideoObserver_Plugin_for_macOS/*.bundle ${RTCPluginPath}/macOS || exit 1

#meta file
cp -rf ../ci/RawDataPlugins/* ${RTCPluginPath} || exit 1

#cs file
sed -i "" "s/\/\/#define USE_PLUGIN/#define USE_PLUGIN/g" ../API-Example-Unity/Assets/API-Example/Examples/Advanced/Plugin/PluginSceneSample.cs || exit 1

cd ..

exit 0
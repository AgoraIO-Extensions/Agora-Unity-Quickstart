/Applications/Unity/Hub/Editor/2020.3.30f1c1/Unity.app/Contents/MacOS/Unity -quit -batchmode -nographics -projectPath "./API-Example-Unity" -executeMethod Agora_RTC_Plugin.API_Example.CommandBuild.BuildAndrod
cp -r ./android_studio_template/. ./Build/android_studio
cd ./Build/android_studio
./gradlew assembleRelease

if [ -f ./launcher/build/outputs/apk/release/*.apk ];then
  echo "android apk 导出成功 Build文件夹下"
  mv ./launcher/build/outputs/apk/release/*.apk ../Android.apk
else
  echo "android apk 导出失败了,android_Studio工程放在Build文件夹下"
fi


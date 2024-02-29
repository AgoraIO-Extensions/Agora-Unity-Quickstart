echo `pwd` 
WORKSPACE=$1
RTC=$2
RTM=$3

if [ "$RTC" == "true" ]; then
    BUILD_PATH="Build"
else 
    BUILD_PATH="RtmBuild"
fi

cp -r ./android_studio_template/. ./$BUILD_PATH/android_studio
cd ./$BUILD_PATH/android_studio
./gradlew assembleRelease

if [ -f ./launcher/build/outputs/apk/release/*.apk ];then
  echo "android apk 导出成功 $BUILD_PATH 文件夹下"
  mv ./launcher/build/outputs/apk/release/*.apk ../Android.apk
else
  echo "android apk 导出失败了,android_Studio工程放在$BUILD_PATH文件夹下"
fi

cd ../../
rm -rf ./$BUILD_PATH/android_studio

echo `pwd` 
WORKSPACE=$1

cp -r ./android_studio_template/. ./Build/android_studio
cd ./Build/android_studio
./gradlew assembleRelease

if [ -f ./launcher/build/outputs/apk/release/*.apk ];then
  echo "android apk 导出成功 Build文件夹下"
  mv ./launcher/build/outputs/apk/release/*.apk ../Android.apk
else
  echo "android apk 导出失败了,android_Studio工程放在Build文件夹下"
fi

cd ../../
rm -rf ./Build/android_studio
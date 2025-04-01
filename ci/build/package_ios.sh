echo $(pwd)

WORKSPACE=$1

# Project名称
PROJECT_NAME=Unity-iPhone
## Scheme名
SCHEME_NAME=Unity-iPhone
## 编译类型 Debug/Release二选一
BUILD_TYPE=Release
## 项目根路径，xcodeproj/xcworkspace所在路径
PROJECT_ROOT_PATH=./Build/IPhone
## 打包生成路径
PRODUCT_PATH=./Build

BUILD_TYPE=Release

## project路径
PROJECT_PATH=${PROJECT_ROOT_PATH}/${PROJECT_NAME}.xcodeproj

### 编译打包过程 ###
echo "============Build Clean Begin============"
## 清理缓存
xcodebuild clean -project ${PROJECT_PATH} -scheme ${SCHEME_NAME} -configuration ${BUILD_TYPE} -quiet || exit
echo "============Build Clean End============"

##xcarchive文件的存放路径
ARCHIVE_PATH="./build_temp/xcarchive_${x}/${SCHEME_NAME}_xcarchive.xcarchive"

# 导出archive包
xcodebuild CODE_SIGN_STYLE="Manual" \
    -project ${PROJECT_PATH} \
    -configuration ${BUILD_TYPE} \
    -scheme ${SCHEME_NAME} clean CODE_SIGNING_REQUIRED=NO CODE_SIGNING_ALLOWED=NO archive -archivePath $ARCHIVE_PATH -quiet -destination 'generic/platform=iOS' || exit
echo "============Build Archive Success============"

## 导出IPA原始的包
echo "============Export IPA Begin============"
xcodebuild -exportArchive -archivePath $ARCHIVE_PATH -exportPath ./Build CODE_SIGNING_REQUIRED=NO CODE_SIGNING_ALLOWED=NO -exportOptionsPlist "./ci/ExportOptions/ExportOptions.plist" -quiet || exit
echo "============Export IPA Success============"

# 给ipa包签名
echo "============Sign IPA Begin============"
sh ${WORKSPACE}/sign ./Build/unityexample.ipa

ls ./Build
# ls ${WORKSPACE}

ls .
mv ./unityexample_*.ipa ./Build/

echo "============Sign IPA Sucess============"

echo "============Clear IPA Begin============"
rm -f ./Build/*.plist
rm -f ./Build/*.log
rm -f ./Build/unityexample.ipa
echo "============Clear IPA End============"

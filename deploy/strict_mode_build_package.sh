#!/bin/bash

echo "Start Build for appsflyer-unity-plugin.unitypackage. Strict Mode."


 DEPLOY_PATH=outputs
 UNITY_PATH="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
 PACKAGE_NAME="appsflyer-unity-plugin-strict-mode-6.2.62.unitypackage"
 mkdir -p $DEPLOY_PATH

echo "Changing AppsFlyerFramework to Strict Mode"
sed -i '' 's/AppsFlyerFramework/AppsFlyerFramework\/Strict/g' ../Assets/AppsFlyer/Editor/AppsFlyerDependencies.xml
echo "Changing AppsFlyerFramework to Strict Mode. Done."


 # Build the .unitypackage
 /Applications/Unity/Hub/Editor/2019.4.19f1/Unity.app/Contents/MacOS/Unity \
 -gvh_disable \
 -batchmode \
 -importPackage external-dependency-manager-1.2.144.unitypackage \
 -nographics \
 -logFile create_unity_core.log \
 -projectPath $PWD/../ \
 -exportPackage \
 Assets \
 $PWD/$DEPLOY_PATH/$PACKAGE_NAME \
 -quit \
 && echo "package exported successfully to outputs/appsflyer-unity-plugin-strict-mode-6.2.62.unitypackage" \
 || echo "Failed to export package. See create_unity_core.log for more info."


 if [ $1 == "-p" ]; then
 echo "removing ./Library"
 rm -rf ../Library
 echo "removing ./Logs"
 rm -rf ../Logs
 echo "removing ./Packages"
 rm -rf ../Packages
 echo "removing ./ProjectSettings"
 rm -rf ../ProjectSettings
 echo "removing ./deploy/create_unity_core.log"
 rm ./create_unity_core.log
 echo "Moving  $DEPLOY_PATH/$PACKAGE_NAME to root"
 mv ./outputs/$PACKAGE_NAME ..
 echo "removing ./deploy/outputs"
 rm -rf ./outputs
 else
 echo "dev mode. No files removed. Run with -p flag for production build."
 fi

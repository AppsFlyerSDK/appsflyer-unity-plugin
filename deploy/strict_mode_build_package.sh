#!/bin/bash

echo "Start Build for appsflyer-unity-plugin.unitypackage. Strict Mode."


 DEPLOY_PATH=outputs
 UNITY_PATH="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
 PACKAGE_NAME="appsflyer-unity-plugin-strict-mode-6.8.0.unitypackage"

 mkdir -p $DEPLOY_PATH

#move external dependency manager
echo "moving the external dependency manager to root"
mv external-dependency-manager-1.2.144.unitypackage ..

echo "Changing AppsFlyerFramework to Strict Mode"
sed -i '' 's/AppsFlyerFramework/AppsFlyerFramework\/Strict/g' ../Assets/AppsFlyer/Editor/AppsFlyerDependencies.xml
echo "Changing AppsFlyerFramework to Strict Mode. Done."

echo "Commenting out disableAdvertisingIdentifier"
sed -i '' 's/\[AppsFlyerLib shared\].disableAdvertisingIdentifier/\/\/\[AppsFlyerLib shared\].disableAdvertisingIdentifier/g' ../Assets/AppsFlyer/Plugins/iOS/AppsFlyeriOSWrapper.mm


echo "Commenting out waitForATTUserAuthorizationWithTimeoutInterval"
sed -i '' 's/\[\[AppsFlyerLib shared\] waitForATTUserAuthorizationWithTimeoutInterval:timeoutInterval\];/\/\/\[\[AppsFlyerLib shared\] waitForATTUserAuthorizationWithTimeoutInterval:timeoutInterval\];/g' ../Assets/AppsFlyer/Plugins/iOS/AppsFlyeriOSWrapper.mm
echo "Commenting out functions. Done."


 # Build the .unitypackage
 /Applications/Unity/Hub/Editor/2019.4.26f1/Unity.app/Contents/MacOS/Unity \
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
 && echo "package exported successfully to outputs/appsflyer-unity-plugin-strict-mode-6.8.0.unitypackage" \


 if [ $1 == "-p" ]; then
 echo "moving back the external dependency manager to deploy"
 mv ../external-dependency-manager-1.2.144.unitypackage .
 echo "removing ./Library"
 rm -rf ../Library
 echo "removing ./Logs"
 rm -rf ../Logs
 echo "removing ./Packages"
 rm -rf ../Packages
 echo "removing ./deploy/create_unity_core.log"
 rm ./create_unity_core.log
 echo "Moving  $DEPLOY_PATH/$PACKAGE_NAME to root"
 mv ./outputs/$PACKAGE_NAME ..
 echo "removing ./deploy/outputs"
 rm -rf ./outputs
echo "removing ../Assets surplus
  rm -rf ./Assets/ExternalDependencyManager
  rm ./Assets/ExternalDependencyManager.meta
  rm -rf ./Assets/PlayServicesResolver
  rm ./Assets/PlayServicesResolver.meta

 else
 echo "dev mode. No files removed. Run with -p flag for production build."
 fi

#!/bin/bash

echo "Start Build for appsflyer-unity-plugin.unitypackage. Strict Mode."


 DEPLOY_PATH=outputs
 UNITY_PATH="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
 PACKAGE_NAME="appsflyer-unity-plugin-strict-mode-6.12.21.unitypackage"
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
 /Applications/Unity/Hub/Editor/2021.3.11f1/Unity.app/Contents/MacOS/Unity \
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
 && echo "package exported successfully to outputs/appsflyer-unity-plugin-strict-mode-6.12.21.unitypackage" \
 || echo "Failed to export package. See create_unity_core.log for more info."


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
 echo "Moving $DEPLOY_PATH/$PACKAGE_NAME to strict-mode-sdk folder"
 mv ./outputs/$PACKAGE_NAME ../strict-mode-sdk
 echo "removing ./deploy/outputs"
 rm -rf ./outputs
 echo "removing ./Assets extra files"
 rm -rf ../Assets/ExternalDependencyManager
 rm -rf ../Assets/PlayServicesResolver
 rm ../Assets/ExternalDependencyManager.meta
 rm ../Assets/PlayServicesResolver.meta
 echo "Uncomment disableAdvertisingIdentifier"
 sed -i '' 's/\/\/\[AppsFlyerLib/\[AppsFlyerLib/g' ../Assets/AppsFlyer/Plugins/iOS/AppsFlyeriOSWrapper.mm

 echo "Uncomment waitForATTUserAuthorizationWithTimeoutInterval"
 sed -i '' 's/\/\/\[\[AppsFlyerLib/\[\[AppsFlyerLib/g' ../Assets/AppsFlyer/Plugins/iOS/AppsFlyeriOSWrapper.mm
 echo "Uncomment functions. Done."

 echo "Changing AppsFlyerFramework back"
 sed -i '' 's/AppsFlyerFramework\/Strict/AppsFlyerFramework/g' ../Assets/AppsFlyer/Editor/AppsFlyerDependencies.xml
 echo "Changing AppsFlyerFramework back. Done."

 else
 echo "dev mode. No files removed. Run with -p flag for production build."
 fi

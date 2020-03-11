#!/bin/bash

echo "Start Build for AppsFlyerAndroidPlugin.jar"

cd ../af-unity-android-plugin
./gradlew makeJar
cd ..
rm ./Assets/Plugins/AppsFlyer/Android/AppsFlyerAndroidPlugin.jar
echo "Updating current AppsFlyerAndroidPlugin.jar"
cp ./af-unity-android-plugin/afunityplugin/build/output/AppsFlyerAndroidPlugin.jar ./Assets/Plugins/Android/AppsFlyerAndroidPlugin.jar


echo "Done Build for AppsFlyerAndroidPlugin.jar"
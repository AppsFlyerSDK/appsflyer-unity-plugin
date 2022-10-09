from importlib.resources import path
from nis import match
import re
from unitypackage_extractor.extractor import extractPackage

class checkPackage:

    def __init__(self, pathToPackage):
        self.pathToPackage = pathToPackage

    def extractPackage(self):
        extractPackage(self.pathToPackage, outputPath="./packageUnity")

    def hasCommentedMethods(self, file):
        textfile = open(file, 'r')
        filetext = textfile.read()
        textfile.close()
        matches = re.findall("[/]+.*\[+AppsFlyerLib", filetext)
        return len(matches) > 0


def main():
    package = checkPackage("appsflyer-unity-plugin-6.6.0.unitypackage")
    #strict_package = checkPackage("/Users/margotguetta/Desktop/Projects/Plugin/Unity/appsflyer-unity-plugin/strict-mode-sdk/appsflyer-unity-plugin-strict-mode-6.8.4.unitypackage")

    #testing regular package
    print("Testing AppsFlyeriOSWrapper.mm in unity package")
    package.extractPackage()
    hasComments = package.hasCommentedMethods("packageUnity/Assets/AppsFlyer/Plugins/iOS/AppsFlyeriOSWrapper.mm")
    print("The file in the unity package has comments : ", hasComments)

    # #testing strict mode package
    # print("Testing AppsFlyeriOSWrapper.mm in unity strict package")
    # strict_package.extractPackage()
    # strict_package.extractPackage()
    # hasComments = strict_package.hasCommentedMethods("packageUnity/Assets/AppsFlyer/Plugins/iOS/AppsFlyeriOSWrapper.mm")
    # print("The file in the unity strict package has comments : ", hasComments)
    


if __name__ == "__main__":
    main()
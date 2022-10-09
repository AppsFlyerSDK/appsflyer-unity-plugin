from importlib.resources import path
from nis import match
import re
import hashlib
import os
import sys
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
    strict_package = checkPackage("strict-mode-sdk/appsflyer-unity-plugin-strict-mode-6.6.0.unitypackage")

    #testing regular package
    print("Testing AppsFlyeriOSWrapper.mm in unity package")
    package.extractPackage()
    hasComments = package.hasCommentedMethods("packageUnity/Assets/AppsFlyer/Plugins/iOS/AppsFlyeriOSWrapper.mm")
    print("The file in the unity package has comments : ", hasComments)

    path_of_the_directory= 'packageUnity/Assets/AppsFlyer/Plugins/iOS/'
    path_of_repo = "Assets/AppsFlyer/Plugins/iOS/"
    for filename in os.listdir(path_of_the_directory):
        f1 = os.path.join(path_of_the_directory,filename)
        if os.path.isfile(f1):
            print(f1)
            f2 = os.path.join(path_of_repo, filename)
            if os.path.isfile(f2):
                if getHash(f1) != getHash(f2):
                    print("the file ", filename, "is not the same")

    #testing strict mode package
    print("Testing AppsFlyeriOSWrapper.mm in unity strict package")
    strict_package.extractPackage()
    strict_package.extractPackage()
    hasComments = strict_package.hasCommentedMethods("packageUnity/Assets/AppsFlyer/Plugins/iOS/AppsFlyeriOSWrapper.mm")
    print("The file in the unity strict package has comments : ", hasComments)

    # path_of_the_directory= 'packageUnity/Assets/AppsFlyer/Plugins/iOS/'
    # path_of_repo = "Assets/AppsFlyer/Plugins/iOS/"
    for filename in os.listdir(path_of_the_directory):
        f1 = os.path.join(path_of_the_directory,filename)
        if os.path.isfile(f1):
            print(f1)
            f2 = os.path.join(path_of_repo, filename)
            if os.path.isfile(f2):
                if getHash(f1) != getHash(f2):
                    print("the file ", filename, "is not the same")
                    sys.exit(5)

    
def getHash(filePath):
    md5 = hashlib.md5()
    with open(filePath,'rb') as file:
        hash = file.read()
        md5.update(hash)
        return md5.hexdigest()

if __name__ == "__main__":
    main()

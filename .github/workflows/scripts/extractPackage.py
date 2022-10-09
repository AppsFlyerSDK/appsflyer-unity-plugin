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

    def extractPackage(self, pathToOuptut):
        extractPackage(self.pathToPackage, outputPath=pathToOuptut)

    def hasCommentedMethods(self, file):
        textfile = open(file, 'r')
        filetext = textfile.read()
        textfile.close()
        matches = re.findall("[/]+.*\[+AppsFlyerLib", filetext)
        return len(matches) > 0




def main():
    package = checkPackage("appsflyer-unity-plugin-6.6.0.unitypackage")
    strict_package = checkPackage("strict-mode-sdk/appsflyer-unity-plugin-strict-mode-6.6.0.unitypackage")

    #testing integreity of files
    print("Testing AppsFlyeriOSWrapper.mm in unity package")
    package.extractPackage("./packageUnity")
    strict_package.extractPackage("./strictPackageUnity")
    
    path_of_the_strict_directory= 'strictPackageUnity/Assets/'
    path_of_the_directory= 'packageUnity/Assets/'
    path_of_repo = "Assets/"
    for filename in os.listdir(path_of_the_directory):
        f1 = os.path.join(path_of_the_directory,filename)
        f2 = os.path.join(path_of_repo, filename)
        f3 = os.path.join(path_of_the_strict_directory, filename)
        if os.path.isfile(f1) and os.path.isfile(f2) and os.path.isfile(f3):
            print(filename)
            if filename == "AppsFlyeriOSWrapper.mm":
                if getHash(f1) != getHash(f2):
                    print("the file ", filename, "is not the same")
                    sys.exit(5)
            else:
                if getHash(f1) != getHash(f2) or getHash(f3) != getHash(f2):
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

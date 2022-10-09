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
    
    path_of_the_strict_directory= 'strictPackageUnity/'
    path_of_the_directory= 'packageUnity/'
    path_of_repo = "Assets/"
    for subdir, dirs, files in os.walk(path_of_repo):
        for file in files:
            print (os.path.join(subdir, file))            
            file_in_package = os.path.join(*[path_of_the_directory, subdir,file])
            file_in_repo = os.path.join(subdir, file)
            file_in_strict_package = os.path.join(*[path_of_the_strict_directory, subdir,file])
            print("file_in_package is ", file_in_package, os.path.isfile(file_in_package))
            print("file_in_repo is ", file_in_repo, os.path.isfile(file_in_repo))
            print("file_in_strict_package is ", file_in_strict_package, os.path.isfile(file_in_strict_package))
            if os.path.isfile(file_in_package) and os.path.isfile(file_in_repo) and os.path.isfile(file_in_strict_package):
                print(file)
                if filename == "AppsFlyeriOSWrapper.mm":
                    if getHash(file_in_package) != getHash(file_in_repo):
                        print("the file ", file, "is not the same")
                        sys.exit(5)
                else:
                    if getHash(file_in_package) != getHash(file_in_repo) or getHash(f3) != getHash(file_in_strict_package):
                        print("the file ", file, "is not the same")
                        sys.exit(5)

            
               

    
def getHash(filePath):
    md5 = hashlib.md5()
    with open(filePath,'rb') as file:
        hash = file.read()
        md5.update(hash)
        return md5.hexdigest()

if __name__ == "__main__":
    main()

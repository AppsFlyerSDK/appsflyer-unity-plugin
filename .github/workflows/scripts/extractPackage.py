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


def main():
    package = checkPackage(sys.argv[1])
    strict_package = checkPackage(sys.argv[2])

    #testing integreity of files
    print("Testing AppsFlyeriOSWrapper.mm in unity package")
    package.extractPackage("./packageUnity")
    strict_package.extractPackage("./strictPackageUnity")
    
    path_of_the_strict_directory= 'strictPackageUnity/'
    path_of_the_directory= 'packageUnity/'
    path_of_repo = "Assets/"
    
    files_to_not_check = ["package.json"]
    files_for_strict_mode_only = ["AppsFlyeriOSWrapper.mm", "AppsFlyerDependencies.xml"]
    
    
    #checksum of files
    for subdir, dirs, files in os.walk(path_of_repo):
        for file in files:
            print (os.path.join(subdir, file))            
            file_in_package = os.path.join(*[path_of_the_directory, subdir,file])
            file_in_repo = os.path.join(subdir, file)
            file_in_strict_package = os.path.join(*[path_of_the_strict_directory, subdir,file])
            if os.path.isfile(file_in_package) and os.path.isfile(file_in_repo) and os.path.isfile(file_in_strict_package):
                print(file)
                if file in files_to_not_check:
                    continue
                if file in files_for_strict_mode_only:
                    if getHash(file_in_package) != getHash(file_in_repo):
                        print("the file ", file, "is not the same")
                        sys.exit(5)
                    if file == "AppsFlyeriOSWrapper.mm":
                       if not hasCommentedMethods(file_in_strict_package):
                           print("the methods are not commented in  ", file_in_strict_package)
                           sys.exit(5)
                    if file == "AppsFlyerDependencies.xml":
                        if not isSrictModeDependency(file_in_strict_package):
                            print("the dependecy is not strict in ",file_in_strict_package )
                            sys.exit(5)
                        
                        
                else:
                    if getHash(file_in_package) != getHash(file_in_repo) or getHash(file_in_repo) != getHash(file_in_strict_package):
                        print("the file ", file, "is not the same")
                        sys.exit(5)


        
    
def getHash(filePath):
    md5 = hashlib.md5()
    with open(filePath,'rb') as file:
        hash = file.read()
        md5.update(hash)
        return md5.hexdigest()
    
    
#check that only the two methods are commented in the strict mode package
def hasCommentedMethods(file):
    textfile = open(file, 'r')
    filetext = textfile.read()
    textfile.close()
    matches1 = re.findall("[/]+.*\[+AppsFlyerLib.*disableAdvertisingIdentifier", filetext)
    matches2 = re.findall("[/]+.*\[+AppsFlyerLib.*waitForATTUserAuthorizationWithTimeoutInterval", filetext)
    matches3 = re.findall("[/]+.*\[+AppsFlyerLib", filetext)
    return len(matches1) == 1 and len(matches2) == 1 and not maches3

#check that we are using the strict dependency in strict mode package
def isSrictModeDependency(file):
    textfile = open(file, 'r')
    filetext = textfile.read()
    textfile.close()
    match = re.findall("AppsFlyerFramework/Strict", filetext)
    return len(match)>0
if __name__ == "__main__":
    main()

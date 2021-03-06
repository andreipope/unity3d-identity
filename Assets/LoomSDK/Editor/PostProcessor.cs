﻿using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;

namespace UnitySwift {
    public static class PostProcessor {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath) {
            if(buildTarget == BuildTarget.iOS) {
                // var projPath = PBXProject.GetPBXProjectPath(buildPath);
                var projPath = buildPath + "/Unity-iPhone.xcodeproj/project.pbxproj";
                var proj = new PBXProject();
                proj.ReadFromFile(projPath);

                string targetGuid = proj.TargetGuidByName(PBXProject.GetUnityTargetName());

                //// Configure build settings
                proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
                proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_BRIDGING_HEADER", "Libraries/LoomSDK/ios/LoomSDKSwift-Bridging-Header.h");
				proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_INTERFACE_HEADER_NAME", "LoomSDKSwift.h");
                proj.AddBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");
				proj.AddBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Libraries/LoomSDK/Frameworks");
				proj.SetBuildProperty(targetGuid, "SWIFT_VERSION", "3.0");

				//frameworks
				/*DirectoryInfo projectParent = Directory.GetParent(Application.dataPath);
				char divider = Path.DirectorySeparatorChar;
				var frameworksPath = "Frameworks/LoomSDK/Frameworks";
				if (PlayerSettings.iOS.sdkVersion == iOSSdkVersion.SimulatorSDK) {
					FileUtil.DeleteFileOrDirectory (buildPath + divider + frameworksPath + divider + "Auth0.framework");
					FileUtil.DeleteFileOrDirectory (buildPath + divider + frameworksPath + divider + "SimpleKeychain.framework");
					var assetsPath=Path.Combine(Application.dataPath, "LoomSDK/Frameworks");
					UnityEngine.Debug.Log (assetsPath);
					UnityEngine.Debug.Log (Application.dataPath);
					FileUtil.ReplaceFile (assetsPath + divider + "Auth0.framework.simulator", buildPath + divider + frameworksPath + divider + "Auth0.framework");
					FileUtil.ReplaceFile (assetsPath + divider + "SimpleKeychain.framework.simulator", buildPath + divider + frameworksPath + divider + "SimpleKeychain.framework");
				} else {
				//	FileUtil.DeleteFileOrDirectory (buildPath + divider + frameworksPath + divider + "Auth0.simulator");
				//	FileUtil.DeleteFileOrDirectory (buildPath + divider + frameworksPath + divider + "SimpleKeychain.simulator");
				}
				DirectoryInfo destinationFolder =
					new DirectoryInfo(buildPath + divider + frameworksPath);
				
				foreach(DirectoryInfo file in destinationFolder.GetDirectories()) {
					string filePath = "Frameworks/LoomSDK/Frameworks/"+ file.Name;
					//proj.AddFile(filePath, filePath, PBXSourceTree.Source);
					string fileGuid =proj.AddFile(filePath, filePath, PBXSourceTree.Source);
					proj.AddFrameworkToProject (targetGuid, file.Name, false);
					PBXProjectExtensions.AddFileToEmbedFrameworks(proj, targetGuid, fileGuid);

				}*/

                proj.WriteToFile(projPath);
				//info.plist
				var plistPath = buildPath+ "/Info.plist";
				var plist = new PlistDocument();
				plist.ReadFromFile(plistPath);
				// Update value
				PlistElementDict rootDict = plist.root;
				//rootDict.SetString("CFBundleIdentifier","$(PRODUCT_BUNDLE_IDENTIFIER)");
				PlistElementArray urls = rootDict.CreateArray ("CFBundleURLTypes");
				PlistElementDict dic =  urls.AddDict ();
				PlistElementArray scheme = dic.CreateArray ("CFBundleURLSchemes");
				scheme.AddString (PlayerSettings.applicationIdentifier);
				dic.SetString ("CFBundleURLName", "auth0");
				// Write plist
				File.WriteAllText(plistPath, plist.WriteToString());
				//Pod file
				string strPodContent="platform :ios, '9.0'\ninhibit_all_warnings!\nuse_frameworks!\n\ntarget 'Unity-iPhone' do\n pod 'Auth0', '~> 1.5'\nend\n";
				File.WriteAllText(buildPath+"/Podfile", strPodContent);
				//Run CocoaPods
				try{
				Process podProcess = new Process();
				podProcess.StartInfo.FileName = "/usr/local/bin/pod";
				podProcess.StartInfo.Arguments="install";
				podProcess.StartInfo.RedirectStandardError = true;
				podProcess.StartInfo.RedirectStandardOutput = true;
				podProcess.StartInfo.UseShellExecute = false;
				podProcess.StartInfo.WorkingDirectory = buildPath;
				podProcess.OutputDataReceived += new DataReceivedEventHandler((s, e) => 
					{ 
					//	UnityEngine.Debug.Log(e.Data); 
					}
				);
				podProcess.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
					{
							//UnityEngine.Debug.Log(e.Data); 
					}
				);
				podProcess.Start();
				podProcess.BeginOutputReadLine ();
				podProcess.WaitForExit ();
				}catch(System.Exception){
					UnityEngine.Debug.LogWarning("Cocoapods required to install Auth0 library. Please install it or run manually \"pod install\" in the Xcode project directory");
				}

            }
        }
    }
}

#endif
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using UnityEditor.iOS.Xcode;
using System.IO;

public class BL_BuildPostProcess {

	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget buildTarget, string path) {

		if (buildTarget == BuildTarget.iOS) {
			// string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
			// DisableBitcode(projPath);
			LinkLibraries (path);
		}
	}

	public static void DisableBitcode (string projPath) {
		PBXProject proj = new PBXProject();
		proj.ReadFromString(File.ReadAllText(projPath));
		string target = proj.TargetGuidByName("Unity-iPhone");
		proj.SetBuildProperty(target, "ENABLE_BITCODE", "false");
		File.WriteAllText(projPath, proj.WriteToString());
	}

	public static void LinkLibraries (string path) {
		// linked library
		string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
		PBXProject proj = new PBXProject();
		proj.ReadFromFile(projPath);	
		string target = proj.TargetGuidByName("Unity-iPhone");
		proj.SetBuildProperty(target, "ENABLE_BITCODE", "false");
		proj.AddFrameworkToProject(target, "CoreTelephony.framework", true);
		proj.AddFrameworkToProject(target, "VideoToolbox.framework", true);
		proj.AddFrameworkToProject(target, "libresolv.tbd", true);
		proj.AddFrameworkToProject(target, "libiPhone-lib.a", true);
		proj.AddFrameworkToProject(target, "CoreText.framework", true);
		proj.AddFrameworkToProject(target, "Metal.framework", true);
		proj.AddFrameworkToProject(target, "CoreML.framework", true);
		proj.AddFrameworkToProject(target, "Accelerate.framework", true);
		File.WriteAllText(projPath, proj.WriteToString());

		// permission
		string pListPath = path + "/Info.plist";
		PlistDocument plist = new PlistDocument();
		plist.ReadFromString(File.ReadAllText(pListPath));
		PlistElementDict rootDic = plist.root;
		var cameraPermission = "NSCameraUsageDescription";
		var micPermission = "NSMicrophoneUsageDescription";
		rootDic.SetString(cameraPermission, "Video need to use camera");
		rootDic.SetString(micPermission, "Voice call need to user mic");
		File.WriteAllText(pListPath, plist.WriteToString());
	}
}

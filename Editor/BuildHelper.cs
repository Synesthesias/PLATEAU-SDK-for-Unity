#if UNITY_IPHONE
 
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace PLATEAU.Editor
{
    public class BuildHelper
    {
        [PostProcessBuildAttribute (1)]
        public static void OnPostProcessBuild (BuildTarget target, string path)
        {
            if (target == BuildTarget.iOS)
            {
                var projectPath = PBXProject.GetPBXProjectPath (path);
                var project = new PBXProject ();
                project.ReadFromString (File.ReadAllText (projectPath));
 
                var targetGUID = project.GetUnityFrameworkTargetGuid ();
                project.SetBuildProperty (targetGUID, "ENABLE_BITCODE", "NO");
                var iphoneGUID = project.GetUnityMainTargetGuid ();
                project.SetBuildProperty (iphoneGUID, "ENABLE_BITCODE", "NO");
 
                File.WriteAllText (projectPath, project.WriteToString ());
            }
        }
    }
}

 
#endif

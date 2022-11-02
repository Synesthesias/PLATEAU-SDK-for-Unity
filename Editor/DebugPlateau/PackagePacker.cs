using UnityEditor;
using UnityEditor.PackageManager;

namespace PLATEAU.Editor.DebugPlateau
{
    public static class PackagePacker
    {
        [MenuItem("PLATEAU/Debug/Pack PLATEAU Package to tarball")]
        public static void Pack()
        {
            string destDir = EditorUtility.SaveFolderPanel("出力先", "", "");
            Client.Pack("Packages/com.synesthesias.plateau-unity-sdk", destDir);
        }
    }
}

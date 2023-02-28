using PLATEAU.Util;
using UnityEditor;
using UnityEditor.PackageManager;

namespace PLATEAU.Editor.DebugPlateau
{
    /// <summary>
    /// Packagesフォルダに入っている PLATEAU SDK for Unity を tarball 形式で出力します。
    /// デプロイで利用します。
    /// </summary>
    internal static class PackagePacker
    {
        [MenuItem("PLATEAU/Debug/Pack PLATEAU Package to tarball")]
        public static void Pack()
        {
            string destDir = EditorUtility.SaveFolderPanel("出力先", "", "");
            Client.Pack(PathUtil.SdkBasePath, destDir);
        }
    }
}

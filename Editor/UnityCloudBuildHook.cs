#if UNITY_CLOUD_BUILD
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor
{
    /// <summary>
    /// Unity Cloud Build でのエラーを回避するためのフックです。
    /// </summary>
    public class UnityCloudBuildHook : MonoBehaviour
    {
        /// <summary>
        /// ブラウザで Unity Cloud Build にログインし、その MacOS向けビルドの詳細設定で、pre-export にメソッド名を指定できます。
        /// そこでこのメソッドを指定してください。
        /// 
        /// 参考:
        /// https://forum.unity.com/threads/solved-cloud-build-plugin-is-used-from-several-locations.461372/#post-3001785
        /// </summary>
        public static void CloudBuildMacPreExport(BuildManifestObject manifest)
        {
            var allImporters = PluginImporter.GetAllImporters();
            foreach (var importer in allImporters) {
                if (importer.assetPath.Contains("libplateau")){
                    importer.SetCompatibleWithAnyPlatform(false);
                }
            }
        }
    }
}
#endif
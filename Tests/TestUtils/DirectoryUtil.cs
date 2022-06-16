using System.IO;
using PlateauUnitySDK.Editor.FileConverter;
using PlateauUnitySDK.Runtime.Util;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Tests.TestUtils
{
    /// <summary>
    /// テストで利用するディレクトリ操作に関するユーティリティです。
    /// テストで利用するディレクトリは次の2種類あります。
    /// ・Unityキャッシュフォルダ内の UnitTestTemporary
    /// ・Unityアセットフォルダ内の UnitTestTemporary
    /// 前者はキャッシュ用フォルダで、Unityプロジェクトの外にあります。
    /// 後者は Assets フォルダ内のテスト用ディレクトリです。
    /// 2つの使い分けについて:
    /// 後者の欠点は、ファイルの生成と破棄に .meta ファイルや AssetDatabase.Refresh を考慮する必要がある点です。
    /// 後者の利点は、Assetsフォルダ内にファイルがないと利用できない機能をテストできる点です。
    /// </summary>
    public static class DirectoryUtil
    {

        /// <summary> Unityキャッシュフォルダ内のテスト用ディレクトリパスです。 </summary>
        public static string TempCacheFolderPath => Path.Combine(Application.temporaryCachePath, "UnitTestTemporary");
        /// <summary> Unityアセットフォルダ内のテスト用ディレクトリパスです。 </summary>
        public static string TempAssetFolderPath => Path.Combine(Application.dataPath, "UnitTestTemporary");
        /// <summary> テストデータが入っている Package 内のフォルダです。 </summary>
        public static string TestDataFolderPath => Path.GetFullPath("Packages/PlateauUnitySDK/Tests/TestData");

        public static string TestDataSimplePath => Path.Combine(TestDataFolderPath, "TestDataSimpleGml/udx/bldg");
        /// <summary> テストデータとして利用できるgmlパスです。 </summary>
        public static string TestSimpleGmlFilePath => Path.Combine(TestDataSimplePath, "53392642_bldg_6697_op2.gml");

        public static string TestTokyoUdxPath => Path.Combine(TestDataFolderPath, "TestDataTokyo/udx");

        /// <summary>
        /// Unityのキャッシュ用ディレクトリにテスト用フォルダを作ります。
        /// そのフォルダの中身を空にします。
        /// </summary>
        public static void SetUpTempCacheFolder()
        {
            SetUpEmptyDir(TempCacheFolderPath);
        }

        /// <summary>
        /// Unityの Assets 内にテスト用フォルダを作ります。
        /// その中身を空にします。
        /// </summary>
        public static void SetUpTempAssetFolder()
        {
            string assetPath = PathUtil.FullPathToAssetsPath(TempAssetFolderPath);
            bool doDirExists = false;
            if (Directory.Exists(TempAssetFolderPath))
            {
                doDirExists = !DeleteTempAssetFolder();
            }

            if (!doDirExists)
            {
                string parent = Directory.GetParent(assetPath)?.ToString();
                string folderName = Path.GetFileName(assetPath);
                AssetDatabase.CreateFolder(parent, folderName);
            }
            
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Assets 内のテスト用フォルダを削除します。
        /// 削除の成否を bool で返します。
        /// </summary>
        public static bool DeleteTempAssetFolder()
        {
            string assetPath = PathUtil.FullPathToAssetsPath(TempAssetFolderPath);
            bool result = AssetDatabase.DeleteAsset(assetPath);

            if (!result)
            {
                Debug.LogError($"{nameof(DirectoryUtil)} : {nameof(DeleteTempAssetFolder)} : Could not delete TempAssetFolder. Path = {assetPath}");
            }
            AssetDatabase.Refresh();
            return result;
        }

        /// <summary>
        /// Unityキャッシュフォルダ内のテスト用ディレクトリを削除します。
        /// </summary>
        public static void DeleteTempCacheFolder()
        {
            DeleteAllInDir(TempCacheFolderPath);
        }

        public static void CopyFileToTempAssetFolder(string srcFilePath, string destFileName)
        {
            string destPath = Path.Combine(TempAssetFolderPath, destFileName);
            File.Copy(srcFilePath, destPath);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// <paramref name="folderPath"/> にフォルダを作り、
        /// その中身を空にします。
        /// </summary>
        private static void SetUpEmptyDir(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            DeleteAllInDir(folderPath);
        }

        /// <summary>
        /// <paramref name="dirPath"/> をパスとするディレクトリの中身をすべて削除します。
        /// </summary>
        private static void DeleteAllInDir(string dirPath)
        {
            var directoryInfo = new DirectoryInfo(dirPath);
            foreach (var file in directoryInfo.GetFiles())
            {
                if (file.Name.Contains(".mtl")) continue;
                file.Delete();
            }

            foreach (var dir in directoryInfo.GetDirectories())
            {
                dir.Delete(true);
            }
        }
    }
}
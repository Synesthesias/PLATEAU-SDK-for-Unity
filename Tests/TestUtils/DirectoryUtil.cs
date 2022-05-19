using System.IO;
using UnityEngine;

namespace PlateauUnitySDK.Tests.TestUtils
{
    /// <summary>
    /// テストで利用するディレクトリ操作に関するユーティリティです。
    /// </summary>
    public static class DirectoryUtil
    {

        public static string TestDataFolderPath => Path.GetFullPath("Packages/PlateauUnitySDK/Tests/TestData");
        public static string TestCacheTempFolderPath => Path.Combine(Application.temporaryCachePath, "UnitTestTemporary");

        /// <summary>
        /// Unityのキャッシュ用ディレクトリにテスト用フォルダを作ります。
        /// そのフォルダの中身を空にします。
        /// </summary>
        public static void SetUpCacheTempFolder()
        {
            SetUpEmptyDir(TestCacheTempFolderPath);
        }

        /// <summary>
        /// <paramref name="folderPath"/> にフォルダを作り、
        /// その中身を空にします。
        /// </summary>
        public static void SetUpEmptyDir(string folderPath)
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
        public static void DeleteAllInDir(string dirPath)
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
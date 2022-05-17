using System;
using System.IO;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter
{
    /// <summary>
    /// ファイルパスが正しいかどうか検証します。
    /// </summary>
    public static class FilePathValidator
    {
        // この値はテストのときのみ変更されるのでreadonlyにしないでください。
        private static string unityProjectDataPath = Application.dataPath;

        /// <summary>
        /// 入力ファイル用のパスとして正しければtrue,不適切であればfalseを返します。
        /// </summary>
        /// <param name="filePath">入力ファイルのパスです。</param>
        /// <param name="expectedExtension">入力ファイルとして想定される拡張子です。</param>
        /// <param name="shouldFileInAssetsFolder">ファイルがUnityプロジェクトのAssetsフォルダ内にあるべきならtrue、他の場所でも良いならfalseを指定します。</param>
        public static bool IsValidInputFilePath(string filePath, string expectedExtension,
            bool shouldFileInAssetsFolder)
        {
            try
            {
                CheckFileExist(filePath);
                CheckExtension(filePath, expectedExtension);
                if (shouldFileInAssetsFolder) CheckSubDirectoryOfAssets(filePath);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 出力用のファイルパスとして正しければtrue,不適切であればfalseを返します。
        /// </summary>
        public static bool IsValidOutputFilePath(string filePath, string expectedExtension)
        {
            try
            {
                CheckDirectoryExist(filePath);
                CheckExtension(filePath, expectedExtension);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 指定パスにファイルが存在するかチェックします。
        /// 存在しなければ例外を投げます。
        /// </summary>
        private static void CheckFileExist(string filePath)
        {
            if (File.Exists(filePath)) return;
            throw new IOException($"File is not found on the path :\n{filePath}");
        }

        /// <summary>
        /// 指定パスのうちディレクトリ部分が存在するかチェックします。
        /// 存在しなければ例外を投げます。
        /// </summary>
        private static void CheckDirectoryExist(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (Directory.Exists(directory)) return;
            throw new IOException($"Directory is not found on the path :\n{directory}.");
        }

        /// <summary>
        /// ファイルパスの拡張子が expectedExtension と同一かチェックします。
        /// 異なる場合は例外を投げます。
        /// </summary>
        private static void CheckExtension(string filePath, string expectedExtension)
        {
            var actualExtension = Path.GetExtension(filePath).ToLower().Replace(".", "");
            if (actualExtension == expectedExtension) return;
            throw new IOException(
                $"The file extension should be '{expectedExtension}', but actual is '{actualExtension}'"
            );
        }

        /// <summary>
        /// ファイルパスがUnityプロジェクトのAssetsフォルダ内であるかチェックします。
        /// そうでない場合は例外を投げます。
        /// </summary>
        private static void CheckSubDirectoryOfAssets(string filePath)
        {
            var fullPath = Path.GetFullPath(filePath);
            var assetsPath = Path.GetFullPath(unityProjectDataPath) + Path.DirectorySeparatorChar;
            if (fullPath.StartsWith(assetsPath)) return;
            throw new IOException($"File must exist in Assets folder, but the path is outside Assets folder.");
        }

        /// <summary>
        /// フルパスをAssetsフォルダからのパスに変換します。
        /// パスがAssetsフォルダ内を指すことが前提です。
        /// </summary>
        public static string FullPathToAssetsPath(string filePath)
        {
            CheckSubDirectoryOfAssets(filePath);
            var fullPath = Path.GetFullPath(filePath);
            var dataPath = Path.GetFullPath(unityProjectDataPath);
            var assetsPath = "Assets" + fullPath.Replace(dataPath, "");
#if UNITY_STANDALONE_WIN
            assetsPath = assetsPath.Replace('\\', '/');
#endif
            // Debug.Log($"assetsPath = {assetsPath}");
            return assetsPath;
        }
    }
}
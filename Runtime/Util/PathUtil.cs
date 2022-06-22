using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Runtime.Util
{
    /// <summary>
    /// ファイルパスが正しいかどうか検証します。
    /// </summary>
    internal static class PathUtil
    {
        // この値はテストのときのみ変更されるのでreadonlyにしないでください。
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
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
                Debug.LogError($"Input file path is invalid. filePath = {filePath}");
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
            catch (Exception e)
            {
                Debug.LogError($"Output file path is invalid.\n{e.Message}");
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
            if (IsSubDirectoryOfAssets(filePath)) return;
            throw new IOException($"File must exist in Assets folder, but the path is outside Assets folder.\filePath = {filePath}");
        }

        public static bool IsSubDirectoryOfAssets(string filePath)
        {
            var fullPath = Path.GetFullPath(filePath);
            var assetsPath = Path.GetFullPath(unityProjectDataPath) + Path.DirectorySeparatorChar;
            return fullPath.StartsWith(assetsPath);
        }

        public static bool IsSubDirectory(string subPath, string dir)
        {
            if (string.IsNullOrEmpty(subPath) || string.IsNullOrEmpty(dir)) return false;
            string subFull = Path.GetFullPath(subPath);
            string dirFull = Path.GetFullPath(dir);
            return subFull.StartsWith(dirFull);
        }

        /// <summary>
        /// フルパスをAssetsフォルダからのパスに変換します。
        /// パスがAssetsフォルダ内を指すことが前提であり、そうでない場合は <see cref="IOException"/> を投げます。
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

        #if UNITY_EDITOR
        /// <summary>
        /// Unity Editor 専用メソッドです。
        /// ディレクトリとファイルを再帰的にコピーします。
        /// <paramref name="src"/> はディレクトリのパスです。
        /// コピー先は (dest)/(srcのフォルダ名) になります。
        /// </summary>
        public static void CloneDirectory(string src, string dest)
        {
            // src がディレクトリのパスであると認識されるためには、末尾がパスのセパレーターである必要があります。
            if(!(src.EndsWith("/") || src.EndsWith("\\") )) src = string.Concat(src, "/");
            
            string srcDirPath = Path.GetDirectoryName(src);
            if (srcDirPath == null)
            {
                throw new IOException("parent dir of src is not found.");
            }
            string srcDirName = Path.GetFileName(srcDirPath);
            dest = Path.Combine(dest, srcDirName);
            if (!Directory.Exists(src))
            {
                throw new IOException($"Src directory is not found.\nsrc = {src}");
            }

            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }
            CloneDirectoryRecursive(src, dest);
            AssetDatabase.Refresh();
        }
        #endif
        
        private static void CloneDirectoryRecursive(string src, string dest)
        {
            // 各子ディレクトリのコピー
            foreach (var directory in Directory.GetDirectories(src))
            {
                string dirName = Path.GetFileName(directory);
                string childDir = Path.Combine(dest, dirName);
                if (!Directory.Exists(childDir))
                {
                    Directory.CreateDirectory(childDir);
                }
                CloneDirectoryRecursive(directory, childDir);
            }

            // 各ファイルのコピー
            foreach (var file in Directory.GetFiles(src))
            {
                if (Path.GetExtension(file) == ".meta") continue;
                string fileName = Path.GetFileName(file);
                string dstFile = Path.Combine(dest, fileName);
                File.Copy(file, dstFile, true);
            }
        }

        public static bool IsStringArrContainsName(string name, string[] array)
        {
            bool isInArray = false;
            foreach (var str in array)
            {
                if (name.Contains(str))
                {
                    isInArray = true;
                    break;
                }
            }

            return isInArray;
        }
    }
}
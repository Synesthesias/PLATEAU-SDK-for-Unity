using System;
using System.IO;
using UnityEngine;

namespace PLATEAU.Util
{
    /// <summary>
    /// PLATEAU のファイルパスに関するユーティリティです。
    /// </summary>
    public static class PathUtil
    {
        // この値はテストのときのみ変更されるのでreadonlyにしないでください。
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private static string unityProjectDataPath = Application.dataPath;

        public static readonly string PLATEAUSrcFetchDir = Path.GetFullPath(Application.streamingAssetsPath + "/.PLATEAU");
        public const string UdxFolderName = "udx";
        private static readonly string packageFormalName = "com.synesthesias.plateau-unity-sdk"; // 外部から使う時はSdkBasePathを使ってください
        private static readonly string packageDirName = "PLATEAU-SDK-for-Unity";

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
            if (!filePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                filePath += Path.DirectorySeparatorChar;
            }
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
            return assetsPath;
        }
        
        
        /// <summary>
        /// PLATEAU SDK のパスは、 GitHub からインポートした場合は Packages 以下になり、
        /// Unity Asset Store からインポートした場合は Assets 以下になります。
        /// SDKの基本パスを返します。
        /// </summary>
        public static string SdkBasePath => IsInPackageDir ? $"Packages/{packageFormalName}" : $"Assets/{packageDirName}";

        /// <summary>
        /// SdkBasePath からの相対パスを受け取り、アセットパスに変換して返します。
        /// </summary>
        public static string SdkPathToAssetPath(string sdkPath)
        {
            #if UNITY_EDITOR
            return Path.Combine(SdkBasePath, sdkPath);
            #else
            throw new NotImplementedException("This function is only supported in editor.");
            #endif
        }
        
        private static bool isInPackageDirCalculated;
        private static bool isInPackageDir;
        /// <summary>
        /// PLATEAU SDK が Package フォルダにある場合は true, Asset フォルダにある場合は false を返します。
        /// 参考 : https://forum.unity.com/threads/how-to-detect-if-we-are-in-a-packager-context-or-in-the-assets-folder.1116505/#post-7182082
        /// </summary>
        private static bool IsInPackageDir {
            get
            {
                #if UNITY_EDITOR
                if (isInPackageDirCalculated) return isInPackageDir;
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(assembly);
                isInPackageDir = packageInfo != null;
                isInPackageDirCalculated = true;
                return isInPackageDir;
                #else
                throw new NotImplementedException("This function is only supported in editor.");
                #endif
            }
            
        }
    }
}
using System.IO;
using PlateauUnitySDK.Runtime.Util;
using UnityEditor;

namespace PlateauUnitySDK.Editor.CityImport
{

    /// <summary>
    /// Plateauの元データをコピーします。
    /// </summary>
    internal static class CopyPlateauSrcFiles
    {

        /// <summary>
        /// Plateau元データのうち、 <paramref name="gmlRelativePaths"/> のリストにある gmlファイルとそれに関連するもののみをコピーします。
        /// 変換先 <paramref name="copyDest"/> は Assets フォルダ内であることが前提です。
        /// </summary>
        public static void SelectCopy(string sourceUdxPath, string copyDest,
            string[] gmlRelativePaths)
        {
            string srcUdxPath = UdxPathToGmlRootPath(sourceUdxPath);
            string gmlRootFolderName = Path.GetFileName(Path.GetDirectoryName(srcUdxPath));
            if (gmlRootFolderName == null) throw new FileNotFoundException($"{nameof(gmlRootFolderName)} is null.");

            // copyDest が存在しない場合、パスの最後のフォルダを自動で作ります。
            // 例: copyDest が         Assets/StreamingAssets/PLATEAU であり、
            //    実際に存在するフォルダは Assets/StreamingAssets/        までである場合、PLATEAU を新たに作ります。
            // TODO StreamingAssetsも存在しない場合は動かない。　足りない部分は複数フォルダであっても作成できるようにするべき。
            if (!Directory.Exists(copyDest))
            {
                var destParentDirInfo = Directory.GetParent(copyDest);
                var destParentPath = destParentDirInfo == null ? "" : destParentDirInfo.FullName;
                string destParent = PathUtil.FullPathToAssetsPath(destParentPath);
                string destFolderName = new DirectoryInfo(copyDest).Name;
                AssetDatabase.CreateFolder(destParent, destFolderName);
            }

            // コピー先のルートフォルダを作成します。
            // 例: Tokyoをコピーする場合のパスの例を以下に示します。
            //     Assets/StreamingAssets/PLATEAU/Tokyo　フォルダを作ります。
            string dstRootFolder = Path.Combine(copyDest, gmlRootFolderName);
            if (!Directory.Exists(dstRootFolder))
            {
                AssetDatabase.CreateFolder(copyDest, gmlRootFolderName);
            }

            // codelists をコピーします。
            // 例: Assets/StreamingAssets/PLATEAU/Tokyo/codelists/****.xml をコピーにより作成します。
            const string codelistsFolderName = "codelists";
            PathUtil.CloneDirectory(Path.Combine(srcUdxPath, codelistsFolderName), dstRootFolder);

            // udxフォルダを作ります。
            // 例: Assets/StreamingAssets/PLATEAU/Tokyo/udx　ができます。
            const string udxFolderName = "udx";
            string dstUdxFolder = Path.Combine(dstRootFolder, udxFolderName);
            if (!Directory.Exists(dstUdxFolder))
            {
                AssetDatabase.CreateFolder(PathUtil.FullPathToAssetsPath(dstRootFolder), udxFolderName);
            }

            // udxフォルダのうち対象のgmlファイルをコピーします。
            foreach (string gml in gmlRelativePaths)
            {
                GmlFileNameParser.Parse(gml, out int _, out string objTypeStr, out int _, out string _);
                // 地物タイプのディレクトリを作ります。
                // 例: gml のタイプが bldg なら、
                //     Assets/StreamingAssets/PLATEAU/Tokyo/udx/bldg　ができます。
                string dstObjTypeFolder = Path.Combine(dstUdxFolder, objTypeStr);
                if (!Directory.Exists(dstObjTypeFolder))
                {
                    AssetDatabase.CreateFolder(PathUtil.FullPathToAssetsPath(dstUdxFolder), objTypeStr);
                }

                // gmlファイルをコピーします。
                // 例: Assets/StreamingAssets/PLATEAU/Tokyo/bldg/1234.gml　ができます。
                string gmlName = Path.GetFileName(gml);
                string srcObjTypeFolder = Path.Combine(srcUdxPath, "udx", objTypeStr);
                File.Copy(Path.Combine(srcObjTypeFolder, gmlName), Path.Combine(dstObjTypeFolder, gmlName), true);

                // gmlファイルに関連するフォルダをコピーします。
                // gmlの名称からオプションと拡張子を除いた文字列がフォルダ名に含まれていれば、コピー対象のディレクトリとみなします。
                // 例: Assets/StreamingAssets/PLATEAU/Tokyo/bldg/1234_appearance/texture_number.jpg　などがコピーされます。 
                string gmlIdentity = GmlFileNameParser.NameWithoutOption(gml);
                foreach (var srcDir in Directory.GetDirectories(srcObjTypeFolder))
                {
                    string srcDirName = new DirectoryInfo(srcDir).Name;
                    if (srcDirName.Contains(gmlIdentity))
                    {
                        PathUtil.CloneDirectory(srcDir, dstObjTypeFolder);
                    }
                }
            }
        }
        
        private static string UdxPathToGmlRootPath(string udxPath)
        {
            return Path.GetFullPath(Path.Combine(udxPath, "../"));
        }
        
        public static string UdxPathToGmlRootFolderName(string udxPath)
        {
            string root = UdxPathToGmlRootPath(udxPath);
            return Path.GetFileName(Path.GetDirectoryName(root));
        }
        
    }
}
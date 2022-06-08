using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{

    /// <summary>
    /// 複数のgmlファイルから、複数のobj および 1つの idToFileテーブルを生成します。
    /// </summary>
    public class MultiGmlConverter
    {
        /// <summary>
        /// 複数のgmlファイルを変換します。
        /// </summary>
        /// <param name="gmlRelativePaths">gmlファイルの相対パスのリストです。</param>
        /// <param name="baseFolderPath"><paramref name="gmlRelativePaths"/>の相対パスの基準となるパスです。</param>
        /// <param name="exportFolderFullPath">出力先のフォルダの絶対パスです。</param>
        public void Convert(IEnumerable<string> gmlRelativePaths, string baseFolderPath, string exportFolderFullPath)
        {
            foreach (var gmlRelativePath in gmlRelativePaths)
            {
                // TODO Configを設定できるようにする
                string gmlFullPath = Path.GetFullPath(Path.Combine(baseFolderPath, gmlRelativePath));
                string gmlFileName = Path.GetFileNameWithoutExtension(gmlRelativePath);
                string objPath = Path.Combine(exportFolderFullPath, gmlFileName + ".obj");
                string idTablePath = Path.Combine(exportFolderFullPath, "idToFileTable.asset");
                var objConverter = new GmlToObjFileConverter();
                var idTableConverter = new GmlToIdFileTableConverter();
                objConverter.Convert(gmlFullPath, objPath);
                idTableConverter.Convert(gmlFullPath, idTablePath);
            }

            AssetDatabase.ImportAsset(FilePathValidator.FullPathToAssetsPath(exportFolderFullPath));
        }
    }
}
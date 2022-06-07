using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace PlateauUnitySDK.Editor.FileConverter.Converters;

public class UdxConverter
{
    public void Convert(IEnumerable<string> gmlFiles, string udxFolderPath, string exportFolderPath)
    {
        foreach (var gmlRelativePath in gmlFiles)
        {
            // TODO Configを設定できるようにする
            string gmlFullPath = Path.GetFullPath(Path.Combine(udxFolderPath, gmlRelativePath));
            string gmlFileName = Path.GetFileNameWithoutExtension(gmlRelativePath);
            string objPath = Path.Combine(exportFolderPath, gmlFileName + ".obj");
            string idTablePath = Path.Combine(exportFolderPath, "idToFileTable.asset");
            var objConverter = new GmlToObjFileConverter();
            var idTableConverter = new GmlToIdFileTableConverter();
            objConverter.Convert(gmlFullPath, objPath);
            idTableConverter.Convert(gmlFullPath, idTablePath);
        }
        AssetDatabase.ImportAsset(FilePathValidator.FullPathToAssetsPath(exportFolderPath));
    }
}
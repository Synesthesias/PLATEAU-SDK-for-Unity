using System.Collections.Generic;
using System.IO;
using PLATEAU.CityLoader.Setting;
using PLATEAU.Udx;
using PLATEAU.Util;
using UnityEditor;

namespace PLATEAU.CityLoader.Load
{
    internal static class CityFilesCopy
    {
        public static string ToStreamingAssets(string sourcePath,/* ICollection<string> areaMeshCodesStr,*/ CityLoadConfig config)
        {
            // var areaMeshCodes = areaMeshCodesStr.Select(str => MeshCode.Parse(str)).ToArray();
            // var collection = UdxFileCollection.Find(sourcePath).FilterByMeshCodes(areaMeshCodes);
            string destPath = PathUtil.plateauSrcFetchDir;
            var fetchTargetGmls = new List<GmlFileInfo>();
            var gmlInfoToDestroy = new List<GmlFileInfo>();
            // var targetPackages = 
            //     config
            //         .ForEachPackagePair
            //         .Where(pair => pair.Value.loadPackage)
            //         .Select(pair => pair.Key);
            // foreach (var package in targetPackages)
            // {
            //     foreach (var gmlPath in collection.GetGmlFiles(package))
            //     {
            //         var gmlInfo = GmlFileInfo.Create(gmlPath);
            //         gmlInfoToDestroy.Add(gmlInfo);
            //         fetchTargetGmls.Add(gmlInfo);
            //     }
            // }
            var gmlPaths = config.SearchMatchingGMLList(sourcePath, out var collection);
            foreach (var gmlPath in gmlPaths)
            {
                var gmlInfo = GmlFileInfo.Create(gmlPath);
                gmlInfoToDestroy.Add(gmlInfo);
                fetchTargetGmls.Add(gmlInfo);
            }

            int targetGmlCount = fetchTargetGmls.Count;
            for (int i = 0; i < targetGmlCount; i++)
            {
                var gml = fetchTargetGmls[i];
                EditorUtility.DisplayProgressBar("", $"インポート処理中 : [{i}/{targetGmlCount}] {Path.GetFileName(gml.Path)}",
                    (float)i / targetGmlCount);
                collection.Fetch(destPath, gml);
            }
            foreach(var gml in gmlInfoToDestroy) gml.Dispose();
            
            EditorUtility.ClearProgressBar();

            string destFolderName = Path.GetFileName(sourcePath);
            string destRootFolderPath = Path.Combine(destPath, destFolderName);
            return destRootFolderPath;
        }
    }
}

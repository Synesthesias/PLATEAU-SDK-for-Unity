using System.IO;
using System.Linq;
using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Runtime.SemanticsLoader;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{
    public class GmlToIdFileTableConverter : IFileConverter
    {
        private CitygmlParserParams parserParams;

        public bool Convert(string srcGmlPath, string dstTableFullPath)
        {
            if (!IsPathValid(srcGmlPath, dstTableFullPath)) return false;
            
            var cityModel = CityGml.Load(srcGmlPath, this.parserParams);
            var cityObjectIds = cityModel.RootCityObjects
                .SelectMany(co => co.CityObjectDescendantsDFS)
                .Select(co => co.ID);
            var table = LoadOrCreateIdGmlTable(dstTableFullPath);
            string gmlFileName = Path.GetFileNameWithoutExtension(srcGmlPath);
            foreach (string id in cityObjectIds)
            {
                if (table.ContainsKey(id)) continue;
                table.Add(id, gmlFileName);
            }
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
            return true;
        }

        private static bool IsPathValid(string srcGmlPath, string dstTableFullPath)
        {
            if (!FilePathValidator.IsValidInputFilePath(srcGmlPath, "gml", false))
            {
                return false;
            }
            if(!FilePathValidator.IsValidOutputFilePath(dstTableFullPath, "asset"))
            {
                return false;
            }
            // TODO outputPathがAssetsフォルダ内かどうかチェックする
            return true;
        }

        private static IdToGmlFileTable LoadOrCreateIdGmlTable(string dstTableFullPath)
        {
            
            
            string dstAssetPath = FilePathValidator.FullPathToAssetsPath(dstTableFullPath);
            if (!File.Exists(dstTableFullPath))
            {
                Debug.Log("creating file");
                var instance = ScriptableObject.CreateInstance<IdToGmlFileTable>();
                AssetDatabase.CreateAsset(instance, dstAssetPath);
                AssetDatabase.SaveAssets();
            }
            return AssetDatabase.LoadAssetAtPath<IdToGmlFileTable>(dstAssetPath);
        }

        public void SetConfig(bool doOptimize, bool doTessellate)
        {
            this.parserParams.Optimize = doOptimize;
            this.parserParams.Tessellate = doTessellate;
        }
    }
}
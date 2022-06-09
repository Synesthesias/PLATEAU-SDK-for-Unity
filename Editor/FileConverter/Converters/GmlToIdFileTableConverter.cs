using System;
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

            try
            {
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
            }catch (FileLoadException e)
            {
                Debug.LogError($"Failed to load gml file.\n gml path = {srcGmlPath}\n{e}");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error generating IdToFile Table.\ngml path = {srcGmlPath}\n{e}");
                return false;
            }
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

        private static IdToFileTable LoadOrCreateIdGmlTable(string dstTableFullPath)
        {
            
            
            string dstAssetPath = FilePathValidator.FullPathToAssetsPath(dstTableFullPath);
            if (!File.Exists(dstTableFullPath))
            {
                Debug.Log("creating file");
                var instance = ScriptableObject.CreateInstance<IdToFileTable>();
                AssetDatabase.CreateAsset(instance, dstAssetPath);
                AssetDatabase.SaveAssets();
            }
            return AssetDatabase.LoadAssetAtPath<IdToFileTable>(dstAssetPath);
        }

        public void SetConfig(bool doOptimize, bool doTessellate)
        {
            this.parserParams.Optimize = doOptimize;
            this.parserParams.Tessellate = doTessellate;
        }
    }
}
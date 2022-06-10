using System;
using System.IO;
using System.Linq;
using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Runtime.CityMapInfo;
using PlateauUnitySDK.Runtime.SemanticsLoader;
using PlateauUnitySDK.Runtime.Util;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{
    public class GmlToIdFileTableConverter : IFileConverter
    {
        private CitygmlParserParams parserParams;

        /// <summary>
        /// gmlファイルをロードして IdToFileTable に書き込みます。
        /// </summary>
        public bool Convert(string srcGmlPath, string dstTableFullPath)
        {
            return ConvertInner(srcGmlPath, dstTableFullPath, null);
        }

        /// <summary>
        /// ロードを省略し、ロード済みの cityModel をもとに変換します。
        /// </summary>
        public bool ConvertWithoutLoad(CityModel cityModel, string srcGmlPath, string dstTableFullPath)
        {
            if (cityModel == null)
            {
                Debug.LogError("cityModel is null.");
                return false;
            }

            return ConvertInner(srcGmlPath, dstTableFullPath, cityModel);
        }

        /// <summary>
        /// IdFileTable に変換します。
        /// 引数の <paramref name="cityModel"/> が null の場合、<see cref="CityModel"/> を新たにロードして変換します。
        /// <paramref name="cityModel"/> が nullでない場合、ロードを省略してそのモデルを変換します。
        /// </summary>
        public bool ConvertInner(string srcGmlPath, string dstTableFullPath, CityModel cityModel)
        {
            if (!IsPathValid(srcGmlPath, dstTableFullPath)) return false;

            try
            {
                cityModel ??= CityGml.Load(srcGmlPath, this.parserParams, DllLogCallback.UnityLogCallbacks);
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

        private static IdToGmlTable LoadOrCreateIdGmlTable(string dstTableFullPath)
        {
            
            
            string dstAssetPath = FilePathValidator.FullPathToAssetsPath(dstTableFullPath);
            if (!File.Exists(dstTableFullPath))
            {
                Debug.Log("creating file");
                var instance = ScriptableObject.CreateInstance<IdToGmlTable>();
                AssetDatabase.CreateAsset(instance, dstAssetPath);
                AssetDatabase.SaveAssets();
            }
            return AssetDatabase.LoadAssetAtPath<IdToGmlTable>(dstAssetPath);
        }

        public void SetConfig(bool doOptimize, bool doTessellate)
        {
            this.parserParams.Optimize = doOptimize;
            this.parserParams.Tessellate = doTessellate;
        }
    }
}
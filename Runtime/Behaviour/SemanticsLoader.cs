using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.CityGML;
using PLATEAU.CityMeta;
using PLATEAU.Interop;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.Behaviour
{
    /// <summary>
    /// Unityのランタイムでゲームオブジェクト名から <see cref="CityGML.CityObject"/> を取得します。
    /// </summary>
    internal class SemanticsLoader
    {
        private readonly Dictionary<string, CityModel> fileToCityModelCache = new Dictionary<string, CityModel>();

        public CityObject Load(string cityObjectId, CityMetaData cityMetaData)
        {
            if (cityMetaData == null)
            {
                throw new ArgumentNullException($"{nameof(cityMetaData)}");
            }
            
            // テーブルから cityObjectId に対応する gmlFileName を検索します。
            if( !cityMetaData.TryGetValueFromGmlTable(cityObjectId, out var gmlFileName))
            {
                throw new KeyNotFoundException($"cityObjectId {cityObjectId} is not found in {nameof(CityMetaData)}.");
            }
            
            // 名前が gmlFileName である gmlファイルを検索します。
            // gmlファイルは StreamingAssets フォルダ内にあることを前提とします（そうでないと実行時に読めないので）。
            
            // キャッシュにあればそれを返します。
            if (this.fileToCityModelCache.TryGetValue(gmlFileName, out CityModel cityModel))
            {
                return GetCityObjectById(cityModel, cityObjectId);
            }
            
            string udxPath = cityMetaData.cityImporterConfig.sourcePath.udxAssetPath;
            // udxフォルダは StreamingAssets フォルダにあることを前提とします。
            if (!PathUtil.IsSubDirectory(udxPath, Application.streamingAssetsPath))
            {
                throw new IOException(
                    $"Could not find gml file, because udx path is not in StreamingAssets folder.\nudxPath = {udxPath}");
            }
            string gmlPath = SearchGmlPath(gmlFileName);
            var loadedModel = CityGml.Load(gmlPath, new CitygmlParserParams(true, false), DllLogCallback.UnityLogCallbacks);
            this.fileToCityModelCache[gmlFileName] = loadedModel;
            return GetCityObjectById(loadedModel, cityObjectId);
        }

        private static string SearchGmlPath(string gmlFileNameWithoutExtension)
        {
            string foundGmlPath = Directory.EnumerateFiles(Application.streamingAssetsPath, gmlFileNameWithoutExtension + ".gml",
                SearchOption.AllDirectories).First();
            foundGmlPath = Path.GetFullPath(foundGmlPath); // Windowsでパスの区切り記号が '/' と '\' で混在するのを統一します。
            return foundGmlPath;
        }

        private static CityObject GetCityObjectById(CityModel model, string id)
        {
            return model.GetCityObjectById(id);
        }
    }
}
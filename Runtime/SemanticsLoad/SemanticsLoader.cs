using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Runtime.CityMapMeta;
using PlateauUnitySDK.Runtime.Util;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.SemanticsLoad
{
    /// <summary>
    /// Unityのランタイムでゲームオブジェクト名から <see cref="CityObject"/> を取得します。
    /// </summary>
    public class SemanticsLoader
    {
        // private CityMapMetaData cityMapMetaData;
        private readonly Dictionary<string, CityModel> fileToCityModelCache = new Dictionary<string, CityModel>();

        public CityObject Load(string cityObjectId, CityMapMetaData cityMapMetaData)
        {
            if (cityMapMetaData == null)
            {
                throw new ArgumentNullException($"{nameof(cityMapMetaData)}");
            }
            
            // テーブルから cityObjectId に対応する gmlFileName を検索します。
            string gmlFileName; // 拡張子を含みません
            if( !cityMapMetaData.TryGetValueFromGmlTable(cityObjectId, out gmlFileName))
            {
                throw new KeyNotFoundException($"cityObjectId {cityObjectId} is not found in {nameof(CityMapMetaData)}.");
            }
            
            // 名前が gmlFileName である gmlファイルを検索します。
            // TODO 今は StreamingAssets フォルダから読み込んでいますが、今後は拡張する必要があります。
            
            // キャッシュにあればそれを返します。
            if (this.fileToCityModelCache.TryGetValue(gmlFileName, out CityModel cityModel))
            {
                return GetCityObjectById(cityModel, cityObjectId);
            }
            
            string udxPath = cityMapMetaData.cityModelImportConfig.sourceUdxFolderPath;
            // udxフォルダは StreamingAssets フォルダにあることを前提とします。
            if (!PathUtil.IsSubDirectory(udxPath, Application.streamingAssetsPath))
            {
                throw new IOException(
                    $"Could not find gml file, because udx path is not in StreamingAssets folder.\nudxPath = {udxPath}");
            }
            string gmlPath = SearchGmlPath(udxPath, gmlFileName);
            var loadedModel = CityGml.Load(gmlPath, new CitygmlParserParams(true, false), DllLogCallback.UnityLogCallbacks);
            this.fileToCityModelCache[gmlFileName] = loadedModel;
            return GetCityObjectById(loadedModel, cityObjectId);
        }

        private static string SearchGmlPath(string directoryPath, string gmlFileNameWithoutExtension)
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
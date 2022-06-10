using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Runtime.CityMapInfo;
using PlateauUnitySDK.Runtime.Util;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.SemanticsLoader
{
    public class SemanticsLoader
    {
        private IdToGmlTable idToGmlTable;
        private Dictionary<string, CityModel> fileToCityModelCache = new Dictionary<string, CityModel>();

        public CityObject Load(string cityObjectId)
        {
            if (this.idToGmlTable == null)
            {
                // テーブルファイルを読み込みます。
                // TODO 今はResourcesファイルから読み込んでいますが、動的読み込みに対応するにあたっては拡張する必要があります。
                var tables = Resources.LoadAll<IdToGmlTable>("");
                if (tables.Length == 0)
                {
                    throw new FileNotFoundException("IdToFileTable is not found.");
                }

                this.idToGmlTable = tables[0];
            }
            // テーブルから cityObjectId に対応する gmlFileName を検索します。
            string gmlFileName; // 拡張子を含みません
            if( !this.idToGmlTable.TryGetValue(cityObjectId, out gmlFileName))
            {
                throw new KeyNotFoundException($"cityObjectId {cityObjectId} is not found in idToFileTable.");
            }
            // 名前が gmlFileName である gmlファイルを検索します。
            // TODO 今は StreamingAssets フォルダから読み込んでいますが、今後は拡張する必要があります。
            if (this.fileToCityModelCache.TryGetValue(gmlFileName, out CityModel cityModel))
            {
                return GetCityObjectById(cityModel, cityObjectId);
            }
            string foundGmlPath = Directory.EnumerateFiles(Application.streamingAssetsPath, gmlFileName + ".gml",
                SearchOption.AllDirectories).First();
            foundGmlPath = Path.GetFullPath(foundGmlPath); // Windowsでパスの区切り記号が '/' と '\' で混在するのを統一します。
            var loadedModel = CityGml.Load(foundGmlPath, new CitygmlParserParams(true, false), DllLogCallback.UnityLogCallbacks);
            this.fileToCityModelCache[gmlFileName] = loadedModel;
            return GetCityObjectById(loadedModel, cityObjectId);
        }

        private static CityObject GetCityObjectById(CityModel model, string id)
        {
            return model.GetCityObjectById(id);
        }
    }
}
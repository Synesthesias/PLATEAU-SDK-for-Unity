using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibPLATEAU.NET.CityGML;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.SemanticsLoader
{
    public class SemanticsLoader
    {
        private IdToFileTable idToFileTable;
        private Dictionary<string, CityModel> fileToCityModelCache = new Dictionary<string, CityModel>();

        public CityObject Load(string cityObjectId)
        {
            if (idToFileTable == null)
            {
                // テーブルファイルを読み込みます。
                // TODO 今はResourcesファイルから読み込んでいますが、動的読み込みに対応するにあたっては拡張する必要があります。
                var tables = Resources.LoadAll<IdToFileTable>("");
                if (tables.Length == 0)
                {
                    throw new FileNotFoundException("IdToFileTable is not found.");
                }

                this.idToFileTable = tables[0];
            }
            // テーブルから cityObjectId に対応する gmlFileName を検索します。
            string gmlFileName; // 拡張子を含みません
            if( !this.idToFileTable.TryGetValue(cityObjectId, out gmlFileName))
            {
                throw new KeyNotFoundException($"cityObjectId {cityObjectId} is not found in idToFileTable.");
            }
            // 名前が gmlFileName である gmlファイルを検索します。
            // TODO 今は StreamingAssets フォルダから読み込んでいますが、今後は拡張する必要があります。
            if (this.fileToCityModelCache.TryGetValue(gmlFileName, out CityModel cityModel))
            {
                return FindCityObjectById(cityModel, cityObjectId);
            }
            string foundGmlPath = Directory.EnumerateFiles(Application.streamingAssetsPath, gmlFileName + ".gml",
                SearchOption.AllDirectories).First();
            foundGmlPath = Path.GetFullPath(foundGmlPath); // Windowsでパスの区切り記号が '/' と '\' で混在するのを統一します。
            // Debug.Log($"Loading {foundGmlPath}");
            var loadedModel = CityGml.Load(foundGmlPath, new CitygmlParserParams(true, false));
            // Debug.Log("Load complete.");
            this.fileToCityModelCache[gmlFileName] = loadedModel;
            return FindCityObjectById(loadedModel, cityObjectId);
        }

        // TODO 全探索で効率が悪いので良い方法を作ります
        private static CityObject FindCityObjectById(CityModel model, string id)
        {
            return model.RootCityObjects
                .SelectMany(co => co.CityObjectDescendantsDFS)
                .First(co => co.ID == id);
        }
    }
}
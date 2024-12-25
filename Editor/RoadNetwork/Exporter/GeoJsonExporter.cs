using GeoJSON.Net.CoordinateReferenceSystem;
using GeoJSON.Net.Feature;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// GeoJSON形式のデータをエクスポートするための機能を提供するクラス
    /// </summary>
    public class GeoJsonExporter
    {
        /// <summary>
        /// GeoJSON形式のデータへ変換する
        /// </summary>
        /// <param name="geoJsonFeatures"></param>
        /// <param name="crs"></param>
        /// <returns></returns>
        public static string CreateGeoJson(List<GeoJsonFeature> geoJsonFeatures, string crs)
        {
            var features = new List<Feature>();
            foreach (var gisFeature in geoJsonFeatures)
            {
                features.Add(gisFeature.ToGeoJsonFeature());
            }
            var featureCollection = new FeatureCollection(features);
            featureCollection.CRS = new NamedCRS(crs);
            return JsonConvert.SerializeObject(featureCollection, Formatting.Indented);
        }

        /// <summary>
        /// GeoJSON形式のデータをファイルに書き込む
        /// </summary>
        /// <param name="path"></param>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        private static async Task WriteJsonAsync(string path, string jsonText)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                await writer.WriteAsync(jsonText);
            }
        }

        /// <summary>
        /// GeoJSON形式のデータを保存する
        /// </summary>
        /// <param name="jsonText"></param>
        /// <param name="path"></param>
        /// <param name="onSuccess"></param>
        /// <returns></returns>
        public static async Task ExportGeoJsonAsync(string jsonText, string path, Action onSuccess)
        {
            await WriteJsonAsync(path, jsonText);
            onSuccess?.Invoke();
        }
    }
}
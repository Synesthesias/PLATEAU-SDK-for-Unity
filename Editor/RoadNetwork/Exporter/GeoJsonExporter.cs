using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using GeoJSON.Net.CoordinateReferenceSystem;
using GeoJSON.Net.Feature;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// GeoJSON形式のデータをエクスポートするための機能を提供するクラス
    /// </summary>
    public class GeoJsonExporter
    {
        /// <summary>
        /// GeoJSONフィーチャリストからGeoJSON文字列を作成します
        /// </summary>
        /// <param name="geoJsonFeatures">GeoJSONフィーチャのリスト</param>
        /// <param name="crs">座標参照系 (CRS) を指定する文字列</param>
        /// <returns>作成されたGeoJSONデータの文字列</returns>
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
        /// 指定したパスに非同期でJSONデータを書き出します
        /// </summary>
        /// <param name="path">書き出すファイルのパス</param>
        /// <param name="jsonText">書き出すJSONデータの文字列</param>
        /// <returns>非同期タスク</returns>
        private static async Task WriteJsonAsync(string path, string jsonText)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                await writer.WriteAsync(jsonText);
            }
        }

        /// <summary>
        /// 非同期でGeoJSONデータを指定したパスにエクスポートします
        /// </summary>
        /// <param name="jsonText">エクスポートするGeoJSONデータの文字列</param>
        /// <param name="path">エクスポート先のファイルパス</param>
        /// <param name="onSuccess">エクスポート完了時に実行されるコールバック</param>
        /// <returns>非同期タスク</returns>
        public static async Task ExportGeoJsonAsync(string jsonText, string path, Action onSuccess)
        {
            await WriteJsonAsync(path, jsonText);
            onSuccess?.Invoke();
        }
    }
}
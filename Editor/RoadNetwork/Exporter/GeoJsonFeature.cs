using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// GeoJSONのフィーチャを表すクラス
    /// ジオメトリ情報とそれに関連するプロパティ情報を保持します。
    /// </summary>
    public class GeoJsonFeature
    {
        /// <summary>
        /// フィーチャのジオメトリ情報
        /// </summary>
        public IGeometryObject Geometry { get; set; }

        /// <summary>
        /// フィーチャに関連付けられたプロパティ情報
        /// </summary>
        public GeoJsonFeatureProperties Properties { get; set; }

        /// <summary>
        /// <see cref="GeoJsonFeature"/> クラスの新しいインスタンスを初期化します
        /// </summary>
        /// <param name="geometry">フィーチャのジオメトリ情報</param>
        /// <param name="properties">フィーチャに関連付けられたプロパティ情報</param>
        public GeoJsonFeature(IGeometryObject geometry, GeoJsonFeatureProperties properties)
        {
            Geometry = geometry;
            Properties = properties;
        }

        /// <summary>
        /// このインスタンスをGeoJSON互換の <see cref="Feature"/> オブジェクトに変換します
        /// </summary>
        /// <returns>このインスタンスを基にしたGeoJSONの <see cref="Feature"/> オブジェクト</returns>
        public Feature ToGeoJsonFeature()
        {
            return new Feature(Geometry, Properties);
        }
    }
}
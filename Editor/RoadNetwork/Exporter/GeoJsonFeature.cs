using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    public class GeoJsonFeature
    {
        public IGeometryObject Geometry { get; set; }
        public GeoJsonFeatureProperties Properties { get; set; }

        public GeoJsonFeature(IGeometryObject geometry, GeoJsonFeatureProperties properties)
        {
            Geometry = geometry;
            Properties = properties;
        }

        public Feature ToGeoJsonFeature()
        {
            return new Feature(Geometry, Properties);
        }
    }
}
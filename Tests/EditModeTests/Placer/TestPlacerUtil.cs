using System.Linq;
using NUnit.Framework;
using PLATEAU.CityMeta;
using PLATEAU.Editor.CityImport;
using PLATEAU.IO;
using PLATEAU.Tests.TestUtils;

namespace PLATEAU.Tests.EditModeTests.Placer
{
    internal static class TestPlacerUtil
    {
        public static CityMetadata ImportSimple(CityMeshPlacerConfig placeConf, MeshGranularity meshGranularity)
        {
            TestImporter.Import(ImportPathForTests.Simple, out CityMetadata metadata, config =>
            {
                config.meshGranularity = meshGranularity;
                config.objConvertTypesConfig.SetLodRangeToAllRange();
                config.gmlSearcherConfig.SetAllAreaId(true);
                config.gmlSearcherConfig.SetAllTypeTarget(true);
                config.cityMeshPlacerConfig = placeConf;
            });
            return metadata;
        }
        
        public static void Place(CityMeshPlacerConfig.PlaceMethod placeMethod, int selectedLod , CityMetadata metadata)
        {
            var placeConf = new CityMeshPlacerConfig()
                .SetPlaceMethodForAllTypes(placeMethod)
                .SetSelectedLodForAllTypes(selectedLod);
            CityMeshPlacerModelV2.Place(placeConf, metadata);
        }
        
        public static void AssertLodPlaced(params int[] shouldContain)
        {
            var gameObjs = SceneUtil.GetObjectsOfEditModeTestScene();
            foreach (var lod in shouldContain)
            {
                bool contains = gameObjs.Any(go => go.name.StartsWith($"LOD{lod}_"));
                Assert.IsTrue(contains, $"LOD{lod} がシーンに配置されます。");
            }
        }

        public static void AssertLodNotPlaced(params int[] shouldNotContain)
        {
            var gameObjs = SceneUtil.GetObjectsOfEditModeTestScene();
            foreach (var lod in shouldNotContain)
            {
                bool contains = gameObjs.Any(go => go.name.StartsWith($"LOD{lod}_"));
                Assert.IsFalse(contains, $"LOD{lod} はシーンに配置されません。");
            }
        }
    }
}
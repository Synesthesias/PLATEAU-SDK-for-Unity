using System;
using System.Linq;
using NUnit.Framework;
using PLATEAU.CityMeta;
using PLATEAU.Editor.CityImport;
using PLATEAU.IO;
using PLATEAU.Tests.TestUtils;

namespace PLATEAU.Tests.EditModeTests.Placer
{
    /// <summary>
    /// シーン配置をテストするときにつかるユーティリティです。
    /// </summary>
    internal static class TestPlacerUtil
    {
        /// <summary>
        /// Simpleテストデータをインポートします。
        /// </summary>
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

        /// <summary>
        /// シーンにモデルを配置します。
        /// 配置時の設定は、
        /// 全GMLタイプについて <see cref="CityMeshPlacerConfig.PlaceMethod"/> を適用し、
        /// 全GMLタイプについて ターゲットLOD = <paramref name="selectedLod"/> を適用したあと、
        /// 追加の設定として <paramref name="additionalConfigFunc"/> を実行したものとなります。
        /// </summary>
        public static void Place(CityMeshPlacerConfig.PlaceMethod placeMethod, int selectedLod , CityMetadata metadata, Action<CityMeshPlacerConfig> additionalConfigFunc)
        {
            var placeConf = new CityMeshPlacerConfig()
                .SetPlaceMethodForAllTypes(placeMethod)
                .SetSelectedLodForAllTypes(selectedLod);
            additionalConfigFunc?.Invoke(placeConf);
            metadata.cityImportConfig.cityMeshPlacerConfig = placeConf;
            CityMeshPlacerModel.Place(placeConf, metadata, null);
        }
        
        /// <summary>
        /// <see cref="Place(PLATEAU.CityMeta.CityMeshPlacerConfig.PlaceMethod,int,PLATEAU.CityMeta.CityMetadata,System.Action{PLATEAU.CityMeta.CityMeshPlacerConfig})"/>
        /// のメソッドの引数 additionalConfigFunc を省略した版です。
        /// </summary>
        public static void Place(CityMeshPlacerConfig.PlaceMethod placeMethod, int selectedLod, CityMetadata metadata)
        {
            Place(placeMethod, selectedLod, metadata, (conf) => {});
        }

        public static void PlaceWithBuildingTypeMask(int lod, CityMetadata metadata, ulong cityObjectTypeFlags)
        {
            Place(CityMeshPlacerConfig.PlaceMethod.PlaceSelectedLodOrMax, lod, metadata,
                (placerConf) =>
                {
                    var buildingConf = placerConf.GetPerTypeConfig(GmlType.Building);
                    buildingConf.cityObjectTypeFlags = cityObjectTypeFlags;
                }
            );
        }
        
        /// <summary>
        /// 引数の各数値について、 "LOD{数値}_" で始まる名前のゲームオブジェクトが存在することを確認します。
        /// </summary>
        public static void AssertLodPlaced(params int[] shouldContain)
        {
            var gameObjs = SceneUtil.GetObjectsOfEditModeTestScene();
            foreach (var lod in shouldContain)
            {
                bool contains = gameObjs.Any(go => go.name.StartsWith($"LOD{lod}_"));
                Assert.IsTrue(contains, $"LOD{lod} がシーンに配置されます。");
            }
        }

        /// <summary>
        /// 引数の各数値について、 "LOD{数値}_" で始まる名前のゲームオブジェクトが存在しないことを確認します。
        /// </summary>
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
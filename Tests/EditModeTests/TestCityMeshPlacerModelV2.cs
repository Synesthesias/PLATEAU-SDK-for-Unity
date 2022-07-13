using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PLATEAU.CityMeta;
using PLATEAU.Editor.CityImport;
using PLATEAU.IO;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using static PLATEAU.CityMeta.CityMeshPlacerConfig;

namespace PLATEAU.Tests.EditModeTests
{
    [TestFixture]
    public class TestCityMeshPlacerModelV2
    {
        private string prevDefaultDstPath;
        private static readonly string testDefaultCopyDestPath = Path.Combine(DirectoryUtil.TempAssetFolderPath, "PLATEAU");
        private CityMetadata metaData;
        
        // インポートには時間がかかるので、 OneTimeSetUp 内でインポートしたものを使い回します。
        // 別のインポート設定でテストしたい場合は、別のソースファイルを作ってください。

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DirectoryUtil.SetUpTempAssetFolder();
            
            // テスト用に一時的に デフォルト出力先を変更します。
            this.prevDefaultDstPath = PlateauUnityPath.StreamingGmlFolder;
            PlateauUnityPath.TestOnly_SetStreamingGmlFolder(testDefaultCopyDestPath);
            
            // インポートします。
            var initialPlaceConf = new CityMeshPlacerConfig().SetPlaceMethodForAllTypes(PlaceMethod.DoNotPlace);
            this.metaData = ImportSimple(initialPlaceConf, MeshGranularity.PerAtomicFeatureObject);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DirectoryUtil.DeleteTempAssetFolder();
            PlateauUnityPath.TestOnly_SetStreamingGmlFolder(this.prevDefaultDstPath);
        }

        [TearDown]
        public void TearDown()
        {
            SceneUtil.DestroyAllGameObjectsInEditModeTestScene();
        }

        [Test]
        public void When_PlaceMethod_Is_DoNotPlace_Then_No_Model_Is_Placed()
        {
            Place(PlaceMethod.DoNotPlace, 1);
            AssertLodNotPlaced(0,1,2);
        }


        [Test]
        public void When_PlaceMethod_Is_PlaceAllLod_Then_All_Lods_Are_Placed()
        {
            Place(PlaceMethod.PlaceAllLod, -1);
            AssertLodPlaced(0,1,2);
    }

        [Test]
        public void When_PlaceMethod_Is_MaxLod_Then_Only_MaxLod_Is_Placed()
        {
            Place(PlaceMethod.PlaceMaxLod, -1);
            AssertLodPlaced(2);
            AssertLodNotPlaced(0,1);
        }

        [Test]
        public void When_PlaceMethod_Is_PlaceSelectedLodOrMax_And_Lod_Not_Found_Then_MaxLod_Is_Placed()
        {
            Place(PlaceMethod.PlaceSelectedLodOrMax, 999);
            AssertLodPlaced(2); // 存在するなかで最大
            AssertLodNotPlaced(0,1);
        }

        [Test]
        public void When_PlaceMethod_Is_PlaceSelectedLodOrMax_And_Lod_Found_Then_TargetLod_Is_Placed()
        {
            Place(PlaceMethod.PlaceSelectedLodOrMax, 1);
            AssertLodPlaced(1);
            AssertLodNotPlaced(0,2);
        }

        [Test]
        public void When_PlaceMethod_Is_PlaceSelectedLodOrDoNotPlace_And_Lod_Not_Found_Then_Do_Not_Place()
        {
            Place(PlaceMethod.PlaceSelectedLodOrDoNotPlace, 999);
            AssertLodNotPlaced(0,1,2);
        }

        [Test]
        public void When_PlaceMethod_Is_PlaceSelectedOrDoNotPlace_And_Lod_Found_Then_TargetLod_Is_Placed()
        {
            Place(PlaceMethod.PlaceSelectedLodOrDoNotPlace, 1);
            AssertLodPlaced(1);
            AssertLodNotPlaced(0,2);
        }

        private static void AssertLodPlaced(params int[] shouldContain)
        {
            var gameObjs = SceneUtil.GetObjectsOfEditModeTestScene();
            foreach (var lod in shouldContain)
            {
                bool contains = gameObjs.Any(go => go.name.StartsWith($"LOD{lod}_"));
                Assert.IsTrue(contains, $"LOD{lod} がシーンに配置されます。");
            }
        }

        private static void AssertLodNotPlaced(params int[] shouldNotContain)
        {
            var gameObjs = SceneUtil.GetObjectsOfEditModeTestScene();
            foreach (var lod in shouldNotContain)
            {
                bool contains = gameObjs.Any(go => go.name.StartsWith($"LOD{lod}_"));
                Assert.IsFalse(contains, $"LOD{lod} はシーンに配置されません。");
            }
        }


        private void Place(PlaceMethod placeMethod, int selectedLod)
        {
            var placeConf = new CityMeshPlacerConfig()
                .SetPlaceMethodForAllTypes(placeMethod)
                .SetSelectedLodForAllTypes(selectedLod);
            CityMeshPlacerModelV2.Place(placeConf, this.metaData);
        }

        private static CityMetadata ImportSimple(CityMeshPlacerConfig placeConf, MeshGranularity meshGranularity)
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
    }
}
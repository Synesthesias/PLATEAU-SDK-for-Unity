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
        
        
        [SetUp]
        public void SetUp()
        {
            DirectoryUtil.SetUpTempAssetFolder();
            
            // テスト用に一時的に MultiGmlConverter のデフォルト出力先を変更します。
            this.prevDefaultDstPath = PlateauUnityPath.StreamingGmlFolder;
            PlateauUnityPath.TestOnly_SetStreamingGmlFolder(testDefaultCopyDestPath);
        }

        [TearDown]
        public void TearDown()
        {
            DirectoryUtil.DeleteTempAssetFolder();
            PlateauUnityPath.TestOnly_SetStreamingGmlFolder(this.prevDefaultDstPath);
            SceneUtil.DestroyAllGameObjectsInEditModeTestScene();
        }

        [Test]
        public void When_PlaceMethod_Is_DoNotPlace_Then_No_Model_Is_Placed()
        {
            Place(PlaceMethod.DoNotPlace, MeshGranularity.PerCityModelArea);
            
            var gameObjs = SceneUtil.GetObjectsOfEditModeTestScene();
            foreach (var go in gameObjs)
            {
                Debug.Log("exists: " + go.name);
            }
            Assert.AreEqual(1, gameObjs.Count, $"DoNotPlace のとき、ルート以下にゲームオブジェクトは配置されません。");
        }


        [Test]
        public void When_PlaceMethod_Is_PlaceAllLod_Then_All_Lods_Are_Placed()
        {
            Place(PlaceMethod.PlaceAllLod, MeshGranularity.PerAtomicFeatureObject);
            AssertLodPlaced(0,1,2);
    }

        [Test]
        public void When_PlaceMethod_Is_MaxLod_Then_Only_MaxLod_Is_Placed()
        {
            Place(PlaceMethod.PlaceMaxLod, MeshGranularity.PerPrimaryFeatureObject);
            AssertLodPlaced(2);
            AssertLodNotPlaced(0,1);
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


        private static void Place(PlaceMethod placeMethod, MeshGranularity meshGranularity)
        {
            var placeConf = PlaceConf(placeMethod);
            var metadata = ImportSimple(placeConf, meshGranularity);

            CityMeshPlacerModelV2.Place(placeConf, metadata);
        }
        
        private static CityMeshPlacerConfig PlaceConf(PlaceMethod placeMethod)
        {
            return new CityMeshPlacerConfig().SetPlaceMethodForAllTypes(placeMethod);
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
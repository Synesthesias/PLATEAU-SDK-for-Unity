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
            SceneUtil.DestroyAllGameObjectsInActiveScene();
        }

        [Test]
        public void When_PlaceMethod_Is_DoNotPlace_Then_No_Model_Is_Placed()
        {
            Place(PlaceMethod.DoNotPlace, MeshGranularity.PerCityModelArea);
            
            var gameObjs = SceneUtil.GetRootObjectsOfEditModeTestScene();
            int numChild = gameObjs[0].transform.childCount;
            Assert.AreEqual(0, numChild, $"DoNotPlace のとき、ルート以下にゲームオブジェクトは配置されません。");
        }
        

        [Test]
        public void When_PlaceMethod_Is_PlaceAllLod_Then_All_Lods_Are_Placed()
        {
            Place(PlaceMethod.PlaceAllLod, MeshGranularity.PerAtomicFeatureObject);
            var gameObjs = SceneUtil.GetRootObjectsOfEditModeTestScene();
            foreach (var go in gameObjs)
            {
                Debug.Log("listed obj: " + go.name);
            }
            // Assert.NotNull(GameObject.Find("53392642_bldg_6697_op2"));
            // Assert.NotNull(GameObject.Find("LOD2_BLD_0772bfd9-fa36-4747-ad0f-1e57f883f745"));
            bool containsLod0 = gameObjs.Any(go => go.name.Contains("LOD0_"));
            bool containsLod1 = gameObjs.Any(go => go.name.Contains("LOD1_"));
            bool containsLod2 = gameObjs.Any(go => go.name.Contains("LOD2_"));
            Assert.IsTrue(containsLod0, "AllLod で Lod0 が配置される");
            Assert.IsTrue(containsLod1, "AllLod で Lod1 が配置される");
            Assert.IsTrue(containsLod2, "AllLod で Lod2 が配置される");
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
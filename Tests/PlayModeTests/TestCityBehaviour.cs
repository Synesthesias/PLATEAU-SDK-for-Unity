using System.IO;
using NUnit.Framework;
using PLATEAU.Behaviour;
using PLATEAU.CityMeta;
using PLATEAU.IO;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.Tests.PlayModeTests
{
    [TestFixture]
    public class TestCityBehaviour
    {
        // private CityBehaviour cityBehaviour;
        private string prevDefaultDstPath;
        private static readonly string testDefaultCopyDestPath = Path.Combine(DirectoryUtil.TempAssetFolderPath, "PLATEAU");
        
        [SetUp]
        public void SetUp()
        {
            DirectoryUtil.SetUpTempAssetFolder();
            
            // テスト用に一時的に MultiGmlConverter のデフォルト出力先を変更します。
            this.prevDefaultDstPath = PlateauUnityPath.StreamingGmlFolder;
            PlateauUnityPath.TestOnly_SetStreamingGmlFolder(testDefaultCopyDestPath);
            
            TestImporter.Import(ImportPathForTests.Simple, out _, config =>
            {
                config.scenePlacementConfig.SetPlaceMethodForAllTypes(ScenePlacementConfig.PlaceMethod.PlaceAllLod);
                config.meshGranularity = MeshGranularity.PerPrimaryFeatureObject;
            });
        }

        [TearDown]
        public void TearDown()
        {
            DirectoryUtil.DeleteTempAssetFolder();
            PlateauUnityPath.TestOnly_SetStreamingGmlFolder(this.prevDefaultDstPath);
        }

        [Test]
        [Ignore("要更新")]
        public void LoadCityObject_Returns_NotNull()
        {
            var cityBehaviour = Object.FindObjectOfType<CityBehaviour>();

            var idGmlTable = cityBehaviour.CityMetaData.idToGmlTable;
            foreach (var pair in idGmlTable)
            {
                Debug.Log($"{pair.Key} => {pair.Value}");
            }

            string objName = cityBehaviour.transform.GetChild(0).GetChild(0).name;
            var cityObj = cityBehaviour.LoadCityObject(objName);
            Debug.Log(cityObj.ID);
        }
    }
}


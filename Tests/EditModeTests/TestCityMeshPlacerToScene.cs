using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using UnityEngine;
using static PLATEAU.CityMeta.CityMeshPlacerConfig;

namespace PLATEAU.Tests.EditModeTests
{
    [Ignore("古い機能のテストなので無視しています。新バージョンCityMeshPlacerToSceneV2向けのテストを新たに作る必要があります。")]
    public class TestCityMeshPlacerToScene
    {
        private static readonly string simpleGmlId = "53392642_bldg_6697_op2";
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
        public void Test_PlaceMethod_PlaceAllLod()
        {
            CheckSimpleObjPlacedToScene(PlaceMethod.PlaceAllLod, 0, 1, -1,
                new Dictionary<int, bool>
                {
                    { 0, true }, { 1, true }
                });
        }
        
        [Test]
        public void Test_PlaceMethod_MaxLod()
        {
            CheckSimpleObjPlacedToScene(PlaceMethod.PlaceMaxLod, 0, 1, 0,
                new Dictionary<int, bool>
                {
                    { 0, false }, { 1, true }
                });
        }

        [Test]
        public void Test_PlaceMethod_PlaceSelectedLodOrDoNotPlace_Of_DoNotPlace_Part()
        {
            CheckSimpleObjPlacedToScene(PlaceMethod.PlaceSelectedLodOrDoNotPlace, 0, 1, -1,
                new Dictionary<int, bool>
                {
                    { 0, false }, { 1, false }
                }
            );
        }

        [Test]
        public void Test_PlaceMethod_PlaceSelectedLodOrDoNotPlace_Of_SelectedLod_Part()
        {
            CheckSimpleObjPlacedToScene(PlaceMethod.PlaceSelectedLodOrDoNotPlace, 0, 1, 0, new Dictionary<int, bool>
                {
                    { 0, true }, { 1, false }
                }
            );
        }

        [Test]
        public void Test_PlaceMethod_PlaceSelectedLodOrMax()
        {
            CheckSimpleObjPlacedToScene(PlaceMethod.PlaceSelectedLodOrMax, 0, 1, -1,
                new Dictionary<int, bool>
                {
                    { 0, false }, { 1, true }
                }
            );
        }
        

        [Test]
        public void Test_PlaceMethod_DoNotPlace()
        {
            CheckSimpleObjPlacedToScene(PlaceMethod.DoNotPlace, 0, 1, 1,
                new Dictionary<int, bool>
                {
                    {0, false}, {1, false}
                });
        }

        private void CheckSimpleObjPlacedToScene(PlaceMethod placeMethod, int minLod, int maxLod, int selectedLod,
            Dictionary<int, bool> lodPlacedDict)
        {
            TestImporter.Import(ImportPathForTests.Simple, out _,
                config =>
                {
                    config.SetConvertLods(minLod, maxLod);
                    var placeConf = config.cityMeshPlacerConfig;
                    placeConf.SetSelectedLodForAllTypes(selectedLod);
                    placeConf.SetPlaceMethodForAllTypes(placeMethod);
                });
                
            AssertGameObjPlaced(simpleGmlId, lodPlacedDict);
        }
        
        /// <summary>
        /// 辞書 (LOD番号 => そのLODがシーン中配置されるべきか) を受け取り、
        /// その辞書の通りになっているかを Assert します。
        /// </summary>
        private void AssertGameObjPlaced(string gmlId, Dictionary<int, bool> lodPlacedDict)
        {
            foreach (var lodPlaced in lodPlacedDict)
            {
                int lod = lodPlaced.Key;
                bool shouldExist = lodPlaced.Value;
                bool doExists = GameObject.Find($"LOD{lod}_{gmlId}") != null;
                string message = shouldExist ? "存在する" : "存在しない";
                Assert.AreEqual(shouldExist, doExists, $"LOD{lod} がシーン中に {message}");
            }
        }
    }
}
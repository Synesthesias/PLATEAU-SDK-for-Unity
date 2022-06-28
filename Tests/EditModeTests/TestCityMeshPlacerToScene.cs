using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using PLATEAU.CityMeta;
using PLATEAU.Editor.CityImport;
using PLATEAU.IO;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using UnityEngine;
using static PLATEAU.CityMeta.ScenePlacementConfig;

namespace PLATEAU.Tests.EditModeTests
{
    public class TestCityMeshPlacerToScene
    {
        private static readonly string simpleGmlId = "53392642_bldg_6697_op2";
        private static readonly string testUdxPathSimple = DirectoryUtil.TestDataSimpleUdxPath;
        private static readonly string[] testGmlRelativePathsSimple =
        {
            "bldg/53392642_bldg_6697_op2.gml"
        };
        private static readonly string testOutputDir = DirectoryUtil.TempAssetFolderPath;
        private CityImporter importer;
        private string prevDefaultDstPath;
        private static readonly string testDefaultCopyDestPath = Path.Combine(DirectoryUtil.TempAssetFolderPath, "PLATEAU");
        
        
        [SetUp]
        public void SetUp()
        {
            this.importer = new CityImporter();
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
        public void Test_PlaceMinLod()
        {
            CheckSimpleObjPlacedToScene(PlaceMethod.PlaceMinLod, 0, 1, 1,
                new Dictionary<int, bool>
                {
                    { 0, true }, { 1, false }
                });
        }
        
        [Test]
        public void Test_PlaceMethod_PlaceSelectedLodOrDoNotPlace()
        {
            var allObjs = Object.FindObjectsOfType<GameObject>();
            foreach (var obj in allObjs)
            {
                Debug.Log(obj.name);
            }
            
            CheckSimpleObjPlacedToScene(PlaceMethod.PlaceSelectedLodOrDoNotPlace, 0, 1, -1,
                new Dictionary<int, bool>
                {
                    { 0, false }, { 1, false }
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

        private void CheckSimpleObjPlacedToScene(PlaceMethod placeMethod, int minLodBuilding, int maxLodBuilding, int selectedLod,
            Dictionary<int, bool> lodPlacedDict)
        {
            Import(testUdxPathSimple, testGmlRelativePathsSimple, MeshGranularity.PerCityModelArea,
                minLodBuilding, maxLodBuilding, selectedLod, placeMethod);
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
        
        private void Import(string testUdxPath, string[] gmlRelativePaths, MeshGranularity meshGranularity,
            int minLodBuilding, int maxLodBuilding,
            int selectedLod, PlaceMethod buildingPlaceMethod)
        {
            var config = new CityImporterConfig
            {
                meshGranularity = meshGranularity,
                UdxPathBeforeImport = testUdxPath,
                importDestPath =
                {
                    dirAssetPath = PathUtil.FullPathToAssetsPath(testOutputDir)
                },
            };
            var typeConfigs = config.gmlSearcherConfig.gmlTypeTarget.GmlTypeConfigs;
            typeConfigs[GmlType.Building].minLod = minLodBuilding;
            typeConfigs[GmlType.Building].maxLod = maxLodBuilding;
            var placeTypeConfigs = config.scenePlacementConfig.PerTypeConfigs;
            placeTypeConfigs[GmlType.Building].placeMethod = buildingPlaceMethod;
            placeTypeConfigs[GmlType.Building].selectedLod = selectedLod;
            
            this.importer.Import(gmlRelativePaths, config, out _);
        }
    }
}
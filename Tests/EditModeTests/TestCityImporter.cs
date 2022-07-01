using System;
using System.IO;
using System.Linq;
using PLATEAU.CityGML;
using NUnit.Framework;
using PLATEAU.Editor.CityImport;
using PLATEAU.Editor.Converters;
using PLATEAU.CityMeta;
using PLATEAU.Interop;
using PLATEAU.IO;
using PLATEAU.Util;
using PLATEAU.Tests.TestUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using static PLATEAU.CityMeta.ScenePlacementConfig;

namespace PLATEAU.Tests.EditModeTests
{
    [TestFixture]
    public class TestCityImporter
    {
        private CityImporter importer;
        private static readonly string testUdxPathTokyo = DirectoryUtil.TestTokyoMiniUdxPath;

        private static readonly string testOutputDir = DirectoryUtil.TempAssetFolderPath;

        private static readonly string[] testGmlRelativePathsTokyo =
        {
            "bldg/53394525_bldg_6697_2_op.gml",
            "dem/533925_dem_6697_op.gml"
        };

        private static readonly string testUdxPathSimple = DirectoryUtil.TestDataSimpleUdxPath;

        private static readonly string[] testGmlRelativePathsSimple =
        {
            "bldg/53392642_bldg_6697_op2.gml"
        };

        

        private static readonly string testDefaultCopyDestPath = Path.Combine(DirectoryUtil.TempAssetFolderPath, "PLATEAU");

        private string prevDefaultDstPath;

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
        }

        [Test]
        public void When_Inputs_Are_2_Gmls_Then_Outputs_Are_Multiple_Objs_And_1_IdTable()
        {

            Import(testUdxPathTokyo, testGmlRelativePathsTokyo, MeshGranularity.PerCityModelArea, out _, 0, 2, 1, 1);
            
            // 変換後、出力されたファイルの数を数えます。
            int objCount = 0;
            int assetCount = 0;
            foreach (var file in Directory.EnumerateFiles(testOutputDir))
            {
                if (Path.GetExtension(file) == ".obj")
                {
                    objCount++;
                }

                if (Path.GetExtension(file) == ".asset")
                {
                    assetCount++;
                }
            }
            Assert.AreEqual(4, objCount);
            Assert.AreEqual(1, assetCount);
        }

        [Test]
        public void ReferencePoint_Is_Set_To_First_ReferencePoint()
        {
            LogAssert.ignoreFailingMessages = true;
            // 2つのGMLファイルを変換します。
            Import(testUdxPathTokyo, testGmlRelativePathsTokyo, MeshGranularity.PerPrimaryFeatureObject, out var mapInfo, 0, 0);
            LogAssert.ignoreFailingMessages = false;
            
            // 値1 : CityMapInfo に記録された Reference Point を取得します。
            var recordedReferencePoint = mapInfo.cityImporterConfig.referencePoint;

            // 値2 : GmlToObjFileConverter にかけたときの Reference Point を取得します。
            string gmlFilePath = Path.Combine(testUdxPathTokyo, testGmlRelativePathsTokyo[0]);
            var cityModel = CityGml.Load(
                gmlFilePath,
                new CitygmlParserParams(),
                DllLogCallback.UnityLogCallbacks);
            var objConverter = new GmlToObjConverter();
            var firstGmlReferencePoint = objConverter.SetValidReferencePoint(cityModel);
            
            // 値1と値2は同一であることを期待します。
            Assert.AreEqual(firstGmlReferencePoint, recordedReferencePoint);
        }

        [Test]
        public void MeshGranularity_Is_Written_To_MetaData()
        {
            // 値1: 変換時の MeshGranularity の設定
            var granularityOnConvert = MeshGranularity.PerAtomicFeatureObject;
            Import(testUdxPathTokyo, testGmlRelativePathsTokyo, granularityOnConvert, out _, 0, 2, 1, 1);

            // 値2: CityMapInfo に書き込まれた MeshGranularity の値
            string metaDataPath =
                Path.Combine(PathUtil.FullPathToAssetsPath(testOutputDir), "CityMapMetaData.asset");
            var loadedMetaData = AssetDatabase.LoadAssetAtPath<CityMetaData>(metaDataPath);
            Assert.NotNull(loadedMetaData, "メタデータをロードできる");
            var granularityOnMapInfo = loadedMetaData.cityImporterConfig.meshGranularity;
            
            // 値1と値2が同一であることを期待します。
            Assert.AreEqual(granularityOnConvert, granularityOnMapInfo, "変換時の粒度設定がメタデータに記録されている");
        }

        [Test]
        public void When_CityMapInfo_Is_Already_Exist_Then_Clear_Its_Data_Before_Convert()
        {
            bool DoContainAtomic(CityMetaData info) => info.idToGmlTable.Keys.Any(id => id.Contains("_wall_"));

            Import(testUdxPathSimple, testGmlRelativePathsSimple, MeshGranularity.PerAtomicFeatureObject, out var mapInfo,  2, 2);
            
            foreach (var key in mapInfo.idToGmlTable.Keys)
            {
                Console.Write(key);
            }
            Assert.IsTrue(DoContainAtomic(mapInfo), "1回目の変換は最小地物を含むことを確認");

            Import(testUdxPathSimple, testGmlRelativePathsSimple, MeshGranularity.PerPrimaryFeatureObject, out var mapInfo2, 0, 0);
            
            bool doContainBuilding = mapInfo2.idToGmlTable.Keys.Any(id => id.Contains("_BLD_"));
            Assert.IsFalse(DoContainAtomic(mapInfo2), "2回目の変換は最小地物を含まないことを確認");
            Assert.IsTrue(doContainBuilding, "2回目の変換は主要地物を含むことを確認");
        }

        [Test]
        public void Importing_Mini_Tokyo_Ends_With_Success()
        {
            int numSuccess = Import(testUdxPathTokyo, testGmlRelativePathsTokyo,
                MeshGranularity.PerPrimaryFeatureObject, out _, 1, 1, 1, 1);

            Assert.AreEqual(2, numSuccess);
        }

        [Test]
        public void When_Lod_Is_2_to_2_Then_Only_Lod2_Objs_Are_Generated()
        {

            Import(testUdxPathSimple, testGmlRelativePathsSimple, MeshGranularity.PerCityModelArea, out _, 2, 2);

            string gmlId = "53392642_bldg_6697_op2";
            bool lod2Exists = File.Exists(Path.Combine(testOutputDir, $"LOD2_{gmlId}.obj"));
            bool lod1Exists = File.Exists(Path.Combine(testOutputDir, $"LOD1_{gmlId}.obj"));
            Assert.IsTrue(lod2Exists);
            Assert.IsFalse(lod1Exists);
        }
        
        [Test]
        public void When_Lod_Is_0_to_1_Then_Only_2_Objs_Are_Generated()
        {

            Import(testUdxPathSimple, testGmlRelativePathsSimple, MeshGranularity.PerCityModelArea, out _,  0, 1);

            string gmlId = "53392642_bldg_6697_op2";
            bool lod2Exists = File.Exists(Path.Combine(testOutputDir, $"LOD2_{gmlId}.obj"));
            bool lod1Exists = File.Exists(Path.Combine(testOutputDir, $"LOD1_{gmlId}.obj"));
            bool lod0Exists = File.Exists(Path.Combine(testOutputDir, $"LOD0_{gmlId}.obj"));
            Assert.IsFalse(lod2Exists, "LOD範囲外は生成されない");
            Assert.IsTrue(lod1Exists, "LOD範囲内は生成される");
            Assert.IsTrue(lod0Exists, "LOD範囲内は生成される");
        }

        [Test]
        public void SrcPath_Of_MetaData_Is_Set_To_Post_Copy_Path()
        {
            Import(testUdxPathSimple, testGmlRelativePathsSimple, MeshGranularity.PerCityModelArea, out var metaData, 0,
                0);
            string expectedUdxPath = Path.Combine(testDefaultCopyDestPath, "TestDataSimpleGml", "udx").Replace('\\', '/');
            string fullUdxPath = metaData.cityImporterConfig.sourcePath.FullUdxPath;
            string actualUdxPath = fullUdxPath.Replace('\\', '/');
            Assert.AreEqual( expectedUdxPath, actualUdxPath, "メモリ上のメタデータの sourcePath がコピー後を指している" );

            var metaDataPath = metaData.cityImporterConfig.importDestPath.MetaDataAssetPath;
            var loadedMetaData = AssetDatabase.LoadAssetAtPath<CityMetaData>(metaDataPath);
            Assert.NotNull(loadedMetaData, "生成後のメタデータをロードできる");
            var loadedSrcPath = fullUdxPath.Replace('\\', '/');
            Assert.AreEqual(expectedUdxPath, loadedSrcPath, "保存されたメタデータの sourcePath がコピー後を指している");
        }
        

        private int Import(string testUdxPath, string[] gmlRelativePaths, MeshGranularity meshGranularity, out CityMetaData metaData,
            int minLodBuilding, int maxLodBuilding, int minLodDem = 0, int maxLodDem = 0,
            int selectedLod = 0, PlaceMethod buildingPlaceMethod = PlaceMethod.PlaceSelectedLodOrMax)
        {
            var config = ImportUtil.MinimumConfig(testUdxPath, PathUtil.FullPathToAssetsPath(testOutputDir));
            config.meshGranularity = meshGranularity;
            var typeConf = config.objConvertTypesConfig;
            var typeLodDict = typeConf.TypeLodDict;
            typeLodDict[GmlType.Building].SetMinMax(minLodBuilding, maxLodBuilding);
            typeLodDict[GmlType.DigitalElevationModel].SetMinMax(minLodDem, maxLodDem);
            var typeLodModeDict = typeConf.TypeExportLowerLodDict;
            typeLodModeDict[GmlType.Building] = true;
            var placeTypeConfigs = config.scenePlacementConfig.PerTypeConfigs;
            placeTypeConfigs[GmlType.Building].placeMethod = buildingPlaceMethod;
            placeTypeConfigs[GmlType.Building].selectedLod = selectedLod;
            
            Debug.Log($"minLodBuilding = {minLodBuilding}, Dict[Building].Min={typeLodDict[GmlType.Building].Min}");
            
            int numSuccess = this.importer.Import(gmlRelativePaths, config, out metaData);
            return numSuccess;
        }

        

    }
}
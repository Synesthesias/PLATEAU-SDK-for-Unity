using System;
using System.IO;
using System.Linq;
using LibPLATEAU.NET.CityGML;
using NUnit.Framework;
using PlateauUnitySDK.Editor.CityImport;
using PlateauUnitySDK.Editor.Converters;
using PlateauUnitySDK.Runtime.CityMeta;
using PlateauUnitySDK.Runtime.Util;
using PlateauUnitySDK.Tests.TestUtils;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Tests.EditModeTests
{
    [TestFixture]
    public class TestCityImporter
    {
        private CityImporter importer;
        private static readonly string testUdxPathTokyo = Path.GetFullPath(Path.Combine(Application.dataPath,
            "../Packages/PlateauUnitySDK/Tests/TestData/TestDataTokyoMini/udx"));

        private static readonly string testOutputDir = DirectoryUtil.TempAssetFolderPath;

        private static readonly string[] testGmlRelativePathsTokyo =
        {
            "bldg/53394525_bldg_6697_2_op.gml",
            "dem/533925_dem_6697_op.gml"
        };

        private static readonly string testUdxPathSimple = Path.GetFullPath(Path.Combine(Application.dataPath,
            "../Packages/PlateauUnitySDK/Tests/TestData/TestDataSimpleGml/udx"));

        private static readonly string[] testGmlRelativePathsSimple =
        {
            "bldg/53392642_bldg_6697_op2.gml"
        };

        private static readonly string testDefaultDstPath = Path.Combine(DirectoryUtil.TempAssetFolderPath, "PLATEAU");

        private string prevDefaultDstPath;

        [SetUp]
        public void SetUp()
        {
            this.importer = new CityImporter();
            DirectoryUtil.SetUpTempAssetFolder();
            
            // テスト用に一時的に MultiGmlConverter のデフォルト出力先を変更します。
            this.prevDefaultDstPath = PlateauPath.StreamingGmlFolder;
            PlateauPath.TestOnly_SetStreamingGmlFolder(testDefaultDstPath);
        }

        [TearDown]
        public void TearDown()
        {
            DirectoryUtil.DeleteTempAssetFolder();
            PlateauPath.TestOnly_SetStreamingGmlFolder(this.prevDefaultDstPath);
        }

        [Test]
        public void When_Inputs_Are_2_Gmls_Then_Outputs_Are_2_Objs_And_1_IdTable()
        {
            // 2つのGMLファイルを変換します。
            var config = new CityImporterConfig
            {
                sourceUdxFolderPath = testUdxPathTokyo,
                exportFolderPath = testOutputDir
            };
            this.importer.Import(testGmlRelativePathsTokyo, config);
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
            Assert.AreEqual(2, objCount);
            Assert.AreEqual(1, assetCount);
        }

        [Test]
        public void ReferencePoint_Is_Set_To_First_ReferencePoint()
        {
            // 2つのGMLファイルを変換します。
            var config = new CityImporterConfig
            {
                sourceUdxFolderPath = testUdxPathTokyo,
                exportFolderPath = testOutputDir
            };
            this.importer.Import(testGmlRelativePathsTokyo, config);
            
            // 値1 : CityMapInfo に記録された Reference Point を取得します。
            var mapInfo = this.importer.LastConvertedCityMetaData;
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
            var config = new CityImporterConfig();
            // 値1: 変換時の MeshGranularity の設定
            var granularityOnConvert = MeshGranularity.PerAtomicFeatureObject;
            config.meshGranularity = granularityOnConvert;
            config.sourceUdxFolderPath = testUdxPathTokyo;
            config.exportFolderPath = testOutputDir;
            this.importer.Import(testGmlRelativePathsTokyo, config);

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
            bool DoContainAtomic(CityMetaData info) => info.idToGmlTable.Keys.Any(id => id.StartsWith("wall"));

            var config = new CityImporterConfig
            {
                meshGranularity = MeshGranularity.PerAtomicFeatureObject,
                sourceUdxFolderPath = testUdxPathSimple,
                exportFolderPath = testOutputDir
            };
            this.importer.Import(testGmlRelativePathsSimple, config);
            var mapInfo = this.importer.LastConvertedCityMetaData;
            foreach (var key in mapInfo.idToGmlTable.Keys)
            {
                Console.Write(key);
            }
            Assert.IsTrue(DoContainAtomic(mapInfo), "1回目の変換は最小地物を含むことを確認");

            config.meshGranularity = MeshGranularity.PerPrimaryFeatureObject;
            config.sourceUdxFolderPath = testUdxPathTokyo;
            config.exportFolderPath = testOutputDir;
            this.importer.Import(testGmlRelativePathsTokyo, config);
            mapInfo = this.importer.LastConvertedCityMetaData;
            bool doContainBuilding = mapInfo.idToGmlTable.Keys.Any(id => id.StartsWith("BLD"));
            Assert.IsFalse(DoContainAtomic(mapInfo), "2回目の変換は最小地物を含まないことを確認");
            Assert.IsTrue(doContainBuilding, "2回目の変換は主要地物を含むことを確認");
        }

        [Test]
        public void Importing_Mini_Tokyo_Ends_With_Success()
        {
            var config = new CityImporterConfig
            {
                meshGranularity = MeshGranularity.PerPrimaryFeatureObject,
                sourceUdxFolderPath = testUdxPathTokyo,
                exportFolderPath = testOutputDir
            };
            int numSuccess = this.importer.Import(testGmlRelativePathsTokyo, config);
            Assert.AreEqual(2, numSuccess);
        }
        
    }
}
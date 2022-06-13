using System.IO;
using LibPLATEAU.NET.CityGML;
using NUnit.Framework;
using PlateauUnitySDK.Editor.CityModelImportWindow;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using PlateauUnitySDK.Runtime.Util;
using PlateauUnitySDK.Tests.TestUtils;
using UnityEngine;

namespace PlateauUnitySDK.Tests.EditModeTests.TestsFileConverter
{
    [TestFixture]
    public class TestMultiGmlConverter
    {
        private MultiGmlConverter converter;
        private static string testUdxPath = Path.GetFullPath(Path.Combine(Application.dataPath,
            "../Packages/PlateauUnitySDK/Tests/TestData/TestDataTokyoMini/udx"));

        private static string testOutputDir = DirectoryUtil.TempAssetFolderPath;

        private static string[] testGmlRelativePaths =
        {
            "bldg/53394525_bldg_6697_2_op.gml",
            "dem/533925_dem_6697_op.gml"
        };

        [SetUp]
        public void SetUp()
        {
            this.converter = new MultiGmlConverter();
            DirectoryUtil.SetUpTempAssetFolder();
        }

        [TearDown]
        public void TearDown()
        {
            DirectoryUtil.DeleteTempAssetFolder();
        }

        [Test]
        public void When_Inputs_Are_2_Gmls_Then_Outputs_Are_2_Objs_And_1_IdTable()
        {
            // 2つのGMLファイルを変換します。
            var config = new CityModelImportConfig();
            this.converter.Convert(testGmlRelativePaths, testUdxPath, testOutputDir, config);
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
            var config = new CityModelImportConfig();
            this.converter.Convert(testGmlRelativePaths, testUdxPath, testOutputDir, config);
            
            // 値1 : CityMapInfo に記録された Reference Point を取得します。
            var mapInfo = this.converter.LastConvertedCityMapInfo;
            var recordedReferencePoint = mapInfo.ReferencePoint;

            // 値2 : GmlToObjFileConverter にかけたときの Reference Point を取得します。
            string gmlFilePath = Path.Combine(testUdxPath, testGmlRelativePaths[0]);
            var cityModel = CityGml.Load(
                gmlFilePath,
                new CitygmlParserParams(),
                DllLogCallback.UnityLogCallbacks);
            var objConverter = new GmlToObjFileConverter();
            var firstGmlReferencePoint = objConverter.SetValidReferencePoint(cityModel);
            
            // 値1と値2は同一であることを期待します。
            Assert.AreEqual(firstGmlReferencePoint, recordedReferencePoint);
        }
        
    }
}
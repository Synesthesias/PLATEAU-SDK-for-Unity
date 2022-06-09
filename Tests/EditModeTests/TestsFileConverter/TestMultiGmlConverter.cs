using System.IO;
using NUnit.Framework;
using PlateauUnitySDK.Editor.CityModelImportWindow;
using PlateauUnitySDK.Editor.FileConverter.Converters;
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
            string[] gmlRelativePaths =
            {
                "bldg/53394525_bldg_6697_2_op.gml",
                "dem/533925_dem_6697_op.gml"
            };
            var config = new CityModelImportConfig();
            string outputDir = DirectoryUtil.TempAssetFolderPath;
            this.converter.Convert(gmlRelativePaths, testUdxPath, outputDir, config);
            // 変換後、出力されたファイルの数を数えます。
            int objCount = 0;
            int assetCount = 0;
            foreach (var file in Directory.EnumerateFiles(outputDir))
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
        
    }
}
using System.IO;
using NUnit.Framework;
using PlateauUnitySDK.Editor.FileConverter;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using PlateauUnitySDK.Tests.TestUtils;

namespace PlateauUnitySDK.Tests.EditModeTests.TestsFileConverter
{
    [TestFixture]
    public class TestGmlToIdFileConverter
    {
        [SetUp]
        public void SetUp()
        {
            DirectoryUtil.SetUpTempAssetFolder();
        }

        [TearDown]
        public void TearDown()
        {
            DirectoryUtil.DeleteTempAssetFolder();
        }

        [Test]
        public void Convert_Generates_Table_File()
        {
            var outputFilePath = Path.Combine(DirectoryUtil.TempAssetFolderPath, "table.asset");
            outputFilePath = FilePathValidator.FullPathToAssetsPath(outputFilePath);
            var converter = new GmlToCityMapInfoConverter();
            converter.Convert(DirectoryUtil.TestSimpleGmlFilePath, outputFilePath);
            // 変換後、ファイルがあれば良しとします。
            Assert.IsTrue(File.Exists(outputFilePath));
        }
    }
}
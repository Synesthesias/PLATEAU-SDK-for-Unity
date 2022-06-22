using System.IO;
using NUnit.Framework;
using PLATEAU.Editor.Converters;
using PLATEAU.Util;
using PLATEAU.Tests.TestUtils;

namespace PLATEAU.Tests.EditModeTests
{
    [TestFixture]
    public class TestGmlToIdConverter
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
            outputFilePath = PathUtil.FullPathToAssetsPath(outputFilePath);
            var converter = new GmlToCityMetaDataConverter();
            converter.Convert(DirectoryUtil.TestSimpleGmlFilePath, outputFilePath);
            // 変換後、ファイルがあれば良しとします。
            Assert.IsTrue(File.Exists(outputFilePath));
        }
    }
}
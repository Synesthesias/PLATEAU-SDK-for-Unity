using NUnit.Framework;
using PLATEAU.Editor.Converters;
using PLATEAU.Tests.TestUtils;
using System.IO;

namespace PLATEAU.Tests.EditModeTests
{
    [TestFixture]
    public class TestObjToFbxConverter
    {

        private static readonly string testObjFileName = "53392642_bldg_6697_op2_obj.obj";

        private static readonly string objFileCopySrc =
            Path.Combine(DirectoryUtil.TestDataSimplePath, testObjFileName);

        private static readonly string testObjFilePath =
            Path.Combine(DirectoryUtil.TempAssetFolderPath, testObjFileName);

        private static readonly string destFbxFilePath =
            Path.Combine(DirectoryUtil.TempCacheFolderPath, "converted_fbx.fbx");

        [SetUp]
        public void SetUp()
        {
            // objファイルのコンバートは変換元がAssetsフォルダの中にないと動かないので、
            // テストデータを Assets/TemporaryUnitTest フォルダにコピーします。
            DirectoryUtil.SetUpTempAssetFolder();
            DirectoryUtil.SetUpTempCacheFolder();
            DirectoryUtil.CopyFileToTempAssetFolder(objFileCopySrc, testObjFileName);
        }

        [TearDown]
        public void TearDown()
        {
            DirectoryUtil.DeleteTempAssetFolder();
        }

        [Test]
        public void Convert_Generates_Fbx_File()
        {
            var converter = new ObjToFbxConverter();
            converter.Convert(testObjFilePath, destFbxFilePath);
            Assert.IsTrue(File.Exists(destFbxFilePath), "変換後、fbxファイルが存在する");
            // fbxファイルの中身まではチェック未実装です。
        }
    }
}
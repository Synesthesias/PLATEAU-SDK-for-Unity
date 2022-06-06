using System;
using System.IO;
using NUnit.Framework;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using PlateauUnitySDK.Tests.TestUtils;

namespace PlateauUnitySDK.Tests.EditModeTests.TestsFileConverter
{
    [TestFixture]
    public class TestObjToFbxFileConverter
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
            // 変換後、fbxファイルが存在すれば良しとします。
            var converter = new ObjToFbxFileConverter();
            converter.Convert(testObjFilePath, destFbxFilePath);
            Assert.IsTrue(File.Exists(destFbxFilePath));
        }
    }
}
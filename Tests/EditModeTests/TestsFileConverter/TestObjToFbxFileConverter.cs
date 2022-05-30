using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Internal.Commands;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using PlateauUnitySDK.Tests.TestUtils;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Tests.EditModeTests.TestsFileConverter
{
    [TestFixture]
    public class TestObjToFbxFileConverter
    {
        private static readonly string temporaryAssetPath =
            Path.Combine(Application.dataPath, "TemporaryUnitTest");

        private static readonly string testObjFileName = "53392642_bldg_6697_op2_obj.obj";

        private static readonly string objFileCopySrc =
            Path.Combine(DirectoryUtil.TestDataFolderPath, testObjFileName);

        private static readonly string testObjFilePath =
            Path.Combine(temporaryAssetPath, testObjFileName);

        private static readonly string destFbxFilePath =
            Path.Combine(DirectoryUtil.TestCacheTempFolderPath, "converted_fbx.fbx");
        
        [SetUp]
        public void SetUp()
        {
            // objファイルのコンバートは変換元がAssetsフォルダの中にないと動かないので、
            // テストデータを Assets/TemporaryUnitTest フォルダにコピーします。
            DirectoryUtil.SetUpEmptyDir(temporaryAssetPath);
            File.Copy(objFileCopySrc, testObjFilePath);
            AssetDatabase.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            // テスト用の一時フォルダを消します。
            Directory.Delete(temporaryAssetPath, true);
            File.Delete(temporaryAssetPath + ".meta");
            AssetDatabase.Refresh();
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
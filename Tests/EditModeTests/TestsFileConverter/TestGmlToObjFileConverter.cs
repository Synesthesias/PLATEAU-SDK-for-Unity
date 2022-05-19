using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using PlateauUnitySDK.Tests.TestUtils;
using UnityEditor.VersionControl;
using UnityEngine;

namespace PlateauUnitySDK.Tests.EditModeTests.TestsFileConverter
{
    [TestFixture]
    public class TestGmlToObjFileConverter
    {
        private static readonly string testGmlFilePath =
            Path.Combine(DirectoryUtil.TestDataFolderPath, "53392642_bldg_6697_op2.gml");
        
        [SetUp]
        public void SetUp()
        {
            DirectoryUtil.SetUpCacheTempFolder();         
        }

        [TearDown]
        public void TearDown()
        {
            // TODO テスト後に一時ファイルを消そうとしても他のプロセスが使っているためエラー。
            // .mat も .obj も使用中らしい。
            // DeleteAllInDirectory(temporaryFolderPath);
        }
        
        [Test]
        public void Convert_Generates_Obj_File()
        {
            var outputFilePath = Path.Combine(DirectoryUtil.TestCacheTempFolderPath, "exported.obj");
            var converter = new GmlToObjFileConverter();
            converter.Convert(testGmlFilePath, outputFilePath);
            Assert.IsTrue(File.Exists(outputFilePath));
        }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor.VersionControl;
using UnityEngine;

namespace PlateauUnitySDK.Tests.EditModeTests.TestsFileConverter
{
    [TestFixture]
    public class TestGmlToObjFileConverter
    {
        private static readonly string testGmlFilePath =
            Path.GetFullPath("Packages/PlateauUnitySDK/Tests/TestData/53392642_bldg_6697_op2.gml");

        private static readonly string temporaryFolderPath =
            Path.Combine(Application.temporaryCachePath, "UnitTestTemporary");

        [SetUp]
        public void SetUp()
        {
            // 空のテスト用の一時ディレクトリを用意します。
            if (!Directory.Exists(temporaryFolderPath))
            {
                Directory.CreateDirectory(temporaryFolderPath);
            }
            DeleteAllInDirectory(temporaryFolderPath);            
        }

        [TearDown]
        public void TearDown()
        {
            // TODO ここで消そうとすると他のプロセスが使っているためエラー。
            // .mat も .obj も使用中らしい。
            // DeleteAllInDirectory(temporaryFolderPath);
        }
        
        [Test]
        public void Test_Convert()
        {
            var outputFilePath = Path.Combine(temporaryFolderPath, "exported.obj");
            var converter = new GmlToObjFileConverter();
            converter.Convert(testGmlFilePath, outputFilePath);
            Assert.IsTrue(File.Exists(outputFilePath));
        }

        public static void DeleteAllInDirectory(string dirPath)
        {
            var directoryInfo = new DirectoryInfo(dirPath);
            foreach (var file in directoryInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (var dir in directoryInfo.GetDirectories())
            {
                dir.Delete(true);
            }
        }
    }
}

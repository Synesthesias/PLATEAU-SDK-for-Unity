using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibPLATEAU.NET.CityGML;
using NUnit.Framework;
using PlateauUnitySDK.Editor.FileConverter;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using PlateauUnitySDK.Tests.TestUtils;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace PlateauUnitySDK.Tests.EditModeTests.TestsFileConverter
{
    [TestFixture]
    public class TestGmlToObjFileConverter
    {
        

        [SetUp]
        public void SetUp()
        {
            DirectoryUtil.SetUpTempAssetFolder();
            DirectoryUtil.SetUpTempCacheFolder();
        }

        [TearDown]
        public void TearDown()
        {
            DirectoryUtil.DeleteTempAssetFolder();
            DirectoryUtil.DeleteTempCacheFolder();
        }
        
        [Test]
        public void Convert_Generates_Obj_File()
        {
            string outputFilePath = Path.Combine(DirectoryUtil.TempCacheFolderPath, "test_convert_generates_obj_file.obj");
            using (var converter = new GmlToObjFileConverter())
            {
                converter.Convert(DirectoryUtil.TestGmlFilePath, outputFilePath);
            }
            // 変換後、objファイルがあればとりあえず良しとします。
            Assert.IsTrue(File.Exists(outputFilePath));
            }

        [Test]
        public void If_MeshGranularity_Is_PerCityModelArea_Then_Mesh_Count_Is_One()
        {
            var meshes = ConvertAndRead(MeshGranularity.PerCityModelArea);
            int count = meshes.Length;
            Debug.Log($"Count : {count}");
            Assert.AreEqual(1, count);
        }
        
        [Test]
        public void If_MeshGranularity_Is_PerPrimaryFeatureObject_Then_Multiple_BLD_Are_Exported()
        {
            var meshes = ConvertAndRead(MeshGranularity.PerPrimaryFeatureObject);
            int count = meshes.Length;
            Debug.Log($"mesh count : {count}");
            Assert.Greater(count, 1);
            for (int i = 0; i < count; i++)
            {
                var mesh = meshes[i];
                Assert.IsTrue(mesh.name.Contains("BLD"));
            }
        }
        
        [Test]
        public void If_MeshGranularity_Is_PerAtomicFeatureObject_Then_Multiple_Walls_Are_Exported()
        {
            var meshes = ConvertAndRead(MeshGranularity.PerAtomicFeatureObject);
            int count = meshes.Length;
            Debug.Log($"count : {count}");
            Assert.Greater(count, 1);
            int wallCount = 0;
            foreach (var mesh in meshes)
            {
                if (mesh.name.Contains("wall"))
                {
                    wallCount++;
                }
            }
            Assert.Greater(wallCount, 1);
        }

        private static Mesh[] ConvertAndRead(MeshGranularity meshGranularity)
        {
            string inputFilePath = DirectoryUtil.TestGmlFilePath;
            string outputFilePath = Path.Combine(DirectoryUtil.TempAssetFolderPath, "exported.obj");
            using (var converter = new GmlToObjFileConverter())
            {
                converter.SetConfig(meshGranularity, AxesConversion.RUF, true);
                bool result = converter.Convert(inputFilePath, outputFilePath);
                Assert.IsTrue(result);
            }
            AssetDatabase.Refresh();

            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(FilePathValidator.FullPathToAssetsPath(outputFilePath));
            var meshes = obj.GetComponentsInChildren<MeshFilter>().Select(mf => mf.sharedMesh).ToArray();
            return meshes;
        }
        
    }
}

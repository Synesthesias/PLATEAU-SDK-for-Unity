using System.IO;
using System.Linq;
using LibPLATEAU.NET.CityGML;
using NUnit.Framework;
using PlateauUnitySDK.Editor.Converters;
using PlateauUnitySDK.Runtime.Util;
using PlateauUnitySDK.Tests.TestUtils;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Tests.EditModeTests
{
    [TestFixture]
    public class TestGmlToObjConverter
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
            using (var converter = new GmlToObjConverter())
            {
                converter.Convert(DirectoryUtil.TestSimpleGmlFilePath, outputFilePath);
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
            string inputFilePath = DirectoryUtil.TestSimpleGmlFilePath;
            string outputFilePath = Path.Combine(DirectoryUtil.TempAssetFolderPath, "exported.obj");
            using (var converter = new GmlToObjConverter())
            {
                var conf = converter.Config;
                conf.MeshGranularity = meshGranularity;
                conf.AxesConversion = AxesConversion.RUF;
                conf.OptimizeFlag = true;
                converter.Config = conf;
                bool result = converter.Convert(inputFilePath, outputFilePath);
                Assert.IsTrue(result);
            }
            AssetDatabase.Refresh();

            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(PathUtil.FullPathToAssetsPath(outputFilePath));
            var meshes = obj.GetComponentsInChildren<MeshFilter>().Select(mf => mf.sharedMesh).ToArray();
            return meshes;
        }
        
    }
}

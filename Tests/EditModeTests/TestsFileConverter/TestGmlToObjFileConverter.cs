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
            // TODO コメントアウトを外す
            // DirectoryUtil.DeleteTempAssetFolder();
            // DirectoryUtil.DeleteTempCacheFolder();
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
        public void If_MeshGranularity_Is_PerPrimaryFeatureObject()
        {
            var meshes = ConvertAndRead(MeshGranularity.PerPrimaryFeatureObject);
            int count = meshes.Length; // TODO この数値が1になるのはおかしい
            Debug.Log($"mesh count : {count}");
            for (int i = 0; i < count; i++)
            {
                var mesh = meshes[i];
                Debug.Log(mesh.name);
                Assert.IsTrue(mesh.name.Contains("BLD"));
            }
        }
        
        [Test]
        public void If_MeshGranularity_Is_PerAtomicFeatureObject()
        {
            var meshes = ConvertAndRead(MeshGranularity.PerAtomicFeatureObject);
            int count = meshes.Length;
            Debug.Log($"count : {count}"); // TODO この数値が1になるのはおかしい
            foreach (var mesh in meshes)
            {
                Debug.Log($"mesh : {mesh.name}");
            }
            // TODO
        }

        private static Mesh[] ConvertAndRead(MeshGranularity meshGranularity)
        {
            string inputFilePath = DirectoryUtil.TestGmlFilePath;
            string outputFilePath = Path.Combine(DirectoryUtil.TempAssetFolderPath, "exported.obj");
            using (var converter = new GmlToObjFileConverter())
            {
                converter.SetConfig(meshGranularity, AxesConversion.RUF, true);
                Debug.Log($"Converting {inputFilePath}");
                bool result = converter.Convert(inputFilePath, outputFilePath);
                Assert.IsTrue(result);
            }
            // AssetDatabase.ImportAsset(FilePathValidator.FullPathToAssetsPath(outputFilePath));
            AssetDatabase.Refresh();

            foreach (var foundObj in AssetDatabase.LoadAllAssetsAtPath(
                         FilePathValidator.FullPathToAssetsPath(outputFilePath)))
            {
                Debug.Log($"type = {foundObj.GetType()}, name = {foundObj.name}");
            }
            // var fbxConverter = new ObjToFbxFileConverter();
            // fbxConverter.Convert(FilePathValidator.FullPathToAssetsPath(outputFilePath), outputFilePath + ".fbx");

            // var meshes = AssetDatabase.LoadAllAssetsAtPath(FilePathValidator.FullPathToAssetsPath(outputFilePath))
            //     .OfType<Mesh>()
            //     .ToArray();

            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(FilePathValidator.FullPathToAssetsPath(outputFilePath));
            var meshes = obj.GetComponentsInChildren<MeshFilter>().Select(mf => mf.sharedMesh).ToArray();
            foreach (var m in meshes)
            {
                Debug.Log($"mesh name: {m.name}");
            }
            return meshes;
        }
        
    }
}

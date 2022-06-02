using System;
using System.IO;
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
            // DirectoryUtil.SetUpTempCacheFolder();
        }

        [TearDown]
        public void TearDown()
        {
            DirectoryUtil.DeleteTempAssetFolder();
            // DirectoryUtil.DeleteTempCacheFolder();
        }
        
        [Test]
        public void Convert_Generates_Obj_File()
        {
            string outputFilePath = Path.Combine(DirectoryUtil.TempAssetFolderPath, "test_convert_generates_obj_file.obj");
            var converter = new GmlToObjFileConverter();
            converter.Convert(DirectoryUtil.TestGmlFilePath, outputFilePath);
            // 変換後、objファイルがあればとりあえず良しとします。
            Assert.IsTrue(File.Exists(outputFilePath));
        }

        [Test]
        public void If_MeshGranularity_Is_PerCityModelArea_Then_Child_Obj_Count_Is_One()
        {
            string outputFilePath = Path.Combine(DirectoryUtil.TempAssetFolderPath, "test_meshgranularity_is_percitymodelarea.obj");
            var converter = new GmlToObjFileConverter();
            converter.SetConfig(MeshGranularity.PerCityModelArea);
            converter.Convert(DirectoryUtil.TestGmlFilePath, outputFilePath);
            AssetDatabase.Refresh();
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(FilePathValidator.FullPathToAssetsPath(outputFilePath));
            int childCount = go.transform.childCount;
            Debug.Log($"MeshGranularity.PerCityModelArea : Child GameObj Count : {childCount}");
            Assert.AreEqual(1, childCount);
        }
        
    }
}

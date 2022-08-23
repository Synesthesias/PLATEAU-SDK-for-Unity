using NUnit.Framework;
using PLATEAU.Editor.Converters;
using PLATEAU.IO;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Tests.EditModeTests
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
        public void Convert_Generates_Obj_File_And_Returns_Generated_File_Name()
        {
            string outputDirectory = DirectoryUtil.TempCacheFolderPath;
            string[] exportedFilePaths;
            using (var converter = new GmlToObjConverter())
            {
                converter.Convert(DirectoryUtil.TestSimpleGmlFilePath, outputDirectory, out exportedFilePaths);
            }
            bool fileExists = File.Exists(Path.Combine(outputDirectory, "LOD0_53392642_bldg_6697_op2.obj"));
            Assert.IsTrue(fileExists, "変換後、objファイルが存在する");
            Assert.IsTrue(exportedFilePaths.Any(name => name.Contains("LOD0_53392642_bldg_6697_op2.obj")), "戻り値に出力ファイル名が含まれる");
            // objファイルの中身まではチェック未実装です。
        }

        [Test]
        public void If_MeshGranularity_Is_PerCityModelArea_Then_Mesh_Count_Is_One()
        {
            var meshes = ConvertAndRead(MeshGranularity.PerCityModelArea, 2);
            int count = meshes.Length;
            Debug.Log($"Count : {count}");
            Assert.AreEqual(1, count, "メッシュを結合する設定のとき、オブジェクト数は1である");
        }

        [Test]
        public void If_MeshGranularity_Is_PerPrimaryFeatureObject_Then_Multiple_BLD_Are_Exported()
        {
            var meshes = ConvertAndRead(MeshGranularity.PerPrimaryFeatureObject, 2);
            int count = meshes.Length;
            Debug.Log($"mesh count : {count}");
            Assert.Greater(count, 1, "粒度が主要地物のとき、2つ以上のメッシュが生成される");
            for (int i = 0; i < count; i++)
            {
                var mesh = meshes[i];
                Assert.IsTrue(mesh.name.Contains("BLD"), "粒度が主要地物のとき、各メッシュ名が BLD を含む（BLDは建物を意味する）");
            }
        }

        [Test]
        public void If_MeshGranularity_Is_PerAtomicFeatureObject_Then_Multiple_Walls_Are_Exported()
        {
            var meshes = ConvertAndRead(MeshGranularity.PerAtomicFeatureObject, 2);
            int count = meshes.Length;
            Debug.Log($"count : {count}");
            Assert.Greater(count, 1, "粒度が最小地物のとき、2つ以上のメッシュが生成される");
            int wallCount = 0;
            foreach (var mesh in meshes)
            {
                if (mesh.name.Contains("wall"))
                {
                    wallCount++;
                }
            }
            Assert.Greater(wallCount, 1, "粒度が最小地物のとき、生成されるメッシュのうち2つ以上は wall という名前を含む");
        }

        private static Mesh[] ConvertAndRead(MeshGranularity meshGranularity, int lod)
        {
            string inputFilePath = DirectoryUtil.TestSimpleGmlFilePath;
            string outputDirectory = DirectoryUtil.TempAssetFolderPath;
            using (var converter = new GmlToObjConverter())
            {
                var conf = converter.Config;
                conf.MeshGranularity = meshGranularity;
                conf.ExportAppearance = false;
                converter.Config = conf;
                bool result = converter.Convert(inputFilePath, outputDirectory, out _);
                Assert.IsTrue(result, "objへの変換が成功する");
            }
            AssetDatabase.Refresh();
            string outputFilePath = Path.Combine(outputDirectory, $"LOD{lod}_{Path.GetFileNameWithoutExtension(inputFilePath)}.obj");
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(PathUtil.FullPathToAssetsPath(outputFilePath));
            var meshes = obj.GetComponentsInChildren<MeshFilter>().Select(mf => mf.sharedMesh).ToArray();
            return meshes;
        }

    }
}

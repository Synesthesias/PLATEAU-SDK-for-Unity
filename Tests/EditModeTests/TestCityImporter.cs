using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PLATEAU.CityImport.Setting;
using PLATEAU.Tests.TestUtils;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace PLATEAU.Tests.EditModeTests
{
    [TestFixture]
    public class TestCityImporter
    {
        private static CityLoadConfig config;

        /// <summary> インポート時、テストデータはこのパスにコピーされることを確認します。 </summary>
        private const string testDataFetchPath = "Assets/StreamingAssets/.PLATEAU/TestDataTokyoMini";
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DeleteFetchedTestDir();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DeleteFetchedTestDir();
        }
        
        [UnityTest]
        public IEnumerator TestImport()
        {
            var cityDefinition = TestCityDefinition.MiniTokyo;
            yield return cityDefinition.Import(out config).AsIEnumerator();
            
            // GMLファイルとその関連ファイルが Assets/StreamingAssets/.PLATEAU にコピーされることを確認します。
            AssertFilesExist(
                testDataFetchPath,
                cityDefinition.GmlDefinitions.Select(def => def.GmlPath).ToArray()
            );

            // ゲームオブジェクトが生成されることを確認します。
            var expectedObjNames =
                cityDefinition
                    .GmlDefinitions
                    .Where(def => def.ContainsMesh)
                    .Select(def => def.GameObjName)
                    .ToList();

            AssertGameObjsExist(expectedObjNames.Concat(new []{"LOD0", "LOD1", "LOD2"}));
            AssertChildHaveMesh(expectedObjNames);
        }

        public static void DeleteFetchedTestDir()
        {
            string fullPath = Path.GetFullPath(testDataFetchPath);
            if (!Directory.Exists(fullPath)) return;
            Directory.Delete(Path.GetFullPath(fullPath), true);
        }

        private static void AssertFilesExist(string basePath, params string[] relativePaths)
        {
            foreach(string relativePath in relativePaths)
            {
                string path = Path.GetFullPath(Path.Combine(basePath, relativePath));
                Assert.IsTrue(File.Exists(path), $"次のパスにファイルが存在する : {path}");
            }
        }

        private static void AssertGameObjsExist(IEnumerable<string> objNames)
        {
            foreach (string objName in objNames)
            {
                SceneUtil.AssertGameObjExists(objName);
            }
        }

        private static void AssertChildHaveMesh(IEnumerable<string> objNames)
        {
            foreach (string objName in objNames)
            {
                var obj = GameObject.Find(objName);
                var meshFilter = obj.GetComponentInChildren<MeshFilter>();
                Assert.NotNull(meshFilter, $"{objName} にMeshFilterが存在する");
                var meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                Assert.NotNull(meshRenderer, $"{objName} にMeshRendererが存在する");
                var mesh = meshFilter.sharedMesh;
                Assert.NotNull(mesh, $"{objName} にmeshが存在する");
                Assert.Greater(mesh.vertexCount, 0, $"{objName} のメッシュに頂点が存在する");
                Assert.Greater(mesh.triangles.Length, 2, $"{objName} のメッシュにポリゴンが存在する");
                Assert.Greater(mesh.subMeshCount, 0, $"{objName} のメッシュにサブメッシュが存在する");
                Assert.AreEqual(mesh.indexFormat, IndexFormat.UInt32, $"{objName} のメッシュが32bit範囲の頂点数に対応する");
            }
        }
    }
}

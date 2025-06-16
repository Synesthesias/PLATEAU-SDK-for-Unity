using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PLATEAU.Tests.TestUtils;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace PLATEAU.Tests.EditModeTests
{
    [TestFixture]
    public class TestCityImporter
    {

        /// <summary> リモートインポートでは一時フォルダが使用されるため、このパスにはファイルが残らないことを確認します。 </summary>
        private const string testDataFetchPath = "Assets/StreamingAssets/.PLATEAU";

        [UnityTest]
        public IEnumerator TestImportLocal()
        {
            var cityDefinition = TestCityDefinition.MiniTokyo;
            LogAssert.ignoreFailingMessages = true;
            yield return cityDefinition.ImportLocal().AsIEnumerator();
            LogAssert.ignoreFailingMessages = false;
            
            // ローカルインポートの場合は、元のファイルが元の場所にそのまま存在することを確認します。
            // Fetch処理を省略しているため、StreamingAssetsにはコピーされません。
            cityDefinition.AssertFilesExist(cityDefinition.SrcRootDirPathLocal);

            // ゲームオブジェクトが生成されることを確認します。
            AssertGameObjsExist(cityDefinition.ExpectedObjNames.Concat(new []{"LOD0", "LOD1", "LOD2"}));
            AssertChildHaveMesh(cityDefinition.ExpectedObjNames);
        }

        [UnityTest]
        [Ignore("モックサーバーが動いていないためテストをスキップ")]
        public IEnumerator TestImportServer()
        {
            var cityDefinition = TestCityDefinition.TestServer23Ku;
            yield return cityDefinition.ImportServer().AsIEnumerator();
            
            // リモートインポートではダウンロードしたファイルは一時フォルダに保存され、インポート後に削除される。
            // そのため、StreamingAssetsにファイルが残っていないことを確認します。
            string expectedPath = testDataFetchPath + "/13100_tokyo23-ku_2020_citygml_3_2_op";
            Assert.False(Directory.Exists(expectedPath), $"一時ファイルが削除されていることを確認: {expectedPath}");
            
            // ゲームオブジェクトが生成されることを確認します。
            AssertGameObjsExist(cityDefinition.ExpectedObjNames.Concat(new []{"LOD0", "LOD1", "LOD2"}));
            AssertChildHaveMesh(cityDefinition.ExpectedObjNames);
            
        }

        public static void DeleteFetchedTestDir()
        {
            string fullPath = Path.GetFullPath(testDataFetchPath);
            if (!Directory.Exists(fullPath)) return;
            Directory.Delete(Path.GetFullPath(fullPath), true);
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

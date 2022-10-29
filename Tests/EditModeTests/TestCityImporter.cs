using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PLATEAU.CityImport.Load;
using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
using PLATEAU.Interop;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Udx;
using PLATEAU.Util.Async;
using UnityEngine;
using UnityEngine.SceneManagement;
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

            var expectedObjNames =
                new[] { "LOD0", "LOD1", "LOD2" }
                    .Concat(
                        cityDefinition
                            .GmlDefinitions
                            .Where(def => def.ContainsMesh)
                            .Select(def => Path.GetFileName(def.GmlPath))
                    ).ToArray();
            AssertGameObjsExist(expectedObjNames);

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

        private static void AssertGameObjsExist(params string[] objNames)
        {
            foreach (string objName in objNames)
            {
                SceneUtil.AssertGameObjExists(objName);
            }
        }
    }
}

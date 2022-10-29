using System.Collections;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using PLATEAU.CityImport.Load;
using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Udx;
using PLATEAU.Util.Async;
using UnityEngine;
using UnityEngine.TestTools;

namespace PLATEAU.Tests.EditModeTests
{
    [TestFixture]
    public class TestCityImporter
    {
        private static CityLoadConfig config;

        /// <summary> テストデータのパスです。 </summary>
        private const string testDataPath =
            "Packages/com.synesthesias.plateau-unity-sdk/Tests/TestData/TestDataTokyoMini";

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
        public IEnumerator ImportCopiesSrcFilesToStreamingAssets()
        {
            yield return ImportMiniTokyo(out config).AsIEnumerator();
            
            string path = testDataFetchPath;
            Assert.IsTrue(Directory.Exists(path));
            Assert.IsTrue(File.Exists(path + "/codelists/Common_districtsAndZonesType.xml"));
            Assert.IsTrue(File.Exists(path + "/udx/bldg/53392546_bldg_6697_2_op.gml"));
            Assert.IsTrue(File.Exists(path + "/udx/bldg/53392547_bldg_6697_2_op.gml"));
            Assert.IsTrue(File.Exists(path + "/udx/bldg/53392547_bldg_6697_2_op.gml"));
            // TODO 下のコメントアウトAssertが通るようにする
            // Assert.IsTrue(File.Exists(path + "/udx/brid/53394525_brid_6697_op.gml"));
            Assert.IsTrue(File.Exists(path + "/udx/dem/533925_dem_6697_op.gml"));
            
            Assert.IsTrue(File.Exists(path + "/udx/tran/533925_tran_6697_op.gml"));
            

        }

        private static Task ImportMiniTokyo(out CityLoadConfig outConfig)
        {
            var progressDisplay = new ProgressDisplayGUI();
            outConfig = new CityLoadConfig();
            // TODO どのパッケージと何が対応するかは要テスト
            uint packageFlagsAll = 0b10000000000000000000000011111111;
            outConfig.InitWithPackageFlags((PredefinedCityModelPackage)packageFlagsAll);
            outConfig.AreaMeshCodes = new[]
            {
                "53394525", "53392546", "53392547", "533925"
            };
            //TODO com.の部分を共通化
            outConfig.SourcePathBeforeImport =
                Path.GetFullPath(testDataPath);

            var task = CityImporter.ImportAsync(outConfig, progressDisplay);
            return task;
        }

        private static void DeleteFetchedTestDir()
        {
            string fullPath = Path.GetFullPath(testDataFetchPath);
            if (!Directory.Exists(fullPath)) return;
            Directory.Delete(Path.GetFullPath(fullPath), true);
        }
    }
}

using System.Collections;
using System.IO;
using System.Threading;
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
            
            AssertFilesExist(
                basePath: testDataFetchPath,
                "codelists/Common_districtsAndZonesType.xml",
                "udx/bldg/53392546_bldg_6697_2_op.gml",
                "udx/bldg/53392547_bldg_6697_2_op.gml",
                "udx/bldg/53392547_bldg_6697_2_op.gml",
                "udx/brid/53394525_brid_6697_op.gml",
                "udx/brid/53394525_brid_6697_appearance/skjp6776.jpg",
                "udx/dem/533925_dem_6697_op.gml",
                "udx/frn/53394525_frn_6697_sjkms_op.gml",
                "udx/frn/53394525_frn_6697_sjkms_appearance/17992.jpg",
                "udx/lsld/533925_lsld_6668_op.gml",
                "udx/luse/533925_luse_6668_2_op.gml",
                "udx/luse/533925_luse_6697_park_op.gml",
                "udx/tran/533925_tran_6697_op.gml",
                "udx/urf/533925_urf_6668_boka_op.gml",
                "udx/urf/533925_urf_6668_kodo_op.gml",
                "udx/urf/533925_urf_6668_yoto_op.gml"
                );


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
            foreach (var packageConf in outConfig.ForEachPackagePair)
            {
                packageConf.Value.includeTexture = true;
            }
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

        private static void AssertFilesExist(string basePath, params string[] relativePaths)
        {
            foreach(string relativePath in relativePaths)
            {
                string path = Path.GetFullPath(Path.Combine(basePath, relativePath));
                Assert.IsTrue(File.Exists(path), $"次のパスにファイルが存在する : {path}");
            }
        }
    }
}

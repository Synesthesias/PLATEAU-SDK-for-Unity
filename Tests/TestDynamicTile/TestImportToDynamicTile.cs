using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.CityImport.Import;
using PLATEAU.Dataset;
using PLATEAU.Editor.DynamicTile;
using PLATEAU.DynamicTile;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PLATEAU.Tests.TestDynamicTile
{
    [TestFixture]
    public class TestImportToDynamicTile
    {
        private const string OutputDirAssetsPath = "Assets/PLATEAUPrefabsTestDyn";

        [SetUp]
        public void SetUp()
        {
            // 出力先フォルダを作成
            if (!AssetDatabase.IsValidFolder(OutputDirAssetsPath))
            {
                var parent = "Assets";
                var folderName = Path.GetFileName(OutputDirAssetsPath);
                if (!AssetDatabase.IsValidFolder(OutputDirAssetsPath))
                {
                    AssetDatabase.CreateFolder(parent, folderName);
                }
                AssetDatabase.Refresh();
            }
        }

        [TearDown]
        public void TearDown()
        {
            // 生成物のうちテスト用出力先（Assets 配下）を掃除
            if (AssetDatabase.IsValidFolder(OutputDirAssetsPath))
            {
                AssetDatabase.DeleteAsset(OutputDirAssetsPath);
                AssetDatabase.Refresh();
            }
        }

        [UnityTest]
        public IEnumerator Test_Import_Local_To_DynamicTile_Succeeds()
        {
            // Arrange: MiniTokyo のテスト定義からローカルデータを使用
            var cityDef = TestCityDefinition.MiniTokyo;

            // CityImportConfig を TestCityDefinition.MakeConfig 相当で生成
            var allPackages = EnumUtil.EachFlags(PredefinedCityModelPackageExtension.All());
            var allPackageLods = new PackageToLodDict();
            foreach (var package in allPackages)
            {
                allPackageLods.MergePackage(package, 3);
            }

            IDatasetSourceConfig datasetSourceConfig = new DatasetSourceConfigLocal(cityDef.SrcRootDirPathLocal);
            var dummyAreaSelectResult = new AreaSelectResult(new ConfigBeforeAreaSelect(datasetSourceConfig, cityDef.CoordinateZoneId), cityDef.AreaGridCodes, AreaSelectResult.ResultReason.Confirm);
            var conf = CityImportConfig.CreateWithAreaSelectResult(dummyAreaSelectResult);
            foreach (var pair in conf.PackageImportConfigDict.ForEachPackagePair)
            {
                pair.Value.IncludeTexture = true;
            }

            // 動的タイル設定を有効化（Assets 配下に出力）
            conf.DynamicTileImportConfig.ImportType = ImportType.DynamicTile;
            conf.DynamicTileImportConfig.OutputPath = OutputDirAssetsPath;

            // Act: ImportToDynamicTile を実行
            ImportToDynamicTile importer = new ImportToDynamicTile(null);
            var cts = new CancellationTokenSource();
            var task = importer.ExecAsync(conf, cts.Token);
            yield return task.AsIEnumerator();

            // Assert: マネージャーが生成され、カタログが保存されていること
            var manager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            Assert.IsNotNull(manager, "PLATEAUTileManager が生成されている");

            var catalogPath = manager.CatalogPath;
            Assert.IsFalse(string.IsNullOrEmpty(catalogPath), "カタログパスが保存されている");
            Assert.IsTrue(File.Exists(catalogPath), $"カタログファイルが存在する: {catalogPath}");

            // メタに 1 件以上のタイルが登録され、初期化できること
            var loader = new AddressableLoader();
            var meta = loader.InitializeAsync(catalogPath).GetAwaiter().GetResult();
            Assert.IsNotNull(meta, "MetaStore がロードできる");
            Assert.Greater(meta.TileMetaInfos?.Count ?? 0, 0, "タイル情報が1件以上ある");
        }
    }
}

using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.CityImport.Import;
using PLATEAU.Dataset;
using PLATEAU.Editor.DynamicTile;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PLATEAU.Tests.TestDynamicTile
{
    [TestFixture]
    public class TestImportToDynamicTile
    {
        [TearDown]
        public void TearDown()
        {
            // 生成物のうちテスト用出力先（Assets 配下）を可能な範囲で掃除
            var tempPrefabs = Path.Combine("Assets", "PLATEAUPrefabsTestDyn");
            if (AssetDatabase.IsValidFolder(tempPrefabs))
            {
                AssetDatabase.DeleteAsset(tempPrefabs);
                AssetDatabase.Refresh();
            }
        }

        [UnityTest]
        public IEnumerator Test_Import_Local_To_DynamicTile_Succeeds()
        {
            // Arrange: MiniTokyo のテスト定義からローカルデータを使用
            var cityDef = TestCityDefinition.MiniTokyo;

            // CityImportConfig を TestCityDefinition.MakeConfig 相当で生成
            var allPackages = EnumUtil.EachFlags(PredefinedCityModelPackageExtension.All());
            var allPackageLods = new PackageToLodDict();
            foreach (var package in allPackages)
            {
                allPackageLods.MergePackage(package, 3);
            }

            IDatasetSourceConfig datasetSourceConfig = new DatasetSourceConfigLocal(cityDef.SrcRootDirPathLocal);
            var dummyAreaSelectResult = new AreaSelectResult(new ConfigBeforeAreaSelect(datasetSourceConfig, cityDef.CoordinateZoneId), cityDef.AreaGridCodes, AreaSelectResult.ResultReason.Confirm);
            var conf = CityImportConfig.CreateWithAreaSelectResult(dummyAreaSelectResult);
            foreach (var pair in conf.PackageImportConfigDict.ForEachPackagePair)
            {
                pair.Value.IncludeTexture = true;
            }

            // 動的タイル設定を有効化（Assets 配下に一時出力する）
            conf.DynamicTileImportConfig.ImportType = ImportType.DynamicTile;
            conf.DynamicTileImportConfig.OutputPath = "Assets/PLATEAUPrefabsTestDyn";

            // Act: ImportToDynamicTile を実行
            var progressDisplay = default(PLATEAU.Editor.Window.ProgressDisplay.ProgressDisplayGUI);
            var importer = new ImportToDynamicTile(progressDisplay);
            var cts = new CancellationTokenSource();
            var task = importer.ExecAsync(conf, cts.Token);
            yield return task.AsIEnumerator();

            // Assert: マネージャーが生成され、カタログが保存されていること
            var manager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            Assert.IsNotNull(manager, "PLATEAUTileManager が生成されている");

            var catalogPath = manager.CatalogPath;
            Assert.IsFalse(string.IsNullOrEmpty(catalogPath), "カタログパスが保存されている");
            Assert.IsTrue(File.Exists(catalogPath), $"カタログファイルが存在する: {catalogPath}");

            // メタに 1 件以上のタイルが登録され、初期化できること
            var loader = new AddressableLoader();
            var meta = loader.InitializeAsync(catalogPath).GetAwaiter().GetResult();
            Assert.IsNotNull(meta, "MetaStore がロードできる");
            Assert.Greater(meta.TileMetaInfos?.Count ?? 0, 0, "タイル情報が1件以上ある");
        }
    }
}



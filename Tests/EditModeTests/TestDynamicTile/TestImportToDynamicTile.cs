using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Config;
using PLATEAU.Dataset;
using PLATEAU.Editor.DynamicTile;
using PLATEAU.DynamicTile;
using PLATEAU.PolygonMesh;
using PLATEAU.CityInfo;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PLATEAU.Tests.TestDynamicTile
{
    /// <summary>
    /// 動的タイルのインポートのユニットテストです。
    /// FIXME: アプリケーションのビルド後に動作するかまではカバーしないので注意。人力でも確認が必要。
    /// FIXME: このユニットテストはHDRPではうまくいかない可能性があります。
    /// </summary>
    [TestFixture]
    public class TestImportToDynamicTile
    {
        private const string OutputDirPathInAssets = "Assets/PLATEAUPrefabsTestDyn";
        private const string TempSceneParentPath = "Assets/PLATEAUPrefabsTestDyn";
        private const string TempScenePath = "Assets/PLATEAUPrefabsTestDyn/TestScene_DynamicTileTest.unity";
        private string prevScenePath;
        private string outputDir; // 必ず各テストの最初にoutputDirを設定すること

        /// <summary>
        /// Assetsフォルダ内をタイル出力先として、タイルがシーンに読み込まれることをテストします。
        /// 別のグリッドコードで2回読み込み、2回目は1回目の追加になっていることを確認します。
        /// </summary>
        [UnityTest]
        public IEnumerator Test_ImportTileInAssetsTwice()
        {
            yield return null;
            outputDir = OutputDirPathInAssets;
            SetUp(outputDir);
            
            // テスト
            string gridCodeStrA = "54390565";
            string gridCodeStrB = "54390566";
            using var gridCodesA = GridCodeList.CreateFromGridCodesStr(new[]{gridCodeStrA});
            using var gridCodesB = GridCodeList.CreateFromGridCodesStr(new[]{gridCodeStrB});
            yield return null;
            // 1回目のインポート
            yield return TestImport(gridCodesA, new string[]{gridCodeStrA});
            EditorSceneManager.SaveOpenScenes();
            yield return null;
            DeletePackedImages();
            yield return null;
            // 2回目のインポート
            yield return TestImport(gridCodesB, new string[] { gridCodeStrA, gridCodeStrB });
            yield return null;
        }

        /// <summary>
        /// Assetsフォルダ外をタイル出力先として、タイルがシーンに読み込まれることをテストします。
        /// 別のグリッドコードで2回読み込み、2回目は1回目の追加になっていることを確認します。
        /// </summary>
        [UnityTest]
        public IEnumerator Test_ImportTileOutsideAssetsTwice()
        {
            // テストの処理時間が増えていく場合はAssets/PLATEAUPrefabsが肥大化していないか確認してください
            
            // プロジェクト外（Assets 配下ではない）に出力先を設定
            outputDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "PLATEAUPrefabsTestDyn_Outside").Replace('\\','/');
            SetUp(outputDir);
            // テスト
            string gridCodeStrA = "54390565";
            string gridCodeStrB = "54390566";
            using var gridCodesA = GridCodeList.CreateFromGridCodesStr(new[]{gridCodeStrA});
            using var gridCodesB = GridCodeList.CreateFromGridCodesStr(new[]{gridCodeStrB});
            yield return null;
            // 1回目のインポート
            yield return TestImport(gridCodesA, new string[]{gridCodeStrA});
            EditorSceneManager.SaveOpenScenes();
            DeletePackedImages();
            AssetDatabase.Refresh();
            yield return new WaitForSeconds(0.2f);
            // 2回目のインポート
            yield return TestImport(gridCodesB, new string[] { gridCodeStrA, gridCodeStrB });
        }
        
        
        private void SetUp(string outputDirForSetup)
        {
            // 新規シーン用のフォルダを作成
            var parent = "Assets";
            var folderName = Path.GetFileName(TempSceneParentPath);
            if (!AssetDatabase.IsValidFolder(TempSceneParentPath))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }

            AssetDatabase.Refresh();
            
            // 既存の一時シーンが残っていれば削除
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TempScenePath) != null)
            {
                AssetDatabase.DeleteAsset(TempScenePath);
                AssetDatabase.Refresh();
            }

            // 現在のシーンを記録し、新規シーンを作成して保存（Addressables ビルド時のセーブダイアログ抑止）
            prevScenePath = SceneManager.GetActiveScene().path;
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(newScene, TempScenePath);
            AssetDatabase.Refresh();

            if (!Directory.Exists(outputDirForSetup))
            {
                Directory.CreateDirectory(outputDirForSetup);
            }
        }

        [TearDown]
        public void TearDown()
        {
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            // 生成物のうちテスト用出力先（Assets 配下）を掃除
            if (AssetDatabase.IsValidFolder(TempSceneParentPath))
            {
                AssetDatabase.DeleteAsset(TempSceneParentPath);
                AssetDatabase.Refresh();
            }
            // 出力先フォルダを削除
            if (!string.IsNullOrEmpty(outputDir) && Directory.Exists(outputDir))
            {
                try { Directory.Delete(outputDir, true); }
                catch (System.Exception) { }
            }
            
            
            // 一時シーンを閉じて削除し、元のシーンを復帰
            // 開いている場合は保存せずクローズ
            var opened = SceneManager.GetActiveScene();
            if (opened.path == TempScenePath)
            {
                // 別シーンに切り替えてから削除
                if (!string.IsNullOrEmpty(prevScenePath) && File.Exists(prevScenePath))
                {
                    EditorSceneManager.OpenScene(prevScenePath, OpenSceneMode.Single);
                }
                else
                {
                    EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                }
            }
            
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TempScenePath) != null)
            {
                AssetDatabase.DeleteAsset(TempScenePath);
                AssetDatabase.Refresh();
            }
            
            DeletePackedImages();
            
            // StreamingAssets/PLATEAUBundles/PLATEAUCityObjectGroup_PLATEAUPrefabsTestDyn を削除
            var bundlesGroupPath = Path.Combine(
                "Assets",
                "StreamingAssets",
                AddressableLoader.AddressableLocalBuildFolderName,
                "PLATEAUCityObjectGroup_PLATEAUPrefabsTestDyn").Replace('\\','/');
            if (AssetDatabase.IsValidFolder(bundlesGroupPath))
            {
                AssetDatabase.DeleteAsset(bundlesGroupPath);
                AssetDatabase.Refresh();
            }
            // 出力先フォルダ名に基づくグループがあれば念のため削除
            if (!string.IsNullOrEmpty(outputDir))
            {
                var groupByOutput = Path.Combine(
                    "Assets",
                    "StreamingAssets",
                    AddressableLoader.AddressableLocalBuildFolderName,
                    "PLATEAUCityObjectGroup_" + Path.GetFileName(outputDir)).Replace('\\','/');
                if (AssetDatabase.IsValidFolder(groupByOutput))
                {
                    AssetDatabase.DeleteAsset(groupByOutput);
                    AssetDatabase.Refresh();
                }
            }
        }

        private void DeletePackedImages()
        {
            // Packages の TestData 内で生成された packed_image_* を削除
            try
            {
                var appearanceDir = Path.Combine(
                    PathUtil.SdkBasePath,
                    "Tests",
                    "TestData",
                    "日本語パステスト",
                    "TestDataSugitoMachi",
                    "udx",
                    "bldg",
                    "54390566_bldg_6697_appearance");
                if (Directory.Exists(appearanceDir))
                {
                    var files = Directory.GetFiles(appearanceDir, "packed_image_*", SearchOption.TopDirectoryOnly);
                    foreach (var f in files)
                    {
                        try { File.Delete(f); }
                        catch (IOException) { }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"packed_image_* の削除に失敗しました: {ex.Message}");
            }
        }

        private IEnumerator TestImport(GridCodeList gridsToImport, string[] gridsAssertedToExist)
        {
            yield return null;
            // テストデータ
            var cityDef = TestCityDefinition.SugitoMachi;

            IDatasetSourceConfig datasetSourceConfig = new DatasetSourceConfigLocal(cityDef.SrcRootDirPathLocal);
            var dummyAreaSelectResult = new AreaSelectResult(new ConfigBeforeAreaSelect(datasetSourceConfig, cityDef.CoordinateZoneId), gridsToImport, AreaSelectResult.ResultReason.Confirm);
            var conf = CityImportConfig.CreateWithAreaSelectResult(dummyAreaSelectResult);
            foreach (var pair in conf.PackageImportConfigDict.ForEachPackagePair)
            {
                var packageConf = pair.Value;
                packageConf.IncludeTexture = true;
                packageConf.MeshGranularity = MeshGranularity.PerCityModelArea; // タイル化時は必ず地域単位
            }

            conf.DynamicTileImportConfig.ImportType = ImportType.DynamicTile;
            conf.DynamicTileImportConfig.OutputPath = outputDir;

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

            // メタに1件以上のタイルが登録され、初期化できること
            var loader = new AddressableLoader();
            var meta = loader.InitializeAsync(catalogPath).GetAwaiter().GetResult();
            Assert.IsNotNull(meta, "MetaStore がロードできる");
            Assert.Greater(meta.TileMetaInfos?.Count ?? 0, 0, "タイル情報が1件以上ある");
            
            // タイルの読み込み
            var sceneView = SceneView.lastActiveSceneView;
            sceneView.pivot = new Vector3(0, 50, 0);
            yield return manager.InitializeTiles().AsIEnumerator();

            // タイルを読み込ませるためシーンビューのループを回す
            for (int i = 0; i < 10; i++)
            {
                sceneView.pivot += Vector3.forward * 0.05f;
                manager.UpdateCameraPosition(sceneView.pivot);
                EditorApplication.QueuePlayerLoopUpdate();
                for (int j = 0; j < 10; j++)
                {
                    yield return null;
                }
            }
            
            // タイルとなるゲームオブジェクトが存在することをチェック
            AssertTileObjectExists(gridsAssertedToExist);
            
            // Playモードのテスト
            Debug.Log("Starting Play Mode Test");
            yield return new EnterPlayMode();
            // タイル読み込みまで待つ
            var end = Time.realtimeSinceStartup + 2f;
            while (Time.realtimeSinceStartup < end)
            {
                yield return null;
            }
            AssertTileObjectExists(gridsAssertedToExist); // Playモードでもタイルが出てくること
            yield return new ExitPlayMode();
            Debug.Log("Exited Play Mode Test");
        }

        private static void AssertTileObjectExists(string[] gridsAssertedToExist)
        {
            for (int i = 0; i < gridsAssertedToExist.Length; i++)
            {
                var gridCodeAsserted = gridsAssertedToExist[i];
                var tileObjects = Resources.FindObjectsOfTypeAll<PLATEAUCityObjectGroup>()
                    .Where(
                        go => // 例: group6 -> 親 -> LOD1 -> 親 -> tile_zoom_11_grid_543905094_bldg_6697_op
                            go.transform.parent.parent.name.Contains(gridCodeAsserted)
                    )
                    .ToArray();
                
                Assert.IsTrue(tileObjects.Any(), $"グリッド {gridCodeAsserted} に相当するタイルが存在する");

                foreach (var tileObj in tileObjects)
                {
                    var meshFilter = tileObj.GetComponent<MeshFilter>();
                    Assert.IsTrue(meshFilter != null, "タイルにMeshFilterが存在する");
                    Assert.IsTrue(meshFilter.sharedMesh != null, "タイルにメッシュが存在する");
                    
                }
            }
        }
    }
}


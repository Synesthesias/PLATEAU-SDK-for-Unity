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
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace PLATEAU.Tests.TestDynamicTile
{
	/// <summary>
	/// TileRebuilder の挙動を検証します。
	/// 1) 一度アセットバンドルにビルド
	/// 2) TilePrefabsToScene でプレハブをシーンへ配置
	/// 3) 編集（Cube 追加）
	/// 4) Rebuild で編集がプレハブに適用され、EditingTiles は消え、再配置されたタイルに編集が反映される
	/// を確認します。
	/// </summary>
	[TestFixture]
	public class TestTileRebuilder
	{
		private const string OutputDirPathInAssets = "Assets/PLATEAUPrefabsTestRebuilder";
		private const string TempSceneParentPath = "Assets/PLATEAUPrefabsTestRebuilder";
		private const string TempScenePath = "Assets/PLATEAUPrefabsTestRebuilder/TestScene_TileRebuilder.unity";
		private string prevScenePath;
		private string outputDir;

		[SetUp]
		public void SetUp()
		{
			// 前回クラッシュ等で残存した一時フォルダを削除してクリーンスタート
			if (AssetDatabase.IsValidFolder(TempSceneParentPath))
			{
				AssetDatabase.DeleteAsset(TempSceneParentPath);
				AssetDatabase.Refresh();
			}
			if (AssetDatabase.IsValidFolder(OutputDirPathInAssets))
            {
                AssetDatabase.DeleteAsset(OutputDirPathInAssets);
                AssetDatabase.Refresh();
			}
		}

		[UnityTest]
		public IEnumerator Test_Rebuild_AppliesPrefabChanges()
        {
            var dummyCancelToken = new CancellationTokenSource().Token;
			// Arrange: 出力先と一時シーンの準備
			outputDir = OutputDirPathInAssets;
			SetUpSceneAndOutput(outputDir);

			// 1) 動的タイルの初回ビルド（アセットバンドル化）
			yield return ImportDynamicTileOnce(outputDir).AsIEnumerator();

			// リビルド前のアセットバンドル数を取得
			var preBundleCount = GetBundleCountInGroupFolder();

			// 初回ビルドで PLATEAUTileManager が生成・設定されているはず
			var manager = Object.FindObjectOfType<PLATEAUTileManager>();
			Assert.IsNotNull(manager, "PLATEAUTileManager が存在する");
			Assert.IsFalse(string.IsNullOrEmpty(manager.CatalogPath), "CatalogPath が設定されている");

			// 2) TilePrefabsToScene: 元となったプレハブをシーンへ配置（EditingTiles 配下）
			var rebuilder = new TileRebuilder();
			yield return rebuilder.TilePrefabsToScene(manager, dummyCancelToken).AsIEnumerator();

			var editingRoot = GameObject.Find(TileRebuilder.EditingTilesParentName);
			Assert.IsNotNull(editingRoot, "EditingTiles ルートが存在する");
			Assert.Greater(editingRoot.transform.childCount, 0, "EditingTiles 配下にプレハブがある");

			// 子に Cube を追加
            foreach (Transform editingTrans in editingRoot.transform)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = "TestCube";
                cube.transform.SetParent(editingTrans, false);
            }

			// 3) Rebuild: 変更（Cube 追加）がプレハブに適用され、EditingTiles は消える
			yield return rebuilder.Rebuild(manager).AsIEnumerator();

			// Rebuild 後、EditingTiles ルートはクリーンアップされている
			Assert.IsNull(GameObject.Find(TileRebuilder.EditingTilesParentName), "Rebuild 後に EditingTiles が削除されている");

			// リビルド後のアセットバンドル数を取得し、前後で一致することを確認
			var postBundleCount = GetBundleCountInGroupFolder();
			Assert.AreEqual(preBundleCount, postBundleCount, "リビルド前後でアセットバンドル数が一致する");

			// 4) Addressables 経由で再配置されたタイルに Cube 追加が反映されていることを確認
			// カメラを動かしてロードを促進
			var sceneView = SceneView.lastActiveSceneView;
			if (sceneView != null)
			{
				sceneView.pivot = new Vector3(0, 50, 0);
                manager.UpdateCameraPosition(sceneView.pivot);
				for (int i = 0; i < 10; i++)
				{
					sceneView.pivot += Vector3.forward * 0.05f;
                    manager.UpdateCameraPosition(sceneView.pivot);
					EditorApplication.QueuePlayerLoopUpdate();
                    
					for (int j = 0; j < 10; j++) yield return null;
				}
			}
            
            // managerを取得し直す
            manager = Object.FindObjectOfType<PLATEAUTileManager>();

			// DynamicTileRoot 配下に TestCube が1つ以上存在すること
            var tileParent = manager.transform.Find(PLATEAUTileManager.TileParentName);
            Assert.IsTrue(tileParent != null, "DynamicTileRootが存在する");
            bool cubeFound = tileParent.GetComponentsInChildren<Transform>(true).Any(t => t.name == "TestCube");
			Assert.IsTrue(cubeFound, "再配置されたタイル配下に TestCube が存在する（変更が維持されている）");
		}

		private static int GetBundleCountInGroupFolder()
		{
			var bundlesGroupPath = Path.Combine(
				"Assets",
				"StreamingAssets",
				AddressableLoader.AddressableLocalBuildFolderName,
				"PLATEAUCityObjectGroup_" + Path.GetFileName(OutputDirPathInAssets)).Replace('\\','/');
			// Asset パスをフルパスへ変換
			var projectRoot = Directory.GetParent(Application.dataPath).FullName;
			var fullPath = Path.Combine(projectRoot, bundlesGroupPath).Replace('\\','/');
			if (!Directory.Exists(fullPath)) return 0;
			return Directory.GetFiles(fullPath, "*.bundle", SearchOption.TopDirectoryOnly).Length;
		}

		private void SetUpSceneAndOutput(string outputDirForSetup)
		{
			// 新規シーン用フォルダ
			var parent = "Assets";
			var folderName = Path.GetFileName(TempSceneParentPath);
			if (!AssetDatabase.IsValidFolder(TempSceneParentPath))
			{
				AssetDatabase.CreateFolder(parent, folderName);
			}
			AssetDatabase.Refresh();

			// 既存シーンを削除
			if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TempScenePath) != null)
			{
				AssetDatabase.DeleteAsset(TempScenePath);
				AssetDatabase.Refresh();
			}

			// 新規シーン作成＆保存（Addressables ビルド時のセーブダイアログ抑止）
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
			// 一時シーンの片付けと元シーン復帰
			var opened = SceneManager.GetActiveScene();
			if (opened.path == TempScenePath)
			{
				if (!string.IsNullOrEmpty(prevScenePath) && File.Exists(prevScenePath))
				{
					EditorSceneManager.OpenScene(prevScenePath, OpenSceneMode.Single);
				}
				else
				{
					Debug.Log("failed to open prevScene before unit test.");
				}
			}
			if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TempScenePath) != null)
			{
				AssetDatabase.DeleteAsset(TempScenePath);
				AssetDatabase.Refresh();
			}

			// テスト用フォルダ削除
			if (AssetDatabase.IsValidFolder(TempSceneParentPath))
			{
				AssetDatabase.DeleteAsset(TempSceneParentPath);
				AssetDatabase.Refresh();
			}
			if (!string.IsNullOrEmpty(outputDir) && Directory.Exists(outputDir))
			{
                try { Directory.Delete(outputDir, true); }
                catch
                {
                    Debug.LogWarning("failed to delete test directory.");
                }
			}

            PLATEAUEditorEventListener.Release();
            
			// StreamingAssets/Addressables の残骸を可能な範囲で掃除
			var bundlesGroupPath = Path.Combine(
				"Assets",
				"StreamingAssets",
				AddressableLoader.AddressableLocalBuildFolderName,
				"PLATEAUCityObjectGroup_" + Path.GetFileName(OutputDirPathInAssets)).Replace('\\','/');
			if (AssetDatabase.IsValidFolder(bundlesGroupPath))
			{
				AssetDatabase.DeleteAsset(bundlesGroupPath);
				AssetDatabase.Refresh();
			}
		}

		private async System.Threading.Tasks.Task ImportDynamicTileOnce(string outDir)
		{
			// テストデータ: SugitoMachi の一部を使用
			var cityDef = TestCityDefinition.SugitoMachi;
			IDatasetSourceConfig datasetSourceConfig = new DatasetSourceConfigLocal(cityDef.SrcRootDirPathLocal);
			using var grids = GridCodeList.CreateFromGridCodesStr(new[]{"54390565"});
			var dummyAreaSelectResult = new AreaSelectResult(new ConfigBeforeAreaSelect(datasetSourceConfig, cityDef.CoordinateZoneId), grids, AreaSelectResult.ResultReason.Confirm);
			var conf = CityImportConfig.CreateWithAreaSelectResult(dummyAreaSelectResult);
			foreach (var pair in conf.PackageImportConfigDict.ForEachPackagePair)
			{
				pair.Value.IncludeTexture = true;
				pair.Value.MeshGranularity = PolygonMesh.MeshGranularity.PerCityModelArea; // タイル化時は地域単位
			}
			conf.DynamicTileImportConfig.ImportType = ImportType.DynamicTile;
			conf.DynamicTileImportConfig.OutputPath = outDir;

			var importer = new ImportToDynamicTile(null);
			var cts = new CancellationTokenSource();
			await importer.ExecAsync(conf, cts.Token);
		}
	}
}




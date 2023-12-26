using System;
using System.Collections;
using NUnit.Framework;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.AreaSelector.Display;
using PLATEAU.CityImport.AreaSelector.Display.Gizmos;
using PLATEAU.CityImport.Config;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityImport.AreaSelector;
using PLATEAU.Tests.EditModeTests.TestDoubles;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace PLATEAU.Tests.EditModeTests
{
    /// <summary>
    /// 範囲選択画面をテストします。
    /// </summary>
    [TestFixture]
    public class TestAreaSelector
    {

        [UnityTest]
        public IEnumerator Components_Exist_In_Area_Select_Scene()
        {
            // 保存済みのシーンが開いている状況でないと範囲選択画面に遷移できないので、ここでアセットフォルダ内の保存済みの空のシーンを利用します。
            // このシーンは空であるはずであり、空でない状態で上書き保存するのは避けてください。（ただしシーンに最初からあるライトとカメラはあって良いです。）
            var emptyScene = EditorSceneManager.OpenScene(PathUtil.SdkPathToAssetPath("Tests/EmptySceneForTest.unity"), OpenSceneMode.Single);
            yield return null;
            SceneManager.SetActiveScene(emptyScene);
            // MiniTokyo の範囲選択画面を開始します。
            var testDef = TestCityDefinition.MiniTokyo;
            var datasetConf = new DatasetSourceConfigLocal(testDef.SrcRootDirPathLocal);
            var resultReceiver = new DummyAreaSelectResultReceiver();
        
            LogAssert.ignoreFailingMessages = true;
            
            AreaSelectorStarter.Start(new ConfigBeforeAreaSelect(datasetConf, testDef.CoordinateZoneId), resultReceiver);

            // EditModeでは yield return new WaitForSeconds() ができないので、原始的なループで地図のダウンロードを待ちます。
            var startT = DateTime.Now;
            while ((DateTime.Now - startT).TotalMilliseconds < 1000)
            {
                yield return null;
            }
            
            LogAssert.ignoreFailingMessages = false;
            
            // コンポーネントの存在をチェックします。
            Assert.IsNotNull(Object.FindObjectOfType<AreaSelectGizmosDrawer>(), "AreaSelectGizmosDrawer が存在します。");
            var basemapObj = GameObject.Find("Basemap");
            Assert.IsNotNull(basemapObj, "Basemapというゲームオブジェクトが存在します。");
            var oneOfMapTrans = basemapObj.transform.Find("12/1614/3637");
            Assert.IsNotNull(oneOfMapTrans, "東京の地図の一部に相当するゲームオブジェクトが存在します。");
            Assert.IsNotNull(oneOfMapTrans.GetComponent<MeshRenderer>().sharedMaterial.mainTexture, "地図にはテクスチャが割り当てられます。");
            var areaSelectorBehaviour = Object.FindObjectOfType<AreaSelectorBehaviour>();
            Assert.IsNotNull(areaSelectorBehaviour, "AreaSelectorBehaviourが存在します。");
            
            // 終了処理
            LogAssert.ignoreFailingMessages = true;
            areaSelectorBehaviour.EndAreaSelection();
            LogAssert.ignoreFailingMessages = false;
            yield return null;
            var newTestScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            yield return null;
            
            // 終了後チェック
            // TODO 範囲選択のUIが変わって以来、下コメントアウト部であるメッシュコードが渡されることのテストが動作していません。
            // var areaSelectResult = resultReceiver.AreaSelectResult;
            // Assert.IsTrue(areaSelectResult.AreaMeshCodes.Length > 0, "範囲選択の結果として、メッシュコードが1つ以上渡されている");
            
            // FIXME メッシュコードが渡されることのチェックの他に、PackageToLods が渡されることのチェックもしたほうが良い
            
            // TODO 複数のユニットテストを実行するとき、なぜかここでシーン EmptySceneForTest が閉じず、開いたまま後続のテストが進行するのを直したほうが良い
            EditorSceneManager.CloseScene(emptyScene, true);
            
            yield return null;
            SceneManager.SetActiveScene(newTestScene);

            yield return null;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            var startT2 = DateTime.Now;
            while ((DateTime.Now - startT2).TotalMilliseconds < 1000)
            {
                yield return null;
            }
            // SceneManager.SetActiveScene(prevScene);
            // yield return null;
        }
    }
}

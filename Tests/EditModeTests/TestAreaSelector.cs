using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.AreaSelector.SceneObjs;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityImport.AreaSelector;
using PLATEAU.Tests.EditModeTests.TestDoubles;
using PLATEAU.Tests.TestUtils;
using UnityEditor;
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
        private IAreaSelectResultReceiver resultReceiver;
        
        [UnityTest]
        public IEnumerator Components_Exist_In_Area_Select_Scene()
        {
            var emptyScene = EditorSceneManager.OpenScene("Packages/com.synesthesias.plateau-unity-sdk/Tests/EmptySceneForTest.unity", OpenSceneMode.Additive);
            yield return null;
            SceneManager.SetActiveScene(emptyScene);
            // MiniTokyo の範囲選択画面を開始します。
            var testDef = TestCityDefinition.MiniTokyo;
            var datasetConf = new DatasetSourceConfig(false, testDef.SrcRootDirPathLocal, "", "", "");
            this.resultReceiver = new DummyAreaSelectResultReceiver();
        
            LogAssert.ignoreFailingMessages = true;
            
            AreaSelectorStarter.Start(datasetConf, this.resultReceiver, testDef.CoordinateZoneId);

            // EditModeでは yield return new WaitForSeconds() ができないので、原始的なループで地図のダウンロードを待ちます。
            var startT = DateTime.Now;
            while ((DateTime.Now - startT).TotalMilliseconds < 1000)
            {
                yield return null;
            }
            
            LogAssert.ignoreFailingMessages = false;
            
            // コンポーネントの存在をチェックします。
            Assert.IsNotNull(Object.FindObjectOfType<AreaSelectGizmosDrawer>(), "AreaSelectGizmosDrawer が存在します。");
            var gsiMapsObj = GameObject.Find("GSIMaps");
            Assert.IsNotNull(gsiMapsObj, "GSIMapsというゲームオブジェクトが存在します。");
            var oneOfMapTrans = gsiMapsObj.transform.Find("12/1614/3637");
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
            
            // TODO 複数のユニットテストを実行するとき、なぜかここでシーン EmptySceneForTest が閉じず、開いたまま後続のテストが進行するのを直したほうが良い
            EditorSceneManager.CloseScene(emptyScene, true);
            
            yield return null;
            SceneManager.SetActiveScene(newTestScene);
            
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

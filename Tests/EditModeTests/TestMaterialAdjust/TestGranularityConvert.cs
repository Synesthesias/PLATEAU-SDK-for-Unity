using NUnit.Framework;
using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.GranularityConvert;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace PLATEAU.Tests.EditModeTests.TestMaterialAdjust
{
    [TestFixture]
    public class TestGranularityConvert
    {

        private TestDataMA testData;

        [SetUp]
        public void SetUp()
        {
            // シーンをリセット
            foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Object.DestroyImmediate(rootGameObject);
            }
            
            testData = new TestDataMA();
        }
        
        // 粒度変換前後の一致チェック
        
        
        [UnityTest]
        public IEnumerator Test_AtomicToAtomic()
        {
            yield return AssertGranularityConverted(GranularityConvert.ConvertGranularity.PerAtomicFeatureObject, GranularityConvert.ConvertGranularity.PerAtomicFeatureObject, true);
        }
        
        [UnityTest]
        public IEnumerator Test_AtomicToPrimary()
        {
            yield return AssertGranularityConverted(GranularityConvert.ConvertGranularity.PerAtomicFeatureObject, GranularityConvert.ConvertGranularity.PerPrimaryFeatureObject, true);
        }
        
        [UnityTest]
        public IEnumerator Test_AtomicToArea()
        {
            yield return AssertGranularityConverted(GranularityConvert.ConvertGranularity.PerAtomicFeatureObject, GranularityConvert.ConvertGranularity.PerCityModelArea, true);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToAtomic()
        {
            yield return AssertGranularityConverted(GranularityConvert.ConvertGranularity.PerPrimaryFeatureObject, GranularityConvert.ConvertGranularity.PerAtomicFeatureObject, false);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToPrimary()
        {
            yield return AssertGranularityConverted(GranularityConvert.ConvertGranularity.PerPrimaryFeatureObject, GranularityConvert.ConvertGranularity.PerPrimaryFeatureObject, true);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToArea()
        {
            yield return AssertGranularityConverted(GranularityConvert.ConvertGranularity.PerPrimaryFeatureObject, GranularityConvert.ConvertGranularity.PerCityModelArea, true);
        }
        
        
        [UnityTest]
        public IEnumerator Test_AreaToAtomic()
        {
            yield return AssertGranularityConverted(GranularityConvert.ConvertGranularity.PerCityModelArea, GranularityConvert.ConvertGranularity.PerAtomicFeatureObject, false);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToPrimary()
        {
            yield return AssertGranularityConverted(GranularityConvert.ConvertGranularity.PerCityModelArea, GranularityConvert.ConvertGranularity.PerPrimaryFeatureObject, false);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToArea()
        {
            yield return AssertGranularityConverted(GranularityConvert.ConvertGranularity.PerCityModelArea, GranularityConvert.ConvertGranularity.PerCityModelArea, true);
        }


        /// <summary>
        /// 粒度<paramref name="srcGran"/>から<paramref name="dstGran"/>に変換して正しいかどうか確認します。
        /// <paramref name="assertOrder"/>は、子ゲームオブジェクトの順序も含めてチェックするならtrue、順不問ならfalseとします。
        /// 現状、細かい粒度に分解するときは順番を保証できません。粗い粒度に結合するときは順番は同じにできます。 
        /// </summary>
        private IEnumerator AssertGranularityConverted(GranularityConvert.ConvertGranularity srcGran, GranularityConvert.ConvertGranularity dstGran, bool assertOrder)
        {
            yield return ConvertGranularity(srcGran, dstGran);
            var expectTrans = testData.CopyBldgSrcOf(dstGran).transform;
            var actualTrans = retGranularityConvertResult.GeneratedRootTransforms.Get.ToArray()[0];
            if (assertOrder)
            {
                MAAssert.AreSameRecursive(expectTrans, actualTrans, dstGran);
            }
            else
            {
                MAAssert.AreSameSetRecursive(expectTrans, actualTrans, dstGran);
            }
        }
        
        private GranularityConvertResult retGranularityConvertResult; // コルーチンの結果を返す用

        private IEnumerator ConvertGranularity(GranularityConvert.ConvertGranularity srcGran, GranularityConvert.ConvertGranularity dstGran)
        {
            Debug.Log($"Checking {srcGran} to {dstGran}");
            var srcObj = testData.CopyBldgSrcOf(srcGran);
            var converter = new CityGranularityConverter();
            var option = new GranularityConvertOptionUnity(
                    new GranularityConvertOption(dstGran, 1),
                    new UniqueParentTransformList(srcObj.transform),
                    true);
            var progressBar = new DummyProgressBar();
            var task = converter.ConvertAsync(option, progressBar).ContinueWithErrorCatch<GranularityConvertResult>();
            yield return task.AsIEnumerator<GranularityConvertResult>(result => retGranularityConvertResult = result);
        }


        
        
        
        
    }
}
using NUnit.Framework;
using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.CityAdjust.MaterialAdjust.ExecutorV2;
using PLATEAU.GranularityConvert;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace PLATEAU.Tests.EditModeTests.TestMaterialAdjust
{
    /// <summary>
    /// 地物型でのマテリアル分けをテストします。
    /// Replace(元オブジェクトを削除する)設定で確認します。
    /// MAはMaterialAdjustの略です。
    /// </summary>
    [TestFixture]
    public class TestMAByTypeReplace
    {
        private TestDataMA testData = new TestDataMA();
        
        
        [UnityTest]
        public IEnumerator Test_AtomicToAtomic()
        {
            yield return AssertMA(ConvertGranularity.PerAtomicFeatureObject, ConvertGranularity.PerAtomicFeatureObject, 0, true);
        }
        
        [UnityTest]
        public IEnumerator Test_AtomicToPrimary()
        {
            yield return AssertMA(ConvertGranularity.PerAtomicFeatureObject, ConvertGranularity.PerPrimaryFeatureObject, 0, true);
        }
        
        [UnityTest]
        public IEnumerator Test_AtomicToArea()
        {
            yield return AssertMA(ConvertGranularity.PerAtomicFeatureObject, ConvertGranularity.PerCityModelArea, 1, true);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToAtomic()
        {
            yield return AssertMA(ConvertGranularity.PerPrimaryFeatureObject, ConvertGranularity.PerAtomicFeatureObject, 0, false);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToPrimary()
        {
            yield return AssertMA(ConvertGranularity.PerPrimaryFeatureObject, ConvertGranularity.PerPrimaryFeatureObject, 0, true);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToArea()
        {
            yield return AssertMA(ConvertGranularity.PerPrimaryFeatureObject, ConvertGranularity.PerCityModelArea, 1, true);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToAtomic()
        {
            yield return AssertMA(ConvertGranularity.PerCityModelArea, ConvertGranularity.PerAtomicFeatureObject, 1, false);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToPrimary()
        {
            yield return AssertMA(ConvertGranularity.PerCityModelArea, ConvertGranularity.PerPrimaryFeatureObject, 1, false);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToArea()
        {
            yield return AssertMA(ConvertGranularity.PerCityModelArea, ConvertGranularity.PerCityModelArea, 0, true);
        }
        
        /// <summary>
        /// <paramref name="srcGran"/>から<paramref name="dstGran"/>への地物タイプマテリアル分けをチェック。
        /// <paramref name="dstDataId"/>は、成果テストデータの番号
        /// <paramref name="assertOrder"/>は、ヒエラルキー上の順序までチェックするかどうか
        /// </summary>
        private IEnumerator AssertMA(ConvertGranularity srcGran, ConvertGranularity dstGran, int dstDataId, bool assertOrder)
        {
            yield return ExecConvert(srcGran, dstGran);
            var actual = retSrcObj.transform;
            var expect = testData.CopyMATypeDstOf(dstGran, dstDataId).transform;
            // ルートの名前だけは違っても良い
            actual.name = expect.name;
            if (assertOrder)
            {
                MAAssert.AreSameRecursive(expect, actual, dstGran);
            }
            else
            {
                MAAssert.AreSameSetRecursive(expect, actual, dstGran);
            }
        }

        private GameObject retSrcObj; // コルーチンの結果を返す用
        
        /// <summary>
        /// ①テストゲームオブジェクトのコピー
        /// ②地物タイプでのマテリアル分け
        /// ③結果をretSrcObjに格納
        /// </summary>
        private IEnumerator ExecConvert(ConvertGranularity srcGran, ConvertGranularity dstGran)
        {
            // テスト用ゲームオブジェクトのコピー
            retSrcObj = testData.CopyBldgSrcOf(srcGran);
            


            var executorConf = new MAExecutorConf(
                testData.MaterialConfigByType(),
                new UniqueParentTransformList(retSrcObj.transform), true, true
            );

            var executor = new MAExecutorV2ByType();
            yield return executor.ExecAsync(executorConf).AsIEnumerator();
        }

        
        
    }
}
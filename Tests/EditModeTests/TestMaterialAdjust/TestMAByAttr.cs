using NUnit.Framework;
using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.CityAdjust.MaterialAdjust.ExecutorV2;
using PLATEAU.GranularityConvert;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace PLATEAU.Tests.EditModeTests.TestMaterialAdjust
{
    [TestFixture]
    public class TestMAByAttr
    {
        private TestDataMA testData = new TestDataMA();
        
        [UnityTest]
        public IEnumerator Test_AtomicToAtomic()
        {
            yield return AssertMA(ConvertGranularity.PerAtomicFeatureObject, ConvertGranularity.PerAtomicFeatureObject, 0, false);
        }
        
        // [UnityTest]
        // public IEnumerator Test_AtomicToMaterialInPrimary()
        // {
        //     yield return AssertMA(ConvertGranularity.PerAtomicFeatureObject, ConvertGranularity.MaterialInPrimary, 0, false);
        // }
        //
        // [UnityTest]
        // public IEnumerator Test_AtomicToPrimary()
        // {
        //     yield return AssertMA(ConvertGranularity.PerAtomicFeatureObject, ConvertGranularity.PerPrimaryFeatureObject, 0, false);
        // }
        //
        // [UnityTest]
        // public IEnumerator Test_AtomicToArea()
        // {
        //     yield return AssertMA(ConvertGranularity.PerAtomicFeatureObject, ConvertGranularity.PerCityModelArea, 0, false);
        // }
        //
        // [UnityTest]
        // public IEnumerator Test_PrimaryToAtomic()
        // {
        //     yield return AssertMA(ConvertGranularity.PerPrimaryFeatureObject, ConvertGranularity.PerAtomicFeatureObject, 0, false);
        // }
        //
        // [UnityTest]
        // public IEnumerator Test_PrimaryToMaterialInPrimary()
        // {
        //     yield return AssertMA(ConvertGranularity.MaterialInPrimary, ConvertGranularity.MaterialInPrimary, 0, false);
        // }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToPrimary()
        {
            yield return AssertMA(ConvertGranularity.PerPrimaryFeatureObject, ConvertGranularity.PerPrimaryFeatureObject, 0, false);
        }
        
        // [UnityTest]
        // public IEnumerator Test_PrimaryToArea()
        // {
        //     yield return AssertMA(ConvertGranularity.PerPrimaryFeatureObject, ConvertGranularity.PerCityModelArea, 0, false);
        // }
        //
        // [UnityTest]
        // public IEnumerator Test_AreaToAtomic()
        // {
        //     yield return AssertMA(ConvertGranularity.PerCityModelArea, ConvertGranularity.PerAtomicFeatureObject, 0, false);
        // }
        //
        // [UnityTest]
        // public IEnumerator Test_AreaToMaterialInPrimary()
        // {
        //     yield return AssertMA(ConvertGranularity.PerCityModelArea, ConvertGranularity.MaterialInPrimary, 0, false);
        // }
        //
        // [UnityTest]
        // public IEnumerator Test_AreaToPrimary()
        // {
        //     yield return AssertMA(ConvertGranularity.PerCityModelArea, ConvertGranularity.PerPrimaryFeatureObject, 0, false);
        // }
        
        [UnityTest]
        public IEnumerator Test_AreaToArea()
        {
            yield return AssertMA(ConvertGranularity.PerCityModelArea, ConvertGranularity.PerCityModelArea, 0, false);
        }
        

        private IEnumerator AssertMA(ConvertGranularity srcGran, ConvertGranularity dstGran, int dstDataId, bool assertOrder)
        {
            yield return ExecConvert(srcGran, dstGran);
            var actual = retResult;
            var expect = testData.CopyMAAttrDstOf(dstGran, dstDataId).transform;
            
            // 変換対象外となるものをチェック対象から外す
            var objsToRemove = new Transform[]
            {
                actual.Find("53393680_tran_6697_op.gml/LOD3/tran_677077fc-1bd2-4226-b82a-901385697899"),
                expect.Find("53393680_tran_6697_op.gml/LOD3/tran_677077fc-1bd2-4226-b82a-901385697899")
            }
                .Where(t => t != null)
                .Select(t => t.gameObject)
                .ToArray();
            foreach (var o in objsToRemove)
            {
                Object.DestroyImmediate(o);
            }
            
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
        
        private Transform retResult; // コルーチンの結果を返す用
        
        /// <summary>
        /// ①テストゲームオブジェクトのコピー
        /// ②属性情報でのマテリアル分け
        /// ③結果をretSrcObjに格納
        /// </summary>
        private IEnumerator ExecConvert(ConvertGranularity srcGran, ConvertGranularity dstGran)
        {
            // テスト用ゲームオブジェクトのコピー
            var srcObj = testData.CopyTranSrcOf(srcGran);
            
            
            var executorConf = new MAExecutorConfByAttr(
                testData.MaterialConfigByAttr(),
                new UniqueParentTransformList(srcObj.transform), true, true, "tran:function"
            );

            var executor = new MAExecutorV2ByAttr();
            yield return executor.ExecAsync(executorConf).ContinueWith(ret => retResult = ret.Result.Get.First()).AsIEnumerator();
        }
    }
}
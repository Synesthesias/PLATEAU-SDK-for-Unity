using NUnit.Framework;
using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
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
            yield return AssertMA(MAGranularity.PerAtomicFeatureObject, MAGranularity.PerAtomicFeatureObject, 0, false);
        }
        
        [UnityTest]
        public IEnumerator Test_AtomicToMaterialInPrimary()
        {
            yield return AssertMA(MAGranularity.PerAtomicFeatureObject, MAGranularity.PerMaterialInPrimary, 0, false);
        }
        
        [UnityTest]
        public IEnumerator Test_AtomicToPrimary()
        {
            yield return AssertMA(MAGranularity.PerAtomicFeatureObject, MAGranularity.PerPrimaryFeatureObject, 0, false);
        }
        
        [UnityTest]
        public IEnumerator Test_AtomicToArea()
        {
            yield return AssertMA(MAGranularity.PerAtomicFeatureObject, MAGranularity.CombineAll, 0, false);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToAtomic()
        {
            yield return AssertMA(MAGranularity.PerPrimaryFeatureObject, MAGranularity.PerAtomicFeatureObject, 0, false);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToMaterialInPrimary()
        {
            yield return AssertMA(MAGranularity.PerMaterialInPrimary, MAGranularity.PerMaterialInPrimary, 0, false);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToPrimary()
        {
            yield return AssertMA(MAGranularity.PerPrimaryFeatureObject, MAGranularity.PerPrimaryFeatureObject, 0, false);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToArea()
        {
            yield return AssertMA(MAGranularity.PerPrimaryFeatureObject, MAGranularity.CombineAll, 0, false);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToAtomic()
        {
            yield return AssertMA(MAGranularity.CombineAll, MAGranularity.PerAtomicFeatureObject, 0, false);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToMaterialInPrimary()
        {
            yield return AssertMA(MAGranularity.CombineAll, MAGranularity.PerMaterialInPrimary, 0, false);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToPrimary()
        {
            yield return AssertMA(MAGranularity.CombineAll, MAGranularity.PerPrimaryFeatureObject, 0, false);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToArea()
        {
            yield return AssertMA(MAGranularity.CombineAll, MAGranularity.CombineAll, 0, false);
        }
        

        private IEnumerator AssertMA(MAGranularity srcGran, MAGranularity dstGran, int dstDataId, bool assertOrder)
        {
            yield return ExecConvert(srcGran, dstGran);
            var actual = retSrcObj.transform;
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
        
        private GameObject retSrcObj; // コルーチンの結果を返す用
        
        /// <summary>
        /// ①テストゲームオブジェクトのコピー
        /// ②属性情報でのマテリアル分け
        /// ③結果をretSrcObjに格納
        /// </summary>
        private IEnumerator ExecConvert(MAGranularity srcGran, MAGranularity dstGran)
        {
            // テスト用ゲームオブジェクトのコピー
            retSrcObj = testData.CopyTranSrcOf(srcGran);
            
            
            var executorConf = new MAExecutorConfByAttr(
                testData.MaterialConfigByAttr(),
                new UniqueParentTransformList(retSrcObj.transform),
                dstGran, true, true, "tran:function"
            );

            var executor = MAExecutorFactory.CreateAttrExecutor(executorConf);
            yield return executor.Exec().AsIEnumerator();
        }
    }
}
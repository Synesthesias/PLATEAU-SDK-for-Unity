using NUnit.Framework;
using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.GranularityConvert;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace PLATEAU.Tests.EditModeTests.TestMaterialAdjust
{
    [TestFixture]
    public class TestGranularityConvert
    {

        private TestDataMA testData;

        [SetUp]
        private void SetUp()
        {
            testData = new TestDataMA();
        }
        
        // 粒度変換前後の一致チェック
        
        
        [UnityTest]
        public IEnumerator Test_AtomicToAtomic()
        {
            yield return AssertGranularityConverted(MAGranularity.PerAtomicFeatureObject, MAGranularity.PerAtomicFeatureObject, true);
        }
        
        [UnityTest]
        public IEnumerator Test_AtomicToPrimary()
        {
            yield return AssertGranularityConverted(MAGranularity.PerAtomicFeatureObject, MAGranularity.PerPrimaryFeatureObject, true);
        }
        
        [UnityTest]
        public IEnumerator Test_AtomicToArea()
        {
            yield return AssertGranularityConverted(MAGranularity.PerAtomicFeatureObject, MAGranularity.CombineAll, true);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToAtomic()
        {
            yield return AssertGranularityConverted(MAGranularity.PerPrimaryFeatureObject, MAGranularity.PerAtomicFeatureObject, false);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToPrimary()
        {
            yield return AssertGranularityConverted(MAGranularity.PerPrimaryFeatureObject, MAGranularity.PerPrimaryFeatureObject, true);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToArea()
        {
            yield return AssertGranularityConverted(MAGranularity.PerPrimaryFeatureObject, MAGranularity.CombineAll, true);
        }
        
        
        [UnityTest]
        public IEnumerator Test_AreaToAtomic()
        {
            yield return AssertGranularityConverted(MAGranularity.CombineAll, MAGranularity.PerAtomicFeatureObject, false);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToPrimary()
        {
            yield return AssertGranularityConverted(MAGranularity.CombineAll, MAGranularity.PerPrimaryFeatureObject, false);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToArea()
        {
            yield return AssertGranularityConverted(MAGranularity.CombineAll, MAGranularity.CombineAll, true);
        }


        /// <summary>
        /// 粒度<paramref name="srcGran"/>から<paramref name="dstGran"/>に変換して正しいかどうか確認します。
        /// <paramref name="assertOrder"/>は、子ゲームオブジェクトの順序も含めてチェックするならtrue、順不問ならfalseとします。
        /// 現状、細かい粒度に分解するときは順番を保証できません。粗い粒度に結合するときは順番は同じにできます。 
        /// </summary>
        private IEnumerator AssertGranularityConverted(MAGranularity srcGran, MAGranularity dstGran, bool assertOrder)
        {
            yield return ConvertGranularity(srcGran, dstGran);
            var srcTrans = retSrcObj.transform;
            var expectTrans = testData.CopyBldgSrcOf(dstGran).transform;
            srcTrans.name = expectTrans.name;
            // ここでsrcObjは変換後になっているはず
            if (assertOrder)
            {
                MAAssert.AreSameRecursive(expectTrans, srcTrans, dstGran);
            }
            else
            {
                MAAssert.AreSameSetRecursive(expectTrans, srcTrans, dstGran);
            }
        }

        private GameObject retSrcObj; // コルーチンの結果を返す用

        private IEnumerator ConvertGranularity(MAGranularity srcGran, MAGranularity dstGran)
        {
            Debug.Log($"Checking {srcGran} to {dstGran}");
            retSrcObj = testData.CopyBldgSrcOf(srcGran);
            var conf = new MAExecutorConf(
                null,
                new UniqueParentTransformList(retSrcObj.transform),
                dstGran, true, false);
            yield return new CityGranularityConverter().ConvertProgressiveAsync(conf, new MAConditionSimple()).AsIEnumerator();
        }


        
        
        
        
    }
}
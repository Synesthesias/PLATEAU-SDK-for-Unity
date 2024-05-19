using NUnit.Framework;
using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.GranularityConvert;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace PLATEAU.Tests.EditModeTests.TestMaterialAdjust
{
    /// <summary>
    /// 地物型でのマテリアル分けをテストします。
    /// MAはMaterialAdjustの略です。
    /// </summary>
    [TestFixture]
    public class TestMAByType
    {
        private TestDataMA testData = new TestDataMA();
        
        
        [UnityTest]
        public IEnumerator Test_AtomicToAtomic()
        {
            yield return AssertMA(MAGranularity.PerAtomicFeatureObject, MAGranularity.PerAtomicFeatureObject);
        }
        
        [UnityTest]
        public IEnumerator Test_AtomicToPrimary()
        {
            yield return AssertMA(MAGranularity.PerAtomicFeatureObject, MAGranularity.PerPrimaryFeatureObject);
        }
        
        [UnityTest]
        public IEnumerator Test_AtomicToArea()
        {
            yield return AssertMA(MAGranularity.PerAtomicFeatureObject, MAGranularity.CombineAll);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToAtomic()
        {
            yield return AssertMA(MAGranularity.PerPrimaryFeatureObject, MAGranularity.PerAtomicFeatureObject);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToPrimary()
        {
            yield return AssertMA(MAGranularity.PerPrimaryFeatureObject, MAGranularity.PerPrimaryFeatureObject);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToArea()
        {
            yield return AssertMA(MAGranularity.PerPrimaryFeatureObject, MAGranularity.CombineAll);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToAtomic()
        {
            yield return AssertMA(MAGranularity.CombineAll, MAGranularity.PerAtomicFeatureObject);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToPrimary()
        {
            yield return AssertMA(MAGranularity.CombineAll, MAGranularity.PerPrimaryFeatureObject);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToArea()
        {
            yield return AssertMA(MAGranularity.CombineAll, MAGranularity.CombineAll);
        }
        
        private IEnumerator AssertMA(MAGranularity srcGran, MAGranularity dstGran)
        {
            yield return ExecConvert(srcGran, dstGran);
            var actual = retSrcObj.transform;
            var expect = testData.CopyMATypeDstOf(dstGran).transform;
            // ルートの名前だけは違っても良い
            actual.name = expect.name;
            MAAssert.AreSameSetRecursive(expect, actual);
        }

        private GameObject retSrcObj; // コルーチンの結果を返す用
        
        /// <summary>
        /// ①テストゲームオブジェクトのコピー
        /// ②地物タイプでのマテリアル分け
        /// ③結果をretSrcObjに格納
        /// </summary>
        private IEnumerator ExecConvert(MAGranularity srcGran, MAGranularity dstGran)
        {
            // テスト用ゲームオブジェクトのコピー
            retSrcObj = testData.CopySrcOf(srcGran);
            
            // 以下に設定値を用意
            
            // テストデータに含まれる地物型を記述
            var types = new CityObjectType[]
                {
                    CityObjectType.COT_Building,
                    CityObjectType.COT_WallSurface,
                    CityObjectType.COT_RoofSurface,
                    CityObjectType.COT_GroundSurface,
                    CityObjectType.COT_OuterCeilingSurface
                }
                .Select(type => type.ToTypeNode())
                .ToArray();
            
            // 地物型ををもとに壁面を緑、屋根面を青とする設定にします
            var matConf = new MAMaterialConfig<CityObjectTypeHierarchy.Node>(types);
            var wall = matConf.GetConfFor(CityObjectType.COT_WallSurface.ToTypeNode());
            var roof = matConf.GetConfFor(CityObjectType.COT_RoofSurface.ToTypeNode());
            wall.ChangeMaterial = true;
            roof.ChangeMaterial = true;
            wall.Material = testData.Material(0);
            roof.Material = testData.Material(1);


            var executorConf = new MAExecutorConf(
                matConf,
                new UniqueParentTransformList(retSrcObj.transform),
                dstGran, true, true
            );

            var matChanger = new MAMaterialChanger(matConf, new MAMaterialSelectorByType());
            yield return new CityGranularityConverter()
                .ConvertProgressiveAsync(executorConf, new MAConditionMatChange(matChanger))
                .AsIEnumerator();
        }
    }
}
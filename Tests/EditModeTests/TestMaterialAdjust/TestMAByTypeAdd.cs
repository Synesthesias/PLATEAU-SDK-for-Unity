using NUnit.Framework;
using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
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
    /// Add(元オブジェクトを削除しない)設定で確認します。
    /// MAはMaterialAdjustの略です。
    /// </summary>
    [TestFixture]
    public class TestMAByTypeAdd
    {
        private TestDataMA testData = new TestDataMA();
        
        
        [UnityTest]
        public IEnumerator Test_AtomicToAtomic()
        {
            yield return AssertSameGran(MAGranularity.PerAtomicFeatureObject);
        }

        [UnityTest]
        public IEnumerator Test_PrimaryToPrimary()
        {
            yield return AssertSameGran(MAGranularity.PerPrimaryFeatureObject);
        }

        [UnityTest]
        public IEnumerator Test_AreaToArea()
        {
            yield return AssertSameGran(MAGranularity.CombineAll);
        }
        
        /// <summary>
        /// 同じGranularityに変換したときのチェック事項
        /// </summary>
        private IEnumerator AssertSameGran(MAGranularity gran)
        {
            yield return ExecConvert(gran, gran);
            var actual = retSrcObj.transform;
            var src = testData.CopyBldgSrcOf(gran).transform;
            
            // 同じ粒度で追加する設定なので、ヒエラルキーの各階層ごとにMeshRendererの数は2倍になっていることをチェックします。
            // ただし、LOD0と1は変換対象外で数は変わらないのでLOD2以下のみチェックします。
            string targetTransPath = "53393642_bldg_6697_2_op.gml/LOD2"; 
            var actualCountDict = MeshRendererCountDict(actual.Find(targetTransPath));
            var srcCountDict = MeshRendererCountDict(src.Find(targetTransPath));

            if (gran != MAGranularity.CombineAll)
            {
                // 数が2倍チェック
                Assert.AreEqual(srcCountDict.Count, actualCountDict.Count, "メッシュを子に持つparentTransformの数が同じ");
                foreach (var (parentName, count) in actualCountDict)
                {
                    Assert.IsTrue(srcCountDict.ContainsKey(parentName), $"同名の親が存在 : {parentName}");
                    Assert.AreEqual(srcCountDict[parentName] * 2, actualCountDict[parentName], $"{parentName}の子の数が2倍になっている");
                }
            }
            else
            {
                // 地域単位同士の変換だけ例外
                Assert.AreEqual(1, srcCountDict.Count);
                Assert.AreEqual(2, actualCountDict.Count);
            }
            
            
        }

        /// <summary>
        /// MeshRendererを数え上げるための辞書を作ります。キーが親Transformの名前、valueがその子のMeshRendererの数です。
        /// </summary>
        private Dictionary<string, int> MeshRendererCountDict(Transform transform)
        {
            var dict = new Dictionary<string, int>();
            new UniqueParentTransformList(transform).BfsExec(tran =>
            {
                var mr = tran.GetComponent<MeshRenderer>();
                if (mr == null) return NextSearchFlow.Continue;
                var num = dict.GetValueOrCreate(tran.name);
                dict[tran.name] = num + 1;
                return NextSearchFlow.Continue;
            });
            return dict;
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
            retSrcObj = testData.CopyBldgSrcOf(srcGran);
            
            var executorConf = new MAExecutorConf(
                testData.MaterialConfigByType(),
                new UniqueParentTransformList(retSrcObj.transform),
                dstGran, false, true
            );

            var executor = MAExecutorFactory.CreateTypeExecutor(executorConf);
            yield return executor.Exec().AsIEnumerator();
        }
    }
}
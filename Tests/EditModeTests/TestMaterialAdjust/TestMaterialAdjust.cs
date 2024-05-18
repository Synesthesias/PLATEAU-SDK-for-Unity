using NUnit.Framework;
using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace PLATEAU.Tests.EditModeTests.TestMaterialAdjust
{
    [TestFixture]
    public class TestMaterialAdjust
    {
        private static readonly string testDataPathBase =
            PathUtil.SdkPathToAssetPath("Tests/EditModeTests/TestMaterialAdjust/TestData");

        private static readonly string testDataPathBldgAtomic = testDataPathBase + "/BldgAtomic.prefab";
        private static readonly string testDataPathBldgPrimary = testDataPathBase + "/BldgPrimary.prefab";
        private static readonly string testDataPathBldgArea = testDataPathBase + "/BldgArea.prefab";

        /// <summary>
        /// 粒度とそれに対応するテストデータの辞書
        /// </summary>
        private static readonly Dictionary<MeshGranularity, GameObject> granularityTestDataDict =
        new Dictionary<MeshGranularity, GameObject>{
            {
                MeshGranularity.PerAtomicFeatureObject,
                AssetDatabase.LoadAssetAtPath<GameObject>(testDataPathBldgAtomic)
            },
            {
                MeshGranularity.PerPrimaryFeatureObject,
                AssetDatabase.LoadAssetAtPath<GameObject>(testDataPathBldgPrimary)
            },
            {
                MeshGranularity.PerCityModelArea,
                AssetDatabase.LoadAssetAtPath<GameObject>(testDataPathBldgArea)
            }
        };
        
        /// <summary>
        /// 粒度変換前後の一致チェック
        /// </summary>
        [UnityTest]
        public IEnumerator Test_AtomicToAtomic()
        {
            yield return AssertGranularityConverted(MeshGranularity.PerAtomicFeatureObject, MeshGranularity.PerAtomicFeatureObject, true);
        }
        
        [UnityTest]
        public IEnumerator Test_AtomicToPrimary()
        {
            yield return AssertGranularityConverted(MeshGranularity.PerAtomicFeatureObject, MeshGranularity.PerPrimaryFeatureObject, true);
        }
        
        [UnityTest]
        public IEnumerator Test_AtomicToArea()
        {
            yield return AssertGranularityConverted(MeshGranularity.PerAtomicFeatureObject, MeshGranularity.PerCityModelArea, true);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToAtomic()
        {
            yield return AssertGranularityConverted(MeshGranularity.PerPrimaryFeatureObject, MeshGranularity.PerAtomicFeatureObject, false);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToPrimary()
        {
            yield return AssertGranularityConverted(MeshGranularity.PerPrimaryFeatureObject, MeshGranularity.PerPrimaryFeatureObject, true);
        }
        
        [UnityTest]
        public IEnumerator Test_PrimaryToArea()
        {
            yield return AssertGranularityConverted(MeshGranularity.PerPrimaryFeatureObject, MeshGranularity.PerCityModelArea, true);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToAtomic()
        {
            yield return AssertGranularityConverted(MeshGranularity.PerCityModelArea, MeshGranularity.PerAtomicFeatureObject, false);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToPrimary()
        {
            yield return AssertGranularityConverted(MeshGranularity.PerCityModelArea, MeshGranularity.PerPrimaryFeatureObject, false);
        }
        
        [UnityTest]
        public IEnumerator Test_AreaToArea()
        {
            yield return AssertGranularityConverted(MeshGranularity.PerCityModelArea, MeshGranularity.PerCityModelArea, true);
        }
        
        
        /// <summary>
        /// 粒度<paramref name="srcGran"/>から<paramref name="dstGran"/>に変換して正しいかどうか確認します。
        /// <paramref name="assertOrder"/>は、子ゲームオブジェクトの順序も含めてチェックするならtrue、順不問ならfalseとします。
        /// 現状、細かい粒度に分解するときは順番を保証できません。粗い粒度に結合するときは順番は同じにできます。 
        /// </summary>
        private IEnumerator AssertGranularityConverted(MeshGranularity srcGran, MeshGranularity dstGran, bool assertOrder)
        {
            yield return ConvertGranularity(srcGran, dstGran);
            var srcTrans = retSrcObj.transform;
            var expectTrans = granularityTestDataDict[dstGran].transform;
            srcTrans.name = expectTrans.name;
            // ここでsrcObjは変換後になっているはず
            if (assertOrder)
            {
                AssertSameRecursive(expectTrans, srcTrans);
            }
            else
            {
                AssertSameSetRecursive(expectTrans, srcTrans);
            }
        }

        private GameObject retSrcObj; // コルーチンの結果を返す用

        private IEnumerator ConvertGranularity(MeshGranularity srcGran, MeshGranularity dstGran)
        {
            Debug.Log($"Checking {srcGran} to {dstGran}");
            retSrcObj = UnityEngine.Object.Instantiate(granularityTestDataDict[srcGran]);
            retSrcObj.name = retSrcObj.name.Replace("(Clone)", "");
            var conf = new MAExecutorConf(
                null,
                new UniqueParentTransformList(retSrcObj.transform),
                dstGran.ToMAGranularity(), true, false);
            yield return new CityGranularityConverter().ConvertProgressiveAsync(conf, new MAConditionSimple()).AsIEnumerator();
        }


        /// <summary>
        /// 2つのゲームオブジェクトとその子が同じことを確認します。子の順番が同じことも確認します。
        /// </summary>
        private void AssertSameRecursive(Transform op1, Transform op2)
        {
            AssertSame(op1, op2);
            // 再帰処理
            for (int i = 0; i < op1.childCount; i++)
            {
                AssertSameRecursive(op1.GetChild(i), op2.GetChild(i));
            }
        }

        /// <summary>
        /// 2つのゲームオブジェクトとその子が同じことを確認します。ただし子の順番は問いません。
        /// </summary>
        private void AssertSameSetRecursive(Transform op1, Transform op2)
        {
            AssertSame(op1, op2);
            for (int i = 0; i < op1.childCount; i++)
            {
                var op1Child = op1.GetChild(i);
                var op2Child = op2.Find(op1Child.name);
                AssertSameSetRecursive(op1Child, op2Child);
            }
        }
        
        private void AssertSame(Transform op1, Transform op2)
        {
            // 名前の同一をチェックします。ただし、地域単位に変換するときは名前がgroupとcombineで一致しないのでそこだけはOKとします。
            if (!(op1.name.Contains("group") && op2.name == "combined"))
            {
                Assert.AreEqual(op1.name, op2.name, $"ゲームオブジェクト名が同一 : {op1.name} == {op2.name}");
            }
            
            var mf1 = op1.GetComponent<MeshFilter>();
            var mf2 = op2.GetComponent<MeshFilter>();
            Assert.AreEqual(mf1 == null, mf2 == null, $"MeshFilterの有無が同一 : {op1.name}");
            // 頂点数のチェック
            if (mf1 != null)
            {
                var mesh1 = mf1.sharedMesh;
                var mesh2 = mf2.sharedMesh;
                Assert.AreEqual(mesh1.triangles.Length, mesh2.triangles.Length, $"頂点数が同一 : {op1.name}");
            }
            Assert.AreEqual(op1.childCount, op2.childCount, $"子の数が同一 : {op1.name}");
        }
        
        
        
    }
}
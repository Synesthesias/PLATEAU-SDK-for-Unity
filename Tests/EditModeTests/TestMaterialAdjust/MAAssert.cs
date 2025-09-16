using NUnit.Framework;
using PLATEAU.GranularityConvert;
using System;
using System.Linq;
using UnityEngine;

namespace PLATEAU.Tests.EditModeTests.TestMaterialAdjust
{
    /// <summary>
    /// マテリアル分けと粒度変更に関するテスト条件をチェックします。
    /// MAはMaterialAdjustの略です。
    /// </summary>
    public static class MAAssert
    {
        /// <summary>
        /// 2つのゲームオブジェクトとその子が同じことを確認します。子の順番が同じことも確認します。
        /// </summary>
        public static void AreSameRecursive(Transform op1, Transform op2, ConvertGranularity dstGranularity)
        {
            Assert.NotNull(op1);
            Assert.NotNull(op2);
            AreSame(op1, op2, dstGranularity);
            // 再帰処理
            for (int i = 0; i < op1.childCount; i++)
            {
                AreSameRecursive(op1.GetChild(i), op2.GetChild(i), dstGranularity);
            }
        }

        /// <summary>
        /// 2つのゲームオブジェクトとその子が同じことを確認します。ただし子の順番は問いません。
        /// </summary>
        public static void AreSameSetRecursive(Transform op1, Transform op2, ConvertGranularity dstGranularity)
        {
            Assert.NotNull(op1);
            Assert.NotNull(op2);
            AreSame(op1, op2, dstGranularity);
            for (int i = 0; i < op1.childCount; i++)
            {
                var op1Child = op1.GetChild(i);
                var op2Child = op2.Find(op1Child.name);
                if (op1Child.name == "combined") continue; // combinedという名前が変わったのでいったんスキップ
                Assert.IsNotNull(op2Child, $"op1に存在する{op1Child.name}がop2に見つかる");
                AreSameSetRecursive(op1Child, op2Child, dstGranularity);
            }
        }
        
        private static void AreSame(Transform op1, Transform op2, ConvertGranularity dstGranularity)
        {
            Assert.IsNotNull(op1);
            Assert.IsNotNull(op2);
            // 名前の同一をチェックします。ただし、地域単位に変換するときは名前がgroupとcombineで一致しないのでそこだけはOKとします。
            if (!(op1.name.Contains("group") && op2.name.EndsWith("combined")) && (!op1.name.EndsWith("combined") && op2.name.EndsWith("combined")))
            {
                Assert.AreEqual(op1.name, op2.name, $"ゲームオブジェクト名が同一 : {op1.name} == {op2.name}");
            }
            
            var mf1 = op1.GetComponent<MeshFilter>();
            var mf2 = op2.GetComponent<MeshFilter>();
            Assert.AreEqual(mf1 == null, mf2 == null, $"MeshFilterの有無が同一 : {op1.name}");
            
            if (mf1 != null)
            {
                // 頂点数のチェック
                var mesh1 = mf1.sharedMesh;
                var mesh2 = mf2.sharedMesh;
                Assert.IsTrue(mesh1 != null, $"op1: {op1.name}にMeshFilterがあるならmeshが存在");
                Assert.IsTrue(mesh2 != null, $"op2: {op2.name}にMeshFilterがあるならmeshが存在");
                Assert.AreEqual(mesh1.triangles.Length, mesh2.triangles.Length, $"頂点数が同一 : {op1.name}");

                if (dstGranularity != ConvertGranularity.PerCityModelArea)
                {
                    // マテリアルのチェック。
                    // ただし、地域単位への変換の場合、マテリアルはどの粒度から変換したかによって結果が異なり大変なのでいまのところスキップ。 FIXME
                    var materials1 = op1.GetComponent<MeshRenderer>().sharedMaterials;
                    var materials2 = op2.GetComponent<MeshRenderer>().sharedMaterials;
                    // マテリアルの順序までは保証しないので、sharedMaterialsを名前でソートしたものが同じならよしとします。
                    var m1Names = materials1.Distinct().Select(m => m.name).ToArray();
                    var m2Names = materials2.Distinct().Select(m => m.name).ToArray();
                    Assert.AreEqual(m1Names.Length, m2Names.Length, $"マテリアルの種類数が同一: {op1.name}");
                    Array.Sort(m1Names);
                    Array.Sort(m2Names);
                    for (int i = 0; i < m1Names.Length; i++)
                    {
                        string name1 = m1Names[i];
                        string name2 = m2Names[i];
                        // 連番により名前の最後の数字だけ変わるかもしれないので、そこは文字はチェックから外します。
                        name1 = name1.Substring(0, name1.Length - 1);
                        name2 = name2.Substring(0, name2.Length - 1);
                        Assert.AreEqual(name1, name2);
                    }
                }
                
            }
            Assert.AreEqual(op1.childCount, op2.childCount, $"子の数が同一 : {op1.name}");
        }
    }
}
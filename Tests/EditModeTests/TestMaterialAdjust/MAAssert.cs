using NUnit.Framework;
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
        public static void AreSameRecursive(Transform op1, Transform op2)
        {
            AreSame(op1, op2);
            // 再帰処理
            for (int i = 0; i < op1.childCount; i++)
            {
                AreSameRecursive(op1.GetChild(i), op2.GetChild(i));
            }
        }

        /// <summary>
        /// 2つのゲームオブジェクトとその子が同じことを確認します。ただし子の順番は問いません。
        /// </summary>
        public static void AreSameSetRecursive(Transform op1, Transform op2)
        {
            AreSame(op1, op2);
            for (int i = 0; i < op1.childCount; i++)
            {
                var op1Child = op1.GetChild(i);
                var op2Child = op2.Find(op1Child.name);
                AreSameSetRecursive(op1Child, op2Child);
            }
        }
        
        private static void AreSame(Transform op1, Transform op2)
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
                Assert.IsTrue(mesh1 != null, $"op1: {op1.name}にMeshFilterがあるならmeshが存在");
                Assert.IsTrue(mesh2 != null, $"op2: {op2.name}にMeshFilterがあるならmeshが存在");
                Assert.AreEqual(mesh1.triangles.Length, mesh2.triangles.Length, $"頂点数が同一 : {op1.name}");
            }
            Assert.AreEqual(op1.childCount, op2.childCount, $"子の数が同一 : {op1.name}");
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.GranularityConvert
{
    /// <summary>
    /// Transformとその子を幅優先探索するクラスです。
    /// 探索で何をするかはラムダ式で渡します。
    /// </summary>
    public static class TransformBFS
    {
        /// <summary>
        /// <paramref name="rootTransforms"/>とその子を幅優先探索し、各Transformに対して
        /// ラムダ式 <paramref name="forEachTransform"/> を実行します。
        /// ラムダ式は、Transformを引数とし、探索を続けるかどうかをboolで返します。
        /// ラムダ式がfalseを返した時点で探索処理を終了します。
        /// </summary>
        public static void Exec(IEnumerable<Transform> rootTransforms, Func<Transform, bool> forEachTransform)
        {
            var queue = new Queue<Transform>(rootTransforms);
            while (queue.Count > 0)
            {
                var trans = queue.Dequeue();
                bool doContinue = forEachTransform(trans);
                if (!doContinue) break;
                for (int i = 0; i < trans.childCount; i++)
                {
                    queue.Enqueue(trans.GetChild(i));
                }
            }
        }
    }
}
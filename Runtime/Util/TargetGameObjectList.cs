using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.Util
{
    /// <summary>
    /// ユーザーが親ゲームオブジェクトを複数選択するときに使います。
    /// ある処理について、処理の対象が、選択されたゲームオブジェクトとその子であるとき、
    /// objAとその子objBが両方選択されていたら、重複となりobjBとその子の処理が2回行われてしまいます。
    /// それを防ぐため、親ゲームオブジェクトとしてユニークなもののみを保持し、重複は無視するクラスです。
    /// 加えてnullが渡された場合も無視します。
    /// </summary>
    public class UniqueParentTransformList
    {
        private List<Transform> data = new ();

        public UniqueParentTransformList() : this(new Transform[]{})
        {
            
        }
        
        public UniqueParentTransformList(IEnumerable<Transform> src)
        {
            Init(src);
        }

        public void Init(IEnumerable<Transform> src)
        {
            data.Clear();
            AddRange(src);
        }

        public IReadOnlyCollection<Transform> Get => data;
        public int Count => data.Count;

        public void Add(Transform op)
        {
            if (op == null) return; // nullは無視
            foreach (var d in data)
            {
                if (d == op) return; // 重複は無視
                else if (d.IsChildOf(op))  // 親が渡されたら、子と入れ替える
                {
                    data.Remove(d);
                    data.Add(op);
                    return;
                }else if (op.IsChildOf(d)) // 子が渡されたら無視
                {
                    return;
                }
            }
            data.Add(op); // ユニークな親なら追加
        }

        public void AddRange(IEnumerable<Transform> transforms)
        {
            foreach (var t in transforms)
            {
                Add(t);
            }
        }
        
        /// <summary>
        /// <see cref="data"/>とその子を幅優先探索し、各Transformに対して
        /// ラムダ式 <paramref name="forEachTransform"/> を実行します。
        /// ラムダ式は、Transformを引数とし、探索を続けるかどうかをboolで返します。
        /// ラムダ式がfalseを返した時点で探索処理を終了します。
        /// </summary>
        public void BfsExec(Func<Transform, bool> forEachTransform)
        {
            var queue = new Queue<Transform>(data);
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

        public void Reset()
        {
            data.Clear();
        }
    }
}
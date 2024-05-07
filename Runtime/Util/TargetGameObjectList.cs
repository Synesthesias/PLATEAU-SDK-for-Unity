using System.Collections.Generic;
using System.Threading.Tasks;
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
    public class UniqueParentTransformList // API公開
    {
        private List<Transform> data = new ();

        /// <summary> 空配列で初期化 </summary>
        public UniqueParentTransformList() : this(new Transform[]{})
        {
            
        }

        /// <summary> 1つのみのゲームオブジェクトを初期値として初期化 </summary>
        public UniqueParentTransformList(Transform oneSrc) : this(new Transform[] { oneSrc })
        {
            
        }
        
        /// <summary> ゲームオブジェクトの配列を初期値として初期化 </summary>
        public UniqueParentTransformList(IEnumerable<Transform> src)
        {
            Init(src);
        }

        public UniqueParentTransformList(UniqueParentTransformList copyFrom)
        {
            Init(copyFrom.Get);
        }

        /// <summary> 汎用初期化メソッド </summary>
        public void Init(IEnumerable<Transform> src)
        {
            data.Clear();
            AddRange(src);
        }

        /// <summary> 持っているTransformの一覧を読み取り専用で返します </summary>
        public IReadOnlyCollection<Transform> Get => data;
        public int Count => data.Count;

        /// <summary>
        /// Transformを1つ追加します。
        /// ただし、重複や親子関係にあるTransformが渡された場合は追加しません。
        /// </summary>
        public void Add(Transform op)
        {
            if (op == null) return; // nullは無視
            foreach (var d in data)
            {
                if (d == op) return; // 重複は無視
                else if (d.IsChildOf(op))  // 親が渡されたら、子と入れ替える
                {
                    int id = data.IndexOf(d);
                    data[id] = op;
                    return;
                }else if (op.IsChildOf(d)) // 子が渡されたら無視
                {
                    return;
                }
            }
            data.Add(op); // ユニークな親なら追加
        }

        /// <summary> <see cref="Add"/>の複数渡す版 </summary>
        public void AddRange(IEnumerable<Transform> transforms)
        {
            foreach (var t in transforms)
            {
                Add(t);
            }
        }

        /// <summary>
        /// <see cref="data"/>とその子を幅優先探索し、各Transformに対して
        /// 式 <paramref name="forEachTransform"/> を実行します。
        /// </summary>
        public void BfsExec(ForEachTransform forEachTransform)
        {
            BfsExec(false,
                t => Task.FromResult(forEachTransform(t))
                    ).Wait();
        }
        
        /// <summary>
        /// <see cref="data"/>とその子を深さ優先探索し、各Transformに対して
        /// 式 <paramref name="forEachTransform"/> を実行します。
        /// </summary>
        public void DfsExec(ForEachTransform forEachTransform)
        {
            DfsExec(false,
                t => Task.FromResult(forEachTransform(t))
            ).Wait();
        }
        

        /// <summary>
        /// <see cref="BfsExec"/>の非同期版です。
        /// </summary>
        public async Task BfsExecAsync(ForEachTransformAsync forEachTransform)
        {
            await BfsExec(true, forEachTransform);
        }

        /// <summary>
        /// <see cref="DfsExec"/>の非同期版です。
        /// </summary>
        public async Task DfsExecAsync(ForEachTransformAsync forEachTransform)
        {
            await DfsExec(true, forEachTransform);
        }
        
        /// <summary>
        /// <see cref="data"/>とその子を幅優先探索し、各Transformに対して
        /// 式 <paramref name="forEachTransform"/> を実行します。
        /// </summary>
        private async Task BfsExec(bool isAsync, ForEachTransformAsync forEachTransform)
        {
            var queue = new Queue<Transform>(data);
            while (queue.Count > 0)
            {
                var trans = queue.Dequeue();
                var nextSearchFlow = isAsync ? await forEachTransform(trans) : forEachTransform(trans).Result;
                if (nextSearchFlow == NextSearchFlow.Abort)
                {
                    break;
                }
                else if (nextSearchFlow == NextSearchFlow.SkipChildren)
                {
                    continue; // 子をキューに入れない
                }

                for (int i = 0; i < trans.childCount; i++)
                {
                    queue.Enqueue(trans.GetChild(i));
                }
            }
        }

        /// <summary>
        /// <see cref="data"/>とその子を深さ優先探索し、各Transformに対して
        /// デリゲート <paramref name="forEachTransform"/> を実行します。
        /// </summary>
        private async Task DfsExec(bool isAsync, ForEachTransformAsync forEachTransform)
        {
            var stack = new Stack<Transform>(data);
            while (stack.Count > 0)
            {
                var trans = stack.Pop();
                var nextSearchFlow = isAsync ? await forEachTransform(trans) : forEachTransform(trans).Result;
                if (nextSearchFlow == NextSearchFlow.Abort)
                {
                    break;
                }
                else if (nextSearchFlow == NextSearchFlow.SkipChildren)
                {
                    continue; // 子をスタックに入れない
                }

                for (int i = 0; i < trans.childCount; i++)
                {
                    stack.Push(trans.GetChild(i));
                }
            }
        }

        public void Reset()
        {
            data.Clear();
        }
    }

    /// <summary>
    /// ノードを探索する際の、次の探索の流れを指定します。
    /// </summary>
    public enum NextSearchFlow
    {
        /// <summary> 通常通り探索続行 </summary>
        Continue,
        /// <summary> 現在の子の探索をスキップ </summary>
        SkipChildren,
        /// <summary> 探索をここで打ち切り </summary>
        Abort
    }

    
    /// <summary> Transformの走査について、各Transformで行う処理を行い、次にするべき探索処理を返す式の型です。 </summary>
    public delegate NextSearchFlow ForEachTransform(Transform t);
    
    /// <summary> <see cref="ForEachTransform"/>の非同期版です。 </summary>
    public delegate Task<NextSearchFlow> ForEachTransformAsync(Transform t);
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.CommonDataStructure
{
    /// <summary>
    /// 自作データ構造です。 次の性質を持ちます。
    /// ・木構造であり、各ノードが子ノードのリストを持ちます。
    /// ・子ノードリストの順番が常にソートされた状態を保ちます。
    ///
    /// 木全体で1つのソートを表現する二分探索木とは異なり、
    /// 直近の子のリスト内でのみソートが保たれているのが特徴です。
    /// この特徴は分類を表現するのに役立つので、「分類ツリー」と名付けます。
    ///
    /// 例:
    /// {
    ///   { 1.東京都　 => 1-1.世田谷区, 1-2.練馬区　 }
    ///   { 2.神奈川県 => 2-1.横浜市　, 2-3.相模原市 }
    /// }
    /// ここに "2-2.川崎市" を追加すると、必ずその順番は 2-1. のあとになります。
    /// </summary>
    internal class ClassificationTree<TKey, TVal>
        where TKey : IComparable<TKey>
    {
        private readonly TVal value;

        private readonly SortedDictionary<TKey, ClassificationTree<TKey, TVal>> children =
            new SortedDictionary<TKey, ClassificationTree<TKey, TVal>>();

        public ClassificationTree<TKey, TVal> Parent { get; }

        /// <summary>
        /// ノードを作ります。
        /// ルートノードの場合は <paramref name="parent"/> は null にしてください。
        /// </summary>
        public ClassificationTree(TVal value, ClassificationTree<TKey, TVal> parent)
        {
            this.value = value;
            Parent = parent;
        }

        public TVal Value => this.value;

        public void AddChild(TKey key, TVal val)
        {
            this.children.Add(key, new ClassificationTree<TKey, TVal>(val, this));
        }

        /// <summary>
        /// 子に <paramref name="key"/> を含むかどうかをboolで返します。
        /// 再帰的には処理しません。
        /// </summary>
        public bool ContainsInChildren(TKey key)
        {
            return this.children.ContainsKey(key);
        }

        public bool HasAnyChild()
        {
            return this.children.Any();
        }

        public ClassificationTree<TKey, TVal> GetChild(TKey childKey)
        {
            return this.children[childKey];
        }



        /// <summary>
        /// 深さ優先探索 (DFS) で木の全体をイテレートします。
        /// </summary>
        public IEnumerable<(int depth, ClassificationTree<TKey, TVal> node)> IterateDfsWithDepth()
        {
            var values = IterateDfsRecursive(this, 1);
            foreach (var val in values)
            {
                yield return val;
            }
        }


        /// <summary>
        /// 深さ優先探索 (DFS) で、指定のノード以下をイテレートします。
        /// </summary>
        public static IEnumerable<(int depth, ClassificationTree<TKey, TVal> node)> IterateDfsRecursive(ClassificationTree<TKey, TVal> node, int depth)
        {
            if (node == null)
            {
                throw new ArgumentNullException($"{nameof(node)}");
            }

            yield return (depth, node);
            foreach (var child in node.children)
            {
                var childNode = child.Value;
                var childRecursive = IterateDfsRecursive(childNode, depth + 1);
                foreach (var c in childRecursive)
                {
                    yield return c;
                }
            }
        }

    }
}
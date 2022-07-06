using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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
    internal class ClassificationTree<TKey, TValue>
        where TKey : IComparable<TKey>
    {
    private TValue value;

    private readonly SortedDictionary<TKey, ClassificationTree<TKey, TValue>> children =
        new SortedDictionary<TKey, ClassificationTree<TKey, TValue>>();

    public ClassificationTree(TValue value)
    {
        this.value = value;
    }

    public void AddChild(TKey key, TValue val)
    {
        this.children.Add(key, new ClassificationTree<TKey, TValue>(val));
    }

    /// <summary>
    /// 子に <paramref name="key"/> を含むかどうかをboolで返します。
    /// 再帰的には処理しません。
    /// </summary>
    public bool ContainsInChildren(TKey key)
    {
        return this.children.ContainsKey(key);
    }

    public ClassificationTree<TKey, TValue> GetChild(TKey childKey)
    {
        return this.children[childKey];
    }

    // private IEnumerator<T> Children => this.children.GetEnumerator();

    /// <summary>
    /// 深さ優先探索 (DFS) で木をイテレートします。
    /// </summary>
    public IEnumerable<(int depth, TValue value)> IterateDfs()
    {
        var values = IterateDfsRecursive(this, 1);
        foreach (var val in values)
        {
            yield return val;
        }
    }

    private IEnumerable<(int depth, TValue value)> IterateDfsRecursive(ClassificationTree<TKey, TValue> node, int depth)
    {
        if (node == null)
        {
            throw new ArgumentNullException($"{nameof(node)}");
        }

        yield return (depth, node.value);
        foreach (var child in node.children)
        {
            var childNode = child.Value;
            var childRecursive = IterateDfsRecursive(childNode, depth+1);
            foreach (var c in childRecursive)
            {
                yield return c;
            }
        }
    }

    }
}
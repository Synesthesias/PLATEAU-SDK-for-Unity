using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PLATEAU.CityAdjust.NonLibData
{
    
    /// <summary>
    /// ゲームオブジェクトの変換において、変換元から変換後にデータをコピーする目的でデータを覚えておくための辞書です。
    /// ジェネリック型には、覚えておきたいデータの型を指定します。
    /// キーは<see cref="NonLibPath"/>であり、値は<see cref="NonLibValue{ValueT}"/>のリストです。
    /// 値がリストになっている理由は、ゲームオブジェクトが同名でパスが同じものをすべて覚えておくためで、区別方法は index = 走査順番とします。
    /// ここで走査順番を使うということは、走査の順番は構築と復元で同じである必要があり、覚えておきたいデータがない場合もnullを登録する必要があります。
    /// </summary>
    internal class NonLibDictionary<ValueT> where ValueT : class
    {
        private Dictionary<NonLibPath, List<NonLibValue<ValueT>>> pathToValDict = new();
        
        /// <summary>
        /// 未復元でありnullでないデータの数です。
        /// </summary>
        public int RemainingNonRestored { get; private set; } = 0;

        /// <summary>
        /// 辞書にデータを追加します。
        /// </summary>
        /// <param name="obj">このTransformのパスをキーとして記録します。</param>
        /// <param name="baseTransforms">パスの基準です。</param>
        /// <param name="value">覚えておくデータです。</param>
        public void Add(Transform obj, Transform[] baseTransforms, ValueT value)
        {
            var path = new NonLibPath(obj, baseTransforms);
            if (!pathToValDict.ContainsKey(path))
            {
                pathToValDict[path] = new List<NonLibValue<ValueT>>();
            }
            pathToValDict[path].Add(new NonLibValue<ValueT>(value, false));
            if (value != null)
            {
                RemainingNonRestored++;
            }
        }
        
        /// <summary>
        /// 引数の<paramref name="target"/>と基準Transformからパスを求め、
        /// そのパスに合致する、なおかつ未復元である最初のデータを返します。
        /// そのデータを復元済みとしてマークします。
        /// 該当がなければnullを返します。
        /// </summary>
        public ValueT GetNonRestoredAndMarkRestored(Transform target, Transform[] baseTransforms)
        {
            var path = new NonLibPath(target, baseTransforms);
            if (pathToValDict.TryGetValue(path, out var values))
            {
                var val =  values.FirstOrDefault(val => !val.IsRestored);
                if (val == null) return null;
                val.IsRestored = true;
                if (val.MainValue != null)
                {
                    RemainingNonRestored--;
                }
                return val.MainValue;
            }
            return null;
        }
        
        /// <summary>
        /// 変換の前後で対応するゲームオブジェクトを特定するための情報であり、
        /// ある基準Transformからのパス(スラッシュでつないだもの)をキーとします。
        /// 基準Transformは複数あっても良いものとし、対象objの親を辿っていって、基準Transformのどれかにマッチすればそれが基準となります。
        /// 比較時はPathが考慮され、基準Transformは考慮されません。
        /// </summary>
        private class NonLibPath
        {
            public string Path { get; }
            public string ObjName { get; }
    
            public NonLibPath(Transform obj, Transform[] baseTransforms)
            {
                Path = MakePath(obj, baseTransforms);
                ObjName = obj.name;
            }
    
            public override bool Equals(object obj)
            {
                if (obj is NonLibPath other)
                {
                    return Path == other.Path;
                }
    
                return false;
            }
    
            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }
    
            private string MakePath(Transform obj, Transform[] baseTransforms)
            {
                List<string> pathes = new();
                var current = obj;
                if (baseTransforms.Contains(current))
                {
                    return current.name;
                }
                
                while (current != null && !baseTransforms.Contains(current))
                {
                    pathes.Add(current.name);
                    current = current.parent;
                }
    
                var sb = new StringBuilder();
                for (int i = pathes.Count - 1; i >= 0; i--)
                {
                    sb.Append(pathes[i]);
                    if(i != 0) sb.Append('/');
                }
    
                return sb.ToString();
            }
            
            public override string ToString()
            {
                return Path;
            }
        }

        /// <summary>
        /// メイン値と、これが復元済みかどうかのフラグの組です。
        /// </summary>
        private class NonLibValue<MainValueType>
        {
            public MainValueType MainValue;
            public bool IsRestored;

            public NonLibValue(MainValueType mainValue, bool isRestored)
            {
                MainValue = mainValue;
                IsRestored = isRestored;
            }
        }
    }
}